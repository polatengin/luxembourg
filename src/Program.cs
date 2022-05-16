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
