using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketBookingStaffApp.DAL.repositories;

namespace TicketBookingStaffApp.Models
{
    public class SeatBookingRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Tạo ID khi tạo mới
        public string MovieId { get; set; }
        public string HallId { get; set; }
        public string ShowtimeId { get; set; }
        public string SeatId { get; set; } // Đây là Seat.Id
        public string UserId { get; set; } // Nhân viên đặt
        public SeatBookingStatus Status { get; set; } = SeatBookingStatus.Booked; // Mặc định là Booked
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
