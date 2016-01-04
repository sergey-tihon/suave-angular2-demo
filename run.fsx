#r "packages/Suave/lib/net40/suave.dll"

open Suave
open Suave.RequestErrors
open Suave.Operators
open Suave.Filters
open Suave.Web
open Suave.Files
open Suave.Logging
open System
open System.Net

let port = 8083us
let logger = Loggers.ConsoleWindowLogger LogLevel.Verbose

let cfg =
  { defaultConfig with
      bindings =
        [ HttpBinding.mk HTTP IPAddress.Loopback port ]
      listenTimeout = TimeSpan.FromMilliseconds 3000. }

let app : WebPart =
  choose [
    log logger logFormat >=> never
    GET >=> choose [
        path "/" >=> file "index.html"
        browse __SOURCE_DIRECTORY__
    ]
    NOT_FOUND "Found no handlers."
  ]

System.Diagnostics.Process.Start(sprintf "http://localhost:%d/" port);
startWebServer cfg app
