F-Logging
=======

Library for Logging to the terminal.
It logs to the stdout or stderr based on used function. I has colorful output.

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

## Release
1. Increment version in `src/TODO.fsproj`
2. Run `$ fake build target release`
3. Move TODO package (`TODO.VERSION.nupkg`) from `./release` dir to the NugetServer packages dir
4. Update `CHANGELOG.md`

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
