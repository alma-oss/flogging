F-Logging
=========

[![NuGet](https://img.shields.io/nuget/v/Alma.Logging.svg)](https://www.nuget.org/packages/Alma.Logging)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Alma.Logging.svg)](https://www.nuget.org/packages/Alma.Logging)
[![Tests](https://github.com/alma-oss/flogging/actions/workflows/tests.yaml/badge.svg)](https://github.com/alma-oss/flogging/actions/workflows/tests.yaml)

Library for Logging to the terminal.
It logs to the stdout or stderr based on used function. I has colorful output.

## Install

Add following into `paket.references`
```
Alma.Logging
```

## Usage
```fs
open Alma.Logging

use factory = LoggerFactory.create [
    UseLevel LogLevel.Trace

    LogToConsole

    LogToSerilog [
        SerilogOption.LogToConsole
        SerilogOption.LogToConsoleAsJson

        AddMeta ("domain", "domain")
        AddMeta ("context", "context")
        AddMeta ("purpose", "purpose")
        AddMeta ("version", "version")
    ]
]

let logger = factory.CreateLogger("Example")

logger.LogTrace("{Level} message", "Trace")
```

## Configure from environment variables

### Log output from environment variables
You can add dynamic option `LogToFromEnvironment` with name of a environment variable, which will be used to determine the output of the logs.
You can add multiple values, separated by `;` (_you can add spaces for readability_).

```sh
# log to console (multiple options to set up)
LOG_TO="console"
LOG_TO="stdout"

# log to console as json (multiple options to set up)
LOG_TO="console-json"
LOG_TO="json"

# log to both console AND to console as json
LOG_TO="console; json"
```

```fs
LoggerFactory.create [
    LogToFromEnvironment "LOG_TO"
]
```

### Log level from environment variables

```sh
LOG_LEVEL="debug"
```

```fs
LoggerFactory.create [
    UseLevelFromEnvironment "LOG_LEVEL"
]
```

Log level is parsed based on following table:

| Final level | Options | Description |
| ---         | ---     | ---         |
| _LogLevel_.**Trace** | `"trace"`, `"vvv"` | Logs that contain the most detailed messages. These messages may contain sensitive application data. These messages are disabled by default and should never be enabled in a production environment. |
| _LogLevel_.**Debug** | `"debug"`, `"vv"` | Logs that are used for interactive investigation during development. These logs should primarily contain information useful for debugging and have no long-term value. |
| _LogLevel_.**Information** | `"information"`, `"v"`, `"normal"` | Logs that track the general flow of the application. These logs should have long-term value. |
| _LogLevel_.**Warning** | `"warning"` | Logs that highlight an abnormal or unexpected event in the application flow, but do not otherwise cause the application execution to stop. |
| _LogLevel_.**Error** | `"error"` | Logs that highlight when the current flow of execution is stopped due to a failure. These should indicate a failure in the current activity, not an application-wide failure. |
| _LogLevel_.**Critical** | `"critical`" | Logs that describe an unrecoverable application or system crash, or a catastrophic failure that requires immediate attention. |
| _LogLevel_.**None** | `"quiet"`, `"q"`, `_anything else_` | Not used for writing log messages. Specifies that a logging category should not write any messages. |

### Add metadata to Serilog by environment variables
> This is useful if you have library which uses a logger factory inside

```sh
LOG_META="domain:tearoom; context:menu"
```

```fs
LoggerFactory.create [
    LogToSerilog [
        AddMetaFromEnvironment "LOG_META"
    ]
]
```

## Useful links
- https://www.tutorialsteacher.com/core/fundamentals-of-logging-in-dotnet-core
- https://benfoster.io/blog/serilog-best-practices/

## Release
1. Increment version in `Logging.fsproj`
2. Update `CHANGELOG.md`
3. Commit new version and tag it

## Development
### Requirements
- [dotnet core](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial)

### Build
```bash
./build.sh build
```

### Tests
```bash
./build.sh -t tests
```
