using System;
using System.Collections.Generic;

namespace ZONAUTO.Models;

public partial class Vendedore
{
    public int VendedorId { get; set; }

    public int UsuarioId { get; set; }

    public bool ServicioPremium { get; set; }

    public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();

    public virtual ICollection<PublicacionesPremium> PublicacionesPremia { get; set; } = new List<PublicacionesPremium>();

    public virtual ICollection<Transaccione> Transacciones { get; set; } = new List<Transaccione>();

    public virtual Usuario Usuario { get; set; } = null!;

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
