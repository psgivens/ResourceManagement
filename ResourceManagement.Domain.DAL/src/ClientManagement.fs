module ResourceManagement.Domain.DAL.ClientManagement

open ResourceManagement.Data.Models
open Common.FSharp.Envelopes
open ResourceManagement.Domain.DomainTypes
open ResourceManagement.Domain.ClientManagement

let defaultDT = "1/1/1900" |> System.DateTime.Parse
let persist (userId:UserId) (streamId:StreamId) (state:ClientManagementState option) =
    use context = new ActionableDbContext () 
    let entity = context.Clients.Find (StreamId.unbox streamId)
    match entity, state with
    | null, Option.None -> ()
    | null, Option.Some(item) -> 
        let details = item.Details
        context.Clients.Add (
            ClientEntity (
                Id = StreamId.unbox streamId,
                Name = details.Name,
                Description = details.Description
            )) |> ignore
        printfn "Persist mh: (%s)" details.Name
    | _, Option.None -> context.Clients.Remove entity |> ignore        
    | _, Some(item) -> 
        let details = item.Details
        entity.Name <- details.Name
        entity.Description <- details.Description
    context.SaveChanges () |> ignore
    
let execQuery (q:ActionableDbContext -> System.Linq.IQueryable<'a>) =
    use context = new ActionableDbContext () 
    q context
    |> Seq.toList

let getAllClients () =
    execQuery (fun ctx -> ctx.Clients :> System.Linq.IQueryable<ClientEntity>)

let getHeadClient () =
    let getClient' (ctx:ActionableDbContext) = 
        query { 
            for client in ctx.Clients do
            select client
        }
    getClient'
    |> execQuery
    |> Seq.head


let find (userId:UserId) (streamId:StreamId) =
    use context = new ActionableDbContext () 
    context.Clients.Find (StreamId.unbox streamId)

let findClientByName name =
    use context = new ActionableDbContext () 
    query { for client in context.Clients do            
            where (client.Name = name)
            select client
            exactlyOne }
