// Learn more about F# at http://fsharp.org

open System
open Alma.ErrorHandling
open Alma.Logging
open Alma.ServiceIdentification
open Microsoft.Extensions.Logging

[<EntryPoint>]
let main argv =
    printfn "Example - logging\n=================\n"

    let exampleInstance = Create.Instance("lmc", "logging", "example", "dev") |> Result.orFail

    use factory = LoggerFactory.create [
        UseLevel LogLevel.Trace                 // this level is "globally" used in logger factory (for additional providers)
        // UseProvider (Tracing.provider())     // add Tracing log provider, which add current log to trace as baggage (see Alma.Tracing)

        LogToSimpleConsole

        LogToSerilog (
            SerilogOptions.ofInstance exampleInstance @ [
                SerilogOption.LogToConsole
                // SerilogOption.LogToConsoleAsJson // ideally in production, by environment value
                SerilogOption.UseLevel LogLevel.Information
            ])
    ]

    let logger = factory.CreateLogger("Example")

    logger.LogTrace("{Level} message", "Trace")
    logger.LogDebug("{level} message", "Debug")
    logger.LogInformation("{level} message", "Information")
    logger.LogWarning("{level} message", "Warning")
    logger.LogError("{level} message", "Error")
    logger.LogCritical("{level} message", "Critical")

    0 // return an integer exit code
