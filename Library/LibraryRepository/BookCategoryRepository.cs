using LibraryModels;
using LibraryQuerying;
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
    public class BookCategoryRepository : IBookCategoryRepository
    {
        private readonly string _connectionString;
        public BookCategoryRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Postgres") ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");
        }

        public async Task<BookCategory?> CreateAsync(CreateBookCategoryDto dto)
        {
            var newId = Guid.NewGuid();

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"INSERT INTO ""BookCategories"" (""Id"", ""BookId"", ""CategoryId"")
                VALUES (@id, @bookId, @categoryId)
                RETURNING ""Id"", ""BookId"", ""CategoryId"";";

            await using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("id", newId);
            cmd.Parameters.AddWithValue("bookId", dto.BookId);
            cmd.Parameters.AddWithValue("categoryId", dto.CategoryId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new BookCategory
                {
                    Id = reader.GetGuid(0),
                    BookId = reader.GetGuid(1),
                    CategoryId = reader.GetGuid(2)
                };
            }

            throw new Exception("Failed to create book-category relation.");

        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"DELETE FROM ""BookCategories"" WHERE ""Id"" = @id;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);

            var affected = await cmd.ExecuteNonQueryAsync();

            return affected > 0;
        }
        public async Task<BookCategory?> GetByIdAsync(Guid id)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"select * from ""BookCategories"" where ""Id"" = @id";
            //using moraš koristiti jer automatski zatvara resurse nakon korištenja
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new BookCategory {
                    Id = reader.GetGuid(0),
                    BookId = reader.GetGuid(1),
                    CategoryId = reader.GetGuid(2)
                };
            }

            return null;
        }
        public async Task<List<BookCategoryJOIN>> GetAllAsync(SortablePaginationQuery query)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = new StringBuilder(@"
                SELECT bc.""Id"", b.""Title"", c.""Name""
                FROM ""BookCategories"" bc 
                JOIN ""Books"" b ON b.""Id"" = bc.""BookId"" 
                JOIN ""Categories"" c ON c.""Id"" = bc.""CategoryId"" 
                WHERE 1 = 1
            ");
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                sql.Append(@" AND (b.""Title"" ILIKE @search OR c.""Name"" ILIKE @search)");
            }

            var orderBy = @"b.""Title""";
            if (query.SortBy?.ToLower() == "category")
                orderBy = @"c.""Name""";

            sql.Append(query.Desc ? $" ORDER BY {orderBy} DESC" : $" ORDER BY {orderBy} ASC");
            sql.Append(" OFFSET @offset LIMIT @limit");
            await using var cmd = new NpgsqlCommand(sql.ToString(), conn);

            if (!string.IsNullOrEmpty(query.Search))
                cmd.Parameters.AddWithValue("search", $"%{query.Search}%");

            cmd.Parameters.AddWithValue("offset", (query.Page - 1) * query.PageSize);
            cmd.Parameters.AddWithValue("limit", query.PageSize);

            await using var reader = await cmd.ExecuteReaderAsync();

            var bookCategories = new List<BookCategoryJOIN>();
            while (await reader.ReadAsync())
            {
                bookCategories.Add(new BookCategoryJOIN
                {
                    Id = reader.GetGuid(0),
                    BookTitle = reader.GetString(1),
                    CategoryName = reader.GetString(2)
                });
            }
            return bookCategories;
        }
        public async Task<bool> ExistsAsync(Guid bookId, Guid categoryId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"select 1 from ""BookCategories"" where ""BookId"" = @bookId and ""CategoryId"" = @categoryId limit 1;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("bookId", bookId);
            cmd.Parameters.AddWithValue("categoryId", categoryId);

            var result = await cmd.ExecuteScalarAsync();
            return result is not null;
        }
        public async Task<List<CategoryWithRelation>> CreateManyAsync(Guid bookId, List<Guid> categoryIds)
        {
            var result = new List<CategoryWithRelation>();

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            foreach (var categoryId in categoryIds)
            {
                if (await ExistsAsync(bookId, categoryId)) continue;

                var id = Guid.NewGuid();

                var sql = @"
                    WITH inserted AS (
                        INSERT INTO ""BookCategories"" (""Id"", ""BookId"", ""CategoryId"")
                        VALUES (@id, @bookId, @categoryId)
                        RETURNING ""Id"", ""CategoryId""
                    )
                    SELECT 
                        inserted.""Id"" AS BookCategoryRelationId,
                        c.""Id"" AS CategoryId,
                        c.""Name"" AS CategoryName
                    FROM inserted
                    JOIN ""Categories"" c ON c.""Id"" = inserted.""CategoryId"";";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("id", id);
                cmd.Parameters.AddWithValue("bookId", bookId);
                cmd.Parameters.AddWithValue("categoryId", categoryId);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    result.Add(new CategoryWithRelation
                    {
                        BookCategoryRelationId = reader.GetGuid(0),
                        CategoryId = reader.GetGuid(1),
                        CategoryName = reader.GetString(2)
                    });
                }

                await reader.CloseAsync();
            }

            return result;
        }


        public async Task<List<Books>> GetBooksForCategoryAsync(Guid categoryId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
                SELECT b.""Id"", b.""Title"", b.""Isbn"", b.""PublishedYear"", b.""Price""
                FROM ""Books"" b
                INNER JOIN ""BookCategories"" bc on b.""Id"" = bc.""BookId""
                WHERE bc.""CategoryId"" = @categoryId";

            var books = new List<Books>();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("categoryId", categoryId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                books.Add(new Books
                {
                    Id = reader.GetGuid(0),
                    Title = reader.GetString(1),
                    Isbn = reader.GetString(2),
                    PublishedYear = reader.GetInt32(3),
                    Price = reader.GetInt32(4)
                });
            }
            return books;
        }

        public async Task<PagedResult<BookWithCategoriesDto>> GetGroupedByBookAsync(PaginationQuery query)
        {
            var result = new Dictionary<Guid, BookWithCategoriesDto>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // 1. Dohvati ID-eve knjiga za trenutnu stranicu
            var idsSql = new StringBuilder(@"
                SELECT DISTINCT b.""Id"", b.""Title""
                FROM ""Books"" b
                LEFT JOIN ""BookCategories"" bc ON b.""Id"" = bc.""BookId""
                LEFT JOIN ""Categories"" c ON bc.""CategoryId"" = c.""Id""
                WHERE 1 = 1");

            if (!string.IsNullOrWhiteSpace(query.Search))
                idsSql.Append(@" AND LOWER(b.""Title"") LIKE @search");

            idsSql.Append($@" ORDER BY b.""Title"" {(query.Desc ? "DESC" : "ASC")}");
            idsSql.Append(" LIMIT @pageSize OFFSET @offset");

            var bookIds = new List<Guid>();

            await using (var idCmd = new NpgsqlCommand(idsSql.ToString(), conn))
            {
                if (!string.IsNullOrWhiteSpace(query.Search))
                    idCmd.Parameters.AddWithValue("search", $"%{query.Search.ToLower()}%");

                idCmd.Parameters.AddWithValue("pageSize", query.PageSize);
                idCmd.Parameters.AddWithValue("offset", (query.Page - 1) * query.PageSize);

                await using var reader = await idCmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    bookIds.Add(reader.GetGuid(0));
            }

            if (!bookIds.Any())
            {
                return new PagedResult<BookWithCategoriesDto>
                {
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalCount = 0,
                    Items = new()
                };
            }

            // 2. Dohvati podatke za te ID-eve
            var dataSql = @"
                SELECT 
                    b.""Id"" AS BookId,
                    b.""Title"" AS BookTitle,
                    c.""Id"" AS CategoryId,
                    c.""Name"" AS CategoryName,
                    bc.""Id"" AS RelationId
                FROM ""Books"" b
                LEFT JOIN ""BookCategories"" bc ON b.""Id"" = bc.""BookId""
                LEFT JOIN ""Categories"" c ON bc.""CategoryId"" = c.""Id""
                WHERE b.""Id"" = ANY(@ids)
                ORDER BY b.""Title"" " + (query.Desc ? "DESC" : "ASC");

            await using (var cmd = new NpgsqlCommand(dataSql, conn))
            {
                cmd.Parameters.AddWithValue("ids", bookIds);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var bookId = reader.GetGuid(0);
                    if (!result.ContainsKey(bookId))
                    {
                        result[bookId] = new BookWithCategoriesDto
                        {
                            BookId = bookId,
                            BookTitle = reader.GetString(1),
                            Categories = new List<CategoryWithRelation>()
                        };
                    }

                    if (!reader.IsDBNull(2))
                    {
                        result[bookId].Categories.Add(new CategoryWithRelation
                        {
                            CategoryId = reader.GetGuid(2),
                            CategoryName = reader.GetString(3),
                            BookCategoryRelationId = reader.GetGuid(4)
                        });
                    }
                }
            }

            // 3. Ukupan broj knjiga (ne ovisi o LIMIT/OFFSET)
            var totalCount = await CountGroupedBookAsync(query);

            return new PagedResult<BookWithCategoriesDto>
            {
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                Items = result.Values.ToList()
            };
        }



        public async Task<int> CountGroupedBookAsync(PaginationQuery query)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = new StringBuilder(@"SELECT COUNT(DISTINCT b.""Id"") FROM ""Books"" b
                LEFT JOIN ""BookCategories"" bc on b.""Id"" = bc.""BookId""
                LEFT JOIN ""Categories"" c on bc.""CategoryId"" = c.""Id"" WHERE 1 = 1");

            await using var cmd = new NpgsqlCommand(sql.ToString(), conn);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                sql.Append(@" AND LOWER(b.""Title"") LIKE @search");
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("search", $"%{query.Search.ToLower()}%");
            }
            else
            {
                cmd.CommandText = sql.ToString();
            }

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }


        public async Task<List<Category>> GetCategoriesWithoutBooksAsync()
        {
            var sql = @"
                SELECT c.*
                FROM ""Categories"" c
                LEFT JOIN ""BookCategories"" bc ON c.""Id"" = bc.""CategoryId""
                WHERE bc.""BookId"" IS NULL;";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            var result = new List<Category>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new Category
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1)
                });

            }
            return result;

        }

        public async Task<int> DeleteRelationsAsync(List<Guid> relationIds)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"DELETE FROM ""BookCategories"" WHERE ""Id"" = ANY(@ids)";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("ids", relationIds.ToArray());
            var affectedRows = await cmd.ExecuteNonQueryAsync();
            return affectedRows;
        }

        public async Task<List<BookWithAuthorsDto>> GetBooksWithAuthorsForCategoryAsync(Guid categoryId)
        {
            var result = new Dictionary<Guid, BookWithAuthorsDto>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
                SELECT
                    b.""Id"" as BookId,
                    b.""Title"",
                    b.""Isbn"",
                    b.""PublishedYear"",
                    b.""Price"",
                    a.""Id"" as AuthorId,
                    a.""FirstName"",
                    a.""LastName""
                FROM ""Books"" b
                INNER JOIN ""BookCategories"" bc ON b.""Id"" = bc.""BookId""
                LEFT JOIN ""BookAuthors"" ba ON ba.""BookId"" = b.""Id""
                LEFT JOIN ""Authors"" a ON a.""Id"" = ba.""AuthorId""
                WHERE bc.""CategoryId"" = @categoryId
                ORDER BY b.""Title""
                ";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("categoryId", categoryId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var bookId = reader.GetGuid(0);
                if(!result.ContainsKey(bookId))
                {
                    result[bookId] = new BookWithAuthorsDto
                    {
                        Id = bookId,
                        Title = reader.GetString(1),
                        Isbn = reader.GetString(2),
                        PublishedYear = reader.GetInt32(3),
                        Price = reader.GetInt32(4),
                        Authors = new List<AuthorDto>()
                    };
                }

                if(!reader.IsDBNull(5))
                {
                    result[bookId].Authors.Add(new AuthorDto
                    {
                        Id = reader.GetGuid(5),
                        FirstName = reader.GetString(6),
                        LastName = reader.GetString(7)
                    });
                }
            }
            return result.Values.ToList();

        }

    }
}

