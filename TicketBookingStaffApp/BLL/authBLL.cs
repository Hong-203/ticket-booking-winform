using System;
using System.Data;
using TicketBookingStaffApp.DAL.repositories;
using TicketBookingStaffApp.Utils;

namespace TicketBookingStaffApp.BLL
{
    public class authBLL
    {
        private readonly authRepository _repo = new authRepository();

        public bool Login(string identifier, string password)
        {
            DataRow user = _repo.Login(identifier, password);

            if (user == null)
                return false;

            // Gán thông tin user vào SessionManager
            SessionManager.SetUser(new createUser
            {
                Id = user["id"].ToString(),
                FullName = user["full_name"].ToString(),
                Email = user["email"].ToString(),
                Phone = user["phone_number"].ToString(),
                Role = user["account_type"].ToString()
            });

            return true;
        }
    }
}
