using System;

namespace TicketBookingStaffApp.Models
{
    public class Showtime
    {
        public string Id { get; set; }
        public DateTime ShowtimeDate { get; set; }
        public string MovieStartTime { get; set; }
        public string ShowType { get; set; }
        public decimal PricePerSeat { get; set; }
        public string HallId { get; set; }       // thêm dòng này
        public string HallName { get; set; }
        public string TheatreName { get; set; }
        public string Location { get; set; }
    }

}
