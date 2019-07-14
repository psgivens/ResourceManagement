  
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
        public IList<ClientScopeMapping> Scopes {get;set;} = new List<ClientScopeMapping>();
    }

    public class ScopeEntity
    {
        [Key]
        public System.Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IList<ClientScopeMapping> Clients {get;set;} = new List<ClientScopeMapping>();

    }
    
    public class ResourceEntity
    {
        [Key]
        public System.Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class ClientScopeMapping {
        public System.Guid ClientId { get; set; }
        public System.Guid ScopeId { get; set; }
    }

    public class Role
    {
        [Key]
        public System.Guid Id { get; set; }
        public string Name { get; set; }
        public IList<RolePrivilegeMapping> PrivilegeMaps {get;set;} = new List<RolePrivilegeMapping>();
    }

    public class Privilege
    {
        [Key]
        public System.Guid Id { get; set; }
        public string Name { get; set; }
        public IList<RolePrivilegeMapping> RoleMaps {get;set;} = new List<RolePrivilegeMapping>();
        public IList<PrivilegeEndpointMapping> EndpointMaps {get;set;} = new List<PrivilegeEndpointMapping>();
    }

    public class DataConstraint
    {
        [Key]
        public System.Guid Id { get; set; }
        // TODO: Figure out what data constraints look like.
        // I may have some documentation somewhere about this. 
        public string Pattern { get; set; }
    }
    
    public class Endpoint
    {
        [Key]
        public System.Guid Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Method { get; set; }
        public IList<DataConstraint> Constraints { get; set; } = new List<DataConstraint>();
        public IList<PrivilegeEndpointMapping> PrivilegeMaps {get;set;} = new List<PrivilegeEndpointMapping>();

    }

    public class RolePrivilegeMapping
    {
        public System.Guid RoleId { get; set; }
        public System.Guid PrivilegeId { get; set; }
    }

    public class PrivilegeEndpointMapping
    {
        public System.Guid PrivilegeId { get; set; }
        public System.Guid Endpoint { get; set; }
    }

}