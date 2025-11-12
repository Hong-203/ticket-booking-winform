using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TicketBookingStaffApp.DAL.repositories
{
    public enum SeatBookingStatus
    {
        Empty,
        Booked
    }

    public class SeatInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public SeatBookingStatus Status { get; set; }
        public string UserId { get; set; }
    }

    public class SeatBookRepository
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task<List<SeatInfo>> GetSeatsForShowtimeAsync(string movieId, string hallId, string showtimeId)
        {
            string url = $"https://ticket-booking-web-7ool.onrender.com/apis/seats/available?movie_id={movieId}&hall_id={hallId}&showtime_id={showtimeId}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            var seatsFromApi = JsonConvert.DeserializeObject<List<ApiSeat>>(json);
            var seats = new List<SeatInfo>();

            foreach (var s in seatsFromApi)
            {
                seats.Add(new SeatInfo
                {
                    Id = s.Id,
                    Name = s.Name,
                    UserId = s.UserId,
                    Status = s.Status == "empty" ? SeatBookingStatus.Empty : SeatBookingStatus.Booked
                });
            }

            return seats;
        }

        private class ApiSeat
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Status { get; set; }
            public string UserId { get; set; }
        }
    }
}
