# Changelog

<!-- There is always Unreleased section on the top. Subsections (Add, Changed, Fix, Removed) should be Add as needed. -->
## Unreleased

## 8.0.0 - 2023-09-08
- [**BC**] Use `Alma` namespace

## 7.1.0 - 2023-08-10
- Update external dependencies

## 7.0.0 - 2023-08-10
- [**BC**] Use net7.0

## 6.0.0 - 2022-01-04
- [**BC**] Use net6.0

## 5.1.0 - 2021-10-04
- Allow to add `Serilog` metadata by environment variables

## 5.0.0 - 2021-10-04
- Update dependencies
- [**BC**] Remove Graylog
- [**BC**] Remove ConsoleStyle
- [**BC**] Remove ApplicationLogger
- Use common Logger extensions to provide a logger factory
- Add `Serilog` logger

## 4.2.0 - 2021-03-18
- Add explicit dependency on `Microsoft.Extensions.Logging (5.0)`

## 4.1.0 - 2021-02-15
- Update dependencies

## 4.0.0 - 2020-11-23
- [**BC**] Use .netcore 5.0

## 3.3.0 - 2020-11-23
- Update dependencies

## 3.2.0 - 2020-04-16
- Add `ApplicationLogger`

## 3.1.0 - 2020-04-16
- Add `GraylogService` type
- Add `Lmc.Logging.Environment` namespace with `Graylog` module, to allow simplified parsing Environment variables.

## 3.0.0 - 2020-04-15
- Change git host to bitbucket
- Update dependencies
- Add `AssemblyInfo`
- [**BC**] Require .netcore 3.1
- [**BC**] Change namespace to `Lmc.Logging`

## 2.2.0 - 2019-07-16
- Allow graylog to connect the cluster (_one of the cluster nodes_)
- Add graylog configuration functions
    - `Graylog.Configuration.createCluster`
    - `Graylog.Configuration.createClusterForService`

## 2.1.1 - 2019-07-16
- Fix consul host

## 2.1.0 - 2019-07-15
- Add `Graylog.Diagnostics.isAliveResult` function to get verbose result with explicit Errors on _not alive_ state

## 2.0.0 - 2019-07-15
- [**BC**] Change order of parameters for functions, to be more user friendly
    - `Graylog.Configuration.create`
    - `Graylog.Configuration.createForService`
    - `Graylog.Configuration.createFromBasic`
    - `Graylog.Configuration.createFromBasicOrFail`
- [**BC**] Change implementation of `Graylog.isAlive` function, to use `consul` as health check and consul service as identification

## 1.6.0 - 2019-06-26
- Add lint

## 1.5.0 - 2019-06-12
- Allow `Service` identification to graylog additional options

## 1.4.0 - 2019-06-06
- Add `Graylog.Diagnostics` module
    - Add `isAlive` function to allow checking Graylog status

## 1.3.0 - 2019-06-05
- Expose functions for getting a current verbosity level

## 1.2.0 - 2019-06-05
- Change Graylog message
    - Use `Environment.MachineName` as `source` instead of `facility`
    - Use `Facility` as `facility`

## 1.1.0 - 2019-06-05
- Allow logging to Graylog

## 1.0.0 - 2019-01-31
- Initial implementation
