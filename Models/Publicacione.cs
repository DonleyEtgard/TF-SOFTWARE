using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class Publicacione
{
    public int PublicacionId { get; set; }

    public int VendedorId { get; set; }

    public int? AutoId { get; set; }

    public int? PropiedadId { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public DateTime FechaPublicacion { get; set; }

    public int CategoriaId { get; set; }

    public string? ImagenRuta { get; set; }

    public int? UsuarioId { get; set; }

    public virtual Auto? Auto { get; set; }

    public virtual Categoria Categoria { get; set; } = null!;

    public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();

    public virtual ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();

    public virtual ICollection<ImagenesPublicada> ImagenesPublicada { get; set; } = new List<ImagenesPublicada>();

    public virtual ICollection<Oferta> Oferta { get; set; } = new List<Oferta>();

    public virtual Propiedade? Propiedad { get; set; }

    public virtual ICollection<PublicacionesPremium> PublicacionesPremia { get; set; } = new List<PublicacionesPremium>();

    public virtual ICollection<Transaccione> Transacciones { get; set; } = new List<Transaccione>();

    public virtual Usuario? Usuario { get; set; }

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
    
}
