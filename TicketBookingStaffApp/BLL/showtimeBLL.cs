using System;
using System.Collections.Generic;
using System.Data;
using TicketBookingStaffApp.DAL.repositories;
using TicketBookingStaffApp.Models;

namespace TicketBookingStaffApp.BLL
{
    public class ShowtimeBLL
    {
        private readonly ShowtimeRepository _showtimeRepository = new ShowtimeRepository();

        /// <summary>
        /// Lấy danh sách lịch chiếu của một phim dựa theo MovieId (UUID)
        /// </summary>
        public List<Showtime> GetShowtimesByMovieId(string movieId)
        {
            DataTable dt = _showtimeRepository.GetShowtimesByMovieId(movieId);
            List<Showtime> showtimes = new List<Showtime>();

            foreach (DataRow row in dt.Rows)
            {
                Showtime showtime = new Showtime
                {
                    Id = row["showtime_id"].ToString(),
                    ShowtimeDate = Convert.ToDateTime(row["showtime_date"]),
                    MovieStartTime = row["movie_start_time"].ToString(),
                    ShowType = row["show_type"].ToString(),
                    PricePerSeat = Convert.ToDecimal(row["price_per_seat"]),
                    HallName = row["hall_name"].ToString(),
                    HallId = row["hall_id"].ToString(),
                    TheatreName = row["theatre_name"].ToString(),
                    Location = row["location"].ToString()
                };

                showtimes.Add(showtime);
            }

            return showtimes;
        }

        /// <summary>
        /// Lọc danh sách lịch chiếu theo ngày cụ thể
        /// </summary>
        public List<Showtime> GetShowtimesByDate(string movieId, DateTime date)
        {
            var allShowtimes = GetShowtimesByMovieId(movieId);
            return allShowtimes.FindAll(s => s.ShowtimeDate.Date == date.Date);
        }

        /// <summary>
        /// Lấy danh sách lịch chiếu trong khoảng ngày (tùy chọn)
        /// </summary>
        public List<Showtime> GetShowtimesInRange(string movieId, DateTime startDate, DateTime endDate)
        {
            var allShowtimes = GetShowtimesByMovieId(movieId);
            return allShowtimes.FindAll(s => s.ShowtimeDate.Date >= startDate.Date && s.ShowtimeDate.Date <= endDate.Date);
        }
    }
}
