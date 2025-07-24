using LibraryModels;
using LibraryQuerying;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Runtime.InteropServices.Marshalling;
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

        public async Task<List<BookDto>> GetAllBooksAsync(BookQuery query)
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

        private async Task<List<Guid>> GetPagedBookIdsAsync(NpgsqlConnection conn, BookQuery query, int offset)
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
            if(query.PublishedYear.HasValue)
            {
                idQuery.AppendLine(@"AND b.""PublishedYear"" = @publishedYear");
            }
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
            if(query.PublishedYear.HasValue)
            {
                idCmd.Parameters.AddWithValue("publishedYear", query.PublishedYear);
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

        public async Task<int> CountBooksAsync(BookQuery query)
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
            if(query.PublishedYear.HasValue)
            {
                sql.AppendLine(@"and b.""PublishedYear"" = @publishedYear");
                cmd.Parameters.AddWithValue("@publishedYear", query.PublishedYear);
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
            cmd.Parameters.AddWithValue("@id", id);
            var result = await cmd.ExecuteScalarAsync();
            return result is not null;
        }

        public async Task<Guid> AddBookAsync(BooksCreateUpdate dto)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            Guid bookId = Guid.NewGuid();
            var bookQuery = @"
                INSERT INTO ""Books"" (""Id"",""Title"", ""Isbn"", ""PublishedYear"", ""Price"")
                VALUES(@BookId, @Title, @Isbn, @PublishedYear, @Price)";

            await using var bookCmd = new NpgsqlCommand(bookQuery, conn);
            bookCmd.Parameters.AddWithValue("@BookId", bookId);
            bookCmd.Parameters.AddWithValue("@Title", dto.Title);
            bookCmd.Parameters.AddWithValue("@Isbn", dto.Isbn);
            bookCmd.Parameters.AddWithValue("@PublishedYear", dto.PublishedYear);
            bookCmd.Parameters.AddWithValue("@Price", (object?)dto.Price ?? DBNull.Value);
            await bookCmd.ExecuteNonQueryAsync();

            if (dto.AuthorIds != null && dto.AuthorIds.Count > 0)
            {
                var authorQuery = @"
                    INSERT INTO ""BookAuthors"" (""BookId"", ""AuthorId"")
                    values (@BookId, @AuthorId);";
                foreach(var authorId in dto.AuthorIds)
                {
                    await using var authorCmd = new NpgsqlCommand(authorQuery, conn);
                    authorCmd.Parameters.AddWithValue("@BookId", bookId);
                    authorCmd.Parameters.AddWithValue("@AuthorId",authorId);

                    await authorCmd.ExecuteNonQueryAsync();
                }
            }

            if (dto.CategoryIds != null && dto.CategoryIds.Count > 0)
            {
                var categoryQuery = @"
                    INSERT INTO ""BookCategories"" (""BookId"", ""CategoryId"")
                    values (@BookId, @CategoryId);";
                foreach (var categoryId in dto.CategoryIds)
                {
                    await using var categoryCmd = new NpgsqlCommand(categoryQuery, conn);
                    categoryCmd.Parameters.AddWithValue("@BookId", bookId);
                    categoryCmd.Parameters.AddWithValue("@CategoryId", categoryId);

                    await categoryCmd.ExecuteNonQueryAsync();
                }
            }
            return bookId;

        }
        
        public async Task<BookDto?> GetBookByIdAsync(Guid bookId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string bookQuery = @"
            SELECT ""Id"", ""Title"", ""Isbn"", ""PublishedYear"", ""Price"" from ""Books"" where ""Id"" = @BookId";

            await using var bookCmd = new NpgsqlCommand(bookQuery, conn);
            bookCmd.Parameters.AddWithValue("@BookId", bookId);
            BookDto? book = null;

            await using (var reader = await bookCmd.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync())
                    return null;

                book = new BookDto
                {
                    Id = reader.GetGuid(0),
                    Title = reader.GetString(1),
                    Isbn = reader.GetString(2),
                    PublishedYear = reader.GetInt32(3),
                    Price = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    Authors = new List<AuthorDto>(),
                    Categories = new List<Category>()
                };
            }

            const string authorQuery = @"
                select a.""Id"", a.""FirstName"", a.""LastName""
                from ""Authors"" a
                join ""BookAuthors"" ba on ba.""AuthorId"" = a.""Id""
                where ba.""BookId"" = @BookId;";
            await using var authorCmd = new NpgsqlCommand(authorQuery, conn);
            authorCmd.Parameters.AddWithValue("@BookId", bookId);

            await using (var authorReader = await authorCmd.ExecuteReaderAsync())
            {
                while (await authorReader.ReadAsync())
                {
                    book.Authors.Add(new AuthorDto
                    {
                        Id = authorReader.GetGuid(0),
                        FirstName = authorReader.GetString(1),
                        LastName = authorReader.GetString(2)
                    });
                }
            }

            const string categoryQuery = @"
                select c.""Id"", c.""Name""
                from ""Categories"" c
                join ""BookCategories"" bc on bc.""CategoryId"" = c.""Id""
                where bc.""BookId"" = @BookId";
            await using var categoryCmd = new NpgsqlCommand(categoryQuery, conn);
            categoryCmd.Parameters.AddWithValue("@BookId", bookId);
            await using (var categoryReader = await categoryCmd.ExecuteReaderAsync())
            {
                while (await categoryReader.ReadAsync())
                {
                    book.Categories.Add(new Category
                    {
                        Id = categoryReader.GetGuid(0),
                        Name = categoryReader.GetString(1)
                    });
                }
            }

            return book;
        }
        public async Task<bool> DeleteBookAsync(Guid id)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            const string sql = @"delete from ""Books"" where ""Id"" = @Id";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id",id);
            var affected = await cmd.ExecuteNonQueryAsync();
            return affected > 0;
        }

        public async Task<bool> DeleteBookBulkAsync(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
                return false;
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            const string sql = @"delete from ""Books"" where ""Id"" = any(@Ids);";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Ids", ids);
            var affected = await cmd.ExecuteNonQueryAsync();
            return affected > 0;
        }
        
        public async Task<bool> UpdateBookAsync(Guid id, BookUpdateDto dto) {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"update ""Books"" set ""Title"" = @Title, ""Isbn"" = @Isbn, ""PublishedYear"" = @PublishedYear, ""Price""=@Price
                where ""Id"" = @Id";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Title", dto.Title);
            cmd.Parameters.AddWithValue("@Isbn", dto.Isbn);
            cmd.Parameters.AddWithValue("@PublishedYear", dto.PublishedYear);
            cmd.Parameters.AddWithValue("@Price", dto.Price.HasValue ? dto.Price.Value : DBNull.Value);
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
        public async Task<List<AuthorDto>> GetAuthorsByBookIdAsync(Guid id)
        {
            var result = new List<AuthorDto>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
                select a.""Id"", a.""FirstName"", a.""LastName""
                from ""Authors"" a
                join ""BookAuthors"" ba on a.""Id"" = ba.""AuthorId""
                where ba.""BookId"" = @Id";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new AuthorDto
                {
                    Id = reader.GetGuid(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2)
                });

            }
            return result;

        }

        public async Task<List<Category>> GetCategoriesByBookIdAsync(Guid id)
        {
            var result = new List<Category>();

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
                select c.""Id"", c.""Name""
                from ""Categories"" c
                join ""BookCategories"" bc on bc.""CategoryId"" = c.""Id""
                where bc.""BookId"" = @Id;
            ";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new Category { 
                    Id = reader.GetGuid(0),
                    Name=reader.GetString(1)
                });
            }
            return result;
        }
    }
}
