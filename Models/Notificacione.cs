using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class Notificacione
{
    public int NotificacionId { get; set; }

    public int UsuarioId { get; set; }

    public string Mensaje { get; set; } = null!;

    public bool Leido { get; set; }

    public DateTime Fecha { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
