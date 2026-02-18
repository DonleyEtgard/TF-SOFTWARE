using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class TiposUsuario
{
    public int TipoUsuarioId { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
