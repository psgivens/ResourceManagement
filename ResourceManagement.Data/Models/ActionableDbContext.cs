using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ResourceManagement.Data.Models {
    public class ActionableDbContext : DbContext {
        public ActionableDbContext (DbContextOptions<ActionableDbContext> options) : base (options) { }
        public ActionableDbContext () { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        { 
            // FIXME: Pull this information from somewhere else
            // https://stackoverflow.com/questions/52156484/how-exactly-does-microsoft-extensions-configuration-dependent-on-asp-net-core
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.1
            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.jsonconfigurationextensions.addjsonfile?view=aspnetcore-2.2#Microsoft_Extensions_Configuration_JsonConfigurationExtensions_AddJsonFile_Microsoft_Extensions_Configuration_IConfigurationBuilder_System_String_
            var connectionString = "Widget ID=samplesam;Password=Password1;Host=pomodoro-pgsql;Port=5432;Database=ResourceManagementDb;Pooling=true;";
            optionsBuilder.UseNpgsql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<ClientEventEnvelopeEntity>()
                .HasKey(e => new { e.StreamId, e.UserId, e.Id });
            modelBuilder
                .Entity<ScopeEventEnvelopeEntity>()
                .HasKey(e => new { e.StreamId, e.UserId, e.Id });
            modelBuilder
                .Entity<ResourceEventEnvelopeEntity>()
                .HasKey(e => new { e.StreamId, e.UserId, e.Id });

            modelBuilder
                .Entity<ClientScopeMap>()
                .HasKey(e => new { e.ClientId, e.ScopeId });

            // modelBuilder
            //     .Entity<RolePrincipalMap>()
            //     .HasKey(e => new { e.RoleId, e.PrincipalId });

            // Some samples from the Toastmasters example. 
            
            // modelBuilder.Entity<RoleRequestEnvelopeEntity>().ToTable("RoleRequestEvents");
            // modelBuilder.Entity<RolePlacementEnvelopeEntity>().ToTable("RolePlacementEvents");

            // modelBuilder
            //     .Entity<RoleRequestMeeting>()
            //     .HasKey(e => new { e.RoleRequestId, e.MeetingId });

            base.OnModelCreating(modelBuilder);
        }
        public virtual DbSet<ClientEventEnvelopeEntity> ClientEvents { get; set; }
        public virtual DbSet<ScopeEventEnvelopeEntity> ScopeEvents { get; set; }
        public virtual DbSet<ResourceEventEnvelopeEntity> ResourceEvents { get; set; }
        public virtual DbSet<ClientEntity> Clients { get; set; }
        public virtual DbSet<ScopeEntity> Scopes { get;set; }
        public virtual DbSet<ResourceEntity> Resources { get; set; }
    }
}