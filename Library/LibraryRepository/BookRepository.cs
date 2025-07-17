using LibraryModels;
using LibraryQuerying;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Text;




namespace LibraryRepository
{
    public class BookRepository : IBookRepository
    {
        private readonly string _connectionString;

        public BookRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Postgres")
                                ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");
        }

        public async Task<List<BookDto>> GetAllBooksAsync(SortablePaginationQuery query)
        {
            int offset = (query.Page - 1) * query.PageSize;
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var bookIds = await GetPagedBookIdsAsync(conn, query, offset);
            if (!bookIds.Any())
                return new List<BookDto>();
            var books = await GetBooksByIdsAsync(conn, bookIds);
            return bookIds.Select(id => books[id]).ToList();
        }

        private async Task<List<Guid>> GetPagedBookIdsAsync(NpgsqlConnection conn, SortablePaginationQuery query, int offset)
        {
            var idQuery = new StringBuilder(@"
                SELECT DISTINCT b.""Id"", b.""Title"", b.""PublishedYear""
                FROM ""Books"" b
                LEFT JOIN ""BookAuthors"" ba ON b.""Id"" = ba.""BookId""
                LEFT JOIN ""Authors"" a ON a.""Id"" = ba.""AuthorId""
                LEFT JOIN ""BookCategories"" bc ON b.""Id"" = bc.""BookId""
                LEFT JOIN ""Categories"" c ON c.""Id"" = bc.""CategoryId""
                WHERE 1=1    
            ");
            if(!string.IsNullOrWhiteSpace(query.Search))
            {
                idQuery.AppendLine(@"and (lower(b.""Title"") like @search or lower(b.""Isbn"") like @search)");
            }
            string orderColumn = query.SortBy?.ToLower() switch
            {
                "year" => @"b.""PublishedYear""",
                _ => @"b.""Title"""
            };
            idQuery.AppendLine($@"order by {orderColumn} {(query.Desc ? "DESC" : "ASC")}");
            idQuery.AppendLine("offset @offset limit @limit");

            var bookIds = new List<Guid>();
            await using var idCmd = new NpgsqlCommand(idQuery.ToString(), conn);
            idCmd.Parameters.AddWithValue("offset", offset);
            idCmd.Parameters.AddWithValue("limit", query.PageSize);

            if(!string.IsNullOrWhiteSpace(query.Search))
            {
                idCmd.Parameters.AddWithValue("search", $"%{query.Search.ToLower()}%");
            }

            await using var reader = await idCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                bookIds.Add(reader.GetGuid(0));
            }
            return bookIds;
        }

        private async Task<Dictionary<Guid, BookDto>> GetBooksByIdsAsync(NpgsqlConnection conn, List<Guid> bookIds)
        {
            var books = new Dictionary<Guid, BookDto>();
            var dataQuery = @"
                SELECT 
                    b.""Id"" AS ""BookId"",
                    b.""Title"",
                    b.""Isbn"",
                    b.""PublishedYear"",
                    b.""Price"",
                    a.""Id"" AS ""AuthorId"",
                    a.""FirstName"",
                    a.""LastName"",
                    c.""Id"" AS ""CategoryId"",
                    c.""Name"" AS ""CategoryName""
                FROM ""Books"" b
                LEFT JOIN ""BookAuthors"" ba ON b.""Id"" = ba.""BookId""
                LEFT JOIN ""Authors"" a ON a.""Id"" = ba.""AuthorId""
                LEFT JOIN ""BookCategories"" bc ON b.""Id"" = bc.""BookId""
                LEFT JOIN ""Categories"" c ON c.""Id"" = bc.""CategoryId""
                WHERE b.""Id"" = ANY (@bookIds)
            ";

            await using var cmd = new NpgsqlCommand(dataQuery, conn);
            cmd.Parameters.AddWithValue("bookIds", bookIds);

            await using var reader = await cmd.ExecuteReaderAsync();

            while(await reader.ReadAsync())
            {
                MapRowToBook(reader, books);
            }
            return books;
        }

        private void MapRowToBook(NpgsqlDataReader reader, Dictionary<Guid, BookDto> books)
        {
            var bookId = reader.GetGuid(0);
            if(!books.TryGetValue(bookId, out var book))
            {
                book = new BookDto
                {
                    Id = bookId,
                    Title = reader.GetString(1),
                    Isbn = reader.GetString(2),
                    PublishedYear = reader.GetInt32(3),
                    Price = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    Authors = new List<AuthorDto>(),
                    Categories = new List<Category>()
                };
                books.Add(bookId,book);
            }
            if(!reader.IsDBNull(5))
            {
                var authorId = reader.GetGuid(5);
                if(!book.Authors.Any(a => a.Id == authorId))
                {
                    book.Authors.Add(new AuthorDto
                    {
                        Id = authorId,
                        FirstName = reader.GetString(6),
                        LastName = reader.GetString(7)
                    });
                }
            }
            if (!reader.IsDBNull(8))
            {
                var categoryId = reader.GetGuid(8);
                if(!book.Categories.Any(c => c.Id == categoryId))
                {
                    book.Categories.Add(new Category { 
                        Id = categoryId,
                        Name = reader.GetString(9)
                    });
                }
            }
        }

        public async Task<int> CountBooksAsync(SortablePaginationQuery query)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = new StringBuilder(@"
                SELECT COUNT(DISTINCT b.""Id"")
                FROM ""Books"" b
                LEFT JOIN ""BookAuthors"" ba ON b.""Id"" = ba.""BookId""
                LEFT JOIN ""Authors"" a ON a.""Id"" = ba.""AuthorId""
                LEFT JOIN ""BookCategories"" bc ON b.""Id"" = bc.""BookId""
                LEFT JOIN ""Categories"" c ON bc.""CategoryId"" = c.""Id""
                WHERE 1=1
            ");
            await using var cmd = new NpgsqlCommand();
            cmd.Connection = conn;
            if (!string.IsNullOrEmpty(query.Search))
            {
                sql.AppendLine(@"and (lower(b.""Title"") like @search or lower(b.""Isbn"") like @search)");
                cmd.Parameters.AddWithValue("search", $"%{query.Search?.ToLower()}%");
            }

            cmd.CommandText = sql.ToString();
            
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
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
