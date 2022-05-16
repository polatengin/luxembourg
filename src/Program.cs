using System.Xml;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

async Task<NuGetVersion> FindLatestVersionAsync(string id)
{
  var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
  var resource = await repository.GetResourceAsync<FindPackageByIdResource>();

  var versions = await resource.GetAllVersionsAsync(id, new SourceCacheContext(), NullLogger.Instance, CancellationToken.None);

  return versions.OrderByDescending(v => v).FirstOrDefault()!;
}

var files = Directory.GetFiles(".", "*.csproj");
if (files.Length == 0)
{
  Console.WriteLine("No csproj files found.");
  Environment.Exit(1);
}

var csprojFile = files.First();

Console.WriteLine();
Console.WriteLine($"Checking {csprojFile}");
Console.WriteLine();

var xml = File.ReadAllText(csprojFile);

var doc = new XmlDocument();
doc.LoadXml(xml);

var references = doc.SelectNodes("//Project/ItemGroup/PackageReference")!;

var outputList = new Dictionary<string, Tuple<NuGetVersion, NuGetVersion>>();

foreach (XmlNode reference in references)
{
  var id = reference!.Attributes!["Include"]!.Value!;
  var version = new Version(reference!.Attributes!["Version"]!.Value!);

  var latest = new Version((await FindLatestVersionAsync(id)).ToNormalizedString());

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
  else if (latest.Build > version.Build)
  {
    Console.ForegroundColor = ConsoleColor.Yellow;
  }
  Console.Write($"{latest.Build}");

  Console.WriteLine();
}