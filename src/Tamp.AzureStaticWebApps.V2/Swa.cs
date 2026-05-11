namespace Tamp.AzureStaticWebApps.V2;

/// <summary>Facade for the @azure/static-web-apps-cli (swa) 2.x.</summary>
/// <remarks>
/// <para>Resolve via <c>[NuGetPackage(UseSystemPath = true)]</c>:</para>
/// <code>
/// [NuGetPackage("swa", UseSystemPath = true)]
/// readonly Tool SwaTool;
/// </code>
/// <para>For hardened agents without global-install permission, see the
/// npx invocation pattern in the README.</para>
/// </remarks>
public static class Swa
{
    /// <summary><c>swa deploy</c> — the canonical CI verb. Deployment token routed via env var, not argv.</summary>
    public static CommandPlan Deploy(Tool tool, Action<SwaDeploySettings> configure)
    {
        if (tool is null) throw new ArgumentNullException(nameof(tool));
        if (configure is null) throw new ArgumentNullException(nameof(configure));
        var s = new SwaDeploySettings();
        configure(s);
        return s.ToCommandPlan(tool);
    }

    /// <summary><c>swa build</c> — invokes the framework-aware build via swa-cli.</summary>
    public static CommandPlan Build(Tool tool, Action<SwaBuildSettings>? configure = null)
    {
        if (tool is null) throw new ArgumentNullException(nameof(tool));
        var s = new SwaBuildSettings();
        configure?.Invoke(s);
        return s.ToCommandPlan(tool);
    }

    /// <summary>Escape hatch for verbs we haven't typed (init, login, start, db, ...).</summary>
    public static CommandPlan Raw(Tool tool, params string[] arguments)
    {
        if (tool is null) throw new ArgumentNullException(nameof(tool));
        if (arguments is null || arguments.Length == 0)
            throw new ArgumentException("Raw requires at least one argument.", nameof(arguments));
        var s = new SwaRawSettings();
        s.AddArgs(arguments);
        return s.ToCommandPlan(tool);
    }
}
