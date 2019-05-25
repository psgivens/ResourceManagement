module ResourceManagement.Domain.DAL.Database

open ResourceManagement.Data.Models


let initializeDatabase () =
    use context = new ActionableDbContext ()
    context.Database.EnsureDeleted () |> ignore
    context.Database.EnsureCreated () |> ignore
