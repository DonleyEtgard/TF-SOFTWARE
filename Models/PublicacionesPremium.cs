using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class PublicacionesPremium
{
    public int PublicacionPremiumId { get; set; }

    public int PublicacionId { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public string? Estado { get; set; }

    public int VendedorId { get; set; }

    public virtual Publicacione Publicacion { get; set; } = null!;

    public virtual Vendedore Vendedor { get; set; } = null!;
}
