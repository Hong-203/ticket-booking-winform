using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using TicketBookingStaffApp.DAL.database;
using TicketBookingStaffApp.Models;

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
        private readonly DatabaseHelper _dbHelper = new DatabaseHelper();
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        public class BookingResult
        {
            public TicketRecord Ticket { get; set; }
            public List<SeatBookingRecord> SeatBookings { get; set; }
        }

        public async Task<List<SeatInfo>> GetSeatsForShowtimeAsync(string movieId, string hallId, string showtimeId)
        {
            string url = $"http://localhost:3001/apis/seats/available?movie_id={movieId}&hall_id={hallId}&showtime_id={showtimeId}";

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

        public async Task<TicketRecord> CreateTicketAsync(
        List<SeatBookingRecord> seatBookings,
        string userId,
        decimal totalPrice)
        {
            // 1. Chuẩn bị Ticket Record
            var seatTotalPrice = totalPrice; // Giả sử tổng tiền ghế = totalPrice
            var totalFinalPrice = seatTotalPrice; // + Concession (hiện tại là 0)

            var ticketRecord = new TicketRecord
            {
                UserId = userId,
                SeatTotalPrice = seatTotalPrice,
                TotalPrice = totalFinalPrice
            };

            // 2. Thiết lập Transaction
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                MySqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // A. TẠO TICKET (Ticket)
                    string sqlInsertTicket = "INSERT INTO ticket " +
                                             "(id, user_id, seat_total_price, concession_total_price, total_price, status, created_at) " +
                                             "VALUES (@id, @user_id, @seat_total_price, @concession_total_price, @total_price, @status, @created_at)";

                    MySqlParameter[] ticketParams = new MySqlParameter[]
                    {
                    new MySqlParameter("@id", ticketRecord.Id),
                    new MySqlParameter("@user_id", ticketRecord.UserId),
                    new MySqlParameter("@seat_total_price", ticketRecord.SeatTotalPrice),
                    new MySqlParameter("@concession_total_price", ticketRecord.ConcessionTotalPrice),
                    new MySqlParameter("@total_price", ticketRecord.TotalPrice),
                    new MySqlParameter("@status", ticketRecord.Status),
                    new MySqlParameter("@created_at", ticketRecord.CreatedAt)
                    };

                    using (MySqlCommand cmdTicket = new MySqlCommand(sqlInsertTicket, conn, transaction))
                    {
                        cmdTicket.Parameters.AddRange(ticketParams);
                        await cmdTicket.ExecuteNonQueryAsync();
                    }

                    // B. TẠO LIÊN KẾT (TicketSeatBooking)
                    foreach (var sb in seatBookings)
                    {
                        var tsbRecord = new TicketSeatBookingRecord
                        {
                            TicketId = ticketRecord.Id,
                            SeatBookingId = sb.Id
                        };

                        string sqlInsertTSB = "INSERT INTO ticket_seat_booking (id, ticket_id, seat_booking_id) " +
                                              "VALUES (@id, @ticket_id, @seat_booking_id)";

                        MySqlParameter[] tsbParams = new MySqlParameter[]
                        {
                        new MySqlParameter("@id", tsbRecord.Id),
                        new MySqlParameter("@ticket_id", tsbRecord.TicketId),
                        new MySqlParameter("@seat_booking_id", tsbRecord.SeatBookingId)
                        };

                        using (MySqlCommand cmdTSB = new MySqlCommand(sqlInsertTSB, conn, transaction))
                        {
                            cmdTSB.Parameters.AddRange(tsbParams);
                            await cmdTSB.ExecuteNonQueryAsync();
                        }
                    }

                    // C. COMMIT TRANSACTION nếu tất cả đều thành công
                    transaction.Commit();
                    return ticketRecord;
                }
                catch (Exception ex)
                {
                    // D. ROLLBACK nếu có bất kỳ lỗi nào xảy ra
                    try { transaction.Rollback(); } catch (Exception rollbackEx) { /* Log rollback error */ }
                    throw new Exception($"Lỗi khi tạo Ticket/TicketSeatBooking: {ex.Message}");
                }
            }
        }

        // Trong class SeatBookRepository
        // ...

        /// <summary>
        /// Tạo các bản ghi SeatBooking trong Transaction đã mở.
        /// </summary>
        private async Task<List<SeatBookingRecord>> CreateSeatBookingsInTransactionAsync(
            MySqlConnection conn,
            MySqlTransaction transaction,
            List<string> seatIds,
            string movieId,
            string hallId,
            string showtimeId,
            string userId)
        {
            var createdBookings = new List<SeatBookingRecord>();

            foreach (var seatId in seatIds)
            {
                var newBooking = new SeatBookingRecord
                {
                    Id = Guid.NewGuid().ToString(),
                    MovieId = movieId,
                    HallId = hallId,
                    ShowtimeId = showtimeId,
                    SeatId = seatId,
                    UserId = userId,
                    Status = SeatBookingStatus.Booked,
                    CreatedAt = DateTime.Now
                };

                string sqlInsert = "INSERT INTO seat_booking " +
                                   "(id, movie_id, hall_id, showtime_id, seat_id, user_id, status, created_at) " +
                                   "VALUES (@id, @movie_id, @hall_id, @showtime_id, @seat_id, @user_id, @status, @created_at)";

                MySqlParameter[] parameters = new MySqlParameter[]
                {
            new MySqlParameter("@id", newBooking.Id),
            new MySqlParameter("@movie_id", newBooking.MovieId),
            new MySqlParameter("@hall_id", newBooking.HallId),
            new MySqlParameter("@showtime_id", newBooking.ShowtimeId),
            new MySqlParameter("@seat_id", newBooking.SeatId),
            new MySqlParameter("@user_id", newBooking.UserId),
            new MySqlParameter("@status", newBooking.Status.ToString()),
            new MySqlParameter("@created_at", newBooking.CreatedAt),
                };

                using (MySqlCommand cmd = new MySqlCommand(sqlInsert, conn, transaction))
                {
                    cmd.Parameters.AddRange(parameters);
                    await cmd.ExecuteNonQueryAsync();
                }

                createdBookings.Add(newBooking);
            }

            return createdBookings;
        }

        // Trong class SeatBookRepository
        // ...

        /// <summary>
        /// Tạo bản ghi Ticket trong Transaction đã mở.
        /// </summary>
        private async Task<TicketRecord> CreateTicketInTransactionAsync(
            MySqlConnection conn,
            MySqlTransaction transaction,
            string userId,
            decimal totalPrice)
        {
            var ticketRecord = new TicketRecord
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                SeatTotalPrice = totalPrice,
                ConcessionTotalPrice = 0, // Mặc định 0
                TotalPrice = totalPrice,
                Status = "BOOKED",
                CreatedAt = DateTime.Now
            };

            string sqlInsertTicket = "INSERT INTO ticket " +
                                     "(id, user_id, seat_total_price, concession_total_price, total_price, status, created_at) " +
                                     "VALUES (@id, @user_id, @seat_total_price, @concession_total_price, @total_price, @status, @created_at)";

            MySqlParameter[] ticketParams = new MySqlParameter[]
            {
        new MySqlParameter("@id", ticketRecord.Id),
        new MySqlParameter("@user_id", ticketRecord.UserId),
        new MySqlParameter("@seat_total_price", ticketRecord.SeatTotalPrice),
        new MySqlParameter("@concession_total_price", ticketRecord.ConcessionTotalPrice),
        new MySqlParameter("@total_price", ticketRecord.TotalPrice),
        new MySqlParameter("@status", ticketRecord.Status),
        new MySqlParameter("@created_at", ticketRecord.CreatedAt)
            };

            using (MySqlCommand cmdTicket = new MySqlCommand(sqlInsertTicket, conn, transaction))
            {
                cmdTicket.Parameters.AddRange(ticketParams);
                await cmdTicket.ExecuteNonQueryAsync();
            }

            return ticketRecord;
        }

        // Trong class SeatBookRepository
        // ...

        /// <summary>
        /// Tạo các bản ghi liên kết TicketSeatBooking trong Transaction đã mở.
        /// </summary>
        private async Task CreateTicketSeatBookingsInTransactionAsync(
            MySqlConnection conn,
            MySqlTransaction transaction,
            string ticketId,
            List<SeatBookingRecord> seatBookings)
        {
            foreach (var sb in seatBookings)
            {
                string sqlInsertTSB = "INSERT INTO ticket_seat_booking (id, ticket_id, seat_booking_id) " +
                                      "VALUES (@id, @ticket_id, @seat_booking_id)";

                MySqlParameter[] tsbParams = new MySqlParameter[]
                {
            new MySqlParameter("@id", Guid.NewGuid().ToString()),
            new MySqlParameter("@ticket_id", ticketId),
            new MySqlParameter("@seat_booking_id", sb.Id)
                };

                using (MySqlCommand cmdTSB = new MySqlCommand(sqlInsertTSB, conn, transaction))
                {
                    cmdTSB.Parameters.AddRange(tsbParams);
                    await cmdTSB.ExecuteNonQueryAsync();
                }
            }
        }

        // Trong class SeatBookRepository
        // ...

        /// <summary>
        /// Entry point để thực hiện toàn bộ quá trình đặt vé trong một Transaction duy nhất.
        /// </summary>
        public async Task<BookingResult> BookSeatsAndCreateTicketAsync(
            List<string> seatIds,
            string movieId,
            string hallId,
            string showtimeId,
            string userId,
            decimal totalPrice)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                MySqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // 1. Tạo Seat Bookings
                    List<SeatBookingRecord> seatBookings = await CreateSeatBookingsInTransactionAsync(
                        conn, transaction, seatIds, movieId, hallId, showtimeId, userId);

                    // 2. Tạo Ticket
                    TicketRecord ticket = await CreateTicketInTransactionAsync(
                        conn, transaction, userId, totalPrice);

                    // 3. Tạo Ticket Seat Bookings
                    await CreateTicketSeatBookingsInTransactionAsync(
                        conn, transaction, ticket.Id, seatBookings);

                    // 4. Commit nếu tất cả thành công
                    transaction.Commit();
                    return new BookingResult { Ticket = ticket, SeatBookings = seatBookings };
                }
                catch (Exception ex)
                {
                    // Rollback nếu có lỗi
                    try { transaction.Rollback(); } catch { /* Ignore */ }

                    // Xử lý lỗi trùng lặp (ví dụ)
                    if (ex.InnerException is MySqlException mySqlEx && mySqlEx.Number == 1062)
                    {
                        throw new Exception("Ghế đã được đặt bởi người khác. Vui lòng chọn lại.");
                    }

                    throw new Exception($"Lỗi Transaction DB khi đặt vé: {ex.Message}");
                }
            }
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
