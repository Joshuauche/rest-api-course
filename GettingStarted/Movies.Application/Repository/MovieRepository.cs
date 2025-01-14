using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repository
{
    public class MovieRepository : IMovieRepository
    {

        private readonly IDbConnectionFactory _connectionFactory;

        public MovieRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<bool> CreateAsync(Movie movie)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
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

        public Task<bool> DeleteByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Movie>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Movie?> GetByIdAsync(Guid id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
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
            using var connection = await _connectionFactory.CreateConnectionAsync();
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

        public Task<bool> UpdateAsync(Movie movie)
        {
            throw new NotImplementedException();
        }


    }
}
