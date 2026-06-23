# Anti-Patterns

Each entry is **mistake → why → fix**.

## Forgetting to dispose the factory

- **Mistake:** binding the factory with `let` and never disposing it.
- **Why:** the factory owns the underlying sinks; without disposal buffered
  events may not flush and resources leak.
- **Fix:** bind with `use factory = LoggerFactory.create [ ... ]` so it disposes
  at scope end.

## Setting a level but no output

- **Mistake:** passing only `UseLevel` / `UseLevelFromEnvironment` and expecting
  log lines to appear.
- **Why:** a level filters events but defines no sink, so there is nothing to
  write to.
- **Fix:** include at least one output option (`LogToConsole`,
  `LogToConsoleAsJson`, `LogToSimpleConsole`, or a `LogToSerilog [...]` block).

## String-concatenating log messages

- **Mistake:** building the message with interpolation/concatenation, e.g. one
  pre-formatted string with no named placeholders.
- **Why:** the values are lost as structured properties and cannot be queried in
  JSON output; it also defeats Serilog's template caching.
- **Fix:** use named placeholders in the template and pass values as arguments
  (see `examples.md` → Basic Example).

## Confusing the two console options

- **Mistake:** assuming `LogToConsole` and `LogToSimpleConsole` are the same.
- **Why:** `LogToConsole` routes through the Serilog console sink (templated,
  colorized); `LogToSimpleConsole` uses the plain
  Microsoft.Extensions.Logging console provider. They format differently and are
  configured in different option scopes.
- **Fix:** choose deliberately — Serilog console for rich formatting/JSON, simple
  console for the bare MEL provider.

## Malformed environment-variable values

- **Mistake:** writing `LOG_META` or `LOG_TO` with the wrong separators, e.g.
  comma-separated metadata or a single string of joined targets.
- **Why:** `LOG_META` expects `key:value` pairs separated by `;`, and `LOG_TO`
  expects target tokens separated by `;`; other shapes are silently ignored.
- **Fix:** use the documented formats (see `examples.md` → Environment-Driven
  Example) — `LOG_META="key:value; key2:value2"`, `LOG_TO="console; json"`.

## Expecting output from an unknown level string

- **Mistake:** setting `LOG_LEVEL` to a non-listed word and expecting normal
  logs.
- **Why:** unrecognized values map to `None`, which disables writing entirely.
- **Fix:** use one of the accepted tokens (`trace`/`vvv`, `debug`/`vv`,
  `information`/`v`/`normal`, `warning`, `error`, `critical`); reserve
  `quiet`/`q` for intentionally silencing output.

## Hard-coding domain-specific metadata

- **Mistake:** baking environment-, tenant-, or product-specific values into
  `AddMeta` calls in shared code.
- **Why:** it couples the reusable logging setup to one deployment and leaks
  context into a general-purpose configuration.
- **Fix:** derive identity metadata from `SerilogOptions.of*` helpers or supply
  it at runtime via `AddMetaFromEnvironment`, keeping inline values neutral.
