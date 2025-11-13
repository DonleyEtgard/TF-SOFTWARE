using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class Factura
{
    public int FacturaId { get; set; }

    public int CompradorId { get; set; }

    public int VendedorId { get; set; }

    public int VentaId { get; set; }

    public decimal MontoTotal { get; set; }

    public DateTime FechaEmision { get; set; }

    public string? NumeroFactura { get; set; }

    public string? MetodoPago { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Usuario Comprador { get; set; } = null!;

    public virtual Usuario Vendedor { get; set; } = null!;

    public virtual Venta Venta { get; set; } = null!;
}
