  
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ResourceManagement.Data.Models
{
    public class ClientEntity
    {
        [Key]
        public System.Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IList<ClientScopeMap> Scopes {get;set;} = new List<ClientScopeMap>();
    }

    public class ScopeEntity
    {
        [Key]
        public System.Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IList<ClientScopeMap> Clients {get;set;} = new List<ClientScopeMap>();

    }
    
    public class ResourceEntity
    {
        [Key]
        public System.Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class ClientScopeMap {
        public System.Guid ClientId { get; set; }
        public System.Guid ScopeId { get; set; }
    }

}