using System.IO;
using Tamp;
using Xunit;

namespace Tamp.AzureStaticWebApps.V2.Tests;

public sealed class SwaTests
{
    private static Tool FakeTool(string name = "swa") =>
        new(AbsolutePath.Create(Path.Combine(Path.GetTempPath(), name)));

    private static int IndexOf(IReadOnlyList<string> args, string value, int start = 0)
    {
        for (var i = start; i < args.Count; i++)
            if (args[i] == value) return i;
        return -1;
    }

    // ---- shape ----

    [Fact]
    public void Every_Verb_Uses_Tool_Path()
    {
        var t = FakeTool();
        Assert.Equal(t.Executable.Value, Swa.Deploy(t, _ => { }).Executable);
        Assert.Equal(t.Executable.Value, Swa.Build(t).Executable);
        Assert.Equal(t.Executable.Value, Swa.Raw(t, "--version").Executable);
    }

    [Theory]
    [InlineData("deploy")]
    [InlineData("build")]
    public void Verbs_Begin_With_Their_Verb_Token(string verb)
    {
        var plan = verb switch
        {
            "deploy" => Swa.Deploy(FakeTool(), _ => { }),
            "build" => Swa.Build(FakeTool()),
            _ => throw new InvalidOperationException(),
        };
        Assert.Equal(verb, plan.Arguments[0]);
    }

    // ---- common args ----

    [Fact]
    public void Common_Args_Round_Trip_On_Deploy()
    {
        var plan = Swa.Deploy(FakeTool(), s => s
            .SetConfig("swa-cli.config.json")
            .SetConfigName("production")
            .SetVerbose("silly")
            .SetSwaConfigLocation("./swa.config.json"));
        var args = plan.Arguments;
        Assert.Equal("deploy", args[0]);
        Assert.Contains("--config", args); Assert.Contains("swa-cli.config.json", args);
        Assert.Contains("--config-name", args); Assert.Contains("production", args);
        Assert.Contains("--verbose", args); Assert.Contains("silly", args);
        Assert.Contains("--swa-config-location", args);
    }

    // ---- deploy ----

    [Fact]
    public void Deploy_Default_Is_Just_The_Verb()
    {
        var plan = Swa.Deploy(FakeTool(), _ => { });
        Assert.Equal(["deploy"], plan.Arguments);
        Assert.Empty(plan.Secrets);
    }

    [Fact]
    public void Deploy_All_Optional_Flags_Round_Trip()
    {
        var plan = Swa.Deploy(FakeTool(), s => s
            .SetAppLocation("./frontend")
            .SetApiLocation("./api")
            .SetOutputLocation("./dist")
            .SetEnv("production")
            .SetAppName("swa-strata")
            .SetResourceGroup("rg-strata-prod")
            .SetSubscriptionId("sub-id")
            .SetTenantId("tenant-id")
            .SetApiLanguage("dotnetisolated")
            .SetApiVersion("8.0")
            .SetNoUseKeychain()
            .SetDryRun());
        var args = plan.Arguments;
        Assert.Contains("--app-location", args); Assert.Contains("./frontend", args);
        Assert.Contains("--api-location", args); Assert.Contains("./api", args);
        Assert.Contains("--output-location", args); Assert.Contains("./dist", args);
        Assert.Contains("--env", args); Assert.Contains("production", args);
        Assert.Contains("--app-name", args); Assert.Contains("swa-strata", args);
        Assert.Contains("--resource-group", args); Assert.Contains("rg-strata-prod", args);
        Assert.Contains("--subscription-id", args); Assert.Contains("sub-id", args);
        Assert.Contains("--tenant-id", args); Assert.Contains("tenant-id", args);
        Assert.Contains("--api-language", args); Assert.Contains("dotnetisolated", args);
        Assert.Contains("--api-version", args); Assert.Contains("8.0", args);
        Assert.Contains("--no-use-keychain", args);
        Assert.Contains("--dry-run", args);
    }

    // ---- deployment token routing ----

    [Fact]
    public void Deployment_Token_Flows_Through_Env_Var_Not_Argv()
    {
        var token = new Secret("SWA deployment token", "swa-token-1234567890");
        var plan = Swa.Deploy(FakeTool(), s => s
            .SetAppName("swa-strata")
            .SetDeploymentToken(token));

        // SWA_CLI_DEPLOYMENT_TOKEN is set in env...
        Assert.Equal("swa-token-1234567890", plan.Environment["SWA_CLI_DEPLOYMENT_TOKEN"]);
        // ...the token value MUST NOT appear in argv...
        Assert.DoesNotContain("swa-token-1234567890", plan.Arguments);
        Assert.DoesNotContain("--deployment-token", plan.Arguments);
        // ...and the Secret joins the redaction table.
        Assert.Single(plan.Secrets);
        Assert.Same(token, plan.Secrets[0]);
    }

    [Fact]
    public void Deploy_Without_Token_Has_Empty_Secrets_And_No_Env()
    {
        var plan = Swa.Deploy(FakeTool(), s => s.SetAppName("swa-strata"));
        Assert.Empty(plan.Secrets);
        Assert.False(plan.Environment.ContainsKey("SWA_CLI_DEPLOYMENT_TOKEN"));
    }

    [Fact]
    public void Deploy_Custom_Env_Vars_Survive_Alongside_Deployment_Token()
    {
        var token = new Secret("t", "v");
        var plan = Swa.Deploy(FakeTool(), s => s
            .SetEnv("CUSTOM_VAR", "custom-value")
            .SetDeploymentToken(token));
        Assert.Equal("custom-value", plan.Environment["CUSTOM_VAR"]);
        Assert.Equal("v", plan.Environment["SWA_CLI_DEPLOYMENT_TOKEN"]);
    }

    // ---- build ----

    [Fact]
    public void Build_Default_Is_Just_The_Verb()
    {
        Assert.Equal(["build"], Swa.Build(FakeTool()).Arguments);
    }

    [Fact]
    public void Build_With_Custom_Commands()
    {
        var plan = Swa.Build(FakeTool(), s => s
            .SetAppLocation("./frontend")
            .SetOutputLocation("./dist")
            .SetAppBuildCommand("npm run build")
            .SetApiBuildCommand("dotnet publish")
            .SetAuto());
        var args = plan.Arguments;
        Assert.Equal("build", args[0]);
        Assert.Contains("--app-build-command", args);
        Assert.Contains("npm run build", args);
        Assert.Contains("--api-build-command", args);
        Assert.Contains("dotnet publish", args);
        Assert.Contains("--auto", args);
    }

    // ---- raw ----

    [Fact]
    public void Raw_Requires_Args()
    {
        Assert.Throws<ArgumentException>(() => Swa.Raw(FakeTool()));
    }

    [Fact]
    public void Raw_Forwards_Verbatim()
    {
        var plan = Swa.Raw(FakeTool(), "init", "my-app", "--yes");
        Assert.Equal(["init", "my-app", "--yes"], plan.Arguments);
    }

    // ---- nulls ----

    [Fact]
    public void Null_Tool_Throws_For_Every_Verb()
    {
        Assert.Throws<ArgumentNullException>(() => Swa.Deploy(null!, _ => { }));
        Assert.Throws<ArgumentNullException>(() => Swa.Build(null!));
        Assert.Throws<ArgumentNullException>(() => Swa.Raw(null!, "--help"));
    }

    [Fact]
    public void Null_Configurer_Throws_For_Deploy()
    {
        Assert.Throws<ArgumentNullException>(() => Swa.Deploy(FakeTool(), null!));
    }

    [Fact]
    public void Working_Directory_And_Env_Vars_Flow_To_Plan()
    {
        var cwd = Path.GetTempPath();
        var plan = Swa.Deploy(FakeTool(), s => s
            .SetAppName("x")
            .SetWorkingDirectory(cwd)
            .SetEnv("NODE_OPTIONS", "--max-old-space-size=4096"));
        Assert.Equal(cwd, plan.WorkingDirectory);
        Assert.Equal("--max-old-space-size=4096", plan.Environment["NODE_OPTIONS"]);
    }
}
