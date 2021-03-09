using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace EventRouterFunc
{
    class HashFunction
    {
        public static string GetQueueName(string fileUriString)
        {
            if (!Uri.TryCreate(fileUriString, UriKind.Absolute, out Uri fileUri))
            {
                throw new InvalidFileUriException(fileUriString);
            }

            StringBuilder sb = new StringBuilder();
            int i;
            for (i = 1; i < (fileUri.Segments.Length - 1); i++)
            {
                string[] nameValue = fileUri.Segments[i].Split('=');
                if (nameValue.Length != 2)
                    sb.Append(fileUri.Segments[i]);
                else
                {
                    // partitionFound
                    break;
                }
            }

            // Backup one segment to get table name 
            string tableName = fileUri.Segments[(i - 1)];

            // Remove trailing slash
            tableName = tableName.Remove(tableName.Length - 1, 1);

            // Change _ to – (_ not allowed in queue name) and take first 22 characters of table name
            if (tableName.Length > 22)
                tableName = tableName.Remove(22);

            string tablePath = sb.ToString().Remove(sb.Length - 1, 1);
            string queueName = $"{tableName.Replace('_', '-')}-{GetSha1Hash(tablePath)}";

            return queueName;
        }

        static string GetSha1Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                    sb.Append(b.ToString("x2"));

                return sb.ToString();
            }
        }
    }

    public class InvalidFileUriException : Exception
    {
        public InvalidFileUriException() { }
        public InvalidFileUriException(string fileName)
            : base($"Invalid file uri for '{fileName}'") { }
    }

}
