using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class Mensaje
{
    public int MensajeId { get; set; }

    public int ChatId { get; set; }

    public int UsuarioId { get; set; }

    public string Contenido { get; set; } = null!;

    public DateTime? Fecha { get; set; }

    public virtual Chat Chat { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
