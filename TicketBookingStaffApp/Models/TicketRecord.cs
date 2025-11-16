using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketBookingStaffApp.Models
{
    public class TicketRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public decimal SeatTotalPrice { get; set; }
        public decimal ConcessionTotalPrice { get; set; } = 0; // Giả định bằng 0 vì bạn chưa có logic Concession
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "BOOKED"; // Sử dụng string cho Enum
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
