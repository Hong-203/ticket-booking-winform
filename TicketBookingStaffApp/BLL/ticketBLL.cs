using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketBookingStaffApp.DAL.repositories;
using TicketBookingStaffApp.Models;

namespace TicketBookingStaffApp.BLL
{
    public class TicketBLL
    {
        private readonly ticketRepository _ticketRepository = new ticketRepository();

        /// <summary>
        /// Lấy chi tiết vé (Movie, Hall, Seat Info) từ API/Service.
        /// </summary>
        /// <param name="ticketId">ID của vé cần xem chi tiết.</param>
        /// <returns>Đối tượng TicketDetailsDto.</returns>
        public async Task<TicketDetailsDto> GetTicketDetailsAsync(string ticketId)
        {
            if (string.IsNullOrEmpty(ticketId))
            {
                throw new ArgumentException("Mã vé không được rỗng.", nameof(ticketId));
            }

            return await _ticketRepository.GetTicketDetailsAsync(ticketId);
        }
    }
}