namespace PokeWiki.Web.Data.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Contrasenia { get; set; } = null!;

        public UsuarioAuxiliar UsuarioAuxiliar { get; set; } = null!;
    }
}