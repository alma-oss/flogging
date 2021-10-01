namespace Lmc.Logging

open System

[<AutoOpen>]
module internal Utils =
    let tee f a =
        f a
        a

    type NullScope () =
        interface IDisposable with
            member __.Dispose() = ()

    let getEnvVar name =
        try (Environment.GetEnvironmentVariable name |> string).Trim().ToLowerInvariant() |> Some
        with _ -> None

[<RequireQualifiedAccess>]
module LogLevel =
    open Microsoft.Extensions.Logging

    let parse (value: string) =
        match value.ToLowerInvariant().Trim() with
        | "trace" | "vvv" -> LogLevel.Trace
        | "debug" | "vv" -> LogLevel.Debug
        | "information" | "v" | "normal" -> LogLevel.Information
        | "warning" -> LogLevel.Warning
        | "error" -> LogLevel.Error
        | "critical" -> LogLevel.Critical
        | "quiet" | "q" | _ -> LogLevel.None
