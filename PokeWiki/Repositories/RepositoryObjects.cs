using Microsoft.EntityFrameworkCore;
using PokeWiki.Web.Data;
using PokeWiki.Web.Models.ViewModels;
using System.Data;

namespace PokeWiki.Web.Repositories
{
    public class RepositoryObjects
    {
        private readonly ApplicationDbContext _context;

        public RepositoryObjects(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<ObjetoVM> Objetos, int TotalRegistros, int TotalPaginas, int CurrentPage)> GetObjectsPageAsync(string? categoria, string? search, int page, int pageSize)
        {
            if (page < 1)
            {
                page = 1;
            }

            var totalRegistros = await CountObjectsFromDatabaseAsync(categoria, search);
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(totalRegistros / (double)pageSize));

            if (page > totalPaginas)
            {
                page = totalPaginas;
            }

            var objetos = await LoadObjectsFromDatabaseAsync(categoria, search, page, pageSize);
            return (objetos, totalRegistros, totalPaginas, page);
        }

        public async Task<ObjetoDetalleVM?> GetObjectDetailAsync(int itemId)
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command = connection.CreateCommand();
            command.CommandText = @"
SELECT TOP 1
    i.id,
    i.identifier,
    COALESCE(in_es.name, in_en.name, i.identifier) AS nombre,
    COALESCE(ip_es.effect, ip_en.effect, ip_es.short_effect, ip_en.short_effect, '') AS efecto,
    i.cost,
    ic.identifier AS categoria_identifier,
    COALESCE(ipn_es.name, ipn_en.name, p.identifier) AS bolsillo_nombre
FROM items i
INNER JOIN item_categories ic ON ic.id = i.category_id
INNER JOIN item_pockets p ON p.id = ic.pocket_id
LEFT JOIN item_names in_es ON in_es.item_id = i.id AND in_es.local_language_id = 7
LEFT JOIN item_names in_en ON in_en.item_id = i.id AND in_en.local_language_id = 9
LEFT JOIN item_prose ip_es ON ip_es.item_id = i.id AND ip_es.local_language_id = 7
LEFT JOIN item_prose ip_en ON ip_en.item_id = i.id AND ip_en.local_language_id = 9
LEFT JOIN item_pocket_names ipn_es ON ipn_es.item_pocket_id = p.id AND ipn_es.local_language_id = 7
LEFT JOIN item_pocket_names ipn_en ON ipn_en.item_pocket_id = p.id AND ipn_en.local_language_id = 9
WHERE i.id = @itemId";

            var p = command.CreateParameter();
            p.ParameterName = "@itemId";
            p.Value = itemId;
            command.Parameters.Add(p);

            ObjetoDetalleVM? detalle;

            await using (var reader = await command.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync())
                {
                    return null;
                }

                var nombre = reader["nombre"]?.ToString() ?? string.Empty;
                var efecto = reader["efecto"]?.ToString() ?? string.Empty;
                var categoriaIdentifier = reader["categoria_identifier"]?.ToString() ?? string.Empty;
                var bolsillo = reader["bolsillo_nombre"]?.ToString() ?? string.Empty;
                var identifier = reader["identifier"]?.ToString() ?? string.Empty;

                var categoria = MapCategoria(categoriaIdentifier, bolsillo, nombre);
                if (categoria is null)
                {
                    return null;
                }

                var costo = reader["cost"] != DBNull.Value ? Convert.ToInt64(reader["cost"]) : 0;

                detalle = new ObjetoDetalleVM
                {
                    Id = Convert.ToInt32(reader["id"]),
                    Identificador = identifier,
                    Nombre = nombre,
                    Categoria = categoria,
                    Descripcion = string.IsNullOrWhiteSpace(efecto) ? "No description available." : efecto,
                    Efecto = string.IsNullOrWhiteSpace(efecto) ? "No effect registered." : efecto,
                    Rareza = GetRarezaByCost(costo),
                    Icono = GetIcono(categoria),
                    ImagenUrl = BuildItemImageUrl(identifier)
                };
            }

            detalle.DondeSeConsiguePorGeneracion = await LoadWhereFoundByGenerationAsync(itemId);
            return detalle;
        }

        private async Task<int> CountObjectsFromDatabaseAsync(string? categoria, string? search)
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command = connection.CreateCommand();
            command.CommandText = @"
SELECT COUNT(*)
FROM (
    SELECT
        CASE
            WHEN LOWER(COALESCE(ipn_es.name, ipn_en.name, p.identifier)) LIKE '%bayas%' OR LOWER(ic.identifier) LIKE '%berry%' THEN N'Berries'
            WHEN LOWER(COALESCE(ipn_es.name, ipn_en.name, p.identifier)) LIKE '%bolas%' OR LOWER(COALESCE(ipn_es.name, ipn_en.name, p.identifier)) LIKE '%poké balls%' OR LOWER(COALESCE(ipn_es.name, ipn_en.name, p.identifier)) LIKE '%poke balls%' OR LOWER(ic.identifier) LIKE '%ball%' THEN N'Poké Balls'
            WHEN LOWER(COALESCE(ipn_es.name, ipn_en.name, p.identifier)) LIKE '%objetos clave%' OR LOWER(COALESCE(ipn_es.name, ipn_en.name, p.identifier)) LIKE '%key items%' THEN N'Key Items'
            WHEN LOWER(COALESCE(ipn_es.name, ipn_en.name, p.identifier)) LIKE '%medicinas%' OR LOWER(ic.identifier) IN ('healing', 'status-cures', 'revival', 'medicine', 'vitamins', 'pp-recovery') THEN N'Healing'
            WHEN LOWER(COALESCE(ipn_es.name, ipn_en.name, p.identifier)) LIKE '%objetos de combate%' OR LOWER(ic.identifier) IN ('stat-boosts', 'miracle-shooter') THEN N'Battle'
            WHEN LOWER(ic.identifier) LIKE '%evolution%' OR LOWER(ic.identifier) LIKE '%mega-stones%' OR LOWER(COALESCE(in_es.name, in_en.name, i.identifier)) LIKE '%piedra%' OR LOWER(COALESCE(in_es.name, in_en.name, i.identifier)) LIKE '%stone%' THEN N'Evolution'
            ELSE NULL
        END AS categoria,
        COALESCE(in_es.name, in_en.name, i.identifier) AS nombre
    FROM items i
    INNER JOIN item_categories ic ON ic.id = i.category_id
    INNER JOIN item_pockets p ON p.id = ic.pocket_id
    LEFT JOIN item_names in_es ON in_es.item_id = i.id AND in_es.local_language_id = 7
    LEFT JOIN item_names in_en ON in_en.item_id = i.id AND in_en.local_language_id = 9
    LEFT JOIN item_pocket_names ipn_es ON ipn_es.item_pocket_id = p.id AND ipn_es.local_language_id = 7
    LEFT JOIN item_pocket_names ipn_en ON ipn_en.item_pocket_id = p.id AND ipn_en.local_language_id = 9
) AS t
WHERE t.categoria IS NOT NULL
  AND (@categoria = '' OR t.categoria = @categoria)
  AND (@search = '' OR t.nombre LIKE '%' + @search + '%')";

            var categoriaParam = command.CreateParameter();
            categoriaParam.ParameterName = "@categoria";
            categoriaParam.Value = categoria?.Trim() ?? string.Empty;
            command.Parameters.Add(categoriaParam);

            var searchParam = command.CreateParameter();
            searchParam.ParameterName = "@search";
            searchParam.Value = search?.Trim() ?? string.Empty;
            command.Parameters.Add(searchParam);

            var count = await command.ExecuteScalarAsync();
            return count is null || count == DBNull.Value ? 0 : Convert.ToInt32(count);
        }

        private async Task<List<ObjetoVM>> LoadObjectsFromDatabaseAsync(string? categoria, string? search, int page, int pageSize)
        {
            var result = new List<ObjetoVM>();

            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command = connection.CreateCommand();
            command.CommandText = @"
SELECT
    t.id,
    t.identifier,
    t.nombre,
    t.efecto,
    t.cost,
    t.categoria AS categoria
FROM (
    SELECT
        i.id,
        i.identifier,
        COALESCE(in_es.name, in_en.name, i.identifier) AS nombre,
        COALESCE(ip_es.short_effect, ip_en.short_effect, '') AS efecto,
        i.cost,
        CASE
            WHEN LOWER(COALESCE(ipn_es.name, ipn_en.name, p.identifier)) LIKE '%bayas%' OR LOWER(ic.identifier) LIKE '%berry%' THEN N'Berries'
            WHEN LOWER(COALESCE(ipn_es.name, ipn_en.name, p.identifier)) LIKE '%bolas%' OR LOWER(COALESCE(ipn_es.name, ipn_en.name, p.identifier)) LIKE '%poké balls%' OR LOWER(COALESCE(ipn_es.name, ipn_en.name, p.identifier)) LIKE '%poke balls%' OR LOWER(ic.identifier) LIKE '%ball%' THEN N'Poké Balls'
            WHEN LOWER(COALESCE(ipn_es.name, ipn_en.name, p.identifier)) LIKE '%objetos clave%' OR LOWER(COALESCE(ipn_es.name, ipn_en.name, p.identifier)) LIKE '%key items%' THEN N'Key Items'
            WHEN LOWER(COALESCE(ipn_es.name, ipn_en.name, p.identifier)) LIKE '%medicinas%' OR LOWER(ic.identifier) IN ('healing', 'status-cures', 'revival', 'medicine', 'vitamins', 'pp-recovery') THEN N'Healing'
            WHEN LOWER(COALESCE(ipn_es.name, ipn_en.name, p.identifier)) LIKE '%objetos de combate%' OR LOWER(ic.identifier) IN ('stat-boosts', 'miracle-shooter') THEN N'Battle'
            WHEN LOWER(ic.identifier) LIKE '%evolution%' OR LOWER(ic.identifier) LIKE '%mega-stones%' OR LOWER(COALESCE(in_es.name, in_en.name, i.identifier)) LIKE '%piedra%' OR LOWER(COALESCE(in_es.name, in_en.name, i.identifier)) LIKE '%stone%' THEN N'Evolution'
            ELSE NULL
        END AS categoria
    FROM items i
    INNER JOIN item_categories ic ON ic.id = i.category_id
    INNER JOIN item_pockets p ON p.id = ic.pocket_id
    LEFT JOIN item_names in_es ON in_es.item_id = i.id AND in_es.local_language_id = 7
    LEFT JOIN item_names in_en ON in_en.item_id = i.id AND in_en.local_language_id = 9
    LEFT JOIN item_prose ip_es ON ip_es.item_id = i.id AND ip_es.local_language_id = 7
    LEFT JOIN item_prose ip_en ON ip_en.item_id = i.id AND ip_en.local_language_id = 9
    LEFT JOIN item_pocket_names ipn_es ON ipn_es.item_pocket_id = p.id AND ipn_es.local_language_id = 7
    LEFT JOIN item_pocket_names ipn_en ON ipn_en.item_pocket_id = p.id AND ipn_en.local_language_id = 9
) AS t
WHERE t.categoria IS NOT NULL
  AND (@categoria = '' OR t.categoria = @categoria)
  AND (@search = '' OR t.nombre LIKE '%' + @search + '%')
ORDER BY t.categoria, t.nombre
OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            var categoriaParam = command.CreateParameter();
            categoriaParam.ParameterName = "@categoria";
            categoriaParam.Value = categoria?.Trim() ?? string.Empty;
            command.Parameters.Add(categoriaParam);

            var searchParam = command.CreateParameter();
            searchParam.ParameterName = "@search";
            searchParam.Value = search?.Trim() ?? string.Empty;
            command.Parameters.Add(searchParam);

            var offsetParam = command.CreateParameter();
            offsetParam.ParameterName = "@offset";
            offsetParam.Value = (page - 1) * pageSize;
            command.Parameters.Add(offsetParam);

            var pageSizeParam = command.CreateParameter();
            pageSizeParam.ParameterName = "@pageSize";
            pageSizeParam.Value = pageSize;
            command.Parameters.Add(pageSizeParam);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var nombre = reader["nombre"]?.ToString() ?? string.Empty;
                var efecto = reader["efecto"]?.ToString() ?? string.Empty;
                var categoriaTexto = reader["categoria"]?.ToString() ?? string.Empty;
                var identifier = reader["identifier"]?.ToString() ?? string.Empty;

                var costo = reader["cost"] != DBNull.Value ? Convert.ToInt64(reader["cost"]) : 0;

                result.Add(new ObjetoVM
                {
                    Id = Convert.ToInt32(reader["id"]),
                    Identificador = identifier,
                    Nombre = nombre,
                    Categoria = categoriaTexto,
                    Descripcion = string.IsNullOrWhiteSpace(efecto) ? "No description available." : efecto,
                    Efecto = string.IsNullOrWhiteSpace(efecto) ? "No effect registered." : efecto,
                    Rareza = GetRarezaByCost(costo),
                    Icono = GetIcono(categoriaTexto),
                    ImagenUrl = BuildItemImageUrl(identifier)
                });
            }

            return result;
        }

        private async Task<List<ObjetoGeneracionVM>> LoadWhereFoundByGenerationAsync(int itemId)
        {
            var result = new List<ObjetoGeneracionVM>();

            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command = connection.CreateCommand();
            command.CommandText = @"
SELECT
    igi.generation_id,
    COALESCE(gn_es.name, gn_en.name, g.identifier) AS generacion,
    igi.game_index,
    STRING_AGG(COALESCE(vn_es.name, vn_en.name, v.identifier), ', ') AS juegos
FROM item_game_indices igi
INNER JOIN generations g ON g.id = igi.generation_id
LEFT JOIN generation_names gn_es ON gn_es.generation_id = g.id AND gn_es.local_language_id = 7
LEFT JOIN generation_names gn_en ON gn_en.generation_id = g.id AND gn_en.local_language_id = 9
INNER JOIN version_groups vg ON vg.generation_id = g.id
INNER JOIN versions v ON v.version_group_id = vg.id
LEFT JOIN version_names vn_es ON vn_es.version_id = v.id AND vn_es.local_language_id = 7
LEFT JOIN version_names vn_en ON vn_en.version_id = v.id AND vn_en.local_language_id = 9
WHERE igi.item_id = @itemId
GROUP BY igi.generation_id, COALESCE(gn_es.name, gn_en.name, g.identifier), igi.game_index
ORDER BY igi.generation_id";

            var p = command.CreateParameter();
            p.ParameterName = "@itemId";
            p.Value = itemId;
            command.Parameters.Add(p);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new ObjetoGeneracionVM
                {
                    Generacion = reader["generacion"]?.ToString() ?? "Unknown",
                    IndiceJuego = reader["game_index"] != DBNull.Value ? Convert.ToInt64(reader["game_index"]) : 0,
                    Juegos = reader["juegos"]?.ToString() ?? "No games registered"
                });
            }

            return result;
        }

        private static string BuildItemImageUrl(string identifier)
            => $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/items/{identifier}.png";

        private static string? MapCategoria(string categoriaIdentifier, string bolsillo, string nombre)
        {
            var id = categoriaIdentifier.Trim().ToLowerInvariant();
            var pocket = bolsillo.Trim().ToLowerInvariant();
            var n = nombre.Trim().ToLowerInvariant();

            if (pocket.Contains("bayas") || id.Contains("berry"))
                return "Berries";

            if (pocket.Contains("bolas") || pocket.Contains("poké balls") || pocket.Contains("poke balls") || id.Contains("ball"))
                return "Poké Balls";

            if (pocket.Contains("objetos clave") || pocket.Contains("key items"))
                return "Key Items";

            if (pocket.Contains("medicinas") || id is "healing" or "status-cures" or "revival" or "medicine" or "vitamins" or "pp-recovery")
                return "Healing";

            if (pocket.Contains("objetos de combate") || id is "stat-boosts" or "miracle-shooter")
                return "Battle";

            if (id.Contains("evolution") || id.Contains("mega-stones") || n.Contains("piedra") || n.Contains("stone"))
                return "Evolution";

            return null;
        }

        private static string GetRarezaByCost(long cost)
        {
            if (cost <= 0) return "Key";
            if (cost < 500) return "Common";
            if (cost < 3000) return "Medium";
            return "High";
        }

        private static string GetIcono(string categoria)
        {
            return categoria switch
            {
                "Evolution" => "?",
                "Battle" => "??",
                "Berries" => "??",
                "Healing" => "??",
                "Poké Balls" => "?",
                "Key Items" => "???",
                _ => "??"
            };
        }
    }
}
