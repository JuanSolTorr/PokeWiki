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

        public async Task<bool> ExistsEmailAsync(string email)
        {
            return await _context.Usuarios.AnyAsync(u => u.Email == email);
        }

        public async Task RegisterUserAsync(string username, string email, string password)
        {
            var user = new Usuario
            {
                Username = username,
                Email = email,
                Contrasenia = string.Empty
            };

            var (salt, hashString) = CreatePasswordData(password);

            user.UsuarioAuxiliar = new UsuarioAuxiliar
            {
                Salt = salt,
                Contrasenia_Hasheada = hashString
            };

            await _context.Usuarios.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<Usuario?> LogInUserAsync(string email, string password)
        {
            var user = await GetUserWithSecurityDataAsync(email);
            if (user == null)
            {
                return null;
            }

            return IsPasswordValid(password, user.UsuarioAuxiliar) ? user : null;
        }

        public async Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword)
        {
            var user = await GetUserWithSecurityDataAsync(email);
            if (user == null || !IsPasswordValid(currentPassword, user.UsuarioAuxiliar))
            {
                return false;
            }

            var (newSalt, newHash) = CreatePasswordData(newPassword);

            user.Contrasenia = string.Empty;
            user.UsuarioAuxiliar.Salt = newSalt;
            user.UsuarioAuxiliar.Contrasenia_Hasheada = newHash;

            _context.Usuarios.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAccountAsync(string email, string confirmPassword)
        {
            var user = await GetUserWithSecurityDataAsync(email);
            if (user == null || !IsPasswordValid(confirmPassword, user.UsuarioAuxiliar))
            {
                return false;
            }

            _context.Usuarios.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<Usuario?> GetUserWithSecurityDataAsync(string email)
        {
            return await _context.Usuarios
                .Include(u => u.UsuarioAuxiliar)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        private static (string Salt, string Hash) CreatePasswordData(string password)
        {
            var salt = HelperTools.GenerarSalt();
            var passBytes = HelperCryptography.EncryptPassword(password, salt);
            var hashString = Convert.ToBase64String(passBytes);
            return (salt, hashString);
        }

        private static bool IsPasswordValid(string password, UsuarioAuxiliar securityData)
        {
            var hashGuardado = Convert.FromBase64String(securityData.Contrasenia_Hasheada);
            var tempHash = HelperCryptography.EncryptPassword(password, securityData.Salt);
            return HelperTools.CompareArrays(tempHash, hashGuardado);
        }
    }
}