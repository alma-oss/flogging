# Preferred Patterns

## Core Principles

- Configure logging declaratively: pass a list of options to a `LoggerFactory`
  entry point rather than mutating builders by hand.
- Options are order-insensitive for most purposes and de-duplicated internally,
  so repeating an equivalent option is harmless but unnecessary.
- Output routing is automatic by severity: events at `Error` level and above go
  to **stderr**, everything else goes to **stdout**. This holds for both the
  Microsoft.Extensions.Logging console sink and the Serilog console sink.
- Nothing is logged unless at least one output option is present; a level alone
  produces no sink.
- The factory is `IDisposable`; bind it with `use` so sinks flush and dispose.

## Recommended API Usage

Pick the entry point by what you need back and how much control you want:

- `LoggerFactory.create options` — returns an
  `Microsoft.Extensions.Logging.ILoggerFactory`. The default choice for
  application and library code. See `examples.md` → Basic Example and Realistic
  Example.
- `LoggerFactory.createSerilog serilogOptions` — returns a raw Serilog `ILogger`
  configured from a `SerilogOption list`. Use when you specifically need a
  Serilog logger rather than the MEL abstraction.
- `LoggerFactory.createCustom customize customizeSerilog options` — same as
  `create` but exposes hooks to mutate the MEL builder and the Serilog
  `LoggerConfiguration` directly, for filters or sinks the DSL does not cover.
  See `examples.md` → Custom Filter Example.
- `LoggerFactory.createCustomSerilog customize serilogOptions` — the Serilog-only
  counterpart with a single customization hook.

Output selection options:

- `LogToConsole` (top-level) and `SerilogOption.LogToConsole` route through the
  Serilog console sink with a colorized, templated layout.
- `LogToConsoleAsJson` / `SerilogOption.LogToConsoleAsJson` emit one JSON object
  per event — prefer this in production where logs are scraped by machines.
- `LogToSimpleConsole` uses the plain Microsoft.Extensions.Logging console
  provider instead of Serilog.

Level selection: set a level explicitly with `UseLevel` or resolve it at runtime
with `UseLevelFromEnvironment`. Within a `LogToSerilog` block the Serilog level
is set independently via `SerilogOption.UseLevel` / `UseLogEventLevel`.

## Error Handling

- Missing or empty environment variables are treated as absent: env-var options
  contribute nothing rather than throwing, so a misconfigured deployment
  degrades to "no extra config" instead of crashing.
- An unrecognized `LOG_LEVEL` value resolves to `None` (logging effectively
  off); treat silence as a possible sign of a typo'd level value.
- Construct the factory once near application start and reuse the loggers it
  creates; do not rebuild per call site.

## Composition

- Build metadata from a service identity and append extra options with the list
  append operator, e.g. `SerilogOptions.ofInstance instance @ [ ... ]`. See
  `examples.md` → Integration Example.
- `IgnorePaths` accumulates: multiple ignore options (including the
  `IgnorePathHealthCheck` / `IgnorePathMetrics` / `IgnorePathReady` shortcuts)
  merge into one de-duplicated filter set.
- Equivalent options collapse via internal de-duplication, so composing partial
  option lists from helpers and call sites is safe.

## Integration with Other Libraries

- `SerilogOptions.ofService`, `ofInstance`, and `ofBox` convert
  Alma.ServiceIdentification values into `AddMeta` options carrying domain /
  context / purpose / version (and zone / bucket for a `Box`). Use these to
  stamp every event with consistent identity metadata.
- `UseProvider` plugs an external `ILoggerProvider` (for example a tracing
  provider) into the same factory alongside the console/Serilog sinks.
- The library bridges Serilog into Microsoft.Extensions.Logging, so consumers
  depending only on `ILogger` get Serilog output transparently.

## Naming Conventions

- Create one logger per component using a stable category name via
  `factory.CreateLogger("CategoryName")`; the category appears as
  `SourceContext` in the console template.
- Use structured message templates with named placeholders (`{Placeholder}`)
  and pass the values as arguments, so properties stay queryable in JSON output.
- Use neutral, descriptive category and metadata names; avoid embedding
  environment-specific or sensitive values in templates.

## Testing Recommendations

- Inject the produced `ILogger` / `ILoggerFactory` into code under test rather
  than constructing it internally, so tests can substitute a no-op or capturing
  logger.
- To assert on output, use `createCustom` to attach a capturing filter/sink, or
  configure `LogToConsoleAsJson` and parse the emitted JSON.
- Exercise environment-variable parsing by setting `LOG_TO`, `LOG_LEVEL`, and
  `LOG_META` in the test process and asserting the resulting behavior.
