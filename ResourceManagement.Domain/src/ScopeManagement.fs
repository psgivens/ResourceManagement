module ResourceManagement.Domain.ScopeManagement

open System
open Common.FSharp.CommandHandlers
open Common.FSharp.Envelopes
// open ResourceManagement.Domain.DomainTypes

type ScopeDetails = { 
    Name:string
    Description:string
    }

type ScopeManagementStateValue =
    | Active
    | Inactive

type ScopeManagementState =
    { State:ScopeManagementStateValue; Details:ScopeDetails }

type ScopeManagementCommand =
    | Create of ScopeDetails
    | Activate 
    | Deactivate 
    | Update of ScopeDetails

type ScopeManagementEvent = 
    | Created of ScopeDetails
    | Activated
    | Deactivated
    | Updated of ScopeDetails

let (|HasStateValue|_|) expected state =
    match state with 
    | Some(value) when value.State = expected -> Some value
    | _ -> None 

let handle (command:CommandHandlers<ScopeManagementEvent, Version>) (state:ScopeManagementState option) (cmdenv:Envelope<ScopeManagementCommand>) =
    match state, cmdenv.Item with 
    | None, Create scope -> Created scope
    | _, Create _ -> failwith "Cannot create a scope which already exists"
    | HasStateValue ScopeManagementStateValue.Inactive _, ScopeManagementCommand.Activate -> ScopeManagementEvent.Activated
    | _, ScopeManagementCommand.Activate -> failwith "Scope must exist and be inactive to activate"
    | HasStateValue ScopeManagementStateValue.Active _, ScopeManagementCommand.Deactivate -> ScopeManagementEvent.Deactivated
    | _, ScopeManagementCommand.Deactivate -> failwith "Scope must exist and be active to deactivate"
    | Some _, ScopeManagementCommand.Update details -> ScopeManagementEvent.Updated details
    | None, ScopeManagementCommand.Update _ -> failwith "Cannot update a scope which does not exist"             
    |> command.event

let evolve (state:ScopeManagementState option) (event:ScopeManagementEvent) =
    match state, event with 
    | None, ScopeManagementEvent.Created scope -> { State=Active; Details=scope }
    | HasStateValue ScopeManagementStateValue.Inactive st, ScopeManagementEvent.Activated -> { st with State=Active }
    | HasStateValue ScopeManagementStateValue.Active st, ScopeManagementEvent.Deactivated -> { st with State=Inactive }
    | Some st, ScopeManagementEvent.Updated details -> { st with Details=details }
    | _, ScopeManagementEvent.Created _ -> failwith "Cannot create a scope which already exists"
    | _, ScopeManagementEvent.Activated -> failwith "Scope must exist and be inactive to activate"
    | _, ScopeManagementEvent.Deactivated -> failwith "Scope must exist and be active to deactivate"    
    | None, ScopeManagementEvent.Updated _ -> failwith "Cannot update a scope which does not exist"



