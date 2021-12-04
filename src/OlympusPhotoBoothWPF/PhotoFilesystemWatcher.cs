using OlympusPhotoBoothWPF.Configuration;
using OlympusPhotoBoothWPF.WebApi;
using System.IO;
using System.Threading;

namespace OlympusPhotoBoothWPF
{
  internal static class PhotoFilesystemWatcher
  {
    private static FileSystemWatcher _watcher;

    public static void Start()
    {
      if (_watcher == null)
      {
        _watcher = new FileSystemWatcher($"{PhotoBoothConfigurationProvider.CurrentConfig.ServiceConfig.ImagePath}/current", "*.*");
        _watcher.Created += watcher_Created;
        _watcher.Deleted += watcher_Deleted;
        _watcher.EnableRaisingEvents = true;
      }
    }

    private static void watcher_Deleted(object sender, FileSystemEventArgs e)
    {
      CurrentImageCache.Instance.Set(new Microsoft.AspNetCore.Mvc.FileContentResult(RessourceHelper.GetImageFromRessource("InstaBitcoinLogoBitteWarten"), "image/png"), "Temp.jpg");
    }

    internal static void Stop()
    {
      if (_watcher != null)
      {
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
        _watcher = null;
      }
    }

    private static void watcher_Created(object sender, FileSystemEventArgs e)
    {
      lock (_watcher)
      {
        byte[] fileContent;
        while (true)
        {
          try
          {
            using (FileStream fs = File.OpenRead(e.FullPath))
            {
              using (BinaryReader binaryReader = new BinaryReader(fs))
              {
                fileContent = binaryReader.ReadBytes((int)fs.Length);
              }
            }
            CurrentImageCache.Instance.Set(new Microsoft.AspNetCore.Mvc.FileContentResult(fileContent, "image/png"), e.Name);
            var archiveFilePath = Path.Combine($"{PhotoBoothConfigurationProvider.CurrentConfig.ServiceConfig.ImagePath}/archive", e.Name);
            File.WriteAllBytes(archiveFilePath, fileContent);
            return;
          }
          catch (IOException)
          {
            Thread.Sleep(100);
          }
        }
      }
    }
  }
}
