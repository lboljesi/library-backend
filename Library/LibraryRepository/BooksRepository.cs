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
        public async Task<List<Books>> GetBooksAsync(BooksQuery query)
        {
            var books = new List<Books>();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = new StringBuilder(@"SELECT b.""Id"", b.""Title"", b.""Isbn"", b.""PublishedYear"", b.""Price"", STRING_AGG(a.""FirstName""|| ' ' || a.""LastName"",', ') AS ""BookAuthors""
                                        FROM ""Books""b LEFT JOIN ""BookAuthors"" ba ON b.""Id"" = ba.""BookId"" 
                                        LEFT JOIN ""Authors"" a ON ba.""AuthorId"" = a.""Id"" WHERE 1=1");

            if (!string.IsNullOrWhiteSpace(query.Title))
                sql.Append(" AND \"Title\" ILIKE @Title");

            if (query.PublishedYear.HasValue)
                sql.Append(" AND \"PublishedYear\" = @PublishedYear");

            sql.Append(" GROUP BY b.\"Id\", b.\"Title\", b.\"Isbn\", b.\"PublishedYear\", b.\"Price\"");



            string sortColumn = query.SortBy switch
            {
                "Title" => "\"Title\"",
                "PublishedYear" => "\"PublishedYear\"",
                "Isbn"=> "\"Isbn\"",
                "Price" => "\"Price\"",
                _ => "\"Title\""
            };

            string sortOrder = query.SortDesc ? "DESC" : "ASC";
            sql.Append($" ORDER BY {sortColumn} {sortOrder}");
            sql.Append(" LIMIT @PageSize OFFSET @Offset");

            using var cmd = new NpgsqlCommand(sql.ToString(), conn);

            if (!string.IsNullOrWhiteSpace(query.Title))
                cmd.Parameters.AddWithValue("@Title", NpgsqlDbType.Text, $"%{query.Title}%");

            if (query.PublishedYear.HasValue)
                cmd.Parameters.AddWithValue("@PublishedYear", query.PublishedYear.Value);

            cmd.Parameters.AddWithValue("@PageSize", query.PageSize);
            cmd.Parameters.AddWithValue("@Offset", (query.Page - 1) * query.PageSize);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                books.Add(new Books
                {
                    Id = reader.GetGuid(0),
                    Title = reader.GetString(1),
                    Isbn = reader.GetString(2),
                    PublishedYear = reader.GetInt32(3),
                    Price = reader.GetInt32(4),
                    BookAuthors = reader.IsDBNull(5)? null: reader.GetString(5),
                    

                });
            }

            return books;
        }

        public async Task<Books> GetBookByIdAsync(Guid Id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"SELECT ""Id"", ""Title"", ""Isbn"", ""PublishedYear"", ""Price"" 
                                             FROM ""Books"" WHERE ""Id""=@Id", conn);

            cmd.Parameters.AddWithValue("Id", Id);
            using var reader = await cmd.ExecuteReaderAsync();

            if (reader.Read())
            {
                var book = new Books
                {


                    Id = reader.GetGuid(0),
                    Title = reader.GetString(1),
                    Isbn = reader.GetString(2),
                    PublishedYear = reader.GetInt32(3),
                    Price = reader.GetInt32(4)
                };
                return book;




            }
            return null;
        }
        public async Task<Books> UpdateBookAsync(Books book)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();



            using var cmd = new NpgsqlCommand(@"UPDATE ""Books"" SET  ""Title""=@Title, ""Isbn""=@Isbn, 
                                              ""PublishedYear""=@PublishedYear, ""Price""=@Price 
                                              WHERE ""Id""=@Id RETURNING ""Id"", ""Title"", ""Isbn"", ""PublishedYear"",   
                                                ""Price""", conn);

            cmd.Parameters.AddWithValue("Id", book.Id);
            cmd.Parameters.AddWithValue("Title", book.Title);
            cmd.Parameters.AddWithValue("Isbn", book.Isbn);
            cmd.Parameters.AddWithValue("PublishedYear", book.PublishedYear);
            cmd.Parameters.AddWithValue("Price", book.Price);

            using var reader = await cmd.ExecuteReaderAsync();

            if (reader.Read())
            {


                var newBook = new Books
                {
                    Id = reader.GetGuid(0),
                    Title = reader.GetString(1),
                    Isbn = reader.GetString(2),
                    PublishedYear = reader.GetInt32(3),
                    Price = reader.GetInt32(4)


                };
                return newBook;

            }
            return null;

        }

        public async Task<Books> CreateBookAsync(Books book)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"INSERT INTO ""Books""(""Title"", ""Isbn"", ""PublishedYear"", ""Price"") 
                                             VALUES(@Title, @Isbn, @PublishedYear, @Price) 
                                             RETURNING ""Id"", ""Title"", ""Isbn"", ""PublishedYear"", ""Price""", conn);



            cmd.Parameters.AddWithValue("Title", book.Title);
            cmd.Parameters.AddWithValue("Isbn", book.Isbn);
            cmd.Parameters.AddWithValue("PublishedYear", book.PublishedYear);
            cmd.Parameters.AddWithValue("Price", book.Price);
            using var reader = await cmd.ExecuteReaderAsync();


            if (reader.Read())
            {
                var newBook = new Books
                {
                    Id = reader.GetGuid(0),
                    Title = reader.GetString(1),
                    Isbn = reader.GetString(2),
                    PublishedYear = reader.GetInt32(3),
                    Price = reader.GetInt32(4),
                };
                return newBook;
            }
            return null;


        }

        public async Task<bool> DeleteBook(Guid Id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();


            using var cmd = new NpgsqlCommand(@"DELETE FROM ""Books"" WHERE ""Id""=@Id", conn);

            cmd.Parameters.AddWithValue("Id", Id);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected > 0)
            {
                return true;
            }
            else
            {
                return false;
            }


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
