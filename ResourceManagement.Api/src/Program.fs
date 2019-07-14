

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors

// http://blog.tamizhvendan.in/blog/2015/06/11/building-rest-api-in-fsharp-using-suave/

open Common.FSharp.Suave

open ResourceManagement.Api.ProcessingSystem
open ResourceManagement.Api.EndpointCommands
open ResourceManagement.Api.RestQuery

let app =
  choose 
    [ request authenticationHeaders >=> choose
        [ 
          // All requests are handled together because CQRS
          GET >=> choose
            [ pathCi "/" >=> OK "Default route"
              pathCi "/endpoints" >=> (getEndpoints |> Suave.Http.context) 
              pathScanCi "/endpoints/%s" getEndpoint
            ]            

          // Endpoint commands
          POST >=> pathCi "/endpoints" >=> restful postEndpoint
          PUT >=> pathScanCi "/endpoints/%s" (restfulPathScan putEndpoint)
          DELETE >=> pathScanCi "/endpoints/%s" deleteEndpoint

          // Role commands
          BAD_REQUEST "Request path was not found"
        ]
      Suave.RequestErrors.BAD_REQUEST "Request is missing authentication headers"    
    ]

let defaultArgument x y = defaultArg y x


[<EntryPoint>]
let main argv =
    printfn "main argv"

    let config = { defaultConfig with  bindings = [ HttpBinding.createSimple HTTP "0.0.0.0" 7080 ]}

    startWebServer config app
    0

