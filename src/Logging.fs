namespace Lmc.Logging

open System
open Microsoft.Extensions.Logging

type LoggerOption =
    | UseLevel of LogLevel
    | UseLevelFromEnvironment of environmentVariableName: string
    | LogToConsole
    | LogToConsoleSimple
    | LogToConsoleAsJson
    | LogToFromEnv of environmentVariableName: string
    | UseProvider of ILoggerProvider

[<RequireQualifiedAccess>]
module LoggerFactory =
    let create (options: LoggerOption list) =
        LoggerFactory.Create(fun builder ->
            options
            |> List.iter (ignore << function
                | UseLevel level -> builder.SetMinimumLevel(level)
                | UseLevelFromEnvironment envName -> builder.SetMinimumLevel(envName |> getEnvVar |> Option.defaultValue "" |> LogLevel.parse)
                | LogToConsole -> builder.AddConsole()
                | LogToConsoleSimple -> builder.AddSimpleConsole()
                | LogToConsoleAsJson -> builder.AddJsonConsole()
                | LogToFromEnv envName ->
                    match envName |> getEnvVar |> Option.defaultValue "" with
                    | "console" -> builder.AddConsole()
                    | "console-simple" | "simple" -> builder.AddSimpleConsole()
                    | "console-json" | "json" -> builder.AddJsonConsole()
                    | _ -> builder
                | UseProvider provider -> builder.AddProvider(provider)
            )
        )

[<RequireQualifiedAccess>]
module NetLogger =
    let logger (factory: ILoggerFactory) context =
        factory.CreateLogger(context)
