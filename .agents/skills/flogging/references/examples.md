# Examples

The single source of all example code for this skill. Examples are ordered by
increasing complexity and are self-contained.

## Basic Example

Minimal factory with a plain console sink and a structured log line.

```fsharp
open Alma.Logging
open Microsoft.Extensions.Logging

use factory = LoggerFactory.create [
    UseLevel LogLevel.Information
    LogToConsole
]

let logger = factory.CreateLogger("ExampleApi")

logger.LogInformation("Started {Component} on port {Port}", "WebApi", 8080)
```

## Realistic Example

Trace level, both human-readable console and JSON output, plus static metadata.

```fsharp
open Alma.Logging
open Microsoft.Extensions.Logging

use factory = LoggerFactory.create [
    UseLevel LogLevel.Trace

    LogToSerilog [
        SerilogOption.LogToConsole
        SerilogOption.LogToConsoleAsJson

        AddMeta ("service", "ServiceA")
        AddMeta ("component", "Worker")
        AddMeta ("version", "1.0.0")
    ]
]

let logger = factory.CreateLogger("Worker")

logger.LogTrace("{Level} message", "Trace")
logger.LogInformation("Processed {Count} items", 42)
logger.LogError("Failed to reach {Target}", "CacheInstance")
```

## Integration Example

Service-identity metadata from Alma.ServiceIdentification, runtime metadata from
an environment variable, and ignored noise paths.

```fsharp
open Alma.Logging
open Microsoft.Extensions.Logging

let buildFactory instance =
    LoggerFactory.create [
        LogToConsole
        UseLevel LogLevel.Trace

        LogToSerilog (
            SerilogOptions.ofInstance instance @ [
                SerilogOption.UseLevel LogLevel.Information
                AddMetaFromEnvironment "LOG_META"

                SerilogOption.IgnorePathHealthCheck
                SerilogOption.IgnorePathMetrics
                SerilogOption.IgnorePaths [ "/ready" ]
            ]
        )
    ]
```

## Custom Filter Example

`createCustom` with a Serilog customization hook that drops selected events.

```fsharp
open Alma.Logging
open Microsoft.Extensions.Logging
open Serilog
open Serilog.Events

let customizeSerilog (builder: LoggerConfiguration) =
    builder.Filter.ByExcluding(fun (logEvent: LogEvent) ->
        match logEvent.Level with
        | LogEventLevel.Information ->
            logEvent.MessageTemplate.Text.Contains("ignored")
        | _ -> false
    )
    |> ignore

use factory = LoggerFactory.createCustom ignore customizeSerilog [
    UseLevel LogLevel.Trace
    LogToSimpleConsole

    LogToSerilog [
        SerilogOption.LogToConsole
        SerilogOption.UseLevel LogLevel.Information
        SerilogOption.AddMeta ("meta", "serilog-custom")
    ]
]

let logger = factory.CreateLogger("ExampleApi")

logger.LogInformation("{Level} message", "Information")
logger.LogInformation("{Level} message ignored", "Information")
```

## Environment-Driven Example

Resolve level, output targets, and metadata entirely from environment variables.

```sh
# Output targets (separate multiple with ';')
LOG_TO="console"          # or: stdout
LOG_TO="console-json"     # or: json
LOG_TO="console; json"    # both human-readable and JSON

# Level (see accepted tokens)
LOG_LEVEL="debug"

# Structured metadata: 'key:value' pairs separated by ';'
LOG_META="service:ServiceA; component:Worker"
```

```fsharp
open Alma.Logging

use factory = LoggerFactory.create [
    UseLevelFromEnvironment "LOG_LEVEL"
    LogToFromEnvironment "LOG_TO"

    LogToSerilog [
        AddMetaFromEnvironment "LOG_META"
    ]
]

let logger = factory.CreateLogger("ServiceA")

logger.LogInformation("Configuration loaded from environment")
```
