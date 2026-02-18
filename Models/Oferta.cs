using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class Oferta
{
    public int OfertaId { get; set; }

    public int CompradorId { get; set; }

    public int PublicacionId { get; set; }

    public decimal Monto { get; set; }

    public DateTime? FechaOferta { get; set; }

    public virtual Compradore Comprador { get; set; } = null!;

    public virtual Publicacione Publicacion { get; set; } = null!;

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
