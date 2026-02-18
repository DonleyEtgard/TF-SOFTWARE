using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class Transaccione
{
    public int TransaccionId { get; set; }

    public int CompradorId { get; set; }

    public int VendedorId { get; set; }

    public int PublicacionId { get; set; }

    public DateTime? Fecha { get; set; }

    public decimal? MontoFinal { get; set; }

    public string? MetodoDePago { get; set; }

    public bool? Confirmado { get; set; }

    public virtual Compradore Comprador { get; set; } = null!;

    public virtual Publicacione Publicacion { get; set; } = null!;

    public virtual Vendedore Vendedor { get; set; } = null!;

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
