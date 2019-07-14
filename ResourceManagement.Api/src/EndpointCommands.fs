module ResourceManagement.Api.EndpointCommands

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful

open Common.FSharp.Suave
open Common.FSharp.Envelopes

open ResourceManagement.Domain.EndpointChange
open ResourceManagement.Domain
open ResourceManagement.Data.Models

open ResourceManagement.Api.ProcessingSystem
open ResourceManagement.Api.Dtos

// http://blog.tamizhvendan.in/blog/2015/06/11/building-rest-api-in-fsharp-using-suave/

let private dtoToEndpoint (dto:EndpointDto) = 
  { 
      Name = dto.name
      Url = dto.url
      Method = 
        System.Enum.Parse (typeof<EndpointMethod>, dto.method) 
        :?> EndpointMethod
  }

let private tellActor = sendEnvelope actorGroups.EndpointChangeActors.Tell 

let postEndpoint (dto:EndpointDto)=  
  let newEndpointId = StreamId.create ()

  let commandToActor = 
    dto
    |> dtoToEndpoint    
    |> EndpointChangeCommand.Create
    |> tellActor newEndpointId

  let respond = newEndpointId |> toJson |> OK

  commandToActor >=> respond


let deactivateEndpoint (endpoint:Endpoint) = 
  let commandToActor = 
    EndpointChangeCommand.Archive 
    |> tellActor (StreamId.box endpoint.Id)

  let respond = 
    OK (sprintf "Deactivating %s" (endpoint.Id.ToString ()))

  commandToActor >=> respond


let putEndpoint endpointName (dto:EndpointDto) =
  let endpoint = DAL.EndpointChange.findEndpointByName endpointName

  let commandToActor = 
    dto
    |> dtoToEndpoint
    |> EndpointChangeCommand.Update
    |> tellActor (StreamId.box endpoint.Id)

  commandToActor >=> OK "Updating endpoint..."


let deleteEndpoint name =
  let endpoint = DAL.EndpointChange.findEndpointByName name

  let commandToActor = 
    EndpointChangeCommand.Archive
    |> tellActor (StreamId.box endpoint.Id)

  let webpart = 
    endpoint.Id
    |> sprintf "Endpoint with id %A deleted"
    |> OK

  commandToActor >=> webpart







  