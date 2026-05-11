using System.IO;
using Tamp;
using Xunit;
using Xunit.Abstractions;

namespace Tamp.AzureStaticWebApps.V2.IntegrationTests;

/// <summary>
/// Exercises the wrapper against a real <c>swa</c> binary. We avoid
/// any verb that requires a real deployment token / SWA app — those
/// are PAT-gated and live integration belongs in the consumer's
/// pipeline. v0.1.0 integration tests cover <c>--version</c> and
/// <c>--help -V</c> shape probes for the typed verbs.
/// </summary>
public sealed class SwaIntegrationTests
{
    private readonly ITestOutputHelper _output;

    public SwaIntegrationTests(ITestOutputHelper output) => _output = output;

    private static string? ResolveOnPath(string baseName)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
        var names = OperatingSystem.IsWindows()
            ? new[] { $"{baseName}.cmd", $"{baseName}.exe", $"{baseName}.bat", $"{baseName}.ps1", baseName }
            : new[] { baseName };
        foreach (var dir in pathEnv.Split(Path.PathSeparator))
        {
            if (string.IsNullOrEmpty(dir)) continue;
            foreach (var n in names)
            {
                var c = Path.Combine(dir, n);
                if (File.Exists(c)) return c;
            }
        }
        return null;
    }

    private static Tool ResolveTool() =>
        new(AbsolutePath.Create(ResolveOnPath("swa")
            ?? throw new InvalidOperationException("swa not found on PATH. Install: npm i -g @azure/static-web-apps-cli@2")));

    private CaptureResult Run(CommandPlan plan)
    {
        _output.WriteLine($"$ {plan.Executable} {string.Join(' ', plan.Arguments)}");
        var result = ProcessRunner.Capture(plan);
        foreach (var line in result.Lines)
            _output.WriteLine($"  [{line.Type}] {line.Text}");
        _output.WriteLine($"  → exit {result.ExitCode}");
        return result;
    }

    [Fact]
    public void Raw_Version_Reports_2_x()
    {
        var tool = ResolveTool();
        var plan = Swa.Raw(tool, "--version");
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        var combined = result.StdoutText + result.StderrText;
        Assert.Matches(@"\d+\.\d+\.\d+", combined);
    }

    [Fact]
    public void Raw_Help_Lists_Available_Verbs()
    {
        var tool = ResolveTool();
        var plan = Swa.Raw(tool, "--help");
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        var combined = result.StdoutText + result.StderrText;
        foreach (var verb in new[] { "deploy", "build", "init" })
        {
            Assert.Contains(verb, combined);
        }
    }

    [Fact]
    public void Raw_Deploy_Help_Surfaces_Expected_Flags()
    {
        var tool = ResolveTool();
        var plan = Swa.Raw(tool, "deploy", "--help");
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        var combined = result.StdoutText + result.StderrText;
        foreach (var flag in new[] { "--app-location", "--output-location", "--env", "--deployment-token" })
        {
            Assert.Contains(flag, combined);
        }
    }

    [Fact]
    public void Raw_Build_Help_Surfaces_Expected_Flags()
    {
        var tool = ResolveTool();
        var plan = Swa.Raw(tool, "build", "--help");
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        var combined = result.StdoutText + result.StderrText;
        Assert.Contains("--app-location", combined);
        Assert.Contains("--api-location", combined);
    }
}
