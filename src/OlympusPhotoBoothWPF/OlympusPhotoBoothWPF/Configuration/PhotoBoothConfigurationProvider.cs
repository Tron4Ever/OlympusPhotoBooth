using Microsoft.Extensions.Configuration;
using System.IO;

namespace OlympusPhotoBoothWPF.Configuration
{
  internal static class PhotoBoothConfigurationProvider
  {
    public static PhotoBoothConfiguration CurrentConfig
    {
      get
      {
        return GetPhotoBoothConfiguration();
      }
    }

    public static IConfigurationRoot GetAppSettings()
    {
      string applicationExeDirectory = ApplicationExeDirectory();

      var builder = new ConfigurationBuilder()
      .SetBasePath(applicationExeDirectory)
      .AddJsonFile("appsettings.json");

      return builder.Build();
    }

    public static PhotoBoothConfiguration GetPhotoBoothConfiguration()
    {
      return GetAppSettings().Get<PhotoBoothConfiguration>();
    }

    private static string ApplicationExeDirectory()
    {
      var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
      var appRoot = Path.GetDirectoryName(location);

      return appRoot;
    }
  }
}