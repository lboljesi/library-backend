using LibraryModels;
using LibraryRepository.Common;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LibraryRepository
{
    public class MemberRepository : IMemberRepository
    {
        private readonly string _connectionString;
        public MemberRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Postgres") ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");
        }

        public async Task<List<MemberDto>> GetAllAsync()
        {
            var members = new List<MemberDto>();

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"select ""Id"", ""Name"", ""MembershipDate"", ""BirthYear"" from ""Members"" order by ""Name"" ";

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while(await reader.ReadAsync())
            {
                members.Add(new MemberDto
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    MembershipDate = reader.GetDateTime(2),
                    BirthYear = reader.GetInt32(3)
                });
            }
            return members;
        }

        public async Task<MemberDto?> GetByIdAsync(Guid id)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"select ""Id"", ""Name"", ""MembershipDate"", ""BirthYear"" from ""Members"" where ""Id"" = @id";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new MemberDto{
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    MembershipDate = reader.GetDateTime(2),
                    BirthYear = reader.GetInt32(3)
                };
            }

            return null;
        }

        public async Task<Guid> AddAsync(MemberCreateUpdateDto dto)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var id = Guid.NewGuid();

            const string sql = @"insert into ""Members"" (""Id"", ""Name"", ""MembershipDate"", ""BirthYear"")
                values (@Id, @Name, @MembershipDate, @BirthYear);";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Name", dto.Name);
            cmd.Parameters.AddWithValue("@MembershipDate", dto.MembershipDate);            
            cmd.Parameters.AddWithValue("@BirthYear", dto.BirthYear);

            await cmd.ExecuteNonQueryAsync();

            return id;
        }

        public async Task<bool> UpdateAsync(Guid id, MemberCreateUpdateDto dto)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"update ""Members"" set ""Name"" = @Name, ""MembershipDate"" = @MembershipDate, ""BirthYear"" = @BirthYear where ""Id"" = @Id";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Name", dto.Name);
            cmd.Parameters.AddWithValue("@MembershipDate", dto.MembershipDate);
            cmd.Parameters.AddWithValue("@BirthYear", dto.BirthYear);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;

        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @" delete from ""Members"" where ""Id"" = @Id";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
    }
}
