using System;
using System.Collections.Generic;
using System.Data;
using TicketBookingStaffApp.DAL.database;
using TicketBookingStaffApp.Models;
using TicketBookingStaffApp.Utils;
using MySql.Data.MySqlClient;

namespace TicketBookingStaffApp.DAL.repositories
{
    public class movieRepository
    {
        private readonly DatabaseProvider dbProvider = new DatabaseProvider();

        /// <summary>
        /// Lấy danh sách phim có phân trang (UUID id)
        /// </summary>
        public List<Movie> GetAllMovies(int page = 1, int pageSize = 10)
        {
            List<Movie> movies = new List<Movie>();
            int offset = Paginations.CalculateOffset(page, pageSize);

            string query = @"
                SELECT id, name, image_path, language, synopsis, rating, duration, top_cast, release_date
                FROM movie
                ORDER BY release_date DESC
                LIMIT @Limit OFFSET @Offset;
            ";

            MySqlParameter[] parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Limit", pageSize),
                new MySqlParameter("@Offset", offset)
            };

            DataTable dt = dbProvider.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                movies.Add(new Movie
                {
                    Id = row["id"].ToString(),  // UUID string
                    Name = row["name"]?.ToString(),
                    ImagePath = row["image_path"]?.ToString(),
                    Language = row["language"]?.ToString(),
                    Synopsis = row["synopsis"]?.ToString(),
                    Rating = row["rating"] == DBNull.Value ? 0 : Convert.ToDecimal(row["rating"]),
                    Duration = row["duration"]?.ToString(),
                    TopCast = row["top_cast"]?.ToString(),
                    ReleaseDate = row["release_date"] == DBNull.Value
                        ? (DateTime?)null
                        : Convert.ToDateTime(row["release_date"])
                });
            }

            return movies;
        }

        /// <summary>
        /// Lấy tổng số phim (để tính tổng trang)
        /// </summary>
        public int GetTotalMovies()
        {
            string query = "SELECT COUNT(*) FROM movie;";
            object result = dbProvider.ExecuteScalar(query);
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// Lấy chi tiết phim theo ID (bao gồm đạo diễn và thể loại)
        /// </summary>
        public Movie GetMovieById(string movieId)
        {
            string query = @"
        SELECT 
            m.id, m.name, m.image_path, m.language, m.synopsis,
            m.rating, m.duration, m.top_cast, m.release_date,
            GROUP_CONCAT(DISTINCT mg.genre SEPARATOR ', ') AS genres,
            GROUP_CONCAT(DISTINCT md.director SEPARATOR ', ') AS directors
                FROM movie m
                LEFT JOIN movie_genre mg ON m.id = mg.movie_id
                LEFT JOIN movie_directors md ON m.id = md.movie_id
                WHERE m.id = @MovieId
                GROUP BY m.id;
            ";

            MySqlParameter[] parameters = new MySqlParameter[]
            {
                new MySqlParameter("@MovieId", movieId)
            };

            DataTable dt = dbProvider.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
                return null;

            DataRow row = dt.Rows[0];

            return new Movie
            {
                Id = row["id"].ToString(),
                Name = row["name"]?.ToString(),
                ImagePath = row["image_path"]?.ToString(),
                Language = row["language"]?.ToString(),
                Synopsis = row["synopsis"]?.ToString(),
                Rating = row["rating"] == DBNull.Value ? 0 : Convert.ToDecimal(row["rating"]),
                Duration = row["duration"]?.ToString(),
                TopCast = row["top_cast"]?.ToString(),
                ReleaseDate = row["release_date"] == DBNull.Value
                    ? (DateTime?)null
                    : Convert.ToDateTime(row["release_date"]),
                Genres = row["genres"]?.ToString(),
                Directors = row["directors"]?.ToString()
            };
        }

    }
}
