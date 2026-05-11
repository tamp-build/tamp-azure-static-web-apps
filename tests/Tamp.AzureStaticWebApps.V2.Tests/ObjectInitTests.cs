using System.IO;
using Tamp;
using Xunit;

namespace Tamp.AzureStaticWebApps.V2.Tests;

/// <summary>
/// TAM-161 (satellite fanout): every wrapper verb that accepts an
/// <c>Action&lt;TSettings&gt;</c> configurer also exposes a parallel
/// object-init overload that takes a pre-populated settings instance.
/// Both authoring styles must emit byte-equal <see cref="CommandPlan"/>s.
/// </summary>
public sealed class ObjectInitTests
{
    private static Tool FakeTool(string name = "swa") =>
        new(AbsolutePath.Create(Path.Combine(Path.GetTempPath(), name)));

    [Fact]
    public void Deploy_ObjectInit_Emits_Identical_Plan_To_Fluent()
    {
        var swa = FakeTool();

        var fluent = Swa.Deploy(swa, s => s
            .SetAppLocation("./frontend")
            .SetOutputLocation("./dist")
            .SetEnv("production")
            .SetAppName("swa-strata")
            .SetResourceGroup("rg-strata-prod")
            .SetNoUseKeychain());

        var objectInit = Swa.Deploy(swa, new SwaDeploySettings
        {
            AppLocation = "./frontend",
            OutputLocation = "./dist",
            Env = "production",
            AppName = "swa-strata",
            ResourceGroup = "rg-strata-prod",
            NoUseKeychain = true,
        });

        Assert.Equal(fluent.Executable, objectInit.Executable);
        Assert.Equal(fluent.Arguments, objectInit.Arguments);
    }

    [Fact]
    public void All_ObjectInit_Overloads_Surface_Compiles_And_Returns_CommandPlan()
    {
        // Smoke test: each wrapper accepts an object-init settings argument and
        // returns a non-null CommandPlan. One assertion per added overload.
        var swa = FakeTool();

        Assert.NotNull(Swa.Deploy(swa, new SwaDeploySettings { AppName = "swa-strata" }));
        Assert.NotNull(Swa.Build(swa, new SwaBuildSettings { AppLocation = "./frontend" }));
    }
}
