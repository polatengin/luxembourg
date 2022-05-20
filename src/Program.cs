using System.Xml;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

public class Program
{
  async static Task<NuGetVersion> FindLatestVersionAsync(string id, bool noPreview)
  {
    var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
    var resource = await repository.GetResourceAsync<FindPackageByIdResource>();

    var versions = await resource.GetAllVersionsAsync(id, new SourceCacheContext(), NullLogger.Instance, CancellationToken.None);

    var filtered = versions;

    if (noPreview)
    {
      filtered = versions.Where(v => !v.IsPrerelease);
    }

    return filtered.OrderByDescending(v => v).FirstOrDefault()!;
  }

  async static Task ProcessProjectFileAsync(string file, bool noPreview, bool update)
  {
    Console.WriteLine();
    Console.WriteLine($"Checking {file}...");
    Console.WriteLine();

    var xml = File.ReadAllText(file);

    var doc = new XmlDocument();
    doc.LoadXml(xml);

    var references = doc.SelectNodes("//Project/ItemGroup/PackageReference")!;

    var outputList = new Dictionary<string, Tuple<NuGetVersion, NuGetVersion>>();

    foreach (XmlNode reference in references)
    {
      var id = reference!.Attributes?["Include"]?.Value;
      if (id == null)
      {
        id = reference!.Attributes?["Update"]?.Value;
      }
      if (id == null)
      {
        continue;
      }
      var version = new NuGetVersion(reference!.Attributes!["Version"]!.Value!);
      var latest = await FindLatestVersionAsync(id, noPreview);

      outputList.Add(id, Tuple.Create(version, latest));
    }

    var longestIdLength = outputList.Keys.Max(k => k.Length) + 2;
    var longestVersionLength = outputList.Values.Max(v => v.Item1.ToString().Length) + 2;
    var longestLatestLength = outputList.Values.Max(v => v.Item2.ToString().Length) + 2;

    foreach (var kvp in outputList)
    {
      var id = kvp.Key;
      var version = kvp.Value.Item1;
      var latest = kvp.Value.Item2;

      var firstSpacerLength = longestIdLength - id.Length;

      Console.Write($"{id} {new string(' ', firstSpacerLength)}");

      var secondSpacerLength = longestVersionLength - version.ToString().Length;
      var thirdSpacerLength = longestLatestLength - latest.ToString().Length;

      Console.Write($"{new string(' ', secondSpacerLength)}{version}   →{new string(' ', thirdSpacerLength)}");

      if (latest.Major > version.Major)
      {
        Console.ForegroundColor = ConsoleColor.Red;
      }
      Console.Write($"{latest.Major}.");
      if (latest.Major > version.Major)
      {
        Console.ForegroundColor = ConsoleColor.Red;
      }
      else if (latest.Minor > version.Minor)
      {
        Console.ForegroundColor = ConsoleColor.Blue;
      }
      Console.Write($"{latest.Minor}.");
      if (latest.Major > version.Major)
      {
        Console.ForegroundColor = ConsoleColor.Red;
      }
      else if (latest.Minor > version.Minor)
      {
        Console.ForegroundColor = ConsoleColor.Blue;
      }
      else if (latest.Patch > version.Patch)
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
      }
      Console.Write($"{latest.Patch}");
      if (latest.Release != "")
      {
        if (latest.Major > version.Major)
        {
          Console.ForegroundColor = ConsoleColor.Red;
        }
        else if (latest.Minor > version.Minor)
        {
          Console.ForegroundColor = ConsoleColor.Blue;
        }
        else if (latest.Patch > version.Patch)
        {
          Console.ForegroundColor = ConsoleColor.Yellow;
        }
        else if (latest.Release != version.Release)
        {
          Console.ForegroundColor = ConsoleColor.Green;
        }
        Console.Write($"-{latest.Release}");
      }

      Console.ResetColor();

      Console.WriteLine();
    }
  }

  static void Main(string[] args)
  {
    var files = Directory.GetFiles(".", "*.csproj").ToList();
    if (files.Count == 0)
    {
      Console.WriteLine("No csproj files found.");
      Environment.Exit(1);
    }

    var noPreview = false;
    var update = false;
    if (args.Length > 0)
    {
      if (args.Any(e => e == "-np") || args.Any(e => e == "--no-preview"))
      {
        noPreview = true;
      }
      if (args.Any(e => e == "-u") || args.Any(e => e == "--update"))
      {
        update = true;
      }
    }
    foreach (var file in files)
    {
      ProcessProjectFileAsync(file).Wait();
    }
  }
}
