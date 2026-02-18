using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class Direccione
{
    public int DireccionId { get; set; }

    public int PersonaId { get; set; }

    public string Calle { get; set; } = null!;

    public string Ciudad { get; set; } = null!;

    public string Provincia { get; set; } = null!;

    public string? CodigoPostal { get; set; }

    public string Pais { get; set; } = null!;

    public virtual Persona Persona { get; set; } = null!;

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
