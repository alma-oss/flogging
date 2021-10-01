// Learn more about F# at http://fsharp.org

open System
open Lmc.ErrorHandling
open Lmc.Logging
open Microsoft.Extensions.Logging

[<EntryPoint>]
let main argv =
    printfn "Example - logging"

    use factory = LoggerFactory.create [
        UseLevel LogLevel.Trace
        LogToConsole
        //LogToConsoleAsJson
    ]

    let logger = factory.CreateLogger("Example")

    logger.LogTrace("Trace message")
    logger.LogDebug("Debug message")
    logger.LogInformation("Information message")
    logger.LogWarning("Warning message")
    logger.LogError("Error message")
    logger.LogCritical("Critical message")

    0 // return an integer exit code
