using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TicketBookingStaffApp.Models;

namespace TicketBookingStaffApp.DAL.repositories
{
    internal class ticketRepository
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private const string BASE_URL = "http://localhost:3001/apis/tickets/";

        public async Task<TicketDetailsDto> GetTicketDetailsAsync(string ticketId)
        {
            if (string.IsNullOrEmpty(ticketId))
            {
                throw new ArgumentException("Ticket ID không được rỗng.", nameof(ticketId));
            }

            string url = $"{BASE_URL}{ticketId}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                var ticketDetails = JsonConvert.DeserializeObject<TicketDetailsDto>(json);

                if (ticketDetails == null)
                {
                    throw new InvalidOperationException("Không thể giải mã dữ liệu vé từ API.");
                }

                return ticketDetails;
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404"))
            {
                throw new HttpRequestException($"Không tìm thấy vé với ID: {ticketId}. Vui lòng kiểm tra lại.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy chi tiết vé: {ex.Message}", ex);
            }
        }
    }
}