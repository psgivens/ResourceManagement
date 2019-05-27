module ResourceManagement.Domain.DAL.ResourceManagement

open ResourceManagement.Data.Models
open Common.FSharp.Envelopes
open ResourceManagement.Domain.DomainTypes
open ResourceManagement.Domain.ResourceManagement

let defaultDT = "1/1/1900" |> System.DateTime.Parse
let persist (userId:UserId) (streamId:StreamId) (state:ResourceManagementState option) =
    use context = new ActionableDbContext () 
    let entity = context.Resources.Find (StreamId.unbox streamId)
    match entity, state with
    | null, Option.None -> ()
    | null, Option.Some(item) -> 
        let details = item.Details
        context.Resources.Add (
            ResourceEntity (
                Id = StreamId.unbox streamId,
                Name = details.Name,
                Description = details.Description
            )) |> ignore
        printfn "Persist mh: (%s)" details.Name
    | _, Option.None -> context.Resources.Remove entity |> ignore        
    | _, Some(item) -> 
        let details = item.Details
        entity.Name <- details.Name
        entity.Description <- details.Description
    context.SaveChanges () |> ignore
    
let execQuery (q:ActionableDbContext -> System.Linq.IQueryable<'a>) =
    use context = new ActionableDbContext () 
    q context
    |> Seq.toList

let getAllResources () =
    execQuery (fun ctx -> ctx.Resources :> System.Linq.IQueryable<ResourceEntity>)

let getHeadResource () =
    let getResource' (ctx:ActionableDbContext) = 
        query { 
            for resource in ctx.Resources do
            select resource
        }
    getResource'
    |> execQuery
    |> Seq.head


let find (userId:UserId) (streamId:StreamId) =
    use context = new ActionableDbContext () 
    context.Resources.Find (StreamId.unbox streamId)

let findResourceByName name =
    use context = new ActionableDbContext () 
    query { for resource in context.Resources do            
            where (resource.Name = name)
            select resource
            exactlyOne }
