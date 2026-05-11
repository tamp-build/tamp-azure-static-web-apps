namespace Tamp.AzureStaticWebApps.V2;

/// <summary>Escape hatch for verbs we haven't typed (init, login, start, db).</summary>
public sealed class SwaRawSettings : SwaSettingsBase
{
    public List<string> RawArguments { get; } = [];

    public SwaRawSettings AddArgs(params string[] args) { RawArguments.AddRange(args); return this; }

    protected override IEnumerable<string> BuildVerbArguments() => RawArguments;
}
