module ResourceManagement.Domain.EndpointChange

open System
open Common.FSharp.CommandHandlers
open Common.FSharp.Envelopes

type EndpointActiveState = 
    | Active
    | Archived 

type EndpointMethod = 
    | GET
    | POST
    | DELETE
    | PUT

type EndpointDetails = {
    Name: string
    Url: string
    Method: EndpointMethod
}

type EndpointDataConstraint = { Pattern:string }

type EndpointChangeState = { 
    State: EndpointActiveState; 
    Endpoint: EndpointDetails 
    DataConstraints: EndpointDataConstraint list
    }

type EndpointChangeCommand =
    | Create of EndpointDetails
    | Update of EndpointDetails
    | AddConstraint of EndpointDataConstraint
    | RemoveConstraint of EndpointDataConstraint
    | Archive

type EndpointChangeEvent = 
    | Created of EndpointDetails
    | Updated of EndpointDetails
    | ConstraintAdded of EndpointDataConstraint
    | ConstraintRemoved of EndpointDataConstraint
    | Archived

let private (|IsArchived|_|) state =
    match state with 
    | Some(value) when value.State = EndpointActiveState.Archived -> Some value
    | _ -> None 

let handle (command:CommandHandlers<EndpointChangeEvent, Version>) (state:EndpointChangeState option) (cmdenv:Envelope<EndpointChangeCommand>) =    
    match state, cmdenv.Item with 
    | IsArchived _, cmd -> failwith <| sprintf "Cannot perform action %A on archived Endpoint" cmd
    | None, Update _ -> failwith "Cannot update a Endpoint which does not exist"
    | None, Archive -> failwith "Cannot archive a Endpoint which does not exist"
    | None, AddConstraint _ -> failwith "Cannot add contraint to Endpoint which does not exist"
    | None, RemoveConstraint _ -> failwith "Cannot remove contraint from Endpoint which does not exist"
    | Some _, Create _ -> failwith "Cannot create a Endpoint which already exists"
    | None, Create details -> Created details |> command.event
    | Some _, Update details -> Updated details |> command.event
    | Some _, AddConstraint c -> ConstraintAdded c |> command.event
    | Some _, RemoveConstraint c -> ConstraintRemoved c |> command.event
    | Some _, Archive -> Archived |> command.event

let remove item l =
    let notMatch i = i <> item
    l |> List.filter notMatch 

let evolve (state:EndpointChangeState option) (event:EndpointChangeEvent) =
    match state, event with 
    | IsArchived _, _ -> failwith <| sprintf "Cannot perform action %A on archived Endpoint" event
    | None, Updated _ -> failwith "Cannot update a Endpoint which does not exist"
    | None, Archived -> failwith "Cannot archive a Endpoint which does not exist"
    | None, ConstraintAdded _ -> failwith "Cannot add a constraint to an Endpoint which does not exist"
    | None, ConstraintRemoved _ -> failwith "Cannot remove a constraint from an Endpoint which does not exist"
    | Some _, Created _ -> failwith "Cannot create a Endpoint which already exists"
    | None, Created details -> { State=Active; Endpoint=details; DataConstraints=[] }
    | Some state', Updated details -> { state' with Endpoint=details }
    | Some state', ConstraintAdded c -> { state' with DataConstraints = c::state'.DataConstraints }
    | Some state', ConstraintRemoved c -> { state' with DataConstraints = state'.DataConstraints |> remove c }
    | Some state', Archived -> { state' with State=EndpointActiveState.Archived }

