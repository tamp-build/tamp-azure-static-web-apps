namespace Tamp.AzureStaticWebApps.V2;

/// <summary>
/// Settings for <c>swa build</c> — drives the framework-aware app
/// build via the swa-cli orchestrator (calls into your configured
/// build command from <c>swa-cli.config.json</c>).
/// </summary>
public sealed class SwaBuildSettings : SwaSettingsBase
{
    public string? AppLocation { get; set; }
    public string? ApiLocation { get; set; }
    public string? OutputLocation { get; set; }
    public string? AppBuildCommand { get; set; }
    public string? ApiBuildCommand { get; set; }
    public bool Auto { get; set; }

    public SwaBuildSettings SetAppLocation(string path) { AppLocation = path; return this; }
    public SwaBuildSettings SetApiLocation(string path) { ApiLocation = path; return this; }
    public SwaBuildSettings SetOutputLocation(string path) { OutputLocation = path; return this; }
    public SwaBuildSettings SetAppBuildCommand(string cmd) { AppBuildCommand = cmd; return this; }
    public SwaBuildSettings SetApiBuildCommand(string cmd) { ApiBuildCommand = cmd; return this; }
    public SwaBuildSettings SetAuto(bool v = true) { Auto = v; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        var args = new List<string> { "build" };
        EmitCommonArguments(args);
        if (!string.IsNullOrEmpty(AppLocation)) { args.Add("--app-location"); args.Add(AppLocation!); }
        if (!string.IsNullOrEmpty(ApiLocation)) { args.Add("--api-location"); args.Add(ApiLocation!); }
        if (!string.IsNullOrEmpty(OutputLocation)) { args.Add("--output-location"); args.Add(OutputLocation!); }
        if (!string.IsNullOrEmpty(AppBuildCommand)) { args.Add("--app-build-command"); args.Add(AppBuildCommand!); }
        if (!string.IsNullOrEmpty(ApiBuildCommand)) { args.Add("--api-build-command"); args.Add(ApiBuildCommand!); }
        if (Auto) args.Add("--auto");
        return args;
    }
}
