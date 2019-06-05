// Learn more about F# at http://fsharp.org

open System
open MF.ConsoleStyle
open Logging

let graylogExample () =
    let logger =
        Graylog.Configuration.createDefaultFromBasicOrFail "gray.dev1.services.lmc" "logging-example"
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

    graylogExample()

    Console.success "Done"
    0 // return an integer exit code
