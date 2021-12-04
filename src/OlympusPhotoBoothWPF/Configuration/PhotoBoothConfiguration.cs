namespace OlympusPhotoBoothWPF.Configuration
{
  public class PhotoBoothConfiguration
  {
    public string Title { get; set; }

    public ServiceConfiguration ServiceConfig { get; set; }
  }

  public class ServiceConfiguration
  {
    public static bool UseRecMode { get; set; }
    public string BaseUrl { get; set; }
    public int DelayInMillisecondsBetween1st2ndpush { get; set; }
    public string ImagePath { get; set; } = @"./images";
  }
}
