using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Serilog;
using Serilog.Events;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build : NukeBuild
{
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Version of Wintun")]
    readonly string WintunVersion;

    [Parameter("SHA of Wintun zip")]
    readonly string WintunHash;

    [Solution("NetWintun.sln", GenerateProjects = true)]
    readonly Solution Solution;

    [GitVersion]
    readonly GitVersion GitVersion;

    AbsolutePath WintunDirectory => RootDirectory/ "wintun";

    static AbsolutePath BuildDirectory => RootDirectory / "build";

    public static int Main () => Execute<Build>(x => x.Pack);

    Target Restore => _ => _.Executes(() => DotNetToolRestore());

    public string ZipName => $"wintun-{WintunVersion}.zip";

    public AbsolutePath ZipPath => RootDirectory / ZipName;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            DotNetClean(c => c
                .SetProject(Solution.NetWintun)
                .SetConfiguration(Configuration));
            WintunDirectory.CreateOrCleanDirectory();
            BuildDirectory.CreateOrCleanDirectory();
            ZipPath.DeleteFile();
        });

    Target FetchWintun => _ => _
        .After(Restore)
        .Executes(async () =>
        {
            if (File.Exists(ZipPath))
            {
                Log.Write(LogEventLevel.Information, $"{ZipName} already exists. Skipping");
                return;
            }

            var uriBuilder = new UriBuilder(Uri.UriSchemeHttps, "www.wintun.net")
                { Path = $"builds/{ZipName}" };
            Log.Information($"Downloading Wintun {WintunVersion} from: {uriBuilder.Uri}");

            await DownloadToFileAsync(uriBuilder.Uri, ZipPath).ConfigureAwait(false);
            await AssertShaAsync(ZipPath, WintunHash).ConfigureAwait(false);

            ZipPath.UnZipTo(RootDirectory);
        });

    Target Test => _ => _
        .DependsOn(FetchWintun)
        .Executes(() =>
            DotNetTest(c => c
                .SetProjectFile(Solution.tests.NetWintun_Tests)
                .SetConfiguration(Configuration)
                .SetLoggers("trx")));

    Target Compile => _ => _
        .DependsOn(FetchWintun)
        .Executes(() =>
            DotNetBuild(c => c
                .SetProjectFile(Solution.NetWintun)
                .SetVersion(GitVersion.FullSemVer)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetConfiguration(Configuration)
            ));

    Target Pack => _ => _
        .After(Restore)
        .DependsOn(FetchWintun)
        .Executes(() =>
            DotNetPack(c => c
                    .SetProject(Solution.NetWintun)
                    .SetOutputDirectory(BuildDirectory)
                    .SetVersion(GitVersion.FullSemVer)
                    .SetAssemblyVersion(GitVersion.AssemblySemVer)
                    .SetConfiguration(Configuration)
                ));


    /// <summary>
    /// Downloads online file <paramref name="source"/> to local file <paramref name="dest"/>.
    /// </summary>
    static async ValueTask DownloadToFileAsync(Uri source, AbsolutePath dest)
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync(source).ConfigureAwait(false);
        Assert.True(response.IsSuccessStatusCode, response.ToString());
        await using var stream = await response.Content.ReadAsStreamAsync();
        await using var fileStream = File.Create(dest);
        await stream.CopyToAsync(fileStream).ConfigureAwait(false);
    }

    async ValueTask AssertShaAsync(AbsolutePath path, string expected)
    {
        await using var fs = new FileStream(path, FileMode.Open);
        var sha = await SHA256.HashDataAsync(fs);
        var actual = Convert.ToHexString(sha);
        Assert.True(string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase), $"Sha256 values do not match. Got {actual}, expected {expected}");
    }

}
