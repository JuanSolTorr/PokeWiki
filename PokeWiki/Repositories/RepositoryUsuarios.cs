using Microsoft.EntityFrameworkCore;
using MvcCoreCryptography.Helpers;
using PokeWiki.Web.Data;
using PokeWiki.Web.Data.Entities;

namespace PokeWiki.Web.Repositories
{
    public class RepositoryUsuarios
    {
        private readonly ApplicationDbContext _context;

        public RepositoryUsuarios(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task RegisterUserAsync(string username, string email, string password)
        {
            Usuario user = new Usuario
            {
                Username = username,
                Email = email,
                Contrasenia = password
            };

            string salt = HelperTools.GenerarSalt();
            byte[] passBytes = HelperCryptography.EncryptPassword(password, salt);
            string hashString = Convert.ToBase64String(passBytes);

            UsuarioAuxiliar aux = new UsuarioAuxiliar
            {
                Salt = salt,
                Contrasenia_Hasheada = hashString
            };

            user.UsuarioAuxiliar = aux;
            await _context.Usuarios.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<Usuario?> LogInUserAsync(string email, string password)
        {
            Usuario? user = await _context.Usuarios
                                          .Include(u => u.UsuarioAuxiliar)
                                          .FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            string salt = user.UsuarioAuxiliar.Salt;
            byte[] hashGuardado = Convert.FromBase64String(user.UsuarioAuxiliar.Contrasenia_Hasheada);

            byte[] tempHash = HelperCryptography.EncryptPassword(password, salt);

            if (HelperTools.CompareArrays(tempHash, hashGuardado))
            {
                return user;
            }
            return null;
        }
    }
}