using LibraryModels;
using LibraryRepository.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryRepository
{
    public class LoanRepository : ILoanRepository
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;   

        public LoanRepository(IConfiguration configuration, ILogger<LoanRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("Postgres");
            _logger = logger;
        }

        public async Task<LoanResult> GetAllLoansAsync(
        int page = 1,
        int pageSize = 10,
        string? sortBy = "LoanDate",
        string? sortDirection = "asc",
        string? search = null,
        bool? isReturned = null)
            {
            var result = new LoanResult
            {
                Loans = new List<Loan>(),
                TotalCount = 0
            };

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var offset = (page - 1) * pageSize;

            var whereClauses = new List<string>();
            if (!string.IsNullOrWhiteSpace(search))
            {
                whereClauses.Add(@"(LOWER(b.""Title"") LIKE LOWER(@search) OR LOWER(m.""Name"") LIKE LOWER(@search))");
            }
            if (isReturned.HasValue)
            {
                if (isReturned.Value)
                    whereClauses.Add(@"l.""ReturnedDate"" IS NOT NULL");
                else
                    whereClauses.Add(@"l.""ReturnedDate"" IS NULL");
            }

            var where = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

            var sortColumn = sortBy switch
            {
                "LoanDate" => "l.\"LoanDate\"",
                "ReturnedDate" => "l.\"ReturnedDate\"",
                "MustReturn" => "l.\"MustReturn\"",
                "Title" => "b.\"Title\"",
                "Name" => "m.\"Name\"",
                _ => "l.\"LoanDate\""
            };

            var sortOrder = sortDirection?.ToLower() == "desc" ? "DESC" : "ASC";

            // ----- 1. Query for total count -----
            var countQuery = $@"
SELECT COUNT(*)
FROM ""Loans"" l
INNER JOIN ""Books"" b ON l.""BookId"" = b.""Id""
INNER JOIN ""Members"" m ON l.""MemberId"" = m.""Id""
{where}";

            await using (var countCommand = new NpgsqlCommand(countQuery, connection))
            {
                if (!string.IsNullOrWhiteSpace(search))
                    countCommand.Parameters.AddWithValue("@search", $"%{search}%");

                result.TotalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
            }

            // ----- 2. Query for paginated loans -----
            var query = $@"
SELECT 
    l.""Id"",
    l.""BookId"",
    l.""MemberId"",
    l.""LoanDate"",
    l.""ReturnedDate"",
    l.""MustReturn"",
    b.""Id"" AS ""BookId"",
    b.""Title"",
    b.""Isbn"",
    b.""PublishedYear"",
    b.""Price"",
    m.""Id"" AS ""MemberId"",
    m.""Name"",
    m.""MembershipDate"",
    m.""BirthYear""
FROM ""Loans"" l
INNER JOIN ""Books"" b ON l.""BookId"" = b.""Id""
INNER JOIN ""Members"" m ON l.""MemberId"" = m.""Id""
{where}
ORDER BY {sortColumn} {sortOrder}
LIMIT @pageSize OFFSET @offset";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@pageSize", pageSize);
            command.Parameters.AddWithValue("@offset", offset);
            if (!string.IsNullOrWhiteSpace(search))
                command.Parameters.AddWithValue("@search", $"%{search}%");

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var loan = new Loan
                {
                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                    BookId = reader.GetGuid(reader.GetOrdinal("BookId")),
                    MemberId = reader.GetGuid(reader.GetOrdinal("MemberId")),
                    LoanDate = reader.GetDateTime(reader.GetOrdinal("LoanDate")),
                    ReturnedDate = reader.IsDBNull(reader.GetOrdinal("ReturnedDate"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("ReturnedDate")),
                    MustReturn = reader.GetDateTime(reader.GetOrdinal("MustReturn")),
                    Book = new Books
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("BookId")),
                        Title = reader.GetString(reader.GetOrdinal("Title")),
                        Isbn = reader.GetString(reader.GetOrdinal("Isbn")),
                        PublishedYear = reader.GetInt32(reader.GetOrdinal("PublishedYear")),
                        Price = reader.GetInt32(reader.GetOrdinal("Price"))
                    },
                    Member = new MemberWithoutLoansREST
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("MemberId")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        MembershipDate = reader.GetDateTime(reader.GetOrdinal("MembershipDate")),
                        BirthYear = reader.GetInt32(reader.GetOrdinal("BirthYear"))
                    }
                };

                result.Loans.Add(loan);
            }

            return result;
        }



        public async Task<Loan?> GetLoanByIdAsync(Guid id)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
        SELECT 
            l.""Id"",
            l.""BookId"",
            l.""MemberId"",
            l.""LoanDate"",
            l.""ReturnedDate"",
            l.""MustReturn"",
    
            b.""Title"" AS BookTitle,
            b.""Isbn"",
            b.""PublishedYear"",
            b.""Price"",
    
            m.""Id"" AS MemberId,
            m.""Name"" AS MemberName,
            m.""MembershipDate"",
            m.""BirthYear""
        FROM ""Loans"" l
        INNER JOIN ""Books"" b ON l.""BookId"" = b.""Id""
        INNER JOIN ""Members"" m ON l.""MemberId"" = m.""Id""
        WHERE l.""Id"" = @id";

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                await using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    _logger.LogInformation("Uspješno pronađena posudba s ID-jem {LoanId}.", id);

                    return new Loan
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("Id")),
                        BookId = reader.GetGuid(reader.GetOrdinal("BookId")),
                        MemberId = reader.GetGuid(reader.GetOrdinal("MemberId")),
                        LoanDate = reader.GetDateTime(reader.GetOrdinal("LoanDate")),
                        ReturnedDate = reader.IsDBNull(reader.GetOrdinal("ReturnedDate"))
                            ? null
                            : reader.GetDateTime(reader.GetOrdinal("ReturnedDate")),
                        MustReturn = reader.GetDateTime(reader.GetOrdinal("MustReturn")),

                        Book = new Books
                        {
                            Id = reader.GetGuid(reader.GetOrdinal("BookId")),
                            Title = reader.GetString(reader.GetOrdinal("BookTitle")),
                            Isbn = reader.GetString(reader.GetOrdinal("Isbn")),
                            PublishedYear = reader.GetInt32(reader.GetOrdinal("PublishedYear")),
                            Price = reader.GetInt32(reader.GetOrdinal("Price"))
                        },

                        Member = new MemberWithoutLoansREST
                        {
                            Id = reader.GetGuid(reader.GetOrdinal("MemberId")),
                            Name = reader.GetString(reader.GetOrdinal("MemberName")),
                            MembershipDate = reader.GetDateTime(reader.GetOrdinal("MembershipDate")),
                            BirthYear = reader.GetInt32(reader.GetOrdinal("BirthYear"))
                        }
                    };
                }

                _logger.LogWarning("Posudba s ID-jem {LoanId} nije pronađena.", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška prilikom dohvaćanja posudbe s ID-jem {LoanId}.", id);
                throw;
            }
        }



        public async Task<Loan> AddLoanAsync(CreateLoanDto loanDto)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // 1. Provjeri postoji li već aktivna posudba (bez ReturnedDate) za istu knjigu i člana
                var checkQuery = @"
            SELECT COUNT(*) 
            FROM ""Loans"" 
            WHERE ""BookId"" = @bookId 
              AND ""MemberId"" = @memberId 
              AND ""ReturnedDate"" IS NULL
        ";

                await using var checkCmd = new NpgsqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@bookId", loanDto.BookId);
                checkCmd.Parameters.AddWithValue("@memberId", loanDto.MemberId);

                var existingCount = (long)await checkCmd.ExecuteScalarAsync();

                if (existingCount > 0)
                {
              
                    throw new InvalidOperationException("Korisnik već ima aktivnu posudbu za ovu knjigu.");
                }

                // 2. Ako nema aktivne posudbe, nastavi sa unosom
                var insertQuery = @"
            INSERT INTO ""Loans"" 
            (""BookId"", ""MemberId"", ""LoanDate"", ""ReturnedDate"", ""MustReturn"") 
            VALUES (@bookId, @memberId, @loanDate, @returnedDate, @mustReturn)
            RETURNING ""Id""
        ";

                await using var insertCmd = new NpgsqlCommand(insertQuery, connection);
                insertCmd.Parameters.AddWithValue("@bookId", loanDto.BookId);
                insertCmd.Parameters.AddWithValue("@memberId", loanDto.MemberId);
                insertCmd.Parameters.AddWithValue("@loanDate", loanDto.LoanDate);
                insertCmd.Parameters.AddWithValue("@returnedDate", loanDto.ReturnedDate ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@mustReturn", loanDto.MustReturn);

                var result = await insertCmd.ExecuteScalarAsync();
                var newId = (Guid)result;

                var createdLoan = new Loan
                {
                    Id = newId,
                    BookId = loanDto.BookId,
                    MemberId = loanDto.MemberId,
                    LoanDate = loanDto.LoanDate,
                    ReturnedDate = loanDto.ReturnedDate,
                    MustReturn = loanDto.MustReturn,
                    Book = null,
                    Member = null
                };

                _logger.LogInformation("Posudba je uspješno dodana s ID - jem {LoanId}", createdLoan.Id);

                return createdLoan;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška prilikom dodavanja posudbe ");
                throw;
            }
        }




        public async Task UpdateLoanAsync(Guid id, LoanUpdateDTO loan)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                UPDATE ""Loans"" 
                SET   
                    ""LoanDate"" = @loanDate,
                    ""ReturnedDate"" = @returnedDate,   
                    ""MustReturn"" = @mustReturn
                WHERE ""Id"" = @id;";

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@loanDate", loan.LoanDate);
                command.Parameters.AddWithValue("@returnedDate", loan.ReturnedDate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@mustReturn", loan.MustReturn);
                command.Parameters.AddWithValue("@id", id);


                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    _logger.LogWarning($"Posuba s ID -jem {id} nije pronađena");
                    throw new KeyNotFoundException($"Posudba nije pronađena");
                }

                _logger.LogInformation($"Posudba s ID- jem {id} uspješno je ažurirana");
            }

            catch (Exception ex) {

                _logger.LogError($"Greška prilikom ažuriranja člana s ID - jem {id}");

            }
            
        }


        public async Task DeleteLoanAsync(Guid id)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"DELETE FROM ""Loans"" WHERE ""Id"" = @id";

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                var affectedRows = await command.ExecuteNonQueryAsync();
                if (affectedRows == 0)
                {
                    _logger.LogWarning($"Posuba s ID -jem {id} nije pronađena");
                    throw new KeyNotFoundException("Posudba nije pronađena");
                }

                _logger.LogInformation($"Posudba s ID - jem {id} uspješno obrisana");
            }

            catch (Exception ex) {

                _logger.LogError($"Greška prilikom brisanja posudbe s ID - jem {id}");
            }
            
        }
    }

    }

