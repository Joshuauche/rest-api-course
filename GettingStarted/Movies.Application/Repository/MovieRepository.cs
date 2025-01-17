using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repository
{
    public class MovieRepository : IMovieRepository
    {

        private readonly IDbConnectionFactory _dbConnectionFactory;

        public MovieRepository(IDbConnectionFactory connectionFactory)
        {
            _dbConnectionFactory = connectionFactory;
        }

        public async Task<bool> CreateAsync(Movie movie)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            var result = await connection.ExecuteAsync(new CommandDefinition(
                """
                insert into movies (id, slug, title, yearofrelease)
                values(@Id, @Slug, @Title, @YearOfRelease)
                """, movie, transaction));

            if (result > 0)
            {
                foreach (var genre in movie.Genres)
                {
                    await connection.ExecuteAsync(new CommandDefinition(
                        """
                            insert into genres_new (movieId, name)
                            values (@MovieId, @Name)
                            """,
                            new { MovieId = movie.Id, Name = genre }, transaction));
                }
            }

            transaction.Commit();
            return result > 0;
        }

        public async Task<bool> DeleteByIdAsync(Guid id)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            await connection.ExecuteAsync(new CommandDefinition(
                """
                delete from genres_new where movieId = @id
                """, new { id }, transaction
                ));

            var result = await connection.ExecuteAsync(new CommandDefinition(
                """
                delete from movies where id = @id
                """, new { id }, transaction
                ));

            transaction.Commit();

            return result > 0;
        }

        public async Task<bool> ExistsByIdAsync(Guid id)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();
            return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
                """
                select count(1) from movies where id = @id
                """, new { id }
            ));
        }

        public async Task<IEnumerable<Movie>> GetAllAsync()
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();
            var result = await connection.QueryAsync(new CommandDefinition(
                """
                select
                    m.*,
                    string_agg(g.name, ',') as genres
                from movies as m
                inner join genres_new as g
                    on m.id = g.movieId
                GROUP BY m.id, m.slug, m.title, m.yearofrelease
                """
                ));


            // Map the result to the Movie objects
            return result.Select(x => new Movie
            {
                // Assign values while ensuring proper handling of nulls
                Id = x.id ?? Guid.NewGuid(),  // Use a default Guid if null
                Title = x.title ?? string.Empty,  // Use empty string if title is null
                YearOfRelease = x.yearofrelease ?? 0,  // Use default value 0 if null
                Genres = string.IsNullOrEmpty(x.genres) ? new List<string>() : Enumerable.ToList(x.genres.Split(','))  // Split genres string into a list
            });
        }

        public async Task<Movie?> GetByIdAsync(Guid id)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();
            var movie = await connection.QuerySingleOrDefaultAsync<Movie>(new CommandDefinition(
                """
                select * from movies where id = @id
                """, new { id }));

            if (movie == null)
                return null;

            var genres = await connection.QueryAsync<string>(new CommandDefinition(
                """
                select name from genres_new where movieId = @id
                """, new { id }));

            foreach (var item in genres)
            {
                movie.Genres.Add(item);
            }

            return movie;

        }

        public async Task<Movie?> GetBySlugAsync(string slug)
        {
            // open connection
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();
            var movie = await connection.QueryFirstOrDefaultAsync<Movie>(new CommandDefinition(
                """
                select * from movies where slug = @slug
                """, new { slug }
               ));

            if (movie == null) return null;

            var genres = await connection.QueryAsync<string>(new CommandDefinition(
                """
                select name from genres_new where movieId = @id
                """, new { id = movie.Id }
                ));


            foreach (var item in genres)
            {
                movie.Genres.Add(item);
            }

            return movie;
        }

        public async Task<bool> UpdateAsync(Movie movie)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            await connection.ExecuteAsync(new CommandDefinition(
                """
                delete from genres_new where movieId = @id
                """, new { id = movie.Id }, transaction
                ));

            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    """
                    insert into genres_new (movieId, name)
                    values(@MovieId, @Name)
                    """, new { movieId = movie.Id, Name = genre }, transaction
                    ));
            }

            var result = await connection.ExecuteAsync(new CommandDefinition(
                """
                update movies set slug = @Slug, title = @title, yearofrelease = @yearOfRelease
                where id = @Id
                """, movie, transaction
                ));
            transaction.Commit();
            return result > 0;

        }


    }
}
