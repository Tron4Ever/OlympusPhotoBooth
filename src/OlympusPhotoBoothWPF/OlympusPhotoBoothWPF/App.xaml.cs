using System.IO;
using System.Windows;
using Microsoft.Extensions.Logging;
using OlympusPhotoBoothWPF.Configuration;
using OlympusPhotoBoothWPF.WebApi;

namespace OlympusPhotoBoothWPF
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    protected override async void OnStartup(StartupEventArgs e)
    {
      var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
      await WebApiLoader.StartAsync();

      var directory = new DirectoryInfo($"{PhotoBoothConfigurationProvider.CurrentConfig.ServiceConfig.ImagePath}/archive");
      if (!directory.Exists) directory.Create();
      directory = new DirectoryInfo($"{PhotoBoothConfigurationProvider.CurrentConfig.ServiceConfig.ImagePath}/current");
      if (!directory.Exists) directory.Create();

      PhotoFilesystemWatcher.Start();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
      PhotoFilesystemWatcher.Stop();
      await WebApiLoader.StopAsync();
      base.OnExit(e);
    }
  }
}
