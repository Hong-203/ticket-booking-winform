using MySql.Data.MySqlClient;
using System;
using System.Data;
using TicketBookingStaffApp.DAL.database;

namespace TicketBookingStaffApp.DAL.repositories
{
    public class ShowtimeRepository
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public DataTable GetShowtimesByMovieId(string movieId)
        {
            string query = @"
                SELECT 
                    s.id AS showtime_id,
                    s.showtime_date,
                    s.movie_start_time,
                    s.show_type,
                    s.price_per_seat,
                    h.id AS hall_id, 
                    h.name AS hall_name,
                    t.name AS theatre_name,
                    t.location
                FROM showtimes s
                JOIN shown_in si ON s.id = si.showtime_id
                JOIN hall h ON si.hall_id = h.id
                JOIN theatre t ON h.theatre_id = t.id
                WHERE si.movie_id = @movieId
                ORDER BY s.showtime_date, s.movie_start_time;
            ";

            MySqlParameter[] parameters = {
                new MySqlParameter("@movieId", MySqlDbType.VarChar) { Value = movieId }
            };

            return dbHelper.ExecuteQuery(query, parameters);
        }
    }
}
