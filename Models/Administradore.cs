using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class Administradore
{
    public int AdministradorId { get; set; }

    public int UsuarioId { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
