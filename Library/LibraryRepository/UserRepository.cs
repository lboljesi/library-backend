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
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;
        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Postgres") ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(@"
                SELECT ""Id"", ""Email"", ""PasswordHash"", ""FullName""
                FROM ""Users"" WHERE ""Email"" = @email", conn);
            cmd.Parameters.AddWithValue("email", email);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return new User
            {
                Id = reader.GetGuid(0),
                Email = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                FullName = reader.GetString(3)
            };
        }
        public async Task CreateUserAsync(string email, string passwordHash, string fullName)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(@"
                INSERT INTO ""Users"" (""Email"", ""PasswordHash"", ""FullName"") VALUES (@email, @passwordHash, @fullName)", conn);
            cmd.Parameters.AddWithValue("email", email);
            cmd.Parameters.AddWithValue("passwordHash", passwordHash);
            cmd.Parameters.AddWithValue("fullName", fullName);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
