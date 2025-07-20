using LibraryModels;
using LibraryRepository.Common;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryRepository
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly string _connectionString;

        public AuthorRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Postgres") ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");
        }

        public async Task<HashSet<Guid>> GetExistingAuthorIdsAsync(List<Guid> ids)
        {
            var existingIds = new HashSet<Guid>();
            if (ids.Count == 0) return existingIds;

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"select ""Id"" from ""Authors"" where ""Id"" = ANY(@ids)";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ids", ids.ToArray());

            await using var reader = await cmd.ExecuteReaderAsync();
            while(await reader.ReadAsync())
            {
                existingIds.Add(reader.GetGuid(0));
            }

            return existingIds;
        }
        public async Task<List<AuthorDto>> GetAllAsync()
        {
            var authors = new List<AuthorDto>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"select ""Id"", ""FirstName"", ""LastName"" from ""Authors"" order by ""LastName""";
            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                authors.Add(new AuthorDto { 
                    Id = reader.GetGuid(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2)
                });
            }
            return authors;
        }
    }
}
