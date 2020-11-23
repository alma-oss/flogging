// Learn more about F# at http://fsharp.org

open System
open MF.ConsoleStyle
open Lmc.ErrorHandling
open Lmc.Logging

let graylogExample graylogHost =
    let logger =
        Graylog.Configuration.createDefault graylogHost (Graylog.Facility "logging-example")
        |> Graylog.Logger.create

    let debug =
        Graylog.Logger.debug logger
    let info =
        Graylog.Logger.info logger
    let warning =
        Graylog.Logger.warning logger
    let error =
        Graylog.Logger.error logger

    //
    // Logging
    //
    Console.section "Logging"

    Console.message "Debug"
    debug "Debug Message"
    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1.0))

    Console.message "Information"
    info "Info Message"
    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1.0))

    Console.message "Warning"
    warning "Warning Message"
    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1.0))

    Console.message "Error"
    error "Error Message"
    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5.0))

    //
    // Logging With Args
    //
    Console.newLine()
    Console.section "Logging with Args"

    let withArgs =
        Graylog.Logger.withArgs logger

    Console.message "Debug with args"
    withArgs.Debug("Debug with args {foo} {bar}", "FOO", "BAR")
    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1.0))

    Console.message "Info with args"
    withArgs.Info("Info with args {foo} {bar}", "FOO", "BAR")
    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1.0))

    Console.message "Warning with args"
    withArgs.Warning("Warning with args {foo} {bar}", "FOO", "BAR")
    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1.0))

    Console.message "Error with args"
    withArgs.Error("Error with args {foo} {bar}", "FOO", "BAR")
    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1.0))

[<EntryPoint>]
let main argv =
    Console.title "Example - logging"

    asyncResult {
        let graylogService = GraylogService "sys-graylog-common-stable--gelf"

        let isAlive =
            graylogService
            |> Graylog.Diagnostics.isAliveResult  // keep in mind, that this will NOT work on host, where is not a consul agent
            |> AsyncResult.mapError (sprintf "%A")

        printfn "IsAlive: %A" (isAlive |> Async.RunSynchronously)

        let! host =
            "gray.dev1.services.lmc"
            |> Graylog.Host.create
            |> Result.mapError (sprintf "%A")
            |> AsyncResult.ofResult

        host
        |> graylogExample

        return "Example done"
    }
    |> Async.RunSynchronously
    |> printfn "%A"

    Console.success "Done"
    0 // return an integer exit code
