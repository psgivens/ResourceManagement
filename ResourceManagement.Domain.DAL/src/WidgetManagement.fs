module ResourceManagement.Domain.DAL.WidgetManagement

open ResourceManagement.Data.Models
open Common.FSharp.Envelopes
open ResourceManagement.Domain.DomainTypes
open ResourceManagement.Domain.WidgetManagement

let defaultDT = "1/1/1900" |> System.DateTime.Parse
let persist (userId:UserId) (streamId:StreamId) (state:WidgetManagementState option) =
    use context = new ActionableDbContext () 
    let entity = context.Widgets.Find (StreamId.unbox streamId)
    match entity, state with
    | null, Option.None -> ()
    | null, Option.Some(item) -> 
        let details = item.Details
        context.Widgets.Add (
            Widget (
                Id = StreamId.unbox streamId,
                Name = details.Name,
                Description = details.Description
            )) |> ignore
        printfn "Persist mh: (%s)" details.Name
    | _, Option.None -> context.Widgets.Remove entity |> ignore        
    | _, Some(item) -> 
        let details = item.Details
        entity.Name <- details.Name
        entity.Description <- details.Description
    context.SaveChanges () |> ignore
    
let execQuery (q:ActionableDbContext -> System.Linq.IQueryable<'a>) =
    use context = new ActionableDbContext () 
    q context
    |> Seq.toList

let getAllWidgets () =
    execQuery (fun ctx -> ctx.Widgets :> System.Linq.IQueryable<Widget>)

let getHeadWidget () =
    let getWidget' (ctx:ActionableDbContext) = 
        query { 
            for widget in ctx.Widgets do
            select widget
        }
    getWidget'
    |> execQuery
    |> Seq.head


let find (userId:UserId) (streamId:StreamId) =
    use context = new ActionableDbContext () 
    context.Widgets.Find (StreamId.unbox streamId)

let findWidgetByName name =
    use context = new ActionableDbContext () 
    query { for widget in context.Widgets do            
            where (widget.Name = name)
            select widget
            exactlyOne }
