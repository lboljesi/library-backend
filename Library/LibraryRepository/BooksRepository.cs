using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LibraryModels;
using LibraryQuerying;
using LibraryRepository;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;



namespace LibraryRepository
{
    public class BooksRepository : IBooksRepository
    {
        private readonly string _connectionString;

        public BooksRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Postgres");

        }
        public async Task<bool> ExistsAsync(Guid id)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"SELECT 1 FROM ""Books"" WHERE ""Id"" = @id LIMIT 1;";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);
            var result = await cmd.ExecuteScalarAsync();
            return result is not null;
        }
    }
}
