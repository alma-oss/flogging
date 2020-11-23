namespace Lmc.Logging

type GraylogService = GraylogService of string

[<RequireQualifiedAccess>]
module Graylog =
    open System
    open FSharp.Data
    open Gelf.Extensions.Logging
    open Microsoft.Extensions.Logging
    open Lmc.ServiceIdentification
    open Lmc.ErrorHandling

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

    type Connection =
        | Single of Host * Port
        | Cluster of (Host * Port) list

    type Configuration = {
        Connection: Connection
        Facility: Facility
        Service: Service option
    }

    type ConfigurationError =
        | HostError of HostError

    module Configuration =
        let private orFail = function
            | Ok configuration -> configuration
            | Error e -> failwithf "Configuration was not created, because of %A." e

        let create host port facility =
            {
                Connection = Single (host, port)
                Facility = facility
                Service = None
            }

        let createForService service host port facility =
            {
                Connection = Single (host, port)
                Facility = facility
                Service = Some service
            }

        let createDefault host facility =
            create host (Port DefaultPort) facility

        let createDefaultForService service host facility =
            createForService service host (Port DefaultPort) facility

        let createFromBasic host port facility =
            result {
                let! host =
                    host
                    |> Host.create
                    |> Result.mapError HostError

                let facility = Facility facility
                let port = Port port

                return create host port facility
            }

        let createFromBasicOrFail host port facility =
            createFromBasic host port facility
            |> orFail

        let createDefaultFromBasic host facility =
            createFromBasic host DefaultPort facility

        let createDefaultFromBasicOrFail host facility =
            createDefaultFromBasic host facility
            |> orFail

        let createCluster cluster facility =
            {
                Connection = Cluster cluster
                Facility = facility
                Service = None
            }

        let createClusterForService service cluster facility =
            {
                Connection = Cluster cluster
                Facility = facility
                Service = Some service
            }

        let private getRandomItem list =
            list
            |> List.item ((Random()).Next(list.Length))

        let internal toOptions configuration =
            let host, port =
                match configuration.Connection with
                | Single (host, port) -> host, port
                | Cluster cluster -> cluster |> getRandomItem

            let options =
                GelfLoggerOptions(
                    Host = (host |> Host.value),
                    LogSource = Environment.MachineName,
                    Port = (port |> Port.value),
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
        type private HealthCheckSchema = JsonProvider<"src/schema/consul-service-healthcheck.json">

        let private requestServiceHealth service =
            service
            |> sprintf "http://consul.service.consul/v1/health/service/%s"
            |> Http.AsyncRequestString

        let private checkStatus status =
            status = "passing"

        let isAlive (GraylogService graylogService) =
            async {
                try
                    let! response =
                        graylogService
                        |> requestServiceHealth

                    let healthCheck =
                        response
                        |> HealthCheckSchema.Parse

                    return
                        if healthCheck |> Seq.isEmpty then false
                        else
                            healthCheck
                            |> Seq.forall (fun healthCheck ->
                                if healthCheck.Checks |> Seq.isEmpty then false
                                else
                                    healthCheck.Checks
                                    |> Seq.forall (fun check -> check.Status |> checkStatus)
                            )
                with
                | _ -> return false
            }

        type GraylogError =
            | Exception of string
            | ConsulHttpError of string
            | EmptyResponseError
            | NoChecksInResponse of string
            | WrongStatus of string
            | UnknownError

        let isAliveResult (GraylogService graylogService) =
            asyncResult {
                let! response =
                    async {
                        try
                            return! graylogService |> requestServiceHealth
                        with
                        | exn -> return sprintf "Error: %s" exn.Message
                    }
                    |> AsyncResult.ofAsync
                    |> AsyncResult.mapError ConsulHttpError

                if response.StartsWith "Error: " then
                    return! AsyncResult.ofError (Exception <| response.Replace("Error: ", ""))

                let healthCheck = response |> HealthCheckSchema.Parse

                return!
                    if healthCheck |> Seq.isEmpty then AsyncResult.ofError EmptyResponseError
                    else
                        let mutable lastError = Ok ()

                        let isAlive =
                            healthCheck
                            |> Seq.forall (fun healthCheck ->
                                if healthCheck.Checks |> Seq.isEmpty
                                then
                                    lastError <- Error (NoChecksInResponse response)
                                    false
                                else
                                    healthCheck.Checks
                                    |> Seq.forall (fun check ->
                                        if check.Status |> checkStatus then true
                                        else
                                            lastError <- Error (WrongStatus check.Status)
                                            false
                                    )
                            )

                        if isAlive then AsyncResult.ofSuccess true
                        else
                            match lastError with
                            | Ok _ -> UnknownError
                            | Error error -> error
                            |> AsyncResult.ofError
            }
