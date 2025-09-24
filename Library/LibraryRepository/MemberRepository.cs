using LibraryModels;
using LibraryQuerying;
using LibraryRepository.Common;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Globalization;
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

            while (await reader.ReadAsync())
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
                return new MemberDto
                {
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

        public async Task<PagedResultMember<MemberDto>> GetPagedAsync(SortablePaginationQuery q)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var page = q.Page < 1 ? 1 : q.Page;
            var pageSize = q.PageSize < 1 ? 10 : q.PageSize;
            
            var sortKey = (q.SortBy ?? "name").ToLowerInvariant();

            string sortColumn = sortKey switch
            {
                "birthyear" => "\"BirthYear\"",
                "membershipdate" => "\"MembershipDate\"",
                _ => "\"Name\""
            };
            string orderDir = q.Desc ? "DESC" : "ASC";

            var where = new StringBuilder("WHERE 1 = 1");

            bool hasSearch = !string.IsNullOrWhiteSpace(q.Search);
            if(hasSearch)
            {
                where.Append(@" and ""Name"" ILIKE @search");
            }
            var items = new List<MemberDto>();

            var sql = $@"
                SELECT ""Id"", ""Name"", ""MembershipDate"", ""BirthYear""
                FROM ""Members""
                {where}
                ORDER BY {sortColumn} {orderDir}
                LIMIT @limit OFFSET @offset;";



            await using var cmd = new NpgsqlCommand(sql, conn);
            if(hasSearch)
            {
                cmd.Parameters.AddWithValue("@search", NpgsqlDbType.Text).Value = $"%{q.Search}%";
            }
            cmd.Parameters.AddWithValue("@limit", pageSize);
            cmd.Parameters.AddWithValue("@offset", (page - 1) * pageSize);

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    items.Add(new MemberDto
                    {
                        Id = reader.GetGuid(0),
                        Name = reader.GetString(1),
                        MembershipDate = reader.GetDateTime(2),

                        BirthYear = reader.GetInt32(3)
                    });
                }
            }


            var countSql = $@"SELECT COUNT(*) FROM ""Members"" {where};";
            int totalCount;
            await using var countCmd = new NpgsqlCommand(countSql, conn);
            if (hasSearch)
                countCmd.Parameters.Add("@search", NpgsqlDbType.Text).Value = $"%{q.Search}%";
            var obj = await countCmd.ExecuteScalarAsync();
            totalCount = Convert.ToInt32(obj);

            return new PagedResultMember<MemberDto>(items, totalCount);
        }
        
        public async Task<List<LoanDto>> GetLoansByMemberIdAsync(Guid memberId)
        {
            var loans = new List<LoanDto>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
                select l.""Id"", l.""BookId"", l.""MemberId"", l.""LoanDate"", l.""ReturnedDate"", l.""MustReturn"", b.""Title"", b.""Isbn""
                from ""Loans"" l
                join ""Books"" b on b.""Id"" = l.""BookId""
                where l.""MemberId"" = @memberId
                order by l.""LoanDate"" desc";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@memberId", memberId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while(await reader.ReadAsync())
            {
                loans.Add(new LoanDto
                {
                    Id = reader.GetGuid(0),
                    BookId = reader.GetGuid(1),
                    MemberId = reader.GetGuid(2),
                    LoanDate = reader.GetDateTime(3),
                    ReturnedDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                    MustReturn = reader.GetDateTime(5),
                    BookTitle = reader.GetString(6),
                    Isbn = reader.GetString(7)
                });
            }

            return loans;
        }

    }
}
