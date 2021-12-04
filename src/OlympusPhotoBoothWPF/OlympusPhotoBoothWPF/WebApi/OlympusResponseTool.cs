using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OlympusPhotoBoothWPF.WebApi
{
  public class OlympusResponseTool
  {
    public static string GetLastFileName(string source)
    {
      var lines = new List<string>();
      using (var reader = new StringReader(source))
      {
        for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
        {
          lines.Add(line);
        }
      }

      var data = lines.Skip(1);
      var sorted = data.Select(line => new
      {
        SortKey = int.Parse(line.Split(',')[5]),
        Line = line
      })
        .OrderBy(x => x.SortKey)
        .Select(x => x.Line);

      sorted = sorted.Select(line => new
      {
        SortKey = int.Parse(line.Split(',')[4]),
        Line = line
      })
        .OrderBy(x => x.SortKey)
        .Select(x => x.Line);

      return sorted.LastOrDefault()?.Split(",")[1];
    }
  }
}