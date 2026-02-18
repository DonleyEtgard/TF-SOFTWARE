using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class Auto
{
    public int AutoId { get; set; }

    public string Marca { get; set; } = null!;

    public string Modelo { get; set; } = null!;

    public int? Anio { get; set; }

    public int? Kilometro { get; set; }

    public int? UsuarioId { get; set; }
    public string? Descripcion { get; set; }

    public virtual ICollection<Publicacione> Publicaciones { get; set; } = new List<Publicacione>();

    public virtual Usuario? Usuario { get; set; }
}
