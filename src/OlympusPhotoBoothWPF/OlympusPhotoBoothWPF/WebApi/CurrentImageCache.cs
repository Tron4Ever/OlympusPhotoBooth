using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace OlympusPhotoBoothWPF.WebApi
{
  public class CurrentImageCache : INotifyPropertyChanged
  {
    private static CurrentImageCache _instance = null;
    public static CurrentImageCache Instance
    {
      get
      {
        if (_instance == null)
        {
          _instance = new CurrentImageCache();
        }

        return _instance;
      }
    }

    public BitmapImage Image
    {
      get
      {
        if (ImageFile != null)
          return RessourceHelper.LoadImage(ImageFile.FileContents);
        else
          return null;
      }
    }

    public FileContentResult ImageFile { get; private set; }

    public string Name { get; private set; }

    public event PropertyChangedEventHandler PropertyChanged;

    public void Set(FileContentResult imageFile, string name)
    {
      ImageFile = imageFile;
      Name = name;

      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(Image)));
      }
    }
  }
}