using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketBookingStaffApp.DAL.repositories;
using TicketBookingStaffApp.Models;

namespace TicketBookingStaffApp.BLL
{
    public class SeatBLL
    {
        private readonly SeatBookRepository _seatRepository = new SeatBookRepository();

        /// <summary>
        /// Lấy danh sách ghế cho một suất chiếu cụ thể
        /// </summary>
        /// <param name="movieId">ID phim</param>
        /// <param name="hallId">ID phòng chiếu</param>
        /// <param name="showtimeId">ID suất chiếu</param>
        /// <returns>Danh sách ghế kèm trạng thái</returns>
            public Task<List<SeatInfo>> GetSeatsForShowtimeAsync(string movieId, string hallId, string showtimeId)
            {
                return _seatRepository.GetSeatsForShowtimeAsync(movieId, hallId, showtimeId);
            }
    }
}
