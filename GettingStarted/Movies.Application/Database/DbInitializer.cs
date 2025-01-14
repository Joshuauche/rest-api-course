using System;
using Dapper;

namespace Movies.Application.Database
{
    public class DbInitializer
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        public DbInitializer(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task InitializeAsync()
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();


            //Create table if not exists

            await connection.ExecuteAsync("""
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[movies]') AND type = N'U')
                BEGIN
                    CREATE TABLE movies (
                        id UNIQUEIDENTIFIER PRIMARY KEY,
                        slug VARCHAR(255) NOT NULL,
                        title VARCHAR(255) NOT NULL,
                        yearofrelease INT NOT NULL
                    );
                END;
                """
            );

            //Create unique index if not exists
            await connection.ExecuteAsync("""
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'movies_slug_idx' AND object_id = OBJECT_ID(N'[dbo].[movies]'))
                BEGIN
                    CREATE UNIQUE INDEX movies_slug_idx ON movies(slug);
                END;
                """);

            await connection.ExecuteAsync("""
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[genres_new]') AND type = N'U')
                BEGIN
                    CREATE TABLE genres_new (
                        movieId UNIQUEIDENTIFIER references movies(id),
                        name varchar(225) not null,

                        CONSTRAINT FK_Genres_Movies FOREIGN KEY (movieId) REFERENCES movies(id)
                    );
                END;
                """
            );
        }
    }
}
