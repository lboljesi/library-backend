using LibraryModels;
using LibraryQuerying;
using LibraryRepositroy.Common;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Security.Cryptography;
using System.Text;


namespace LibraryRepository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly string _connectionString;

        public CategoryRepository (IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Postgres") ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");
        }

        public async Task<List<Category>> GetAllAsync(PaginationQuery query)
        {
            var categories = new List<Category>();

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = new StringBuilder(@"
                SELECT ""Id"", ""Name"" FROM ""Categories"" WHERE 1 = 1");
            if (!string.IsNullOrWhiteSpace(query.Search))
                sql.Append(@" AND LOWER(""Name"") LIKE @search");

            sql.Append($@" ORDER BY ""Name"" {(query.Desc ? "DESC" : "ASC")}");
            sql.Append(" LIMIT @pageSize OFFSET @offset");

            using var cmd = new NpgsqlCommand(sql.ToString(), conn);

            if (!string.IsNullOrWhiteSpace(query.Search))
                cmd.Parameters.AddWithValue("search", $"%{query.Search.ToLower()}%");
            cmd.Parameters.AddWithValue("pageSize", query.PageSize);
            cmd.Parameters.AddWithValue("offset", (query.Page - 1) * query.PageSize);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                categories.Add(new Category { 
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1)
                });
            }

            return categories;
        }

        public async Task<Category?> GetByIdAsync(Guid id)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                SELECT ""Id"", ""Name"" FROM ""Categories"" WHERE ""Id"" = @id;
            ",conn);
            cmd.Parameters.AddWithValue("id", id);
            await using var reader = await cmd.ExecuteReaderAsync();
            if(await reader.ReadAsync())
            {
                return new Category
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1)
                };
            }
            return null;
        }

        public async Task<Category?> CreateAsync(Category category)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"INSERT INTO ""Categories"" (""Id"", ""Name"")
                VALUES (@id, @name)
                RETURNING ""Id"", ""Name"";";


            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", category.Id);
            cmd.Parameters.AddWithValue("name", category.Name);
            await using var reader = await cmd.ExecuteReaderAsync();
            if(await reader.ReadAsync())
            {
                return new Category { 
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1)
                };
            }
            

            return null;
        }

        public async Task<Category?> UpdateAsync(Category category)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
                UPDATE ""Categories""
                SET ""Name"" = @name
                WHERE ""Id"" = @id
                RETURNING ""Id"", ""Name"";
            ";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", category.Id);
            cmd.Parameters.AddWithValue("name", category.Name);

            await using var reader = await cmd.ExecuteReaderAsync();

            if(await reader.ReadAsync())
            {
                var updatedCategory =  new Category
                { 
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1)
                };
                return updatedCategory;
            }
            return null;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            const string sql = @"DELETE FROM ""Categories"" WHERE ""Id"" = @id;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);

            var affected = await cmd.ExecuteNonQueryAsync();
            return affected > 0;
        }

        public async Task<int> GetTotalCountAsync(PaginationQuery query)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = new StringBuilder(@"
                SELECT COUNT(*)
                FROM ""Categories""
                WHERE 1 = 1
            ");

            if (!string.IsNullOrWhiteSpace(query.Search))
                sql.Append(" AND LOWER(\"Name\") LIKE @search");

            await using var cmd = new NpgsqlCommand(sql.ToString(), conn);

            if (!string.IsNullOrWhiteSpace(query.Search))
                cmd.Parameters.AddWithValue("search", $"%{query.Search.ToLower()}%");

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        public async Task<List<CategoryWithBooks>> GetCategoriesWithBooksAsync()
        {
            var result = new Dictionary<Guid, CategoryWithBooks>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT
                    c.""Id"" AS ""CategoryId"",
                    c.""Name"" AS ""CategoryName"",
                    b.""Id"" AS ""BookId"",
                    b.""Title"",
                    b.""Isbn"",
                    b.""PublishedYear"",
                    b.""Price""
                    FROM ""Categories"" c
                    LEFT JOIN ""BookCategories"" bc ON c.""Id"" = bc.""CategoryId""
                    LEFT JOIN ""Books"" b ON bc.""BookId"" = b.""Id""
                    ORDER BY c.""Name"", b.""Title"";";
            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while(await reader.ReadAsync())
            {
                var categoryId = reader.GetGuid(0);
                CategoryWithBooks category;
                if(!result.ContainsKey(categoryId))
                {
                    category = new CategoryWithBooks
                    {
                        Id = categoryId,
                        Name = reader.GetString(1),
                        Books = new List<Books>()
                    };
                    result.Add(categoryId, category);
                }
                else
                {
                    category = result[categoryId];
                }
                if(!reader.IsDBNull(2))
                {
                    category.Books.Add(new Books
                    {
                        Id = reader.GetGuid(2),
                        Title = reader.GetString(3),
                        Isbn = reader.GetString(4),
                        PublishedYear = reader.GetInt32(5),
                        Price = reader.GetInt32(6)
                    });
                }

            }
            return result.Values.ToList();
        }
        public async Task<bool> ExistsAsync(Guid id)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            const string sql = @"select 1 from ""Categories"" where ""Id"" = @id limit 1;";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);
            var result = await cmd.ExecuteScalarAsync();
            return result is not null;
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"SELECT COUNT(*) FROM ""Categories"" WHERE LOWER(""Name"") = LOWER(@name)";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("name", name);

            var result = await cmd.ExecuteScalarAsync();
            var count = result is not null ? (long)result : 0; // Ensure null safety
            return count > 0;
        }

        public async Task<HashSet<Guid>> GetExistingCategoryIdsAsync(List<Guid> ids)
        { 
            var existingIds = new HashSet<Guid>();
            if (ids.Count == 0) return existingIds;
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"SELECT ""Id"" FROM ""Categories"" WHERE ""Id"" = ANY(@ids)";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("ids", ids.ToArray());
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                existingIds.Add(reader.GetGuid(0));
            }
            return existingIds;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            var result = new List<Category>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"SELECT * FROM ""Categories"" ORDER BY ""Name""";

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while(await reader.ReadAsync())
            {
                result.Add(new Category
                {
                    Id = reader.GetGuid(0),
                    Name=reader.GetString(1)
                });

            }
            return result;
        }
    }
}
