namespace ZONAUTO.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
  
    
    public class PublicacionCreateViewModel
    {
            public Publicacione Publicacione { get; set; } = new Publicacione();
            public IEnumerable<Auto> Autos { get; set; } = new List<Auto>();
            public IEnumerable<Propiedade> Propiedades { get; set; } = new List<Propiedade>();
            public IEnumerable<Categoria> Categorias { get; set; } = new List<Categoria>();
        
    }
    public class DashboardViewModel
    {
        public int TotalUsuarios { get; set; }
        public int TotalAdministradores { get; set; }
        public int TotalPublicaciones { get; set; }
        public int TotalOfertas { get; set; }
        public List<Usuario> UltimosUsuarios { get; set; } = new List<Usuario> ();
        public List<Oferta> UltimasOfertas { get; set; } = new List<Oferta>();
        public int[] PublicacionesMes { get; set; }
        public int[] CantidadPublicacionesMes { get; set; }
    }

}
