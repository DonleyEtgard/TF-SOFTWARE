namespace ZONAUTO.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
 
    
        public class CrearPublicacionViewModel
        {
            public string Titulo { get; set; } = null!;
            public string? Descripcion { get; set; }
            public decimal Precio { get; set; }

            // En lugar de Ids, pasamos atributos legibles
            public string CategoriaNombre { get; set; } = null!;
            public string? AutoDescripcion { get; set; } // Marca + Modelo
            public string? PropiedadUbicacion { get; set; }

            // Imagen
            public IFormFile? Imagen { get; set; }

            // Vendedor (se puede obtener de sesión/autenticación, 
            // pero aquí lo dejo explícito)
            public int VendedorId { get; set; }
        }
    
}
