module ResourceManagement.Domain.ClientManagement

open System
open Common.FSharp.CommandHandlers
open Common.FSharp.Envelopes
// open ResourceManagement.Domain.DomainTypes

type ClientScope = {
    Id: Guid
    Name: string
}

type ClientDetails = { 
    Name:string
    Description:string
    }

type ClientManagementStateValue =
    | Active
    | Inactive

type ClientManagementState =
    { State:ClientManagementStateValue; Details:ClientDetails; Scopes: ClientScope list }

type ClientManagementCommand =
    | Create of ClientDetails
    | Activate 
    | Deactivate 
    | Update of ClientDetails
    | AddScope of ClientScope
    | RemoveScope of ClientScope

type ClientManagementEvent = 
    | Created of ClientDetails
    | Activated
    | Deactivated
    | Updated of ClientDetails
    | ScopeAdded of ClientScope
    | ScopeRemoved of ClientScope

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
    | Some _, ClientManagementCommand.AddScope scope -> ClientManagementEvent.ScopeAdded scope
    | None, ClientManagementCommand.AddScope _ -> failwith "Cannot add scope to non-existant client"
    | Some _, ClientManagementCommand.RemoveScope scope -> ClientManagementEvent.ScopeRemoved scope
    | None, ClientManagementCommand.RemoveScope _ -> failwith "Cannot remove scope from non-existant client"
    |> command.event

let remove scope l =
    let notMatch i = i.Id <> scope.Id
    l |> List.filter notMatch 

let evolve (state:ClientManagementState option) (event:ClientManagementEvent) =
    match state, event with 
    | None, ClientManagementEvent.Created client -> { State=Active; Details=client; Scopes = [] }
    | HasStateValue ClientManagementStateValue.Inactive st, ClientManagementEvent.Activated -> { st with State=Active }
    | HasStateValue ClientManagementStateValue.Active st, ClientManagementEvent.Deactivated -> { st with State=Inactive }
    | Some st, ClientManagementEvent.Updated details -> { st with Details=details }
    | Some st, ClientManagementEvent.ScopeAdded scope -> { st with Scopes = scope :: st.Scopes }
    | Some st, ClientManagementEvent.ScopeRemoved scope -> { st with Scopes = st.Scopes |> remove scope }
    | _, ClientManagementEvent.Created _ -> failwith "Cannot create a client which already exists"
    | _, ClientManagementEvent.Activated -> failwith "Client must exist and be inactive to activate"
    | _, ClientManagementEvent.Deactivated -> failwith "Client must exist and be active to deactivate"
    | _, ClientManagementEvent.ScopeAdded _ -> failwith "Cannot add scope to non-existant client."
    | _, ClientManagementEvent.ScopeRemoved _ -> failwith "Cannot remove scope from non-existant client."
    | None, ClientManagementEvent.Updated _ -> failwith "Cannot update a client which does not exist"



