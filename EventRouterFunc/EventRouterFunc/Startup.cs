using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(EventRouterFunc.Startup))]

namespace EventRouterFunc
{
    /// <summary>
    /// IoC Startup Class (Required)
    /// </summary>
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            //builder.Services.AddSingleton<IConfigCacheClient>((s) => {
            //    return new ConfigCacheClient();
            //});
        }
    }
}
