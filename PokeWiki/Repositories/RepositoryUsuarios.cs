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

        public async Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword)
        {
            Usuario? user = await _context.Usuarios
                .Include(u => u.UsuarioAuxiliar)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null) return false;

            // Verificar contraseña actual
            string currentSalt = user.UsuarioAuxiliar.Salt;
            byte[] hashGuardado = Convert.FromBase64String(user.UsuarioAuxiliar.Contrasenia_Hasheada);
            byte[] tempCurrentHash = HelperCryptography.EncryptPassword(currentPassword, currentSalt);

            if (!HelperTools.CompareArrays(tempCurrentHash, hashGuardado))
            {
                return false; // Contraseña actual incorrecta
            }

            // Generar y guardar nueva contraseña
            string newSalt = HelperTools.GenerarSalt();
            byte[] newPassBytes = HelperCryptography.EncryptPassword(newPassword, newSalt);
            
            user.Contrasenia = newPassword; // Reflejando plano si así lo gestionabas o ignorar si solo usas el hash
            user.UsuarioAuxiliar.Salt = newSalt;
            user.UsuarioAuxiliar.Contrasenia_Hasheada = Convert.ToBase64String(newPassBytes);

            _context.Usuarios.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAccountAsync(string email, string confirmPassword)
        {
            Usuario? user = await _context.Usuarios
                .Include(u => u.UsuarioAuxiliar)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null) return false;

            // Verificar contraseña
            string salt = user.UsuarioAuxiliar.Salt;
            byte[] hashGuardado = Convert.FromBase64String(user.UsuarioAuxiliar.Contrasenia_Hasheada);
            byte[] tempHash = HelperCryptography.EncryptPassword(confirmPassword, salt);

            if (!HelperTools.CompareArrays(tempHash, hashGuardado))
            {
                return false; // Contraseña de confirmación incorrecta
            }

            _context.Usuarios.Remove(user);
            // La entidad auxiliar se eliminará por el DeleteBehavior.Cascade configurado en ApplicationDbContext
            
            await _context.SaveChangesAsync();
            return true;
        }
    }
}