using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ZONAUTO.Models;

public partial class ZonautoContext : DbContext
{
    public ZonautoContext()
    {
    }

    public ZonautoContext(DbContextOptions<ZonautoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Administradore> Administradores { get; set; }

    public virtual DbSet<Auto> Autos { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<Compradore> Compradores { get; set; }

    public virtual DbSet<Direccione> Direcciones { get; set; }

    public virtual DbSet<Factura> Facturas { get; set; }

    public virtual DbSet<Favorito> Favoritos { get; set; }

    public virtual DbSet<ImagenesPublicadum> ImagenesPublicada { get; set; }

    public virtual DbSet<Mensaje> Mensajes { get; set; }

    public virtual DbSet<MetodosPago> MetodosPagos { get; set; }

    public virtual DbSet<Notificacione> Notificaciones { get; set; }

    public virtual DbSet<Oferta> Ofertas { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<Persona> Personas { get; set; }

    public virtual DbSet<Propiedade> Propiedades { get; set; }

    public virtual DbSet<Publicacione> Publicaciones { get; set; }

    public virtual DbSet<PublicacionesPremium> PublicacionesPremia { get; set; }

    public virtual DbSet<TiposUsuario> TiposUsuarios { get; set; }

    public virtual DbSet<Transaccione> Transacciones { get; set; }

    public virtual DbSet<UserLogin> UserLogins { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Vendedore> Vendedores { get; set; }

    public virtual DbSet<Venta> Ventas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DONLEYETGARD\\SQLEXPRESS;Database=ZONAUTO;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administradore>(entity =>
        {
            entity.HasKey(e => e.AdministradorId).HasName("PK__Administ__2C780D76E00C0AFB");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Administradores)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Administrador_Usuario");
        });

        modelBuilder.Entity<Auto>(entity =>
        {
            entity.HasKey(e => e.AutoId).HasName("PK__Autos__6B232905844605FD");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Marca).HasMaxLength(50);
            entity.Property(e => e.Modelo).HasMaxLength(50);

            entity.HasOne(d => d.Categoria).WithMany(p => p.Autos)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Autos_Categoria");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Autos)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK_Autos_Usuarios");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.CategoriaId).HasName("PK__Categori__F353C1E52AB8973C");

            entity.Property(e => e.Tipo).HasMaxLength(50);
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.ChatId).HasName("PK__Chats__A9FBE7C6B1112D70");

            entity.Property(e => e.Fecha).HasColumnType("datetime");

            entity.HasOne(d => d.Comprador).WithMany(p => p.Chats)
                .HasForeignKey(d => d.CompradorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Chat_Comprador");

            entity.HasOne(d => d.Publicacion).WithMany(p => p.Chats)
                .HasForeignKey(d => d.PublicacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Chat_Publicacion");

            entity.HasOne(d => d.Vendedor).WithMany(p => p.Chats)
                .HasForeignKey(d => d.VendedorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Chat_Vendedor");
        });

        modelBuilder.Entity<Compradore>(entity =>
        {
            entity.HasKey(e => e.CompradorId).HasName("PK__Comprado__E521A94D0FDE36EE");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Compradores)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comprador_Usuario");
        });

        modelBuilder.Entity<Direccione>(entity =>
        {
            entity.HasKey(e => e.DireccionId).HasName("PK__Direccio__68906D643D08FA1F");

            entity.Property(e => e.Calle).HasMaxLength(150);
            entity.Property(e => e.Ciudad).HasMaxLength(100);
            entity.Property(e => e.CodigoPostal).HasMaxLength(20);
            entity.Property(e => e.Pais).HasMaxLength(50);
            entity.Property(e => e.Provincia).HasMaxLength(100);

            entity.HasOne(d => d.Persona).WithMany(p => p.Direcciones)
                .HasForeignKey(d => d.PersonaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Direccione_Persona");
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(e => e.FacturaId).HasName("PK__Facturas__5C02486503290046");

            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Emitida");
            entity.Property(e => e.FechaEmision)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MetodoPago).HasMaxLength(50);
            entity.Property(e => e.MontoTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NumeroFactura).HasMaxLength(20);

            entity.HasOne(d => d.Comprador).WithMany(p => p.FacturaCompradors)
                .HasForeignKey(d => d.CompradorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Factura_Comprador");

            entity.HasOne(d => d.Vendedor).WithMany(p => p.FacturaVendedors)
                .HasForeignKey(d => d.VendedorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Factura_Vendedor");

            entity.HasOne(d => d.Venta).WithMany(p => p.Facturas)
                .HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Factura_Venta");
        });

        modelBuilder.Entity<Favorito>(entity =>
        {
            entity.HasKey(e => e.FavoritoId).HasName("PK__Favorito__CFF711E54E9C9135");

            entity.Property(e => e.FechaAgregado).HasColumnType("datetime");

            entity.HasOne(d => d.Comprador).WithMany(p => p.Favoritos)
                .HasForeignKey(d => d.CompradorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Favorito_Comprador");

            entity.HasOne(d => d.Publicacion).WithMany(p => p.Favoritos)
                .HasForeignKey(d => d.PublicacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Favorito_Publicacion");
        });

        modelBuilder.Entity<ImagenesPublicadum>(entity =>
        {
            entity.HasKey(e => e.ImagenId).HasName("PK__Imagenes__0C7D20B7AA1BB714");

            entity.Property(e => e.FechaSubida).HasColumnType("datetime");
            entity.Property(e => e.UrlImagen).HasMaxLength(255);

            entity.HasOne(d => d.Publicacion).WithMany(p => p.ImagenesPublicada)
                .HasForeignKey(d => d.PublicacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Imagen_Publicacion");
        });

        modelBuilder.Entity<Mensaje>(entity =>
        {
            entity.HasKey(e => e.MensajeId).HasName("PK__Mensajes__FEA0555FA2B64E24");

            entity.Property(e => e.Fecha).HasColumnType("datetime");

            entity.HasOne(d => d.Chat).WithMany(p => p.Mensajes)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mensaje_Chat");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Mensajes)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mensaje_Usuario");
        });

        modelBuilder.Entity<MetodosPago>(entity =>
        {
            entity.HasKey(e => e.MetodoPagoId).HasName("PK__MetodosP__A8FEAF5482C4AD0A");

            entity.Property(e => e.TipoDePago).HasMaxLength(50);
        });

        modelBuilder.Entity<Notificacione>(entity =>
        {
            entity.HasKey(e => e.NotificacionId).HasName("PK__Notifica__BCC12024D91BB143");

            entity.Property(e => e.Fecha).HasColumnType("datetime");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notificacion_Usuario");
        });

        modelBuilder.Entity<Oferta>(entity =>
        {
            entity.HasKey(e => e.OfertaId).HasName("PK__Ofertas__F2629429C90A4B99");

            entity.Property(e => e.FechaOferta).HasColumnType("datetime");
            entity.Property(e => e.Monto).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Comprador).WithMany(p => p.Oferta)
                .HasForeignKey(d => d.CompradorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Oferta_Comprador");

            entity.HasOne(d => d.Publicacion).WithMany(p => p.Oferta)
                .HasForeignKey(d => d.PublicacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Oferta_Publicacion");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.PagoId).HasName("PK__Pago__F00B6138608FCF4D");

            entity.ToTable("Pago");

            entity.Property(e => e.Estado).HasMaxLength(50);
            entity.Property(e => e.FechaPago).HasColumnType("datetime");
            entity.Property(e => e.Monto).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MetodoPago).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.MetodoPagoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pago_MetodoPago");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pago_Usuario");
        });

        modelBuilder.Entity<Persona>(entity =>
        {
            entity.HasKey(e => e.PersonaId).HasName("PK__Personas__7C34D303AADD477E");

            entity.Property(e => e.Apellido).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Propiedade>(entity =>
        {
            entity.HasKey(e => e.PropiedadId).HasName("PK__Propieda__D4B8C06D8058F1BC");

            entity.Property(e => e.MetroCuadrados).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Tipo).HasMaxLength(50);
            entity.Property(e => e.Ubicacion).HasMaxLength(255);

            entity.HasOne(d => d.Categoria).WithMany(p => p.Propiedades)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Propiedades_Categoria");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Propiedades)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK_Propiedad_Usuarios");
        });

        modelBuilder.Entity<Publicacione>(entity =>
        {
            entity.HasKey(e => e.PublicacionId).HasName("PK__Publicac__10DF158AA58C26AB");

            entity.ToTable(tb => tb.HasTrigger("TR_CambioPrecio"));

            entity.Property(e => e.FechaPublicacion).HasColumnType("datetime");
            entity.Property(e => e.ImagenRuta).HasMaxLength(255);
            entity.Property(e => e.Precio).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Titulo).HasMaxLength(150);

            entity.HasOne(d => d.Auto).WithMany(p => p.Publicaciones)
                .HasForeignKey(d => d.AutoId)
                .HasConstraintName("FK_Publicacion_Auto");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Publicaciones)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Publicacion_Categoria");

            entity.HasOne(d => d.Propiedad).WithMany(p => p.Publicaciones)
                .HasForeignKey(d => d.PropiedadId)
                .HasConstraintName("FK_Publicacion_Propiedad");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Publicaciones)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK_Publicacion_Usuarios");
        });

        modelBuilder.Entity<PublicacionesPremium>(entity =>
        {
            entity.HasKey(e => e.PublicacionPremiumId).HasName("PK__Publicac__EA797E30297057AE");

            entity.ToTable("PublicacionesPremium");

            entity.Property(e => e.Estado).HasMaxLength(50);
            entity.Property(e => e.FechaFin).HasColumnType("datetime");
            entity.Property(e => e.FechaInicio).HasColumnType("datetime");

            entity.HasOne(d => d.Publicacion).WithMany(p => p.PublicacionesPremia)
                .HasForeignKey(d => d.PublicacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PublicacionesPremium_Publicacion");

            entity.HasOne(d => d.Vendedor).WithMany(p => p.PublicacionesPremia)
                .HasForeignKey(d => d.VendedorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PublicacionesPremium_Vendedor");
        });

        modelBuilder.Entity<TiposUsuario>(entity =>
        {
            entity.HasKey(e => e.TipoUsuarioId).HasName("PK__TiposUsu__7F22C72217D89F84");

            entity.Property(e => e.Nombre)
                .HasMaxLength(60)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Transaccione>(entity =>
        {
            entity.HasKey(e => e.TransaccionId).HasName("PK__Transacc__86A849FEF8439580");

            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.MetodoDePago).HasMaxLength(50);
            entity.Property(e => e.MontoFinal).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Comprador).WithMany(p => p.Transacciones)
                .HasForeignKey(d => d.CompradorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transaccion_Comprador");

            entity.HasOne(d => d.Publicacion).WithMany(p => p.Transacciones)
                .HasForeignKey(d => d.PublicacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transaccion_Publicacion");

            entity.HasOne(d => d.Vendedor).WithMany(p => p.Transacciones)
                .HasForeignKey(d => d.VendedorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transaccion_Vendedor");
        });

        modelBuilder.Entity<UserLogin>(entity =>
        {
            entity.HasKey(e => e.LoginId).HasName("PK__UserLogi__4DDA2818A3B1AA62");

            entity.Property(e => e.DireccionIp).HasMaxLength(50);
            entity.Property(e => e.FechaLogin).HasColumnType("datetime");

            entity.HasOne(d => d.Usuario).WithMany(p => p.UserLogins)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK_UserLogin_Usuario");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UsuarioId).HasName("PK__Usuarios__2B3DE7B80DF3A88D");

            entity.Property(e => e.ContrasenaHash).HasMaxLength(255);
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.TokenVerificacion).HasMaxLength(255);

            entity.HasOne(d => d.Direccion).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.DireccionId)
                .HasConstraintName("FK_Usuario_Direccion");

            entity.HasOne(d => d.Persona).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.PersonaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_Persona");

            entity.HasOne(d => d.TipoUsuario).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.TipoUsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_TipoUsuario");
        });

        modelBuilder.Entity<Vendedore>(entity =>
        {
            entity.HasKey(e => e.VendedorId).HasName("PK__Vendedor__2033EEEC2ECCECC8");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Vendedores)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vendedor_Usuario");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.VentaId).HasName("PK__Ventas__5B4150AC723EAF27");

            entity.Property(e => e.EstadoVenta).HasMaxLength(50);
            entity.Property(e => e.FechaVenta).HasColumnType("datetime");
            entity.Property(e => e.PrecioFinal).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Comprador).WithMany(p => p.Venta)
                .HasForeignKey(d => d.CompradorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Venta_Comprador");

            entity.HasOne(d => d.Oferta).WithMany(p => p.Venta)
                .HasForeignKey(d => d.OfertaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Venta_Oferta");

            entity.HasOne(d => d.Publicacion).WithMany(p => p.Venta)
                .HasForeignKey(d => d.PublicacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Venta_Publicacion");

            entity.HasOne(d => d.Transaccion).WithMany(p => p.Venta)
                .HasForeignKey(d => d.TransaccionId)
                .HasConstraintName("FK_Venta_Transaccion");

            entity.HasOne(d => d.Vendedor).WithMany(p => p.Venta)
                .HasForeignKey(d => d.VendedorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Venta_Vendedor");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
