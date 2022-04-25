using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static Bullseye.Targets;
using static SimpleExec.Command;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var sdk = new DotnetSdkManager();
        var dotnet = await sdk.GetDotnetCliPath();

        Target("default", DependsOn("test"));

        Target("build", DependsOn("verify-OS-is-suppported"),
            Directory.EnumerateFiles("src", "*.sln", SearchOption.AllDirectories),
            solution => Run(dotnet, $"build \"{solution}\" --configuration Release"));

        Target("test", DependsOn("build"),
            Directory.EnumerateFiles("src", "*.Tests.csproj", SearchOption.AllDirectories),
            proj => Run(dotnet, $"test \"{proj}\" --configuration Release --no-build"));

        Target(
            "verify-OS-is-suppported",
            () => { if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) throw new InvalidOperationException("Build is supported on Windows only, at this time."); });

        await RunTargetsAndExitAsync(args);
    }
}
