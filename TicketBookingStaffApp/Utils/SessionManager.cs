namespace TicketBookingStaffApp.Utils
{
    public static class SessionManager
    {
        public static createUser CurrentUser { get; private set; }

        public static void SetUser(createUser user)
        {
            CurrentUser = user;
        }

        public static void ClearSession()
        {
            CurrentUser = null;
        }
    }

    // DTO cho user
    public class createUser
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
    }
}
