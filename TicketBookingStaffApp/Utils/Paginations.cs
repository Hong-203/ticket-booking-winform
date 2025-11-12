using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketBookingStaffApp.Utils
{
    public static class Paginations
    {
        public static int CalculateOffset(int page, int pageSize)
        {
            if (page < 1) page = 1;
            return (page - 1) * pageSize;
        }

        public static int CalculateTotalPages(int totalItems, int pageSize)
        {
            if (pageSize <= 0) return 1;
            return (int)Math.Ceiling((double)totalItems / pageSize);
        }
    }
}
