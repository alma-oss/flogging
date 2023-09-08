namespace Alma.Logging

open System

[<AutoOpen>]
module internal Utils =
    let tee f a =
        f a
        a

    let normalizeString (string: string) =
        string.Trim().ToLowerInvariant()

    let getEnvVar name =
        try Environment.GetEnvironmentVariable name |> string
        with _ -> ""

    [<RequireQualifiedAccess>]
    type LogTo =
        | Console
        | ConsoleAsJson
        | Nowhere

    [<RequireQualifiedAccess>]
    module LogTo =
        let parse = normalizeString >> function
            | "console" | "stdout" -> LogTo.Console
            | "console-json" | "json" -> LogTo.ConsoleAsJson
            | _ -> LogTo.Nowhere

        let parseFromEnv envName =
            match envName |> getEnvVar with
            | null | "" -> []
            | value ->
                value.Split ';'
                |> Seq.toList
                |> List.map parse
                |> List.distinct

[<RequireQualifiedAccess>]
module LogLevel =
    open Microsoft.Extensions.Logging

    let parse = normalizeString >> function
        | "trace" | "vvv" -> LogLevel.Trace
        | "debug" | "vv" -> LogLevel.Debug
        | "information" | "v" | "normal" -> LogLevel.Information
        | "warning" -> LogLevel.Warning
        | "error" -> LogLevel.Error
        | "critical" -> LogLevel.Critical
        | "quiet" | "q" | _ -> LogLevel.None

    let parseFromEnv =
        getEnvVar >> parse

    open Serilog.Events

    let toLogEventLog = function
        | LogLevel.Trace -> LogEventLevel.Verbose
        | LogLevel.Debug -> LogEventLevel.Debug
        | LogLevel.Information -> LogEventLevel.Information
        | LogLevel.Warning -> LogEventLevel.Warning
        | LogLevel.Error -> LogEventLevel.Error
        | _ -> LogEventLevel.Fatal
