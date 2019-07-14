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

open Suave
open Suave.Successful

open Common.FSharp.Suave

let getEndpoint endpointName =
  DAL.EndpointChange.findEndpointByName endpointName
  |> convertToDto
  |> toJson 
  |> OK

let getEndpoints (ctx:HttpContext) =
  DAL.EndpointChange.getAllEndpoints ()
  |> List.map convertToDto
  |> toJson
  |> OK

