module ResourceManagement.Api.RestQuery

open ResourceManagement.Api.Dtos

open Suave
open Suave.Successful

open Common.FSharp.Suave
open ResourceManagement.Domain

let getClient userIdString =
  DAL.ClientManagement.findClientByName userIdString
  |> convertToClientDto
  |> toJson 
  |> OK

let getClients (ctx:HttpContext) =
  DAL.ClientManagement.getAllClients ()
  |> List.map convertToClientDto
  |> toJson
  |> OK

