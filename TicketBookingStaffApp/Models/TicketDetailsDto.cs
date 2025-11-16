using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TicketBookingStaffApp.Models
{
    public class SeatInfoDto
    {
        public string id { get; set; }
        public string name { get; set; } // Ví dụ: "A2"
        public string description { get; set; } // Ví dụ: "Ghế đơn"
    }

    public class TicketSeat
    {
        public string id { get; set; }
        public SeatInfoDto seat { get; set; }
        public string status { get; set; } // Ví dụ: "booked"
    }

    public class MovieDto
    {
        public string id { get; set; }
        public string name { get; set; } // Tên Phim
        public string image_path { get; set; } // URL Poster
        public string language { get; set; }
        public string synopsis { get; set; } // Tóm tắt
        public string rating { get; set; }
        public string duration { get; set; }
        public string top_cast { get; set; } // Diễn viên chính
        public string release_date { get; set; }
    }

    public class TheatreDto
    {
        public string id { get; set; }
        public string name { get; set; } // Tên rạp (CGV Thanh Xuan)
        public string locationDetails { get; set; } // Địa chỉ (Thanh Xuân - HN)
    }

    public class HallDto
    {
        public string id { get; set; }
        public string name { get; set; } // Tên phòng (Room 1)
        public TheatreDto theatre { get; set; }
    }

    public class ShowtimeDto
    {
        public string id { get; set; }
        public string movie_start_time { get; set; } // Giờ bắt đầu (00:00)
        public string show_type { get; set; } // Loại hình (2D)
        public string showtime_date { get; set; } // Ngày chiếu (2025-06-04)
        public decimal price_per_seat { get; set; }
    }

    // --- DTO Chính ---
    public class TicketDetailsDto
    {
        public string id { get; set; }
        public string total_price { get; set; } // Tổng tiền
        public List<TicketSeat> seats { get; set; }
        public MovieDto movie { get; set; }
        public HallDto hall { get; set; }
        public ShowtimeDto showtime { get; set; }
    }
}