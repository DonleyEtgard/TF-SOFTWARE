using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class Chat
{
    public int ChatId { get; set; }

    public int CompradorId { get; set; }

    public int VendedorId { get; set; }

    public int PublicacionId { get; set; }

    public DateTime? Fecha { get; set; }

    public virtual Compradore Comprador { get; set; } = null!;

    public virtual ICollection<Mensaje> Mensajes { get; set; } = new List<Mensaje>();

    public virtual Publicacione Publicacion { get; set; } = null!;

    public virtual Vendedore Vendedor { get; set; } = null!;
}
