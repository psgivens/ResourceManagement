module EndpointControlManagement.Domain.PrivilegeChange

open System
open Common.FSharp.CommandHandlers
open Common.FSharp.Envelopes

type PrivilegeActiveState = 
    | Active
    | Archived 

type PrivilegeEndpoint = { Id: Guid; Name: string }

type PrivilegeChangeState = { 
    State: PrivilegeActiveState; 
    Name:string; 
    Endpoints: PrivilegeEndpoint list }
    

type PrivilegeChangeCommand =
    | Create of string
    | Update of string
    | AddEndpoint of PrivilegeEndpoint
    | RemoveEndpoint of Guid
    | Archive

type PrivilegeChangeEvent = 
    | Created of string
    | Updated of string
    | EndpointAdded of PrivilegeEndpoint
    | EndpointRemoved of Guid
    | Archived

let private (|IsArchived|_|) state =
    match state with 
    | Some(value) when value.State = PrivilegeActiveState.Archived -> Some value
    | _ -> None 

// let remove id values =
//     values |> List.filter     

let handle (command:CommandHandlers<PrivilegeChangeEvent, Version>) (state:PrivilegeChangeState option) (cmdenv:Envelope<PrivilegeChangeCommand>) =
    let event =
        match state, cmdenv.Item with 
        | IsArchived _, cmd -> failwith <| sprintf "Cannot perform action %A on archived Privilege" cmd
        | None, Create name -> Created name
        | None, Update _ -> failwith "Cannot update a Privilege which does not exist"
        | None, Archive -> failwith "Cannot archive a Privilege which does not exist"
        | None, AddEndpoint _ -> failwith "Cannot add an Endpoint to a Privilege which does not exist"
        | None, RemoveEndpoint _ -> failwith "Cannot remove an Endpoint from a archive a Privilege which does not exist"
        | Some _, Create _ -> failwith "Cannot create a Privilege which already exists"
        | Some _, Update name -> Updated name
        | Some _, AddEndpoint endpoint -> EndpointAdded endpoint
        | Some _, RemoveEndpoint id -> EndpointRemoved id
        | Some _, Archive -> Archived

    event |> command.event

let remove id list =
    let notMatch (item:PrivilegeEndpoint) = id <> item.Id
    list |> List.filter notMatch

let evolve (state:PrivilegeChangeState option) (event:PrivilegeChangeEvent) =
    match state, event with 
    | IsArchived _, _ -> failwith <| sprintf "Cannot perform action %A on archived Privilege" event
    | None, Updated _ -> failwith "Cannot update a Privilege which does not exist"
    | None, Archived -> failwith "Cannot archive a Privilege which does not exist"
    | None, EndpointAdded _ -> failwith "Cannot add an Endpoint to a Privilege which does not exist"
    | None, EndpointRemoved _ -> failwith "Cannot remove an Endpoint from a Privilege which does not exist"
    | Some _, Created _ -> failwith "Cannot create a Privilege which already exists"
    | None, Created name -> { State=Active; Name=name; Endpoints=[] }
    | Some state', Updated name -> { state' with Name=name }
    | Some state', EndpointAdded endpoint -> { state' with Endpoints = endpoint::state'.Endpoints }
    | Some state', EndpointRemoved id -> { state' with Endpoints = state'.Endpoints |> remove id } 
    | Some state', Archived -> { state' with State=PrivilegeActiveState.Archived }

