module ResourceManagement.Api.ProcessingSystem

open System
open Akka.Actor
open Akka.FSharp

open ResourceManagement.Domain
open Common.FSharp.Envelopes
open ResourceManagement.Domain.DomainTypes
open ResourceManagement.Domain.ClientManagement
// open ResourceManagement.Domain.WidgetManagement
// open ResourceManagement.Domain.WidgetManagement
open ResourceManagement.Domain
open Common.FSharp.Actors

open ResourceManagement.Domain.DAL.ResourceManagementEventStore
open Common.FSharp.Actors.Infrastructure

open ResourceManagement.Domain.DAL.Database
open Akka.Dispatch.SysMsg
open Common.FSharp
open ResourceManagement.Domain.EndpointChange

open Suave
open Common.FSharp.Suave

type ActorGroups = {
    ClientManagementActors:ActorIO<ClientManagementCommand>
    EndpointChangeActors:ActorIO<EndpointChangeCommand>

    // WidgetManagementActors:ActorIO<WidgetManagementCommand>
    // WidgetManagementActors:ActorIO<WidgetManagementCommand>
    }

let composeActors system =
    // Create member management actors
    let endpointChangeActors = 
        EventSourcingActors.spawn 
            (system,
             "EndpointChange", 
             EndpointChangeEventStore (),
             buildState EndpointChange.evolve,
             EndpointChange.handle,
             DAL.EndpointChange.persist)   

    let clientManagementActors = 
        EventSourcingActors.spawn 
            (system,
             "clientManagement", 
             ClientManagementEventStore (),
             buildState ClientManagement.evolve,
             ClientManagement.handle,
             DAL.ClientManagement.persist)    
             
    // let widgetManagementActors = 
    //     EventSourcingActors.spawn 
    //         (system,
    //          "widgetManagement", 
    //          WidgetManagementEventStore (),
    //          buildState WidgetManagement.evolve,
    //          WidgetManagement.handle,
    //          DAL.WidgetManagement.persist)    
             
    // let widgetManagementActors = 
    //     EventSourcingActors.spawn 
    //         (system,
    //          "widgetManagement", 
    //          WidgetManagementEventStore (),
    //          buildState WidgetManagement.evolve,
    //          WidgetManagement.handle,
    //          DAL.WidgetManagement.persist)    
             
    { ClientManagementActors=clientManagementActors 
      EndpointChangeActors=endpointChangeActors
      // WidgetManagementActors=widgetManagementActors 
      // WidgetManagementActors=widgetManagementActors 
      }


let initialize () = 
    printfn "Resolve newtonsoft..."

    // System set up
    NewtonsoftHack.resolveNewtonsoft ()  

    printfn "Creating a new database..."
    initializeDatabase ()
    
    let system = Configuration.defaultConfig () |> System.create "sample-system"
            
    printfn "Composing the actors..."
    let actorGroups = composeActors system

    let clientCommandRequestReplyCanceled = 
      RequestReplyActor.spawnRequestReplyActor<ClientManagementCommand, ClientManagementEvent> 
        system "widget_management_command" actorGroups.ClientManagementActors

    // let widgetCommandRequestReplyCanceled = 
    //   RequestReplyActor.spawnRequestReplyActor<WidgetManagementCommand, WidgetManagementEvent> 
    //     system "widget_management_command" actorGroups.WidgetManagementActors
    // let widgetCommandRequestReplyCanceled = 
    //   RequestReplyActor.spawnRequestReplyActor<WidgetManagementCommand, WidgetManagementEvent> 
    //     system "widget_management_command" actorGroups.WidgetManagementActors

    let runWaitAndIgnore = 
      Async.AwaitTask
      >> Async.Ignore
      >> Async.RunSynchronously

    let userId = UserId.create ()
    let envelop streamId = envelopWithDefaults userId (TransId.create ()) streamId

    printfn "Creating widget..."
    { 
        Name="Spacely Sprocket"
        Description="Important sprocket for creating floating houses and cars."
    }
    |> ClientManagementCommand.Create
    |> envelop (StreamId.create ())
    |> clientCommandRequestReplyCanceled.Ask
    |> runWaitAndIgnore

    let client = ResourceManagement.Domain.DAL.ClientManagement.findClientByName "Spacely Sprocket"
    printfn "Created Client %s with userId %A" client.Name client.Id         

    let endpointCommandRequestReplyCanceled = 
      RequestReplyActor.spawnRequestReplyActor<EndpointChangeCommand, EndpointChangeEvent> 
        system "endpoint_management_command" actorGroups.EndpointChangeActors

    let runWaitAndIgnore = 
      Async.AwaitTask
      >> Async.Ignore
      >> Async.RunSynchronously

    let userId = UserId.create ()
    let envelop streamId = envelopWithDefaults userId (TransId.create ()) streamId

    printfn "Creating endpoint..."
    { 
      EndpointDetails.Url = "/sample/url"
      EndpointDetails.Method = EndpointMethod.GET
      EndpointDetails.Name = "sample url"
    }
    |> EndpointChangeCommand.Create
    |> envelop (StreamId.create ())
    |> endpointCommandRequestReplyCanceled.Ask
    |> runWaitAndIgnore

    let endpoint = ResourceManagement.Domain.DAL.EndpointChange.findEndpointByName "sample url"
    printfn "Created Endpoint %s with endpointId %A" endpoint.Name endpoint.Id         

    actorGroups

let actorGroups = initialize ()


type DomainContext = {
  UserId: UserId
  TransId: TransId
}

let inline private addContext (item:DomainContext) (ctx:HttpContext) = 
  { ctx with userState = ctx.userState |> Map.add "domain_context" (box item) }

let inline private getDomainContext (ctx:HttpContext) :DomainContext =
  ctx.userState |> Map.find "domain_context" :?> DomainContext

let authenticationHeaders (p:HttpRequest) = 
  let h = 
    ["user_id"; "transaction_id"]
    |> List.map (p.header >> Option.ofChoice)

  match h with
  | [Some userId; Some transId] -> 
    let (us, uid) = userId |> Guid.TryParse
    let (ut, tid) = transId |> Guid.TryParse
    if us && ut then 
      addContext { 
          UserId = UserId.box uid; 
          TransId = TransId.box tid 
      } 
      >> Some 
      >> async.Return
    else noMatch
  | _ -> noMatch

let envelopWithDefaults (ctx:HttpContext) = 
  let domainContext = getDomainContext ctx
  Common.FSharp.Envelopes.Envelope.envelopWithDefaults
    domainContext.UserId
    domainContext.TransId

let sendEnvelope<'a> (tell:Tell<'a>) (streamId:StreamId) (cmd:'a) (ctx:HttpContext) = 
  cmd
  |> envelopWithDefaults ctx streamId
  |> tell
  
  ctx |> Some |> async.Return 
