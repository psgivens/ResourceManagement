module ResourceManagement.Api.WidgetCommands

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful

open Common.FSharp.Suave
open Common.FSharp.Envelopes

open ResourceManagement.Domain.WidgetManagement
open ResourceManagement.Domain
open ResourceManagement.Data.Models

open ResourceManagement.Api.ProcessingSystem
open ResourceManagement.Api.Dtos

// http://blog.tamizhvendan.in/blog/2015/06/11/building-rest-api-in-fsharp-using-suave/

let private dtoToWidget dto = 
  { 
      Name=dto.name
      Description=dto.description
  }

let private tellActor = sendEnvelope actorGroups.WidgetManagementActors.Tell 

let postWidget (dto:WidgetDto)=  
  let newUserId = StreamId.create ()

  let commandToActor = 
    dto
    |> dtoToWidget    
    |> WidgetManagementCommand.Create
    |> tellActor newUserId

  let respond = newUserId |> toJson |> OK

  commandToActor >=> respond


let deactivateWidget (widget:Widget) = 
  let commandToActor = 
    WidgetManagementCommand.Deactivate 
    |> tellActor (StreamId.box widget.Id)

  let respond = 
    OK (sprintf "Deactivating %s" (widget.Id.ToString ()))

  commandToActor >=> respond


let putWidget name (dto:WidgetDto) =
  let widget = DAL.WidgetManagement.findWidgetByName name

  let commandToActor = 
    dto
    |> dtoToWidget
    |> WidgetManagementCommand.Update
    |> tellActor (StreamId.box widget.Id)

  commandToActor >=> OK "Updating widget..."


let deleteWidget name =
  let widget = DAL.WidgetManagement.findWidgetByName name

  let commandToActor = 
    WidgetManagementCommand.Deactivate
    |> tellActor (StreamId.box widget.Id)

  let webpart = 
    widget.Id
    |> sprintf "Widget with id %A deleted"
    |> OK

  commandToActor >=> webpart







  