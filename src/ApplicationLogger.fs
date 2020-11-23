namespace Lmc.Logging

type ApplicationContext = string
type Message = string

type LogMessage = ApplicationContext -> Message -> unit

type ApplicationLogger = {
    Debug: LogMessage
    Log: LogMessage
    Verbose: LogMessage
    VeryVerbose: LogMessage
    Warning: LogMessage
    Error: LogMessage
}

[<RequireQualifiedAccess>]
module ApplicationLogger =
    open ServiceIdentification

    let quietLogger =
        let ignore: LogMessage = fun _ _ -> ()
        {
            Debug = ignore
            Log = ignore
            Verbose = ignore
            VeryVerbose = ignore
            Warning = ignore
            Error = ignore
        }

    let defaultLogger =
        {
            Debug = Log.debug
            Log = Log.normal
            Verbose = Log.verbose
            VeryVerbose = Log.veryVerbose
            Warning = Log.warning
            Error = Log.error
        }

    let graylogLogger instance connections =
        let configureForService service facility =
            match connections with
            | [ (host, port) ] -> Graylog.Configuration.createForService service host port facility
            | cluster -> Graylog.Configuration.createClusterForService service cluster facility

        let logger =
            instance
            |> Instance.concat "-"
            |> Graylog.Facility
            |> configureForService (instance |> Instance.service)
            |> Graylog.Logger.create
            |> Graylog.Logger.withArgs

        let createMessage (message: string) =
            message
                .Replace("{", "(")
                .Replace("}", ")")
            |> sprintf "[{application_context}] %s"

        {
            Debug = fun context message ->
                if Log.isDebug() then
                    logger.Debug(createMessage message, context)

            Log = fun context message ->
                if Log.isNormal() then
                    logger.Info(createMessage message, context)

            Verbose = fun context message ->
                if Log.isVerbose() then
                    logger.Info(createMessage message, context)

            VeryVerbose = fun context message ->
                if Log.isVeryVerbose() then
                    logger.Info(createMessage message, context)

            Warning = fun context message ->
                if Log.isNormal() then
                    logger.Warning(createMessage message, context)

            Error = fun context message ->
                if Log.isNormal() then
                    logger.Error(createMessage message, context)
        }

    [<AutoOpen>]
    module private Utils =
        let private tee f a =
            f a
            a

        /// Compose two functions with 2 parameters and returning unit
        let (>*>) (f1: 'a -> 'b -> unit) (f2: 'a -> 'b -> unit) a =
            tee (f1 a)
            >> f2 a

    let combine logger additionalLogger =
        {
            Debug = logger.Debug >*> additionalLogger.Debug
            Log = logger.Log >*> additionalLogger.Log
            Verbose = logger.Verbose >*> additionalLogger.Verbose
            VeryVerbose = logger.VeryVerbose >*> additionalLogger.VeryVerbose
            Warning = logger.Warning >*> additionalLogger.Warning
            Error = logger.Error >*> additionalLogger.Error
        }

    // todo - add logger with "closured" context in it
