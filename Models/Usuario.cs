using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class Usuario
{
    public int UsuarioId { get; set; }

    public int PersonaId { get; set; }

    public string ContrasenaHash { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }

    public bool Habilitado { get; set; }

    public bool Verificado { get; set; }

    public int TipoUsuarioId { get; set; }

    public int? DireccionId { get; set; }

    public string? TokenVerificacion { get; set; }

    public virtual ICollection<Administradore> Administradores { get; set; } = new List<Administradore>();

    public virtual ICollection<Auto> Autos { get; set; } = new List<Auto>();

    public virtual ICollection<Compradore> Compradores { get; set; } = new List<Compradore>();

    public virtual Direccione? Direccion { get; set; }

    public virtual ICollection<Factura> FacturaCompradors { get; set; } = new List<Factura>();

    public virtual ICollection<Factura> FacturaVendedors { get; set; } = new List<Factura>();

    public virtual ICollection<Mensaje> Mensajes { get; set; } = new List<Mensaje>();

    public virtual ICollection<Notificacione> Notificaciones { get; set; } = new List<Notificacione>();

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    public virtual Persona Persona { get; set; } = null!;

    public virtual ICollection<Propiedade> Propiedades { get; set; } = new List<Propiedade>();

    public virtual ICollection<Publicacione> Publicaciones { get; set; } = new List<Publicacione>();

    public virtual TiposUsuario TipoUsuario { get; set; } = null!;

    public virtual ICollection<UserLogin> UserLogins { get; set; } = new List<UserLogin>();

    public virtual ICollection<Vendedore> Vendedores { get; set; } = new List<Vendedore>();
}
