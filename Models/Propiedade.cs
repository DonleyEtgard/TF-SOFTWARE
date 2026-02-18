using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class Propiedade
{
    public int PropiedadId { get; set; }

    public string Ubicacion { get; set; } = null!;

    public decimal? MetroCuadrados { get; set; }

    public int? Habitaciones { get; set; }

    public string? Tipo { get; set; }

    public string? Descripcion { get; set; }

    public int? UsuarioId { get; set; }

    public virtual ICollection<Publicacione> Publicaciones { get; set; } = new List<Publicacione>();

    public virtual Usuario? Usuario { get; set; }
}
