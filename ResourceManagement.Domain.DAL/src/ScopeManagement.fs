module ResourceManagement.Domain.DAL.ScopeManagement

open ResourceManagement.Data.Models
open Common.FSharp.Envelopes
open ResourceManagement.Domain.DomainTypes
open ResourceManagement.Domain.ScopeManagement

let defaultDT = "1/1/1900" |> System.DateTime.Parse
let persist (userId:UserId) (streamId:StreamId) (state:ScopeManagementState option) =
    use context = new ActionableDbContext () 
    let entity = context.Scopes.Find (StreamId.unbox streamId)
    match entity, state with
    | null, Option.None -> ()
    | null, Option.Some(item) -> 
        let details = item.Details
        context.Scopes.Add (
            ScopeEntity (
                Id = StreamId.unbox streamId,
                Name = details.Name,
                Description = details.Description
            )) |> ignore
        printfn "Persist mh: (%s)" details.Name
    | _, Option.None -> context.Scopes.Remove entity |> ignore        
    | _, Some(item) -> 
        let details = item.Details
        entity.Name <- details.Name
        entity.Description <- details.Description
    context.SaveChanges () |> ignore
    
let execQuery (q:ActionableDbContext -> System.Linq.IQueryable<'a>) =
    use context = new ActionableDbContext () 
    q context
    |> Seq.toList

let getAllScopes () =
    execQuery (fun ctx -> ctx.Scopes :> System.Linq.IQueryable<ScopeEntity>)

let getHeadScope () =
    let getScope' (ctx:ActionableDbContext) = 
        query { 
            for scope in ctx.Scopes do
            select scope
        }
    getScope'
    |> execQuery
    |> Seq.head


let find (userId:UserId) (streamId:StreamId) =
    use context = new ActionableDbContext () 
    context.Scopes.Find (StreamId.unbox streamId)

let findScopeByName name =
    use context = new ActionableDbContext () 
    query { for scope in context.Scopes do            
            where (scope.Name = name)
            select scope
            exactlyOne }
