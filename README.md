F-Logging
=========

Library for Logging to the terminal.
It logs to the stdout or stderr based on used function. I has colorful output.

## Install

Add following into `paket.dependencies`
```
git ssh://git@bitbucket.lmc.cz:7999/archi/nuget-server.git master Packages: /nuget/
# LMC Nuget dependencies:
nuget Lmc.Logging
```

Add following into `paket.references`
```
Lmc.Logging
```

## Useful links
- https://www.tutorialsteacher.com/core/fundamentals-of-logging-in-dotnet-core

## Release
1. Increment version in `Logging.fsproj`
2. Update `CHANGELOG.md`
3. Commit new version and tag it
4. Run `$ ./build.sh -t release`
5. Go to `nuget-server` repo, run `faket build target copyAll` and push new versions

## Development
### Requirements
- [dotnet core](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial)
- [FAKE](https://fake.build/fake-gettingstarted.html)

### Build
```bash
./build.sh
```

### Watch
```bash
./build.sh -t watch
```
