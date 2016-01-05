#r "packages/Suave/lib/net40/suave.dll"
#r "packages/FAKE/tools/FakeLib.dll"

open Fake
open Suave
open Suave.Web
open Suave.Http
open Suave.Operators
open Suave.Sockets.Control
open Suave.WebSocket
open Suave.Utils
open Suave.Files
open Suave.RequestErrors
open Suave.Filters
open System
open System.Net


let port =
    let rec findPort port =
        let portIsTaken =
            System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners()
            |> Seq.exists (fun x -> x.Port = int(port))
        if portIsTaken then findPort (port + 1us) else port
    findPort 8083us
let logger = Logging.Loggers.ConsoleWindowLogger Logging.LogLevel.Verbose
let refreshEvent = new Event<_>()

let handleWatcherEvents (events:FileChange seq) =
    for e in events do
        let fi = fileInfo e.FullPath
        traceImportant <| sprintf "%s was changed." fi.Name
    refreshEvent.Trigger()

let socketHandler (webSocket : WebSocket) =
  fun cx -> socket {
    while true do
      let! refreshed =
        Control.Async.AwaitEvent(refreshEvent.Publish)
        |> Suave.Sockets.SocketOp.ofAsync
      do! webSocket.send Text (ASCII.bytes "refreshed") true
  }

let cfg =
  { defaultConfig with
      homeFolder = Some (__SOURCE_DIRECTORY__)
      bindings =
        [ HttpBinding.mk HTTP IPAddress.Loopback port ]
      listenTimeout = TimeSpan.FromMilliseconds 3000. }

let app : WebPart =
  choose [
    Filters.log logger logFormat >=> never
    Filters.path "/websocket" >=> handShake socketHandler
    Filters.GET >=> Filters.path "/" >=> file "index.html"
    Writers.setHeader "Cache-Control" "no-cache, no-store, must-revalidate"
      >=> Writers.setHeader "Pragma" "no-cache"
      >=> Writers.setHeader "Expires" "0"
      >=> browseHome
    NOT_FOUND "Found no handlers."
  ]


let watcher =
    !! ("app/*.js")
      ++ ("*.html")
    |> WatchChanges handleWatcherEvents

try
    System.Diagnostics.Process.Start(sprintf "http://localhost:%d/index.html" port) |> ignore
    startWebServer cfg app
finally
    watcher.Dispose()