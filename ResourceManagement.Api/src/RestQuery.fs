module ResourceManagement.Api.RestQuery

open ResourceManagement.Api.Dtos

open Suave
open Suave.Successful

open Common.FSharp.Suave
open ResourceManagement.Domain

let getWidget userIdString =
  DAL.WidgetManagement.findWidgetByName userIdString
  |> convertToDto
  |> toJson 
  |> OK

let getWidgets (ctx:HttpContext) =
  DAL.WidgetManagement.getAllWidgets ()
  |> List.map convertToDto
  |> toJson
  |> OK

