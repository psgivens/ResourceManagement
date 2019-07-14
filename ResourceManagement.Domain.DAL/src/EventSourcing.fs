module ResourceManagement.Domain.DAL.ResourceManagementEventStore
open ResourceManagement.Data.Models
open Common.FSharp.Envelopes
open Newtonsoft.Json
open Microsoft.EntityFrameworkCore


type ActionableDbContext with 
    member this.GetAggregateEvents<'a,'b when 'b :> EnvelopeEntityBase and 'b: not struct>
        (dbset:ActionableDbContext->DbSet<'b>)
        (StreamId.Id (aggregateId):StreamId)
        :seq<Envelope<'a>>= 
        query {
            for event in this |> dbset do
            where (event.StreamId = aggregateId)
            select event
        } |> Seq.map (fun event ->
            {
                Id = event.Id
                UserId = UserId.box event.UserId
                StreamId = StreamId.box aggregateId
                TransactionId = TransId.box event.TransactionId
                Version = Version.box (event.Version)
                Created = event.TimeStamp
                Item = (JsonConvert.DeserializeObject<'a> event.Event)
            })

open ResourceManagement.Domain.ClientManagement
type ClientManagementEventStore () =
    interface IEventStore<ClientManagementEvent> with
        member this.GetEvents (streamId:StreamId) =
            use context = new  ActionableDbContext ()
            streamId
            |> context.GetAggregateEvents (fun i -> i.ClientEvents) 
            |> Seq.toList 
            |> List.sortBy(fun x -> x.Version)
        member this.AppendEvent (envelope:Envelope<ClientManagementEvent>) =
            try
                use context = new ActionableDbContext ()
                context.ClientEvents.Add (
                    ClientEventEnvelopeEntity (  Id = envelope.Id,
                                            StreamId = StreamId.unbox envelope.StreamId,
                                            UserId = UserId.unbox envelope.UserId,
                                            TransactionId = TransId.unbox envelope.TransactionId,
                                            Version = Version.unbox envelope.Version,
                                            TimeStamp = envelope.Created,
                                            Event = JsonConvert.SerializeObject(envelope.Item)
                                            )) |> ignore         
                context.SaveChanges () |> ignore
                
            with
                | ex -> System.Diagnostics.Debugger.Break () 


open ResourceManagement.Domain.ScopeManagement
type ScopeManagementEventStore () =
    interface IEventStore<ScopeManagementEvent> with
        member this.GetEvents (streamId:StreamId) =
            use context = new  ActionableDbContext ()
            streamId
            |> context.GetAggregateEvents (fun i -> i.ScopeEvents) 
            |> Seq.toList 
            |> List.sortBy(fun x -> x.Version)
        member this.AppendEvent (envelope:Envelope<ScopeManagementEvent>) =
            try
                use context = new ActionableDbContext ()
                context.ScopeEvents.Add (
                    ScopeEventEnvelopeEntity (  Id = envelope.Id,
                                            StreamId = StreamId.unbox envelope.StreamId,
                                            UserId = UserId.unbox envelope.UserId,
                                            TransactionId = TransId.unbox envelope.TransactionId,
                                            Version = Version.unbox envelope.Version,
                                            TimeStamp = envelope.Created,
                                            Event = JsonConvert.SerializeObject(envelope.Item)
                                            )) |> ignore         
                context.SaveChanges () |> ignore
                
            with
                | ex -> System.Diagnostics.Debugger.Break () 


open ResourceManagement.Domain.ResourceManagement
type ResourceManagementEventStore () =
    interface IEventStore<ResourceManagementEvent> with
        member this.GetEvents (streamId:StreamId) =
            use context = new  ActionableDbContext ()
            streamId
            |> context.GetAggregateEvents (fun i -> i.ResourceEvents) 
            |> Seq.toList 
            |> List.sortBy(fun x -> x.Version)
        member this.AppendEvent (envelope:Envelope<ResourceManagementEvent>) =
            try
                use context = new ActionableDbContext ()
                context.ResourceEvents.Add (
                    ResourceEventEnvelopeEntity (  Id = envelope.Id,
                                            StreamId = StreamId.unbox envelope.StreamId,
                                            UserId = UserId.unbox envelope.UserId,
                                            TransactionId = TransId.unbox envelope.TransactionId,
                                            Version = Version.unbox envelope.Version,
                                            TimeStamp = envelope.Created,
                                            Event = JsonConvert.SerializeObject(envelope.Item)
                                            )) |> ignore         
                context.SaveChanges () |> ignore
                
            with
                | ex -> System.Diagnostics.Debugger.Break () 


open ResourceManagement.Domain.EndpointChange
type EndpointChangeEventStore () =
    interface IEventStore<EndpointChangeEvent> with
        member this.GetEvents (streamId:StreamId) =
            use context = new  ActionableDbContext ()
            streamId
            |> context.GetAggregateEvents (fun i -> i.EndpointEvents) 
            |> Seq.toList 
            |> List.sortBy(fun x -> x.Version)
        member this.AppendEvent (envelope:Envelope<EndpointChangeEvent>) =
            try
                use context = new ActionableDbContext ()
                context.EndpointEvents.Add (
                    EndpointEventEnvelopeEntity (  Id = envelope.Id,
                                            StreamId = StreamId.unbox envelope.StreamId,
                                            UserId = UserId.unbox envelope.UserId,
                                            TransactionId = TransId.unbox envelope.TransactionId,
                                            Version = Version.unbox envelope.Version,
                                            TimeStamp = envelope.Created,
                                            Event = JsonConvert.SerializeObject(envelope.Item)
                                            )) |> ignore         
                context.SaveChanges () |> ignore
                
            with
                // TODO: Replace the debugger break with an exception
                | ex -> System.Diagnostics.Debugger.Break () 


