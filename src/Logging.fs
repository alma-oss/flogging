namespace Lmc.Logging

open System
open Microsoft.Extensions.Logging
open Serilog
open Serilog.Events

type private SerilogBuilderOption =
    | UseLevel of LogEventLevel
    | LogToConsole
    | LogToConsoleAsJson
    | AddMeta of name: string * value: string

type SerilogOption =
    | UseLevel of LogLevel
    | UseLogEventLevel of LogEventLevel
    | UseLevelFromEnvironment of environmentVariableName: string
    | LogToConsole
    | LogToConsoleAsJson
    | LogToFromEnvironment of environmentVariableName: string
    | AddMeta of name: string * value: string
    | AddMetaFromEnvironment of environmentVariableName: string

type private LoggerFactoryOptions =
    | UseLevel of LogLevel
    | LogToConsole
    | LogToSerilog of SerilogOption list
    | UseProvider of ILoggerProvider

type LoggerOption =
    | UseLevel of LogLevel
    | UseLevelFromEnvironment of environmentVariableName: string
    | LogToSimpleConsole
    | LogToConsole
    | LogToConsoleAsJson
    | LogToSerilog of SerilogOption list
    | LogToFromEnvironment of environmentVariableName: string
    | UseProvider of ILoggerProvider

[<RequireQualifiedAccess>]
module private SerilogBuilderOption =
    let ofLogTo = function
        | LogTo.Console -> Some SerilogBuilderOption.LogToConsole
        | LogTo.ConsoleAsJson -> Some SerilogBuilderOption.LogToConsoleAsJson
        | LogTo.Nowhere -> None

[<RequireQualifiedAccess>]
module SerilogOptions =
    open Lmc.ServiceIdentification

    let ofService (service: Service) =
        [
            AddMeta ("domain", service.Domain |> Domain.value)
            AddMeta ("context", service.Context |> Context.value)
        ]

    let ofInstance (instance: Instance) =
        [
            AddMeta ("domain", instance.Domain |> Domain.value)
            AddMeta ("context", instance.Context |> Context.value)
            AddMeta ("purpose", instance.Purpose |> Purpose.value)
            AddMeta ("version", instance.Version |> Version.value)
        ]

    let ofBox (box: Box) =
        [
            AddMeta ("domain", box.Domain |> Domain.value)
            AddMeta ("context", box.Context |> Context.value)
            AddMeta ("purpose", box.Purpose |> Purpose.value)
            AddMeta ("version", box.Version |> Version.value)
            AddMeta ("zone", box.Zone |> Zone.value)
            AddMeta ("bucket", box.Bucket |> Bucket.value)
        ]

    let internal ofLogTo = function
        | LogTo.Console -> [ SerilogOption.LogToConsole ]
        | LogTo.ConsoleAsJson -> [ SerilogOption.LogToConsoleAsJson ]
        | LogTo.Nowhere -> []

[<RequireQualifiedAccess>]
module LoggerFactory =
    [<RequireQualifiedAccess>]
    module private Normalize =
        let serilogBuilderOptions options =
            options
            |> List.collect (function
                | SerilogOption.UseLogEventLevel level -> [ SerilogBuilderOption.UseLevel level ]
                | SerilogOption.UseLevel level -> [ level |> LogLevel.toLogEventLog |> SerilogBuilderOption.UseLevel ]
                | SerilogOption.UseLevelFromEnvironment envVar -> [ envVar |> LogLevel.parseFromEnv |> LogLevel.toLogEventLog |> SerilogBuilderOption.UseLevel ]

                | SerilogOption.LogToConsole -> [ SerilogBuilderOption.LogToConsole ]
                | SerilogOption.LogToConsoleAsJson -> [ SerilogBuilderOption.LogToConsoleAsJson ]
                | SerilogOption.LogToFromEnvironment envVar ->
                    envVar
                    |> LogTo.parseFromEnv
                    |> List.choose SerilogBuilderOption.ofLogTo

                | SerilogOption.AddMeta (key, value) -> [ SerilogBuilderOption.AddMeta (key, value) ]
                | SerilogOption.AddMetaFromEnvironment envVar ->
                    match envVar |> getEnvVar with
                    | null | "" -> []
                    | values ->
                        values.Split ';'
                        |> Seq.choose (function
                            | null | "" -> None
                            | value ->
                                match value.Split ':' |> Seq.map (fun s -> s.Trim()) |> Seq.toList with
                                | [ ""; _ ] -> None
                                | [ key; value ] -> Some (SerilogBuilderOption.AddMeta (key, value))
                                | _ -> None
                        )
                        |> Seq.toList

            )
            |> List.distinct

        let collectSerilogOptions options =
            let (useSerilog, serilogOptions) =
                options
                |> List.collect (function
                    | UseLevel level -> [ SerilogOption.UseLevel level ]
                    | UseLevelFromEnvironment envVar -> [ SerilogOption.UseLevelFromEnvironment envVar ]

                    | LogToConsole -> [ SerilogOption.LogToConsole ]
                    | LogToConsoleAsJson -> [ SerilogOption.LogToConsoleAsJson ]
                    | LogToFromEnvironment envVar ->
                        envVar
                        |> LogTo.parseFromEnv
                        |> List.collect SerilogOptions.ofLogTo

                    | LogToSerilog serilogOptions ->
                        serilogOptions
                        |> List.collect (function
                            | SerilogOption.LogToFromEnvironment envVar ->
                                envVar
                                |> LogTo.parseFromEnv
                                |> List.collect SerilogOptions.ofLogTo

                            | opt -> [ opt ]
                        )

                    | LogToSimpleConsole
                    | UseProvider _ -> []
                )
                |> List.distinct
                |> List.fold (fun (useSerilog, options) opt ->
                    match opt with
                    | SerilogOption.LogToConsole
                    | SerilogOption.LogToConsoleAsJson -> true, (opt :: options)
                    | _ -> (useSerilog, opt :: options)
                ) (false, [])

            if useSerilog then Some (serilogOptions |> List.rev) else None

        let factoryOptions options =
            let factoryOptions =
                options
                |> List.choose (function
                    | UseLevel level -> LoggerFactoryOptions.UseLevel level |> Some
                    | UseLevelFromEnvironment envVar -> envVar |> LogLevel.parseFromEnv |> LoggerFactoryOptions.UseLevel |> Some

                    | LogToSimpleConsole -> LoggerFactoryOptions.LogToConsole |> Some
                    | UseProvider provider -> LoggerFactoryOptions.UseProvider provider |> Some

                    | LogToConsole
                    | LogToConsoleAsJson
                    | LogToSerilog _
                    | LogToFromEnvironment _
                        -> None
                )
                |> List.distinct

            match options |> collectSerilogOptions with
            | Some serilogOptions -> LoggerFactoryOptions.LogToSerilog serilogOptions :: factoryOptions
            | _ -> factoryOptions

    let createSerilog (options: SerilogOption list) =
        let builder = LoggerConfiguration()

        options
        |> Normalize.serilogBuilderOptions
        |> List.iter (ignore << function
            | SerilogBuilderOption.UseLevel level -> builder.MinimumLevel.Is level
            | SerilogBuilderOption.LogToConsole ->
                builder.WriteTo.Console(
                    standardErrorFromLevel = LogEventLevel.Error,
                    outputTemplate = "[{Level:u3}:{Timestamp:HH:mm:ss}][{SourceContext:l}] {Message:lj}{NewLine}{Exception}"
                )
            | SerilogBuilderOption.LogToConsoleAsJson -> builder.WriteTo.Console(Formatting.Json.JsonFormatter(), standardErrorFromLevel = LogEventLevel.Error)
            | SerilogBuilderOption.AddMeta (key, value) -> builder.Enrich.WithProperty(key, value)
        )

        builder.CreateLogger()

    let create (options: LoggerOption list) =
        LoggerFactory.Create(fun builder ->
            options
            |> Normalize.factoryOptions
            |> List.iter (ignore << function
                | LoggerFactoryOptions.UseLevel level -> builder.SetMinimumLevel(level)
                | LoggerFactoryOptions.LogToConsole -> builder.AddConsole(fun c -> c.LogToStandardErrorThreshold <- LogLevel.Error)
                | LoggerFactoryOptions.LogToSerilog serilogOptions -> builder.AddSerilog(createSerilog serilogOptions, true)
                | LoggerFactoryOptions.UseProvider provider -> builder.AddProvider(provider)
            )
        )
