

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors

// http://blog.tamizhvendan.in/blog/2015/06/11/building-rest-api-in-fsharp-using-suave/

open Common.FSharp.Suave

open ResourceManagement.Api.ProcessingSystem
open ResourceManagement.Api.WidgetCommands
open ResourceManagement.Api.RestQuery

let app =
  choose 
    [ request authenticationHeaders >=> choose
        [ 
          // All requests are handled together because CQRS
          GET >=> choose
            [ pathCi "/" >=> OK "Default route"
              pathCi "/widgets" >=> (getWidgets |> Suave.Http.context) 
              pathScanCi "/widgets/%s" getWidget
            ]            

          // Widget commands
          POST >=> pathCi "/widgets" >=> restful postWidget
          PUT >=> pathScanCi "/widgets/%s" (restfulPathScan putWidget)
          DELETE >=> pathScanCi "/widgets/%s" deleteWidget

          // Role commands
          BAD_REQUEST "Request path was not found"
        ]
      // Suave.RequestErrors.UNAUTHORIZED "Request is missing authentication headers"    
      BAD_REQUEST "Request is missing authentication headers"

    ]

let defaultArgument x y = defaultArg y x


[<EntryPoint>]
let main argv =
    printfn "main argv"

    let config = { defaultConfig with  bindings = [ HttpBinding.createSimple HTTP "127.0.0.1" 8080 ]}

    startWebServer config app
    0

