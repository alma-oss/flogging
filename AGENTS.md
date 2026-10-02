# AGENTS.md — Alma.Logging

## Project Purpose

F# library for structured logging to the terminal with colorful output, supporting stdout/stderr routing. Integrates with `Microsoft.Extensions.Logging` and `Serilog`, with environment-variable-based configuration for log level, output format, and metadata. Published as NuGet package `Alma.Logging`.

## Tech Stack

- **Language:** F# (.NET 10)
- **Framework:** .NET SDK library
- **Package management:** Paket
- **Build system:** FAKE (F# Make) via `build.sh`
- **Linting:** fsharplint
- **CI/CD:** GitHub Actions
- **Key dependencies:**
  - `FSharp.Core ~> 10.0`
  - `Microsoft.Extensions.Logging.Console ~> 10.0`
  - `Serilog ~> 4.2`, `Serilog.Sinks.Console ~> 6.0`, `Serilog.Extensions.Logging ~> 10.0`
  - `Feather.ErrorHandling ~> 2.0`
  - `Alma.ServiceIdentification ~> 11.0`

## Commands

```bash
# Install dependencies
dotnet tool restore && dotnet paket install

# Build
./build.sh build

# Run tests
./build.sh -t tests

# Lint
dotnet fsharplint lint Logging.fsproj
```

## Project Structure

```
flogging/
├── Logging.fsproj              # Main project (PackageId: Alma.Logging, v12.0.0)
├── src/
│   ├── Utils.fs                # Internal utilities
│   └── Logging.fs              # Core logging: LoggerFactory, Serilog integration, env var config
├── example/
│   └── example.fsproj          # Example usage project
├── build/
│   └── ...
├── build.sh
├── paket.dependencies
├── paket.references            # Full list: FSharp.Core, M.E.Logging, M.E.Logging.Console, Serilog, etc.
├── global.json                 # .NET SDK 10.0.0
├── fsharplint.json
├── CHANGELOG.md
└── .github/workflows/
    ├── tests.yaml
    ├── pr-check.yaml
    └── publish.yaml
```

## Architecture

Factory-based logging with environment variable configuration:

### LoggerFactory.create options:
- `UseLevel LogLevel.X` — set log level directly
- `UseLevelFromEnvironment "LOG_LEVEL"` — read level from env var
- `LogToConsole` — plain console output
- `LogToFromEnvironment "LOG_TO"` — configure output from env var (`console`, `json`, `console-json`)
- `LogToSerilog [...]` — Serilog sink configuration
- `SerilogOption.LogToConsole` / `SerilogOption.LogToConsoleAsJson`
- `AddMeta ("key", "value")` — add structured metadata
- `AddMetaFromEnvironment "LOG_META"` — metadata from env var (format: `"key:value; key2:value2"`)

### Log Level Mapping (from env var):
| Level | Env values |
|-------|-----------|
| Trace | `"trace"`, `"vvv"` |
| Debug | `"debug"`, `"vv"` |
| Information | `"information"`, `"v"`, `"normal"` |
| Warning | `"warning"` |
| Error | `"error"` |
| Critical | `"critical"` |
| None | `"quiet"`, `"q"`, anything else |

## Build System (FAKE)

Standard library target chain: `Clean → AssemblyInfo → Build → Lint → Tests → Release → Publish`

## CI/CD

- **tests.yaml** — runs on PRs and nightly
- **pr-check.yaml** — blocks fixup commits, runs ShellCheck
- **publish.yaml** — publishes to NuGet on semver tags

## Release Process

1. Increment `<Version>` in `Logging.fsproj`
2. Update `CHANGELOG.md`
3. Commit, tag with version, push

## Conventions

- `LoggerFactory.create` is the primary API — always use DSL-style option list
- Environment variable configuration is preferred for runtime flexibility
- `Alma.ServiceIdentification` for service identity in metadata
- `example/` folder contains usage examples — keep up to date

## Specs and plans

SDD artifacts live under `docs/`: durable specs in `docs/specs/<capability>/spec.md`,
transient plans in `docs/tasks/<work-slug>/plan.md` + `todo.md` (deleted once the work ships).

## Pitfalls

- **No tests** — no test project exists currently
- **No Docker** — pure library
- **No AssemblyInfo.fs in root** — build generates it; source files only under `src/`
- **Serilog + M.E.Logging** — both logging frameworks coexist; Serilog is bridged via `Serilog.Extensions.Logging`
- **Paket, not NuGet CLI** — use `dotnet paket install`
