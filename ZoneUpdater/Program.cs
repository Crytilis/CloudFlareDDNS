using ZoneUpdater.Clients;
using ZoneUpdater.Services;

namespace ZoneUpdater
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config
                        .SetBasePath(context.HostingEnvironment.ContentRootPath)
                        .AddJsonFile($"appsettings.{env}.json", optional: false, reloadOnChange: true)
                        .AddJsonFile("encryptedsettings.json", optional: false, reloadOnChange: true)
                        .Build();
                })
                .ConfigureServices((context, services) =>
                {
                    var apiToken = context.Configuration.GetValue<string>("CloudFlare:ApiKey");
                    services
                        .Configure<HostOptions>(option => { option.ShutdownTimeout = TimeSpan.FromSeconds(10); })
                        .AddSingleton(_ => new CloudFlareClient(apiToken))
                        .AddHostedService<DDnsUpdaterService>()
                        .AddDataProtection();
                });
        }
    }
}