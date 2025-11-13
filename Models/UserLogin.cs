using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class UserLogin
{
    public int LoginId { get; set; }

    public int? UsuarioId { get; set; }

    public DateTime FechaLogin { get; set; }

    public bool FueExitoso { get; set; }

    public string? DireccionIp { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
