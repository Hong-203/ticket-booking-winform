using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketBookingStaffApp.Models
{
    public class Movie
    {
        public string Id { get; set; }          // UUID
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public string Language { get; set; }
        public string Synopsis { get; set; }
        public decimal Rating { get; set; }
        public string Duration { get; set; }
        public string TopCast { get; set; }
        public DateTime? ReleaseDate { get; set; }

        public string Genres { get; set; }
        public string Directors { get; set; }

    }
}
