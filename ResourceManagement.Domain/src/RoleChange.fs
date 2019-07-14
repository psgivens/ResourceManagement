module EndpointControlManagement.Domain.RoleChange

open System
open Common.FSharp.CommandHandlers
open Common.FSharp.Envelopes

type RoleActiveState = 
    | Active
    | Archived 

type RolePrivilege = { Id: Guid; Name: string }

type RoleChangeState = { 
    State: RoleActiveState; 
    Name:string; 
    Privileges:RolePrivilege list 
    }

type RoleChangeCommand =
    | Create of string
    | Update of string
    | AddPrivilege of RolePrivilege
    | RemovePrivilege of Guid
    | Archive

type RoleChangeEvent = 
    | Created of string
    | Updated of string
    | PrivilegeAdded of RolePrivilege
    | PrivilegeRemoved of Guid
    | Archived

let private (|IsArchived|_|) state =
    match state with 
    | Some(value) when value.State = RoleActiveState.Archived -> Some value
    | _ -> None 

let handle (command:CommandHandlers<RoleChangeEvent, Version>) (state:RoleChangeState option) (cmdenv:Envelope<RoleChangeCommand>) =
    let event =
        match state, cmdenv.Item with 
        | IsArchived _, cmd -> failwith <| sprintf "Cannot perform action %A on archived role" cmd
        | None, Update _ -> failwith "Cannot update a role which does not exist"
        | None, Archive -> failwith "Cannot archive a role which does not exist"
        | None, AddPrivilege _ -> failwith "Cannot add a privilege to a role which does not exist"
        | None, RemovePrivilege _ -> failwith "Cannot remvoe a privilege from a role which does not exist"
        | Some _, Create _ -> failwith "Cannot create a role which already exists"
        | None, Create name -> Created name
        | Some _, Update name -> Updated name
        | Some _, AddPrivilege privilege -> PrivilegeAdded privilege
        | Some _, RemovePrivilege id -> PrivilegeRemoved id
        | Some _, Archive -> Archived

    event |> command.event

let remove id list =
    let notMatch (item:RolePrivilege) = id <> item.Id
    list |> List.filter notMatch

let evolve (state:RoleChangeState option) (event:RoleChangeEvent) =
    match state, event with 
    | IsArchived _, _ -> failwith <| sprintf "Cannot perform action %A on archived role" event
    | None, Updated _ -> failwith "Cannot update a role which does not exist"
    | None, Archived -> failwith "Cannot archive a role which does not exist"
    | None, PrivilegeAdded _ -> failwith "Cannot add a privilege to a Role which does not exist"
    | None, PrivilegeRemoved _ -> failwith "Cannot remove a privilege from a Role which does not exist"
    | Some _, Created _ -> failwith "Cannot create a role which already exists"
    | None, Created name -> { State=Active; Name=name; Privileges=[] }
    | Some state', Updated name -> { state' with Name=name }
    | Some state', PrivilegeAdded privilege -> { state' with Privileges = privilege::state'.Privileges }
    | Some state', PrivilegeRemoved id -> { state' with Privileges = state'.Privileges |> remove id }
    | Some state', Archived -> { state' with State=RoleActiveState.Archived }

