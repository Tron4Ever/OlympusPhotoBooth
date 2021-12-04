using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace OlympusPhotoBoothWPF.Extensions
{
  static class FileContentResultExtensions
  {
    public static void WriteToDisk(this FileContentResult fileContentResult, string fullPath)
    {
      var file = new FileInfo(fullPath);
      if (file.Exists) return;
      using var fs = file.Create();
      foreach (var b in fileContentResult.FileContents)
      {
        fs.WriteByte(b);
      }
    }
  }
}