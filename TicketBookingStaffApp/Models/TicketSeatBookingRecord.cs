using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketBookingStaffApp.Models
{
    public class TicketSeatBookingRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string TicketId { get; set; }
        public string SeatBookingId { get; set; }
    }
}
