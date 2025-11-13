using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class Persona
{
    public int PersonaId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateOnly? FechaNacimiento { get; set; }

    public int? DireccionId { get; set; }

    public virtual ICollection<Direccione> Direcciones { get; set; } = new List<Direccione>();

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
