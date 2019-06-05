namespace Logging

[<RequireQualifiedAccess>]
module Log =
    open MF.ConsoleStyle

    let setVerbosityLevel level =
        match level with
        | "q" -> Verbosity.Quiet
        | "v" -> Verbosity.Verbose
        | "vv" -> Verbosity.VeryVerbose
        | "vvv" -> Verbosity.Debug
        | _ -> Verbosity.Normal
        |> Console.setVerbosity

    let isQuiet = Console.isQuiet
    let isNormal = Console.isNormal
    let isVerbose = Console.isVerbose
    let isVeryVerbose = Console.isVeryVerbose
    let isDebug = Console.isDebug

    let normal context text =
        Console.messagef2 "[%s] %s" context text

    let verbose context text =
        if isVerbose() then
            normal context text

    let veryVerbose context text =
        if isVeryVerbose() then
            normal context text

    let debug context text =
        if isDebug() then
            normal context text

    let error context text =
        Console.errorf2 "[Error][%s] %s" context text

    let warning context text =
        Console.errorf2 "[Warning][%s] %s" context text
