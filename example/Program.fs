// Learn more about F# at http://fsharp.org

open System
open Alma.ErrorHandling
open Alma.Logging
open Alma.ServiceIdentification
open Microsoft.Extensions.Logging
open Serilog
open Serilog.Events

module WithCustom =
    let logWithCustom exampleInstance =
        printfn "\n\nWithCustom.logWithCustom\n------------------------\n"

        let customizeSerilog (builder: LoggerConfiguration) =
            builder.Filter.ByExcluding(fun (logEvent: LogEvent) ->
                match logEvent.Level with
                | LogEventLevel.Information ->
                    let res = logEvent.MessageTemplate.Text.Contains("ignored")

                    if res then printfn "[LOG] Ignored"
                    (* printfn "Exclude? %s |: %A"
                        ([
                            "L", logEvent.Level |> string
                            "M", logEvent.MessageTemplate.Text
                            "P", logEvent.Properties |> Seq.map (fun kv -> sprintf "%A => %A" kv.Key kv.Value) |> String.concat "; "
                        ] |> List.map (fun kv -> kv ||> sprintf "%s: %s") |> String.concat " | ")
                        res *)

                    res
                | _ -> false
            )
            |> ignore

        use factory = LoggerFactory.createCustom ignore customizeSerilog [
            UseLevel LogLevel.Trace                 // this level is "globally" used in logger factory (for additional providers)
            // UseProvider (Tracing.provider())     // add Tracing log provider, which add current log to trace as baggage (see Alma.Tracing)

            LogToSimpleConsole
            LogToConsole
            UseLevel LogLevel.Trace

            LogToSerilog (
                SerilogOptions.ofInstance exampleInstance @ [
                    SerilogOption.AddMeta ("meta", "serilog-custom")
                    SerilogOption.LogToConsole
                    // SerilogOption.LogToConsoleAsJson // ideally in production, by environment value
                    SerilogOption.UseLevel LogLevel.Information

                    SerilogOption.IgnorePathHealthCheck
                    SerilogOption.IgnorePathMetrics
                    SerilogOption.IgnorePaths [ "/health" ]
                ])
        ]

        let logger = factory.CreateLogger("ExampleWithCustom")

        logger.LogTrace("{Level} message", "Trace")
        logger.LogDebug("{level} message", "Debug")
        logger.LogInformation("{level} message", "Information")
        logger.LogInformation("{level} message ignored", "Information")
        logger.LogWarning("{level} message", "Warning")
        logger.LogError("{level} message", "Error")
        logger.LogCritical("{level} message", "Critical")
        ()

[<EntryPoint>]
let main argv =
    printfn "Example - logging\n=================\n"

    let exampleInstance = Create.Instance("lmc", "logging", "example", "dev") |> Result.orFail

    (* use factory = LoggerFactory.create [
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
    logger.LogCritical("{level} message", "Critical") *)

    WithCustom.logWithCustom exampleInstance

    0 // return an integer exit code
