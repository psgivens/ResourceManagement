module ResourceManagement.Domain.WidgetManagement

open System
open Common.FSharp.CommandHandlers
open Common.FSharp.Envelopes
// open ResourceManagement.Domain.DomainTypes

type WidgetDetails = { 
    Name:string
    Description:string
    }

type WidgetManagementStateValue =
    | Active
    | Inactive

type WidgetManagementState =
    { State:WidgetManagementStateValue; Details:WidgetDetails }

type WidgetManagementCommand =
    | Create of WidgetDetails
    | Activate 
    | Deactivate 
    | Update of WidgetDetails

type WidgetManagementEvent = 
    | Created of WidgetDetails
    | Activated
    | Deactivated
    | Updated of WidgetDetails

let (|HasStateValue|_|) expected state =
    match state with 
    | Some(value) when value.State = expected -> Some value
    | _ -> None 

let handle (command:CommandHandlers<WidgetManagementEvent, Version>) (state:WidgetManagementState option) (cmdenv:Envelope<WidgetManagementCommand>) =
    match state, cmdenv.Item with 
    | None, Create widget -> Created widget
    | _, Create _ -> failwith "Cannot create a widget which already exists"
    | HasStateValue WidgetManagementStateValue.Inactive _, WidgetManagementCommand.Activate -> WidgetManagementEvent.Activated
    | _, WidgetManagementCommand.Activate -> failwith "Widget must exist and be inactive to activate"
    | HasStateValue WidgetManagementStateValue.Active _, WidgetManagementCommand.Deactivate -> WidgetManagementEvent.Deactivated
    | _, WidgetManagementCommand.Deactivate -> failwith "Widget must exist and be active to deactivate"
    | Some _, WidgetManagementCommand.Update details -> WidgetManagementEvent.Updated details
    | None, WidgetManagementCommand.Update _ -> failwith "Cannot update a widget which does not exist"             
    |> command.event

let evolve (state:WidgetManagementState option) (event:WidgetManagementEvent) =
    match state, event with 
    | None, WidgetManagementEvent.Created widget -> { State=Active; Details=widget }
    | HasStateValue WidgetManagementStateValue.Inactive st, WidgetManagementEvent.Activated -> { st with State=Active }
    | HasStateValue WidgetManagementStateValue.Active st, WidgetManagementEvent.Deactivated -> { st with State=Inactive }
    | Some st, WidgetManagementEvent.Updated details -> { st with Details=details }
    | _, WidgetManagementEvent.Created _ -> failwith "Cannot create a widget which already exists"
    | _, WidgetManagementEvent.Activated -> failwith "Widget must exist and be inactive to activate"
    | _, WidgetManagementEvent.Deactivated -> failwith "Widget must exist and be active to deactivate"    
    | None, WidgetManagementEvent.Updated _ -> failwith "Cannot update a widget which does not exist"



