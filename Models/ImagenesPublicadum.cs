using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class ImagenesPublicadum
{
    public int ImagenId { get; set; }

    public int PublicacionId { get; set; }

    public string UrlImagen { get; set; } = null!;

    public bool? EsPrincipal { get; set; }

    public DateTime? FechaSubida { get; set; }

    public virtual Publicacione Publicacion { get; set; } = null!;
}
