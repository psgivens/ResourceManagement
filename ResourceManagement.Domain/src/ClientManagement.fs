module ResourceManagement.Domain.ClientManagement

open System
open Common.FSharp.CommandHandlers
open Common.FSharp.Envelopes
// open ResourceManagement.Domain.DomainTypes

type ClientDetails = { 
    Name:string
    Description:string
    }

type ClientManagementStateValue =
    | Active
    | Inactive

type ClientManagementState =
    { State:ClientManagementStateValue; Details:ClientDetails }

type ClientManagementCommand =
    | Create of ClientDetails
    | Activate 
    | Deactivate 
    | Update of ClientDetails

type ClientManagementEvent = 
    | Created of ClientDetails
    | Activated
    | Deactivated
    | Updated of ClientDetails

let (|HasStateValue|_|) expected state =
    match state with 
    | Some(value) when value.State = expected -> Some value
    | _ -> None 

let handle (command:CommandHandlers<ClientManagementEvent, Version>) (state:ClientManagementState option) (cmdenv:Envelope<ClientManagementCommand>) =
    match state, cmdenv.Item with 
    | None, Create client -> Created client
    | _, Create _ -> failwith "Cannot create a client which already exists"
    | HasStateValue ClientManagementStateValue.Inactive _, ClientManagementCommand.Activate -> ClientManagementEvent.Activated
    | _, ClientManagementCommand.Activate -> failwith "Client must exist and be inactive to activate"
    | HasStateValue ClientManagementStateValue.Active _, ClientManagementCommand.Deactivate -> ClientManagementEvent.Deactivated
    | _, ClientManagementCommand.Deactivate -> failwith "Client must exist and be active to deactivate"
    | Some _, ClientManagementCommand.Update details -> ClientManagementEvent.Updated details
    | None, ClientManagementCommand.Update _ -> failwith "Cannot update a client which does not exist"             
    |> command.event

let evolve (state:ClientManagementState option) (event:ClientManagementEvent) =
    match state, event with 
    | None, ClientManagementEvent.Created client -> { State=Active; Details=client }
    | HasStateValue ClientManagementStateValue.Inactive st, ClientManagementEvent.Activated -> { st with State=Active }
    | HasStateValue ClientManagementStateValue.Active st, ClientManagementEvent.Deactivated -> { st with State=Inactive }
    | Some st, ClientManagementEvent.Updated details -> { st with Details=details }
    | _, ClientManagementEvent.Created _ -> failwith "Cannot create a client which already exists"
    | _, ClientManagementEvent.Activated -> failwith "Client must exist and be inactive to activate"
    | _, ClientManagementEvent.Deactivated -> failwith "Client must exist and be active to deactivate"    
    | None, ClientManagementEvent.Updated _ -> failwith "Cannot update a client which does not exist"



