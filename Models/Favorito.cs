using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class Favorito
{
    public int FavoritoId { get; set; }

    public int CompradorId { get; set; }

    public int PublicacionId { get; set; }

    public DateTime? FechaAgregado { get; set; }

    public virtual Compradore Comprador { get; set; } = null!;

    public virtual Publicacione Publicacion { get; set; } = null!;
}
