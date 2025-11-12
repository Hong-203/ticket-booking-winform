using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace TicketBookingStaffApp.Utils
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public static class Logger
    {
        private static readonly string logFilePath = "application_log.txt";

        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            File.AppendAllText(logFilePath, logEntry + Environment.NewLine);

            if (level == LogLevel.Error)
            {
                MessageBox.Show($"[ERROR] {message}", "Logger", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
