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

open ResourceManagement.Domain.WidgetManagement
type WidgetManagementEventStore () =
    interface IEventStore<WidgetManagementEvent> with
        member this.GetEvents (streamId:StreamId) =
            use context = new  ActionableDbContext ()
            streamId
            |> context.GetAggregateEvents (fun i -> i.WidgetEvents) 
            |> Seq.toList 
            |> List.sortBy(fun x -> x.Version)
        member this.AppendEvent (envelope:Envelope<WidgetManagementEvent>) =
            try
                use context = new ActionableDbContext ()
                context.WidgetEvents.Add (
                    WidgetEventEnvelopeEntity (  Id = envelope.Id,
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


