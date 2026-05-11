# Tamp.AzureStaticWebApps

Wrapper for **`@azure/static-web-apps-cli`** (the `swa` command).
Deployment token typed as `Secret` and routed through the
`SWA_CLI_DEPLOYMENT_TOKEN` environment variable — never argv.

```csharp
using Tamp.AzureStaticWebApps.V2;
```

| Package | swa-cli | Status |
|---|---|---|
| `Tamp.AzureStaticWebApps.V2` | 2.x | preview |

Requires `Tamp.Core ≥ 1.0.5`.

## Verbs (v0.1.0)

| Verb | Notes |
|---|---|
| `Deploy` | The canonical CI verb. Token routed via env var. `--app-location`, `--api-location`, `--output-location`, `--env`, `--app-name`, `--resource-group`, `--subscription-id`, `--tenant-id`, `--api-language` / `--api-version`, `--no-use-keychain`, `--dry-run`. |
| `Build` | Framework-aware build via swa-cli orchestrator. `--app-build-command`, `--api-build-command`, `--auto`. |
| `Raw` | Escape hatch for `init`, `login`, `start`, `db`, ... |

**Common flags (all verbs)**: `--config`, `--config-name`,
`--verbose [level]`, `--swa-config-location`.

## Quick example — production deploy

```csharp
using Tamp;
using Tamp.AzureStaticWebApps.V2;

[NuGetPackage("swa", UseSystemPath = true)]
readonly Tool SwaTool = null!;

[Secret("SWA deployment token", EnvironmentVariable = "AZURE_STATIC_WEB_APPS_API_TOKEN")]
readonly Secret SwaToken = null!;

Target DeployFrontend => _ => _
    .Requires(() => SwaToken != null)
    .Executes(() =>
        Swa.Deploy(SwaTool, s => s
            .SetAppLocation("./frontend")
            .SetOutputLocation("./frontend/dist")
            .SetEnv("production")
            .SetAppName("swa-strata-prod")
            .SetResourceGroup("rg-strata-prod")
            .SetDeploymentToken(SwaToken)
            .SetNoUseKeychain()
            .SetWorkingDirectory(RootDirectory)));
```

## CI behaviour to know about

### `npm install -g` fails on hardened agents

If your CI agent rejects `npm install -g` with EACCES (the
container doesn't run as root), use one of these patterns:

**Pattern A — local install + node_modules/.bin:**
```yaml
- run: npm install --save-dev @azure/static-web-apps-cli@2
- run: dotnet tamp DeployFrontend
  env:
    PATH: ./node_modules/.bin:$PATH
```

**Pattern B — npx wrapper:**

The wrapper's `Tool` points at `swa`, but you can construct a
plan that uses `npx` instead:

```csharp
[NuGetPackage("npx", UseSystemPath = true)]
readonly Tool NpxTool = null!;

Target DeployFrontendViaNpx => _ => _.Executes(() =>
{
    var plan = Swa.Deploy(NpxTool, s => s.SetEnv("production").SetDeploymentToken(SwaToken));
    return plan with
    {
        Arguments = new[] { "--yes", "@azure/static-web-apps-cli@2", "deploy" }
            .Concat(plan.Arguments.Skip(1))  // skip the original "deploy" token
            .ToList()
    };
});
```

### CWD discipline

swa-cli refuses to run from inside the artifact (output-location)
directory. Set `SetWorkingDirectory` to the parent dir (typically
the repo root or the frontend source dir).

### Deployment token via env, not argv

The wrapper routes `SetDeploymentToken(Secret)` through the
`SWA_CLI_DEPLOYMENT_TOKEN` environment variable — argv tokens leak
to process listings; env vars don't. The Secret also joins the
runner's redaction table, so any log line that echoes the value is
scrubbed.

## What's NOT in v0.1.0

`init`, `login`, `start`, `db init` — available via `Raw` for now.
Typed surface ships in v0.2.0+ if there's demand.

## Releasing

See [MAINTAINERS.md](MAINTAINERS.md).
