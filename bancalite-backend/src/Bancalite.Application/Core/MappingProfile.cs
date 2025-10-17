using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bancalite.Domain;
using Bancalite.Application.Clientes.ClienteList;
using Bancalite.Application.Clientes.GetCliente;

namespace Bancalite.Application.Core
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            // Cliente -> ClienteListItem
            CreateMap<Cliente, ClienteListItem>()
                .ForMember(d => d.ClienteId, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.PersonaId, opt => opt.MapFrom(s => s.PersonaId))
                .ForMember(d => d.Nombres, opt => opt.MapFrom(s => s.Persona.Nombres))
                .ForMember(d => d.Apellidos, opt => opt.MapFrom(s => s.Persona.Apellidos))
                .ForMember(d => d.Edad, opt => opt.MapFrom(s => s.Persona.Edad))
                .ForMember(d => d.GeneroId, opt => opt.MapFrom(s => s.Persona.GeneroId))
                .ForMember(d => d.GeneroNombre, opt => opt.MapFrom(s => s.Persona.Genero.Nombre))
                .ForMember(d => d.GeneroCodigo, opt => opt.MapFrom(s => s.Persona.Genero.Codigo))
                .ForMember(d => d.TipoDocumentoIdentidadId, opt => opt.MapFrom(s => s.Persona.TipoDocumentoIdentidadId))
                .ForMember(d => d.TipoDocumentoIdentidadNombre, opt => opt.MapFrom(s => s.Persona.TipoDocumentoIdentidad.Nombre))
                .ForMember(d => d.TipoDocumentoIdentidadCodigo, opt => opt.MapFrom(s => s.Persona.TipoDocumentoIdentidad.Codigo))
                .ForMember(d => d.NumeroDocumento, opt => opt.MapFrom(s => s.Persona.NumeroDocumento))
                .ForMember(d => d.Direccion, opt => opt.MapFrom(s => s.Persona.Direccion))
                .ForMember(d => d.Telefono, opt => opt.MapFrom(s => s.Persona.Telefono))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Persona.Email))
                .ForMember(d => d.Estado, opt => opt.MapFrom(s => s.Estado))
                .ForMember(d => d.RolId, opt => opt.Ignore())
                .ForMember(d => d.RolNombre, opt => opt.Ignore());

            // Cliente -> ClienteDto
            CreateMap<Cliente, ClienteDto>()
                .ForMember(d => d.ClienteId, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.PersonaId, opt => opt.MapFrom(s => s.PersonaId))
                .ForMember(d => d.Nombres, opt => opt.MapFrom(s => s.Persona.Nombres))
                .ForMember(d => d.Apellidos, opt => opt.MapFrom(s => s.Persona.Apellidos))
                .ForMember(d => d.Edad, opt => opt.MapFrom(s => s.Persona.Edad))
                .ForMember(d => d.GeneroId, opt => opt.MapFrom(s => s.Persona.GeneroId))
                .ForMember(d => d.GeneroNombre, opt => opt.MapFrom(s => s.Persona.Genero.Nombre))
                .ForMember(d => d.TipoDocumentoIdentidadId, opt => opt.MapFrom(s => s.Persona.TipoDocumentoIdentidadId))
                .ForMember(d => d.TipoDocumentoIdentidadNombre, opt => opt.MapFrom(s => s.Persona.TipoDocumentoIdentidad.Nombre))
                .ForMember(d => d.NumeroDocumento, opt => opt.MapFrom(s => s.Persona.NumeroDocumento))
                .ForMember(d => d.Direccion, opt => opt.MapFrom(s => s.Persona.Direccion))
                .ForMember(d => d.Telefono, opt => opt.MapFrom(s => s.Persona.Telefono))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Persona.Email))
                .ForMember(d => d.Estado, opt => opt.MapFrom(s => s.Estado))
                .ForMember(d => d.RolId, opt => opt.Ignore())
                .ForMember(d => d.RolNombre, opt => opt.Ignore());
        }
    }
}
