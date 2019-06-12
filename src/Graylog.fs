namespace Logging

[<RequireQualifiedAccess>]
module Graylog =
    open System
    open FSharp.Data
    open Gelf.Extensions.Logging
    open Microsoft.Extensions.Logging
    open ServiceIdentification

    type Logger = private Logger of ILogger

    [<Literal>]
    let DefaultPort = 12201

    type Host = private Host of string

    type HostError =
        | HostContainsProtocolError of string

    module Host =
        let value (Host host) = host

        let create (host: string) =
            result {
                if host.Contains "://" then
                    return! Error (HostContainsProtocolError host)

                return Host host
            }

    type Facility = Facility of string

    module Facility =
        let value (Facility facility) = facility

    type Port = Port of int

    module Port =
        let value (Port port) = port

    type Configuration = {
        Host: Host
        Port: Port
        Facility: Facility
        Service: Service option
    }

    type ConfigurationError =
        | HostError of HostError

    module Configuration =
        let private orFail = function
            | Ok configuration -> configuration
            | Error e -> failwithf "Configuration was not created, because of %A." e

        let create host facility port =
            {
                Host = host
                Port = port
                Facility = facility
                Service = None
            }

        let createForService service host facility port =
            {
                Host = host
                Port = port
                Facility = facility
                Service = Some service
            }

        let createDefault host facility =
            create host facility (Port DefaultPort)

        let createDefaultForService service host facility =
            createForService service host facility (Port DefaultPort)

        let createFromBasic host facility port =
            result {
                let! host =
                    host
                    |> Host.create
                    |> Result.mapError HostError

                let facility = Facility facility
                let port = Port port

                return create host facility port
            }

        let createFromBasicOrFail host facility port =
            createFromBasic host facility port
            |> orFail

        let createDefaultFromBasic host facility =
            createFromBasic host facility DefaultPort

        let createDefaultFromBasicOrFail host facility =
            createDefaultFromBasic host facility
            |> orFail

        let internal toOptions configuration =
            let options =
                GelfLoggerOptions(
                    Host = (configuration.Host |> Host.value),
                    LogSource = System.Environment.MachineName,
                    Port = (configuration.Port |> Port.value),
                    Protocol = GelfProtocol.Udp
                )
            options.AdditionalFields.Add("facility", configuration.Facility |> Facility.value)

            match configuration.Service with
            | Some service ->
                options.AdditionalFields.Add("domain", service.Domain |> Domain.value)
                options.AdditionalFields.Add("context", service.Context |> Context.value)
            | _ -> ()

            options

    module Logger =
        type LoggerWithArgs internal (logger: ILogger) =
            member __.Debug(message: string, [<ParamArray>] args: Object[]) =
                logger.LogDebug(EventId(), message, args)

            member __.Info(message: string, [<ParamArray>] args: Object[]) =
                logger.LogInformation(EventId(), message, args)

            member __.Warning(message: string, [<ParamArray>] args: Object[]) =
                logger.LogWarning(EventId(), message, args)

            member __.Error(message: string, [<ParamArray>] args: Object[]) =
                logger.LogError(EventId(), message, args)

        let private provideLogger (options: GelfLoggerOptions) =
            // When GelfLoggerProvider is disposed, logger will not log anymore.
            (new GelfLoggerProvider(options))
                .CreateLogger("graylog")

        let create configuration =
            configuration
            |> Configuration.toOptions
            |> provideLogger
            |> Logger

        let debug (Logger logger) message =
            logger.LogDebug(EventId(), message)

        let info (Logger logger) message =
            logger.LogInformation(EventId(), message)

        let warning (Logger logger) message =
            logger.LogWarning(EventId(), message)

        let error (Logger logger) message =
            logger.LogError(EventId(), message)

        let withArgs (Logger logger) =
            LoggerWithArgs(logger)

    module Diagnostics =
        let isAlive (Host host) =
            async {
                try
                    let! response =
                        host.TrimEnd('/')
                        |> sprintf "https://%s/api/system/lbstatus"
                        |> Http.AsyncRequestString

                    return response = "ALIVE"
                with
                | _ -> return false
            }
