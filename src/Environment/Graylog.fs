namespace Lmc.Logging.Environment

open Lmc.ErrorHandling
open Lmc.Logging

//
// Errors
//

[<RequireQualifiedAccess>]
type LoggingError =
    | VariableNotFoundError of string
    | InvalidGraylogConnectionString of string
    | InvalidGraylogHost of Graylog.HostError
    | InvalidPort of string

[<RequireQualifiedAccess>]
module LoggingError =
    let format = function
        | LoggingError.VariableNotFoundError name -> sprintf "Environment variable %A for logger is not set." name
        | LoggingError.InvalidGraylogConnectionString name -> sprintf "Graylog connection string %A is invalid." name
        | LoggingError.InvalidGraylogHost name -> sprintf "Graylog host %A is invalid." name
        | LoggingError.InvalidPort name -> sprintf "Graylog port %A is invalid." name

//
// Types
//

type ResourceChecker = {
    GraylogHosts: string
    GraylogService: GraylogService
    Checker: unit -> AsyncResult<bool, Graylog.Diagnostics.GraylogError>
}

[<RequireQualifiedAccess>]
module Graylog =
    open System
    open Result.Operators

    let logger getEnvironmentValue (loggerVariable, loggerServiceVariable) = result {
        let! (graylogHosts: string) =
            loggerVariable
            |> getEnvironmentValue
            <!> id
            <@> LoggingError.VariableNotFoundError

        let! graylogService =
            loggerServiceVariable
            |> getEnvironmentValue
            <!> GraylogService
            <@> LoggingError.VariableNotFoundError

        let! hostsPorts =
            graylogHosts.Split ","
            |> Array.filter (String.IsNullOrEmpty >> not)
            |> Array.map (fun graylog ->
                match graylog.Split ":" with
                | [| host |] -> Ok (host, None)
                | [| host; port |] -> Ok (host, Some port)
                | _ -> Error (LoggingError.InvalidGraylogConnectionString graylog)
            )
            |> Array.toList
            |> Result.sequence

        let! graylogHostsPorts =
            hostsPorts
            |> List.map (fun (host, port) ->
                result {
                    let! host =
                        host
                        |> Graylog.Host.create
                        |> Result.mapError LoggingError.InvalidGraylogHost

                    let! port =
                        match port with
                        | Some port ->
                            match Int32.TryParse port with
                            | true, port -> Ok <| Some (Graylog.Port port)
                            | _ -> Error (LoggingError.InvalidPort port)
                        | _ -> Ok None

                    return (host, port |> Option.defaultValue (Graylog.Port Graylog.DefaultPort))
                }
            )
            |> Result.sequence

        if graylogHostsPorts |> List.isEmpty then
            return! Error (LoggingError.InvalidGraylogConnectionString graylogHosts)

        let resourceChecker =
            {
                GraylogHosts = graylogHosts
                GraylogService = graylogService
                Checker = (fun () -> graylogService |> Graylog.Diagnostics.isAliveResult)
            }

        return graylogHostsPorts, resourceChecker
    }
