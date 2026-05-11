namespace Tamp.AzureStaticWebApps.V2;

/// <summary>
/// Settings for <c>swa deploy</c> — the canonical CI verb for shipping
/// a built React/SPA artifact to Azure Static Web Apps.
///
/// <para>The deployment token is typed as <see cref="Secret"/> and
/// flows through the <c>SWA_CLI_DEPLOYMENT_TOKEN</c> environment
/// variable instead of <c>--deployment-token</c> on argv. Argv tokens
/// leak into process listings; env vars don't. The Secret also joins
/// the runner's redaction table.</para>
///
/// <para>CWD discipline: swa-cli refuses to run from inside the
/// artifact (output-location) directory. Use <see cref="SetWorkingDirectory"/>
/// to set the parent dir, or invoke from the project root.</para>
/// </summary>
public sealed class SwaDeploySettings : SwaSettingsBase
{
    /// <summary>Source-code directory. Maps to <c>--app-location</c> / <c>-a</c>.</summary>
    public string? AppLocation { get; set; }

    /// <summary>API source directory (Functions). Maps to <c>--api-location</c>.</summary>
    public string? ApiLocation { get; set; }

    /// <summary>Built artifact directory. Maps to <c>--output-location</c> / <c>-O</c>.</summary>
    public string? OutputLocation { get; set; }

    /// <summary>SWA environment name. Maps to <c>--env</c>. Standard values: <c>production</c>, <c>preview</c>, <c>staging</c>.</summary>
    public string? Env { get; set; }

    /// <summary>Static Web App name. Maps to <c>--app-name</c>.</summary>
    public string? AppName { get; set; }

    /// <summary>Resource group name. Maps to <c>--resource-group</c>.</summary>
    public string? ResourceGroup { get; set; }

    /// <summary>Subscription ID. Maps to <c>--subscription-id</c>.</summary>
    public string? SubscriptionId { get; set; }

    /// <summary>Tenant ID. Maps to <c>--tenant-id</c>.</summary>
    public string? TenantId { get; set; }

    /// <summary>Deployment token. Routed through <c>SWA_CLI_DEPLOYMENT_TOKEN</c> env, NOT <c>--deployment-token</c> argv.</summary>
    public Secret? DeploymentToken { get; set; }

    /// <summary>API runtime language. Maps to <c>--api-language</c>. Values: <c>dotnetisolated</c>, <c>node</c>, <c>python</c>, <c>java</c>.</summary>
    public string? ApiLanguage { get; set; }

    /// <summary>API runtime version. Maps to <c>--api-version</c>.</summary>
    public string? ApiVersion { get; set; }

    /// <summary>Don't use the OS keychain for credential caching. Maps to <c>--no-use-keychain</c>. Recommended in CI.</summary>
    public bool NoUseKeychain { get; set; }

    /// <summary>Print what would be deployed without actually deploying. Maps to <c>--dry-run</c>.</summary>
    public bool DryRun { get; set; }

    public SwaDeploySettings SetAppLocation(string path) { AppLocation = path; return this; }
    public SwaDeploySettings SetApiLocation(string path) { ApiLocation = path; return this; }
    public SwaDeploySettings SetOutputLocation(string path) { OutputLocation = path; return this; }
    public SwaDeploySettings SetEnv(string env) { Env = env; return this; }
    public SwaDeploySettings SetAppName(string name) { AppName = name; return this; }
    public SwaDeploySettings SetResourceGroup(string name) { ResourceGroup = name; return this; }
    public SwaDeploySettings SetSubscriptionId(string id) { SubscriptionId = id; return this; }
    public SwaDeploySettings SetTenantId(string id) { TenantId = id; return this; }
    public SwaDeploySettings SetDeploymentToken(Secret? token) { DeploymentToken = token; return this; }
    public SwaDeploySettings SetApiLanguage(string lang) { ApiLanguage = lang; return this; }
    public SwaDeploySettings SetApiVersion(string version) { ApiVersion = version; return this; }
    public SwaDeploySettings SetNoUseKeychain(bool v = true) { NoUseKeychain = v; return this; }
    public SwaDeploySettings SetDryRun(bool v = true) { DryRun = v; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        var args = new List<string> { "deploy" };
        EmitCommonArguments(args);
        if (!string.IsNullOrEmpty(AppLocation)) { args.Add("--app-location"); args.Add(AppLocation!); }
        if (!string.IsNullOrEmpty(ApiLocation)) { args.Add("--api-location"); args.Add(ApiLocation!); }
        if (!string.IsNullOrEmpty(OutputLocation)) { args.Add("--output-location"); args.Add(OutputLocation!); }
        if (!string.IsNullOrEmpty(Env)) { args.Add("--env"); args.Add(Env!); }
        if (!string.IsNullOrEmpty(AppName)) { args.Add("--app-name"); args.Add(AppName!); }
        if (!string.IsNullOrEmpty(ResourceGroup)) { args.Add("--resource-group"); args.Add(ResourceGroup!); }
        if (!string.IsNullOrEmpty(SubscriptionId)) { args.Add("--subscription-id"); args.Add(SubscriptionId!); }
        if (!string.IsNullOrEmpty(TenantId)) { args.Add("--tenant-id"); args.Add(TenantId!); }
        if (!string.IsNullOrEmpty(ApiLanguage)) { args.Add("--api-language"); args.Add(ApiLanguage!); }
        if (!string.IsNullOrEmpty(ApiVersion)) { args.Add("--api-version"); args.Add(ApiVersion!); }
        if (NoUseKeychain) args.Add("--no-use-keychain");
        if (DryRun) args.Add("--dry-run");
        return args;
    }

    public new CommandPlan ToCommandPlan(Tool tool)
    {
        if (tool is null) throw new ArgumentNullException(nameof(tool));
        var args = BuildVerbArguments().ToList();
        var env = new Dictionary<string, string>(EnvironmentVariables);
        if (DeploymentToken is not null)
            env["SWA_CLI_DEPLOYMENT_TOKEN"] = DeploymentToken.Reveal();
        return new CommandPlan
        {
            Executable = tool.Executable.Value,
            Arguments = args,
            Environment = env,
            WorkingDirectory = WorkingDirectory,
            Secrets = DeploymentToken is null ? Array.Empty<Secret>() : new[] { DeploymentToken },
        };
    }
}
