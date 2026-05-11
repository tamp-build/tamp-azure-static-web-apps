namespace Tamp.AzureStaticWebApps.V2;

/// <summary>
/// Common base for <c>swa &lt;verb&gt;</c> settings. The swa CLI's
/// global args are sparse — verbosity, config file path, config name.
/// Most flags are per-verb.
/// </summary>
public abstract class SwaSettingsBase
{
    public string? WorkingDirectory { get; set; }
    public Dictionary<string, string> EnvironmentVariables { get; } = new();

    /// <summary>swa-cli config file path. Maps to <c>--config</c>.</summary>
    public string? Config { get; set; }

    /// <summary>Named configuration block inside the config file. Maps to <c>--config-name</c>.</summary>
    public string? ConfigName { get; set; }

    /// <summary>Increase logging verbosity. Maps to <c>--verbose</c>. Accepted values: <c>silly</c>, <c>info</c> (default), <c>log</c>, <c>silent</c>.</summary>
    public string? Verbose { get; set; }

    /// <summary>swa.config.json path. Maps to <c>--swa-config-location</c>.</summary>
    public string? SwaConfigLocation { get; set; }

    protected abstract IEnumerable<string> BuildVerbArguments();

    protected virtual IReadOnlyList<Secret> CollectSecrets() => Array.Empty<Secret>();

    protected void EmitCommonArguments(List<string> args)
    {
        if (!string.IsNullOrEmpty(Config)) { args.Add("--config"); args.Add(Config!); }
        if (!string.IsNullOrEmpty(ConfigName)) { args.Add("--config-name"); args.Add(ConfigName!); }
        if (!string.IsNullOrEmpty(Verbose)) { args.Add("--verbose"); args.Add(Verbose!); }
        if (!string.IsNullOrEmpty(SwaConfigLocation)) { args.Add("--swa-config-location"); args.Add(SwaConfigLocation!); }
    }

    public CommandPlan ToCommandPlan(Tool tool)
    {
        if (tool is null) throw new ArgumentNullException(nameof(tool));
        var args = BuildVerbArguments().ToList();
        return new CommandPlan
        {
            Executable = tool.Executable.Value,
            Arguments = args,
            Environment = new Dictionary<string, string>(EnvironmentVariables),
            WorkingDirectory = WorkingDirectory,
            Secrets = CollectSecrets(),
        };
    }
}

/// <summary>Generic fluent setters for the shared base.</summary>
public static class SwaSettingsBaseExtensions
{
    public static T SetWorkingDirectory<T>(this T s, string? cwd) where T : SwaSettingsBase { s.WorkingDirectory = cwd; return s; }
    public static T SetEnv<T>(this T s, string key, string value) where T : SwaSettingsBase { s.EnvironmentVariables[key] = value; return s; }
    public static T SetConfig<T>(this T s, string path) where T : SwaSettingsBase { s.Config = path; return s; }
    public static T SetConfigName<T>(this T s, string name) where T : SwaSettingsBase { s.ConfigName = name; return s; }
    public static T SetVerbose<T>(this T s, string level = "log") where T : SwaSettingsBase { s.Verbose = level; return s; }
    public static T SetSwaConfigLocation<T>(this T s, string path) where T : SwaSettingsBase { s.SwaConfigLocation = path; return s; }
}
