module ResourceManagement.Domain.DAL.EndpointChange

open ResourceManagement.Data.Models
open Common.FSharp.Envelopes
open ResourceManagement.Domain.DomainTypes
open ResourceManagement.Domain.EndpointChange

let defaultDT = "1/1/1900" |> System.DateTime.Parse
let persist (userId:UserId) (streamId:StreamId) (state:EndpointChangeState option) =
    use context = new ActionableDbContext () 
    let entity = context.Endpoints.Find (StreamId.unbox streamId)
    match entity, state with
    | null, Option.None -> ()
    | null, Option.Some(item) -> 
        let details = item.Endpoint
        context.Endpoints.Add (
            Endpoint (
                Id = StreamId.unbox streamId,
                Url = details.Url,
                Method = details.Method.ToString ()
            )) |> ignore
        printfn "Persist mh: (%A:%s)" details.Method details.Url
    | _, Option.None -> context.Endpoints.Remove entity |> ignore        
    | _, Some(item) -> 
        let details = item.Endpoint
        entity.Url <- details.Url
        entity.Method <- details.Method.ToString ()
    context.SaveChanges () |> ignore
    
let execQuery (q:ActionableDbContext -> System.Linq.IQueryable<'a>) =
    use context = new ActionableDbContext () 
    q context
    |> Seq.toList

let getAllEndpoints () =
    execQuery (fun ctx -> ctx.Endpoints :> System.Linq.IQueryable<Endpoint>)

let getHeadUser () =
    let getUser' (ctx:ActionableDbContext) = 
        query { 
            for user in ctx.Endpoints do
            select user
        }
    getUser'
    |> execQuery
    |> Seq.head


let find (userId:UserId) (streamId:StreamId) =
    use context = new ActionableDbContext () 
    context.Endpoints.Find (StreamId.unbox streamId)

let findEndpointByDetails (method, url) =
    use context = new ActionableDbContext () 
    query { for endpoint in context.Endpoints do            
            where (endpoint.Url = url && endpoint.Method = method)
            select endpoint
            exactlyOne }

let findEndpointByName name =
    use context = new ActionableDbContext () 
    query { for endpoint in context.Endpoints do            
            where (endpoint.Name = name)
            select endpoint
            exactlyOne }
