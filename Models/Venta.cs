using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class Venta
{
    public int VentaId { get; set; }

    public int OfertaId { get; set; }

    public int VendedorId { get; set; }

    public int CompradorId { get; set; }

    public int PublicacionId { get; set; }

    public int? TransaccionId { get; set; }

    public decimal PrecioFinal { get; set; }

    public DateTime? FechaVenta { get; set; }

    public string? EstadoVenta { get; set; }

    public virtual Compradore Comprador { get; set; } = null!;

    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    public virtual Oferta Oferta { get; set; } = null!;

    public virtual Publicacione Publicacion { get; set; } = null!;

    public virtual Transaccione? Transaccion { get; set; }

    public virtual Vendedore Vendedor { get; set; } = null!;
}
