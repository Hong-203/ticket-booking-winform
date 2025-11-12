using MySql.Data.MySqlClient;
using System;
using System.Data;
using TicketBookingStaffApp.DAL.database;
using BCrypt.Net; 

namespace TicketBookingStaffApp.DAL.repositories
{
    public class authRepository
    {
        private readonly DatabaseProvider _db = new DatabaseProvider();

        public DataRow Login(string identifier, string password)
        {
            bool isEmail = identifier.Contains("@");
            string query = isEmail
                            ? "SELECT * FROM user WHERE email = @identifier AND account_type = 'staff'"
                            : "SELECT * FROM user WHERE phone_number = @identifier AND account_type = 'staff'";

            object[] parameters = { new MySqlParameter("@identifier", identifier) };
            DataTable result = _db.ExecuteQuery(query, parameters);

            if (result.Rows.Count == 0)
                return null;

            DataRow user = result.Rows[0];
            string hashedPassword = user["password"].ToString();

            bool isPasswordValid = false;
            try
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi verify mật khẩu: {ex.Message}");
                return null;
            }

            if (isPasswordValid)
                return user;
            else
                return null;
        }
    }
}
