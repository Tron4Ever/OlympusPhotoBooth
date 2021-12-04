using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace OlympusPhotoBoothWPF
{
  class RessourceHelper
  {
    public static byte[] GetImageFromRessource(string ressourceName)
    {
      var resourceName = typeof(RessourceHelper).Assembly.GetManifestResourceNames().FirstOrDefault(x => x.Contains(ressourceName));
      if (resourceName == null) return null;
      using var stream = typeof(RessourceHelper).Assembly.GetManifestResourceStream(resourceName);
      if (stream == null) return null;
      using var memoryStream = new MemoryStream();
      stream.CopyTo(memoryStream);
      memoryStream.Position = 0;
      return memoryStream.ToArray();
    }

    public static BitmapImage LoadImage(byte[] imageData)
    {
      if (imageData == null || imageData.Length == 0) return null;
      var image = new BitmapImage();
      using (var mem = new MemoryStream(imageData))
      {
        mem.Position = 0;
        image.BeginInit();
        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = null;
        image.StreamSource = mem;
        image.EndInit();
      }
      image.Freeze();
      return image;
    }
  }
}
