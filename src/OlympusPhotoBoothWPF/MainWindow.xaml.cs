using BespokeFusion;
using OlympusPhotoBoothWPF.Configuration;
using OlympusPhotoBoothWPF.WebApi;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace OlympusPhotoBoothWPF
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private Exception _lastException = null;
    private DispatcherTimer _timer;
    private bool _cameraConnected = false;

    public MainWindow()
    {
      InitializeComponent();
      SetErrorText("Fotobox defekt.\nBitte Robin rufen\n:-(");

      _timer = new DispatcherTimer();
      _timer.Tick += _timer_Tick;
      _timer.Interval = TimeSpan.FromSeconds(10);
      _timer.Start();
      _timer_Tick(null, null);
    }

    private void _timer_Tick(object sender, EventArgs e)
    {
      Ping pingSender = new Ping();
      IPAddress address = IPAddress.Parse("192.168.0.10");
      string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
      byte[] buffer = Encoding.ASCII.GetBytes(data);
      int timeout = 1000;
      PingOptions options = new PingOptions(64, true);
      PingReply reply = pingSender.Send(address, timeout, buffer, options);
      _cameraIpStatus.Fill = reply.Status == IPStatus.Success ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
      _cameraConnected = reply.Status == IPStatus.Success;

      pingSender = new Ping();
      address = IPAddress.Parse("192.168.1.10");
      reply = pingSender.Send(address, timeout, buffer, options);
      _wifiRemoteStatus.Fill = reply.Status == IPStatus.Success ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
    }

    private async void ButtonPhotoClick(object sender, RoutedEventArgs e)
    {
      e.Handled = true;
      if (_cameraConnected == false) return;

      CountdownLabel.Visibility = Visibility.Collapsed;
      FotoboxImage.Visibility = Visibility.Visible;

      try
      {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:5001");
        httpClient.Timeout = TimeSpan.FromSeconds(20);

        HttpResponseMessage response = await httpClient.PostAsync("OlympusCamera/TakePictureAllInOne", null);
      }
      catch (Exception ex)
      {
        _lastException = ex;
        SetErrorText("Fotobox defekt.\nBitte Robin rufen\n:-(");
      }
    }

    private void SetErrorText(string errorText)
    {
      FotoboxImage.Visibility = Visibility.Hidden;
      CountdownLabel.Visibility = Visibility.Visible;
      CountdownLabel.Text = errorText;
    }

    private async void ButtonPhotoClickIn10Seconds(object sender, RoutedEventArgs e)
    {
      e.Handled = true;
      if (_cameraConnected == false) return;

      FotoboxImage.Visibility = Visibility.Hidden;
      CountdownLabel.Visibility = Visibility.Visible;

      for (int i = 10; i != 0; i--)
      {
        CountdownLabel.Text = i.ToString();
        if (i != 0) await Task.Delay(1000);
      }

      ButtonPhotoClick(sender, e);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
      base.OnMouseLeftButtonDown(e);

      DragMove();
    }

    protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
    {
      base.OnMouseDoubleClick(e);
      
      if (e.Source is Button) return;
      if (e.OriginalSource is Button) return;
      WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
    {
      base.OnMouseRightButtonDown(e);
      if (_lastException != null)
      {
        MaterialMessageBox.ShowError(_lastException.ToString());
      }

      MessageBoxResult result = MaterialMessageBox.ShowWithCancel($"Party zuende ??", "Fotobox beenden");
      if (result == MessageBoxResult.OK)
      {
        Close();
        WebApiLoader.StopAsync().Wait(10000);
        Environment.Exit(0);
      }
    }

    private void ButtonPhotoNextClick(object sender, RoutedEventArgs e)
    {
      e.Handled = true;

      var directory = new DirectoryInfo($"{PhotoBoothConfigurationProvider.CurrentConfig.ServiceConfig.ImagePath}/archive");
      var currentFile = directory.GetFiles().FirstOrDefault(f => f.Name == CurrentImageCache.Instance.Name);
      if (currentFile == null) return;

      var file = directory.GetFiles().OrderBy(f => f.LastWriteTime).FirstOrDefault(f => f.LastWriteTime > currentFile.LastWriteTime);
      if (file == null) return;

      using FileStream fs = file.OpenRead();
      using BinaryReader binaryReader = new BinaryReader(fs);
      var fileContent = binaryReader.ReadBytes((int)fs.Length);
      CurrentImageCache.Instance.Set(new Microsoft.AspNetCore.Mvc.FileContentResult(fileContent, "image/png"), file.Name);
    }

    private void ButtonPhotoBackClick(object sender, RoutedEventArgs e)
    {
      e.Handled = true;

      var directory = new DirectoryInfo($"{PhotoBoothConfigurationProvider.CurrentConfig.ServiceConfig.ImagePath}/archive");
      var currentFile = directory.GetFiles().FirstOrDefault(f => f.Name == CurrentImageCache.Instance.Name);
      if (currentFile == null) return;

      var file = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).FirstOrDefault(f => f.LastWriteTime < currentFile.LastWriteTime);
      if (file == null) return;

      CountdownLabel.Visibility = Visibility.Collapsed;
      FotoboxImage.Visibility = Visibility.Visible;

      using FileStream fs = file.OpenRead();
      using BinaryReader binaryReader = new BinaryReader(fs);
      var fileContent = binaryReader.ReadBytes((int)fs.Length);
      CurrentImageCache.Instance.Set(new Microsoft.AspNetCore.Mvc.FileContentResult(fileContent, "image/png"), file.Name);
    }
  }
}
