using BootcampApp.Repository.Common;
using LibraryModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace LibraryRepository
{
    public class MemberRepository : IMemberRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<MemberRepository> _logger;

        public MemberRepository(IConfiguration configuration, ILogger<MemberRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("Postgres");
            _logger = logger;
        }

        public async Task<MemberResult> GetAllMembersAsync(
    int page = 1,
    int pageSize = 10,
    string? sortBy = null,
    string? sortDirection = "asc",
    string? search = null,
    int? birthYear = null)
        {
            var members = new List<MemberREST>();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var whereClauses = new List<string>();
            if (!string.IsNullOrWhiteSpace(search))
                whereClauses.Add("LOWER(\"Name\") LIKE LOWER(@search)");
            if (birthYear.HasValue)
                whereClauses.Add("\"BirthYear\" = @birthYear");

            var where = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";
            var sortClause = !string.IsNullOrWhiteSpace(sortBy)
                ? $"ORDER BY \"{sortBy}\" {(sortDirection?.ToLower() == "desc" ? "DESC" : "ASC")}"
                : "";

            var offset = (page - 1) * pageSize;

            // -------- Total count query --------
            var countQuery = $@"
SELECT COUNT(*) FROM ""Members""
{where}";

            int totalCount;
            await using (var countCommand = new NpgsqlCommand(countQuery, connection))
            {
                if (!string.IsNullOrWhiteSpace(search))
                    countCommand.Parameters.AddWithValue("@search", $"%{search}%");
                if (birthYear.HasValue)
                    countCommand.Parameters.AddWithValue("@birthYear", birthYear.Value);

                totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
            }

            // -------- Paginated query --------
            var query = $@"
SELECT ""Id"", ""Name"", ""MembershipDate"", ""BirthYear"" 
FROM ""Members""
{where}
{sortClause}
LIMIT @pageSize OFFSET @offset";

            await using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@pageSize", pageSize);
                command.Parameters.AddWithValue("@offset", offset);

                if (!string.IsNullOrWhiteSpace(search))
                    command.Parameters.AddWithValue("@search", $"%{search}%");
                if (birthYear.HasValue)
                    command.Parameters.AddWithValue("@birthYear", birthYear.Value);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    members.Add(new MemberREST
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("Id")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        MembershipDate = reader.GetDateTime(reader.GetOrdinal("MembershipDate")),
                        BirthYear = reader.GetInt32(reader.GetOrdinal("BirthYear")),
                        Loans = new List<LoanREST>()
                    });
                }
            }

            // -------- Load loans for members --------
            if (members.Count > 0)
            {
                var memberDict = members.ToDictionary(m => m.Id);

                var loanQuery = @"
                        SELECT 
                            l.""Id"", l.""BookId"", l.""MemberId"", l.""LoanDate"", l.""ReturnedDate"", l.""MustReturn"",
                            b.""Id"" AS ""BookId"", b.""Title"" AS ""BookName"", b.""Isbn"", b.""PublishedYear"", b.""Price""
                        FROM ""Loans"" l
                        INNER JOIN ""Books"" b ON l.""BookId"" = b.""Id""
                        WHERE l.""MemberId"" = ANY(@memberIds)";

                await using var loanCommand = new NpgsqlCommand(loanQuery, connection);
                loanCommand.Parameters.AddWithValue("@memberIds", memberDict.Keys.ToArray());

                await using var loanReader = await loanCommand.ExecuteReaderAsync();
                while (await loanReader.ReadAsync())
                {
                    var memberId = loanReader.GetGuid(loanReader.GetOrdinal("MemberId"));

                    if (memberDict.TryGetValue(memberId, out var member))
                    {
                        var loanDto = new LoanREST
                        {
                            Id = loanReader.GetGuid(loanReader.GetOrdinal("Id")),
                            BookId = loanReader.GetGuid(loanReader.GetOrdinal("BookId")),
                            LoanDate = loanReader.GetDateTime(loanReader.GetOrdinal("LoanDate")),
                            ReturnedDate = loanReader.IsDBNull(loanReader.GetOrdinal("ReturnedDate"))
                                ? null
                                : loanReader.GetDateTime(loanReader.GetOrdinal("ReturnedDate")),
                            MustReturn = loanReader.GetDateTime(loanReader.GetOrdinal("MustReturn")),
                            Book = new Books
                            {
                                Id = loanReader.GetGuid(loanReader.GetOrdinal("BookId")),
                                Title = loanReader.GetString(loanReader.GetOrdinal("BookName")),
                                Isbn = loanReader.GetString(loanReader.GetOrdinal("Isbn")),
                                PublishedYear = loanReader.GetInt32(loanReader.GetOrdinal("PublishedYear")),
                                Price = loanReader.GetInt32(loanReader.GetOrdinal("Price"))
                            }
                        };

                        member.Loans.Add(loanDto);
                    }
                }
            }

            return new MemberResult
            {
                Members = members,
                TotalCount = totalCount
            };
        }




        public async Task<MemberREST?> GetMemberByIdAsync(Guid id)
        {
            try
            {
                MemberREST? member = null;

                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                            SELECT 
                                m.""Id"" AS MemberId,
                                m.""Name"" AS MemberName,
                                m.""MembershipDate"",
                                m.""BirthYear"",

                                l.""Id"" AS LoanId,
                                l.""BookId"",
                                l.""LoanDate"",
                                l.""ReturnedDate"",
                                l.""MustReturn"",

                                b.""Title"" AS BookName,
                                b.""Isbn"",
                                b.""PublishedYear"",
                                b.""Price""

                            FROM ""Members"" m
                            LEFT JOIN ""Loans"" l ON m.""Id"" = l.""MemberId""
                            LEFT JOIN ""Books"" b ON l.""BookId"" = b.""Id""
                            WHERE m.""Id"" = @id";

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    if (member == null)
                    {
                        member = new MemberREST
                        {
                            Id = reader.GetGuid(reader.GetOrdinal("MemberId")),
                            Name = reader.GetString(reader.GetOrdinal("MemberName")),
                            MembershipDate = reader.GetDateTime(reader.GetOrdinal("MembershipDate")),
                            BirthYear = reader.GetInt32(reader.GetOrdinal("BirthYear")),
                            Loans = new List<LoanREST>()
                        };
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("LoanId")))
                    {
                        var loan = new LoanREST
                        {
                            Id = reader.GetGuid(reader.GetOrdinal("LoanId")),
                            BookId = reader.GetGuid(reader.GetOrdinal("BookId")),
                            LoanDate = reader.GetDateTime(reader.GetOrdinal("LoanDate")),
                            ReturnedDate = reader.IsDBNull(reader.GetOrdinal("ReturnedDate"))
                                ? null
                                : reader.GetDateTime(reader.GetOrdinal("ReturnedDate")),
                            MustReturn = reader.GetDateTime(reader.GetOrdinal("MustReturn")),
                            Book = new Books
                            {
                                Id = reader.GetGuid(reader.GetOrdinal("BookId")),
                                Title = reader.GetString(reader.GetOrdinal("BookName")),
                                Isbn = reader.GetString(reader.GetOrdinal("Isbn")),
                                PublishedYear = reader.GetInt32(reader.GetOrdinal("PublishedYear")),
                                Price = reader.GetInt32(reader.GetOrdinal("Price"))
                            }
                        };

                        member.Loans.Add(loan);
                    }
                }

                _logger.LogInformation("Uspješno pronađen Member s ID - jem {MemberId}.", id);

                return member;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška prilikom dohvaćanja Membera s ID - jem {MemberId}.", id);
                throw;
            }
        }


        public async Task<Member> AddMemberAsync(Member member)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    INSERT INTO ""Members"" (""Name"", ""MembershipDate"", ""BirthYear"") 
                    VALUES (@name, @membershipDate, @birthYear) 
                    RETURNING ""Id"";";

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@name", member.Name ?? throw new ArgumentNullException(nameof(member.Name)));
                command.Parameters.AddWithValue("@membershipDate", member.MembershipDate);
                command.Parameters.AddWithValue("@birthYear", member.BirthYear);

                var result = await command.ExecuteScalarAsync();

                member.Id = (Guid)result;

                _logger.LogInformation("Member je uspješno dodan s ID - jem {MemberId}", member.Id);

                return member;
            }

            catch (Exception ex) {
                _logger.LogError(ex, "Greška prilikom dodavanja člana ");
                throw;
            }
            
        }


        public async Task UpdateMemberAsync(Guid id, Member member)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                UPDATE ""Members"" 
                SET ""Name"" = @name, ""MembershipDate"" = @membershipDate, ""BirthYear"" = @birthYear 
                WHERE ""Id"" = @id";

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@name", member.Name);
                command.Parameters.AddWithValue("@membershipDate", member.MembershipDate);
                command.Parameters.AddWithValue("@birthYear", member.BirthYear);
                command.Parameters.AddWithValue("@id", id);

                var affectedRows = await command.ExecuteNonQueryAsync();
                if (affectedRows == 0)
                {
                    throw new KeyNotFoundException($"Član s ID - jem nije pronađen '{id}'.");
                }

                _logger.LogInformation($"Ažuriran član uspješno s ID - jem {id}");

            }

            catch (Exception ex) {
                _logger.LogError(ex, $"Greška prilikom ažuriranja člana s ID {id}");
                throw;
            }
            
        }

        public async Task DeleteMemberAsync(Guid id)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"DELETE FROM ""Members"" WHERE ""Id"" = @id";

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                var affectedRows = await command.ExecuteNonQueryAsync();

                if (affectedRows == 0)
                {
                    _logger.LogWarning($"Član s ID {id} nije pronađen za brisanje");
                    throw new KeyNotFoundException($"Clan s ID - jem '{id}' nije pronađen");
                }

                _logger.LogInformation($"Član s ID - jem uspješno obrisan {id}");
            }

            catch (Exception ex) {
                _logger.LogError($"Greška prilikom brisanja člana s ID - jem {id}");
                
            }
            
        }
    }
}