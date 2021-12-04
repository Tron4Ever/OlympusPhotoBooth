using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OlympusPhotoBoothWPF.Configuration;

namespace OlympusPhotoBoothWPF.WebApi.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class OlympusCameraController : ControllerBase
  {
    private readonly ILogger<OlympusCameraController> _logger;
    private readonly HttpClient _httpClient;
    private readonly ServiceConfiguration _serviceConfiguration;

    /// <summary>
    /// I know... It`s bad... But hey. This is a photo booth project... :-) 
    /// The Olympus Camera cannot handle concurrent calls. So we have to make sure here in the API that no concurrency will happen!
    /// </summary>
    public static bool _activeRequest = false;

    public OlympusCameraController(ILogger<OlympusCameraController> logger, IHttpClientFactory httpClientFactory, ServiceConfiguration serviceConfiguration)
    {
      _logger = logger;
      _httpClient = httpClientFactory.CreateClient("OlympusHttpClient");
      _serviceConfiguration = serviceConfiguration;
    }

    [HttpPost("TakePicture")]
    public async Task<IActionResult> TakePicture()
    {
      if (_activeRequest)
      {
        return NoContent();
      }
      _activeRequest = true;

      try
      {
        var responseMessage = await _httpClient.GetAsync("switch_cammode.cgi?mode=shutter"!);
        if (!responseMessage.IsSuccessStatusCode) return BadRequest();

        if (_serviceConfiguration.DelayInMillisecondsBetween1st2ndpush > 0)
        {
          responseMessage = await _httpClient.GetAsync("exec_shutter.cgi?com=1stpush");
          if (!responseMessage.IsSuccessStatusCode) return BadRequest();

          await Task.Delay(_serviceConfiguration.DelayInMillisecondsBetween1st2ndpush);

          responseMessage = await _httpClient.GetAsync("exec_shutter.cgi?com=2ndpush");
          if (!responseMessage.IsSuccessStatusCode) return BadRequest();
        }
        else
        {
          responseMessage = await _httpClient.GetAsync("exec_shutter.cgi?com=1st2ndpush");
          if (!responseMessage.IsSuccessStatusCode) return BadRequest();
        }

        await Task.Delay(250);
        responseMessage = await _httpClient.GetAsync("exec_shutter.cgi?com=2nd1strelease");
        if (!responseMessage.IsSuccessStatusCode) return BadRequest();

        return Ok();
      }
      catch
      {
        return NotFound();
      }
      finally
      {
        _activeRequest = false;
      }
    }

    [HttpPost("TakePictureRecView")]
    public async Task<IActionResult> TakePictureRecView()
    {
      if (_activeRequest)
      {
        return NoContent();
      }
      _activeRequest = true;

      try
      {
        var responseMessage = await _httpClient.GetAsync("switch_cammode.cgi?mode=rec&lvqty=0640x0480");
        if (!responseMessage.IsSuccessStatusCode) return BadRequest();
        responseMessage = await _httpClient.GetAsync("exec_takemisc.cgi?com=startliveview&port=28488");
        if (responseMessage.IsSuccessStatusCode)
          responseMessage = await _httpClient.GetAsync("exec_takemotion.cgi?com=starttake&point=0320x0240");
        if (responseMessage.IsSuccessStatusCode)
          responseMessage = await _httpClient.GetAsync("exec_takemisc.cgi?com=stopliveview");
        responseMessage = await _httpClient.GetAsync("exec_takemisc.cgi?com=getrecview");
        if (responseMessage.IsSuccessStatusCode)
        {
          if (responseMessage.Content.Headers.ContentType.MediaType == "image/jpeg")
          {
            var currentFileName = Path.Combine($"{PhotoBoothConfigurationProvider.CurrentConfig.ServiceConfig.ImagePath}/current", $"{DateTime.Now:yyyyMMddHHmmss}.jpg");
            var bytes = await responseMessage.Content.ReadAsByteArrayAsync();
            System.IO.File.WriteAllBytes(currentFileName, bytes);
          }
        }
        return Ok();
      }
      catch
      {
        return NotFound();
      }
      finally
      {
        _activeRequest = false;
      }
    }

    [HttpGet("GetLastPicture")]
    public async Task<ActionResult<byte[]>> GetLastPicture()
    {
      if (_activeRequest)
      {
        return NoContent();
      }
      _activeRequest = true;

      try
      {
        var responseMessage = await _httpClient.GetAsync("switch_cammode.cgi?mode=play"!);
        if (!responseMessage.IsSuccessStatusCode) return BadRequest();

        responseMessage = await _httpClient.GetAsync("get_imglist.cgi?DIR=/DCIM/100OLYMP");
        if (!responseMessage.IsSuccessStatusCode) return BadRequest();

        var content = await responseMessage.Content.ReadAsStringAsync();
        var lastFile = OlympusResponseTool.GetLastFileName(content);

        var currentFileName = Path.Combine($"{PhotoBoothConfigurationProvider.CurrentConfig.ServiceConfig.ImagePath}/current", lastFile);

        if (System.IO.File.Exists(lastFile) == false)
        {
          responseMessage = await _httpClient.GetAsync($"get_resizeimg.cgi?DIR=/DCIM/100OLYMP/{lastFile}&size=1024");
          if (!responseMessage.IsSuccessStatusCode) return BadRequest();

          var bytes = await responseMessage.Content.ReadAsByteArrayAsync();
          System.IO.File.WriteAllBytes(currentFileName, bytes);
        }

        return System.IO.File.ReadAllBytes(currentFileName);
      }
      finally
      {
        _activeRequest = false;
      }
    }

    [HttpPost("RefreshImageCache")]
    public async Task<IActionResult> RefreshImageCache()
    {
      _ = await GetLastPicture();
      return Ok();
    }

    [HttpPost("SetTemporaryPicture")]
    public IActionResult SetTemporaryPicture()
    {
      DeleteCurrentImage();
      return Ok();
    }

    [HttpPost("TakePictureAllInOne")]
    public async Task<IActionResult> TakePictureAllInOne()
    {
      SetTemporaryPicture();

      if (ServiceConfiguration.UseRecMode)
      {
        await TakePictureRecView();
      }
      else
      {
        await TakePicture();
        await RefreshImageCache();
      }
      
      return Ok();
    }

    private static void DeleteCurrentImage()
    {
      foreach (string file in Directory.GetFiles($"{PhotoBoothConfigurationProvider.CurrentConfig.ServiceConfig.ImagePath}/current"))
      {
        System.IO.File.Delete(file);
      }
    }
  }
}