using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class MetodosPago
{
    public int MetodoPagoId { get; set; }

    public string TipoDePago { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
