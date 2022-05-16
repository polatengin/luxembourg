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

var outputList = new Dictionary<string, Tuple<Version, Version>>();

foreach (XmlNode reference in references)
{
  var id = reference!.Attributes!["Include"]!.Value!;
  var version = new Version(reference!.Attributes!["Version"]!.Value!);

  var latest = new Version((await FindLatestVersionAsync(id)).ToNormalizedString());

  outputList.Add(id, Tuple.Create(version, latest));
}
