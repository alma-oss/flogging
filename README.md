F-Logging
=======

Library for Logging to the terminal.
It logs to the stdout or stderr based on used function. I has colorful output.

## Install

Add following into `paket.dependencies`
```
git ssh://git@stash.int.lmc.cz:7999/archi/nuget-server.git master Packages: /nuget/
# LMC Nuget dependencies:
nuget Lmc.Logging
```

Add following into `paket.references`
```
Lmc.Logging
```

## Verbosity
- `q` -> Verbosity.Quiet
- `v` -> Verbosity.Verbose
- `vv` -> Verbosity.VeryVerbose
- `vvv` -> Verbosity.Debug
- _any other_ -> Verbosity.Normal (_also a default_)

## Logging
Functions uses external lib for logging and formatting the output.
It uses verbosity directly in functions, so you can use specific _verbosity_ and output will be shown only if it should be by that specific verbosity.
For more info, see https://github.com/MortalFlesh/console-style

### Examples
#### Normal verbosity (default)
```fs
// without setting any verbosity -> Verbosity.Normal
Log.normal "Example" "Show some output"
Log.verbose "Example" "Show some more output"
```
Output will be:
```
[Example] Show some output
```

#### Verbose verbosity
```fs
Log.setVerbosityLevel "v"
Log.normal "Example" "Show some output"
Log.verbose "Example" "Show some more output"
```
Output will be:
```
[2019-02-01 10:21:34]    [Example] Show some output
[2019-02-01 10:21:34]    [Example] Show some more output
```

## Logging to Graylog
Send logging messages to the Graylog with UDP.

### Examples
See more [examples](https://stash.int.lmc.cz/projects/ARCHI/repos/flogging/browse/example).

#### Simple string messages
```fs
open Logging

let logger =
    Graylog.Configuration.createDefaultFromBasicOrFail "gray.dev1.services.lmc" "facility"
    |> Graylog.Logger.create

let debug = Graylog.Logger.debug logger
let info = Graylog.Logger.info logger
let warning = Graylog.Logger.warning logger
let error = Graylog.Logger.error logger

debug "Debug Message"
info "Info Message"
warning "Warning Message"
error "Error Message"
```

#### Messages with args
There could be any number of additional args.

```fs
open Logging

let logger =
    Graylog.Configuration.createDefaultFromBasicOrFail "gray.dev1.services.lmc" "facility"
    |> Graylog.Logger.create
    |> Graylog.Logger.withArgs

logger.Debug("[{context}] Debug Message with {foo} {bar}", context, "Foo", "Bar")
logger.Info("[{context}] Info Message", context)
logger.Warning("[{context}] Warning Message", context)
logger.Error("[{context}] Error Message", context)
```

## Release
1. Increment version in `Logging.fsproj`
2. Update `CHANGELOG.md`
3. Commit new version and tag it
4. Run `$ fake build target release`
5. Go to `nuget-server` repo, run `faket build target copyAll` and push new versions

## Development
### Requirements
- [dotnet core](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial)
- [FAKE](https://fake.build/fake-gettingstarted.html)

### Build
```bash
fake build
```

### Watch
```bash
fake build target watch
```
