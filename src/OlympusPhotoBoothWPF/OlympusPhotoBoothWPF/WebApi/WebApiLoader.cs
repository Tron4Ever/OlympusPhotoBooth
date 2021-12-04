using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OlympusPhotoBoothWPF.Configuration;
using Microsoft.Extensions.Logging.Console;
using System.Threading;
using System.Threading.Tasks;

namespace OlympusPhotoBoothWPF.WebApi
{
  internal class WebApiLoader
  {
    private static IHost _webHost;

    public static IHost WebHost
    {
      get
      {
        return _webHost;
      }
    }

    public static async Task StartAsync()
    {
      PhotoBoothConfiguration config = null;
      _webHost = Host.CreateDefaultBuilder()
          .ConfigureAppConfiguration((hostingContext, configuration) =>
          {
            configuration.Sources.Clear();
            configuration
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            config = configuration.Build().Get<PhotoBoothConfiguration>();
          })
          .ConfigureWebHostDefaults(builder =>
          {
            builder.UseStartup<Startup>();
          })
          .Build();
      await _webHost.StartAsync();
    }

    public static async Task StopAsync()
    {
      await _webHost.StopAsync(CancellationToken.None);
      _webHost.Dispose();
    }
  }
}
