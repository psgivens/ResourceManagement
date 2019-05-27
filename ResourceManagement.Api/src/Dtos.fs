module ResourceManagement.Api.Dtos

open ResourceManagement.Data.Models

type ClientDto = { 
    id : string
    name : string 
    description : string }

let convertToClientDto (client:ClientEntity) = {
  ClientDto.id = client.Id.ToString () 
  name = client.Name
  description = client.Description }

type ScopeDto = { 
    id : string
    name : string 
    description : string }

let convertToScopeDto (scope:ScopeEntity) = {
  ScopeDto.id = scope.Id.ToString () 
  name = scope.Name
  description = scope.Description }

type ResourceDto = { 
    id : string
    name : string 
    description : string }

let convertToResourceDto (resource:ResourceEntity) = {
  ResourceDto.id = resource.Id.ToString () 
  name = resource.Name
  description = resource.Description }
