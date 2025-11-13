using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class Categoria
{
    public int CategoriaId { get; set; }

    public string Tipo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<Auto> Autos { get; set; } = new List<Auto>();

    public virtual ICollection<Propiedade> Propiedades { get; set; } = new List<Propiedade>();

    public virtual ICollection<Publicacione> Publicaciones { get; set; } = new List<Publicacione>();
}
