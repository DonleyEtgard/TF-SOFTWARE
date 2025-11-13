using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class Pago
{
    public int PagoId { get; set; }

    public int UsuarioId { get; set; }

    public int MetodoPagoId { get; set; }

    public decimal Monto { get; set; }

    public DateTime FechaPago { get; set; }

    public string Estado { get; set; } = null!;

    public virtual MetodosPago MetodoPago { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
