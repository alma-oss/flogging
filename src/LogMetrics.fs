namespace Alma.Logging

open Microsoft.Extensions.Logging
open Serilog.Core
open Serilog.Events
open Feather.ErrorHandling
open Alma.Metrics
open Alma.ServiceIdentification

/// Counter of emitted log events per level, in the Alma.Metrics default registry.
[<RequireQualifiedAccess>]
module LogMetrics =
    /// `log_messages_total`
    let name = MetricName.createOrFail "log_messages_total"

    /// Renders the counter with `# HELP` / `# TYPE` for a `/metrics` response, or "" when nothing was logged yet.
    let format () =
        match State.getMetric name with
        | Some metric ->
            { metric with
                Description = Some "Total number of emitted log messages per level."
                Type = Some MetricType.Counter
            }
            |> Metric.format
        | None -> ""

/// Serilog sink incrementing `log_messages_total{level}` for every event it receives.
type internal LogLevelCounterSink (instance: Instance) =
    let keyByLevel =
        [ LogLevel.Trace; LogLevel.Debug; LogLevel.Information; LogLevel.Warning; LogLevel.Error; LogLevel.Critical ]
        |> List.map (fun level ->
            let key =
                [ "level", level |> LogLevel.label ]
                |> DataSetKey.createFromInstance instance
                |> Result.orFail

            level, key
        )
        |> Map.ofList

    interface ILogEventSink with
        member _.Emit logEvent =
            match keyByLevel |> Map.tryFind (logEvent.Level |> LogLevel.ofLogEventLevel) with
            | Some key -> State.incrementMetricSetValue (MetricValue.Int 1) LogMetrics.name key |> ignore
            | None -> ()
