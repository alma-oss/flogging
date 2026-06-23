---
name: flogging
description: >-
  Use whenever generating or reviewing F# code that configures or calls the
  Alma.Logging library: building a logger via LoggerFactory.create /
  createSerilog / createCustom, composing LoggerOption or SerilogOption lists,
  adding metadata with AddMeta / AddMetaFromEnvironment, routing logs to
  console/JSON/stderr, ignoring paths (IgnorePathHealthCheck, IgnorePaths), or
  reading config from environment variables LOG_TO, LOG_LEVEL, LOG_META. Trigger
  also on mentions of structured logging in F# with Serilog or
  Microsoft.Extensions.Logging, colorful terminal logging, stdout/stderr log
  routing, or SerilogOptions.ofInstance / ofService / ofBox.
---

# F-Logging

Library: [alma-oss/flogging](https://github.com/alma-oss/flogging)
NuGet: `Alma.Logging`

## Purpose

Alma.Logging is an F# library for structured, colorful terminal logging. It
produces a `Microsoft.Extensions.Logging.ILoggerFactory` (or a raw Serilog
`ILogger`) configured through a declarative option-list DSL, and bridges
Microsoft.Extensions.Logging with Serilog. Configuration can be supplied
inline or read from environment variables at runtime.

## When to Use

- Setting up logging in an F# application or library that needs console output.
- Configuring log level, output format (plain console / JSON), or structured
  metadata.
- Routing log configuration through environment variables for deployment
  flexibility.
- Adding service-identity metadata to every log event.

## When NOT to Use

- Non-F# projects, or when a plain `Console.WriteLine` suffices.
- File, network, or database sinks — this library targets the terminal only.
- When the host already provides a fully configured logging pipeline you must
  reuse unchanged.

## Main Concepts

- **LoggerFactory** — module with the factory entry points (`create`,
  `createSerilog`, `createCustom`, `createCustomSerilog`).
- **LoggerOption** — top-level DSL options for the returned `ILoggerFactory`
  (level, console sinks, nested Serilog config, env-var sources, custom
  provider).
- **SerilogOption** — DSL options applied to the Serilog sub-configuration
  (level, console/JSON sinks, metadata, ignored paths, env-var sources).
- **SerilogOptions** — helper module turning Alma.ServiceIdentification values
  (`Service`, `Instance`, `Box`) into lists of `AddMeta` options.
- **LogTo / LogLevel parsing** — internal mapping of env-var strings to output
  targets and levels.
- **customize hooks** — functions passed to `createCustom` /
  `createCustomSerilog` to mutate the underlying builders directly.
- **IgnorePaths** — Serilog filter that drops events whose `Path` property
  matches a configured set (with health-check / metrics / ready shortcuts).

## Related Libraries

- `Microsoft.Extensions.Logging` — the `ILoggerFactory` / `ILogger` abstractions
  returned by `create`.
- `Serilog` — the underlying sink engine for `LogToSerilog`.
- `Alma.ServiceIdentification` — provides `Service` / `Instance` / `Box` consumed
  by the `SerilogOptions.of*` helpers.
- `Feather.ErrorHandling` — result helpers commonly used alongside this library.

## Keywords for Search

Alma.Logging, F# logging, LoggerFactory.create, createSerilog, createCustom,
LoggerOption, SerilogOption, SerilogOptions.ofInstance, AddMeta,
AddMetaFromEnvironment, LogToConsole, LogToConsoleAsJson, LogToSerilog,
LogToFromEnvironment, UseLevelFromEnvironment, IgnorePathHealthCheck,
IgnorePaths, LOG_TO, LOG_LEVEL, LOG_META, structured logging, Serilog,
Microsoft.Extensions.Logging, stdout, stderr, JSON logging.

## Pointers to Reference Files

- For composition principles, recommended API usage, error handling,
  integration, naming, and testing guidance, read
  `references/preferred-patterns.md`.
- For known pitfalls, incorrect assumptions, and replacement guidance, read
  `references/anti-patterns.md`.
- For worked, runnable code examples (the single source of all example code),
  read `references/examples.md`.
