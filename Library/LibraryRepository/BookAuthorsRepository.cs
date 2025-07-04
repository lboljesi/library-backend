using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using LibraryModels;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace LibraryRepository
{
    public class BookAuthorsRepository : IBookAuthorsRepository
    {
        private readonly string _connectionString;

        public BookAuthorsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Postgres");
        }

        public async Task<List<BookAuthors>> GetBookAuthorsAsync()
        {
            var bookAuthors = new List<BookAuthors>();

            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"SELECT ba.""Id"", ba.""BookId"", ba.""AuthorId"", b.""Id"", b.""Title"", a.""Id"",a.""FirstName"", a.""LastName"" 
                                              FROM ""BookAuthors"" ba 
                                              LEFT JOIN ""Books"" b ON ba.""BookId""=b.""Id"" LEFT JOIN ""Authors"" a ON ba.""AuthorId""=a.""Id""", conn);

            using var reader = await cmd.ExecuteReaderAsync();


            while (reader.Read())
            {
                bookAuthors.Add(new BookAuthors
                {
                    Id = reader.GetGuid(0),
                    BookId = reader.IsDBNull(1) ? null : reader.GetGuid(1),
                    AuthorId = reader.IsDBNull(2) ? null : reader.GetGuid(2),

                    Book = reader.IsDBNull(3) ? null : new Books
                    {
                        Id = reader.GetGuid(3),
                        Title = reader.GetString(4)
                    },

                    Author = reader.IsDBNull(5) ? null : new Authors
                    {
                        Id = reader.GetGuid(5),
                        FirstName = reader.GetString(6),
                        LastName = reader.GetString(7)
                    }

                });

            }
            return bookAuthors;


        }

        public async Task<BookAuthors> GetBookAuthorsByIdAsync(Guid id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"SELECT ba.""Id"", ba.""BookId"", ba.""AuthorId"", b.""Id"", b.""Title"", a.""Id"",a.""FirstName"", a.""LastName"" 
                                              FROM ""BookAuthors"" ba 
                                              LEFT JOIN ""Books"" b ON ba.""BookId""=b.""Id"" LEFT JOIN ""Authors"" a ON ba.""AuthorId""=a.""Id"" WHERE ba.""Id""=@id", conn);

            cmd.Parameters.AddWithValue("Id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (reader.Read())
            {
                var bookAuthor = new BookAuthors
                {
                    Id = reader.GetGuid(0),
                    BookId = reader.IsDBNull(1) ? null : reader.GetGuid(1),
                    AuthorId = reader.IsDBNull(2) ? null : reader.GetGuid(2),

                    Book = reader.IsDBNull(3) ? null : new Books
                    {
                        Id = reader.GetGuid(3),
                        Title = reader.GetString(4)
                    },

                    Author = reader.IsDBNull(5) ? null : new Authors
                    {
                        Id = reader.GetGuid(5),
                        FirstName = reader.GetString(6),
                        LastName = reader.GetString(7)
                    }

                };
                return bookAuthor;
            }
            return null;

        }

        public async Task<BookAuthors> UpdateBookAuthorAsync(BookAuthors bookAuthor)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"UPDATE ""BookAuthors"" SET ""BookId""=@BookId, ""AuthorId""=@AuthorId WHERE ""Id""=@Id", conn);

            cmd.Parameters.AddWithValue("Id", bookAuthor.Id);
            cmd.Parameters.AddWithValue("BookId", bookAuthor.BookId.HasValue ? bookAuthor.BookId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("AuthorId", bookAuthor.AuthorId.HasValue ? bookAuthor.AuthorId.Value : DBNull.Value);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            if (rowsAffected != 0)
            {
                return await (GetBookAuthorsByIdAsync(bookAuthor.Id));
            }
            else
            {
                return null;
            }




        }

        public async Task<BookAuthors> CreateBookAuthorsAsync(BookAuthors bookAuthors)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"INSERT INTO ""BookAuthors"" (""BookId"",""AuthorId"") VALUES (@BookId, @AuthorId) 
                                            RETURNING ""Id"", ""BookId"", ""AuthorId""", conn);


            cmd.Parameters.AddWithValue("BookId", bookAuthors.BookId.HasValue ? bookAuthors.BookId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("AuthorId", bookAuthors.AuthorId.HasValue ? bookAuthors.AuthorId.Value : DBNull.Value);

            using var reader = await cmd.ExecuteReaderAsync();

            if (reader.Read())
            {
                var newbookAuthors = new BookAuthors
                {
                    Id = reader.GetGuid(0),
                    BookId = reader.IsDBNull(1) ? null : reader.GetGuid(1),
                    AuthorId = reader.IsDBNull(2) ? null : reader.GetGuid(2)
                };
                return newbookAuthors;
            }
            return null;


        }

        public async Task<bool> DeleteBookAuthorsAsync(Guid Id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"DELETE FROM ""BookAuthors"" WHERE ""Id""=@Id", conn);

            cmd.Parameters.AddWithValue("Id", Id);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            if (rowsAffected != 0)
            {
                return true;

            }
            else
            {
                return false;
            }


        }

        public async Task<List<Books>> GetAllBooksByAuthorAsync(Guid id)
        {

            var booksbyAuthor = new List<Books>();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"SELECT b. * FROM ""BookAuthors"" ba  JOIN ""Books"" b ON ba.""BookId"" = b.""Id"" WHERE ""AuthorId"" = @Id", conn);

            cmd.Parameters.AddWithValue("Id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                booksbyAuthor.Add(new Books
                {
                    Id = reader.GetGuid(0),
                    Title = reader.GetString(1),
                    Isbn = reader.GetString(2),
                    PublishedYear = reader.GetInt32(3),
                    Price = reader.GetInt32(4)
                });

            }
            return booksbyAuthor;
            
        }

        public async Task <List<Authors>> GetAllAuthorsByBookAsync (Guid id)
        {
            var authorsbyBook = new List<Authors>();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"SELECT a. * FROM ""BookAuthors"" ba 
                                              JOIN ""Authors"" a ON ba.""AuthorId""= a.""Id"" WHERE ""BookId""=@Id", conn);

            cmd.Parameters.AddWithValue("Id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                authorsbyBook.Add(new Authors
                {
                    Id = reader.GetGuid(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2)

                });



            }
            return authorsbyBook;



        }
        
    }
}
