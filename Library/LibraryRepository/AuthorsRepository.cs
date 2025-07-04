using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LibraryModels;
using LibraryQuerying;
using LibraryRepository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Npgsql;
using NpgsqlTypes;




namespace LibraryRepository
{
    public class AuthorsRepository : IAuthorsRepository
    {
        private readonly string _connectionString;

        public AuthorsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Postgres");

        }
        public async Task<List<Authors>> GetAuthorsAsync(AuthorsQuery query)
        {
            var authors = new List<Authors>();

            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = new StringBuilder(@" SELECT 
                                        a.""Id"", 
                                        a.""FirstName"", 
                                        a.""LastName"", 
                                        STRING_AGG(b.""Title"", ', ') AS ""BookTitles""
                                        FROM ""Authors"" a
                                        LEFT JOIN ""BookAuthors"" ba ON a.""Id"" = ba.""AuthorId""
                                        LEFT JOIN ""Books"" b ON ba.""BookId"" = b.""Id""
                                        WHERE 1=1
                                        ");

            // coalesce u query-u zbog provjere u readeru, s obzirom da pratim jel null, nema potrebe za coalesce

            if (!string.IsNullOrWhiteSpace(query.FirstName))
                sql.Append(" AND \"FirstName\" ILIKE @FirstName");

            if (!string.IsNullOrWhiteSpace(query.LastName))
                sql.Append(" AND \"LastName\" ILIKE @LastName");

            sql.Append(" GROUP BY a.\"Id\", a.\"FirstName\", a.\"LastName\"");

            // Sorting
            string sortColumn = query.SortBy switch
            {
                "FirstName" => "\"FirstName\"",
                "LastName" => "\"LastName\"",
                _ => "\"LastName\""
            };

            string sortOrder = query.SortDesc ? "DESC" : "ASC";
            sql.Append($" ORDER BY {sortColumn} {sortOrder}");

            // Pagination
            sql.Append(" LIMIT @PageSize OFFSET @Offset");

            using var cmd = new NpgsqlCommand(sql.ToString(), conn);

            // Parameters
            if (!string.IsNullOrWhiteSpace(query.FirstName))
                cmd.Parameters.AddWithValue("@FirstName", NpgsqlDbType.Text, $"%{query.FirstName}%");

            if (!string.IsNullOrWhiteSpace(query.LastName))
                cmd.Parameters.AddWithValue("@LastName", NpgsqlDbType.Text, $"%{query.LastName}%");

            cmd.Parameters.AddWithValue("@PageSize", NpgsqlDbType.Integer, query.PageSize);
            cmd.Parameters.AddWithValue("@Offset", NpgsqlDbType.Integer, (query.Page - 1) * query.PageSize);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                authors.Add(new Authors
                {
                    Id = reader.GetGuid(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    BookTitles = reader.IsDBNull(3) ? null : reader.GetString(3)


                });
            }

            return authors;
        }


        public async Task<Authors> GetAuthorByIdAsync(Guid Id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"SELECT ""Id"", ""FirstName"", ""LastName""
                                             FROM ""Authors"" WHERE ""Id""=@Id", conn);

            cmd.Parameters.AddWithValue("Id", Id);
            using var reader = await cmd.ExecuteReaderAsync();

            if (reader.Read())
            {
                var author = new Authors
                {


                    Id = reader.GetGuid(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2)
                };
                return author;




            }
            return null;
        }
        public async Task<Authors> UpdateAuthorAsync(Authors author)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();



            using var cmd = new NpgsqlCommand(@"UPDATE ""Authors"" SET  ""FirstName""=@FirstName, ""LastName""=@LastName                                           
                                              WHERE ""Id""=@Id RETURNING ""Id"", ""FirstName"", ""LastName""", conn);

            cmd.Parameters.AddWithValue("Id", author.Id);
            cmd.Parameters.AddWithValue("FirstName", author.FirstName);
            cmd.Parameters.AddWithValue("LastName", author.LastName);


            using var reader = await cmd.ExecuteReaderAsync();

            if (reader.Read())
            {


                var updatedAuthor = new Authors
                {
                    Id = reader.GetGuid(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2)


                };
                return updatedAuthor;

            }
            return null;

        }

        public async Task<Authors> CreateAuthorAsync(Authors author)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"INSERT INTO ""Authors""(""FirstName"", ""LastName"") 
                                             VALUES(@FirstName, @LastName) 
                                             RETURNING ""Id"", ""FirstName"", ""LastName""", conn);



            cmd.Parameters.AddWithValue("FirstName", author.FirstName);
            cmd.Parameters.AddWithValue("LastName", author.LastName);

            using var reader = await cmd.ExecuteReaderAsync();


            if (reader.Read())
            {
                var newAuthor = new Authors
                {
                    Id = reader.GetGuid(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),

                };
                return newAuthor;
            }
            return null;


        }

        public async Task<bool> DeleteAuthor(Guid Id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();


            using var cmd = new NpgsqlCommand(@"DELETE FROM ""Authors"" WHERE ""Id""=@Id", conn);

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
    }
}
