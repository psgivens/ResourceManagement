module ResourceManagement.Domain.ResourceManagement

open System
open Common.FSharp.CommandHandlers
open Common.FSharp.Envelopes
// open ResourceManagement.Domain.DomainTypes

type ResourceDetails = { 
    Name:string
    Description:string
    Secret:string
    }

type ResourceManagementStateValue =
    | Active
    | Inactive

type ResourceManagementState =
    { State:ResourceManagementStateValue; Details:ResourceDetails }

type ResourceManagementCommand =
    | Create of ResourceDetails
    | Activate 
    | Deactivate 
    | Update of ResourceDetails

type ResourceManagementEvent = 
    | Created of ResourceDetails
    | Activated
    | Deactivated
    | Updated of ResourceDetails

let (|HasStateValue|_|) expected state =
    match state with 
    | Some(value) when value.State = expected -> Some value
    | _ -> None 

let handle (command:CommandHandlers<ResourceManagementEvent, Version>) (state:ResourceManagementState option) (cmdenv:Envelope<ResourceManagementCommand>) =
    match state, cmdenv.Item with 
    | None, Create resource -> Created resource
    | _, Create _ -> failwith "Cannot create a resource which already exists"
    | HasStateValue ResourceManagementStateValue.Inactive _, ResourceManagementCommand.Activate -> ResourceManagementEvent.Activated
    | _, ResourceManagementCommand.Activate -> failwith "Resource must exist and be inactive to activate"
    | HasStateValue ResourceManagementStateValue.Active _, ResourceManagementCommand.Deactivate -> ResourceManagementEvent.Deactivated
    | _, ResourceManagementCommand.Deactivate -> failwith "Resource must exist and be active to deactivate"
    | Some _, ResourceManagementCommand.Update details -> ResourceManagementEvent.Updated details
    | None, ResourceManagementCommand.Update _ -> failwith "Cannot update a resource which does not exist"             
    |> command.event

let evolve (state:ResourceManagementState option) (event:ResourceManagementEvent) =
    match state, event with 
    | None, ResourceManagementEvent.Created resource -> { State=Active; Details=resource }
    | HasStateValue ResourceManagementStateValue.Inactive st, ResourceManagementEvent.Activated -> { st with State=Active }
    | HasStateValue ResourceManagementStateValue.Active st, ResourceManagementEvent.Deactivated -> { st with State=Inactive }
    | Some st, ResourceManagementEvent.Updated details -> { st with Details=details }
    | _, ResourceManagementEvent.Created _ -> failwith "Cannot create a resource which already exists"
    | _, ResourceManagementEvent.Activated -> failwith "Resource must exist and be inactive to activate"
    | _, ResourceManagementEvent.Deactivated -> failwith "Resource must exist and be active to deactivate"    
    | None, ResourceManagementEvent.Updated _ -> failwith "Cannot update a resource which does not exist"



