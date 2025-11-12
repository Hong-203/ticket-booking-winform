using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TicketBookingStaffApp.DAL.database
{
    class DatabaseProvider
    {
        private string connectDB = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        public static MySqlConnection GetMySqlConnection()
        {
            string connString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            return new MySqlConnection(connString);
        }
        // SELECT - Trả về DataTable
        public DataTable ExecuteQuery(string query, object[] parameter = null)
        {
            DataTable data = new DataTable();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectDB))
                {
                    connection.Open();
                    //MessageBox.Show("Database connection opened successfully!", "Debug");

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        //MessageBox.Show($"Executing SQL: {query}", "Debug");
                        AddParameters(command, query, parameter);

                        MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                        adapter.Fill(data);
                    }

                    connection.Close();
                    //MessageBox.Show("Database connection closed!", "Debug");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SQL execution error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return data;
        }


        // INSERT, UPDATE, DELETE - Trả về số dòng bị ảnh hưởng
        public int ExecuteNonQuery(string query, object[] parameter = null)
        {
            int result = 0;
            using (MySqlConnection connection = new MySqlConnection(connectDB))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    AddParameters(command, query, parameter);
                    result = command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return result;
        }


        // Lấy giá trị đơn
        public object ExecuteScalar(string query, object[] parameter = null)
        {
            object result = null;
            using (MySqlConnection connection = new MySqlConnection(connectDB))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    AddParameters(command, query, parameter);
                    result = command.ExecuteScalar();
                }
                connection.Close();
            }
            return result;
        }

        // Thêm tham số vào câu lệnh
        private void AddParameters(MySqlCommand command, string query, object[] parameter)
        {
            if (parameter != null && parameter.Length > 0)
            {
                if (parameter.All(p => p is MySqlParameter))
                {
                    foreach (MySqlParameter param in parameter)
                    {
                        //MessageBox.Show($"Adding parameter: {param.ParameterName} = {param.Value}", "Debug");
                        command.Parameters.Add(param);
                    }
                }
                else
                {
                    MatchCollection matches = Regex.Matches(query, @"@\w+");
                    int index = 0;

                    foreach (Match match in matches)
                    {
                        if (index >= parameter.Length) break;

                        string paramName = match.Value;
                        object paramValue = parameter[index] ?? DBNull.Value;

                        //MessageBox.Show($"Adding parameter: {paramName} = {paramValue}", "Debug");

                        if (paramValue is MySqlParameter mySqlParam)
                        {
                            command.Parameters.Add(mySqlParam);
                        }
                        else
                        {
                            command.Parameters.AddWithValue(paramName, paramValue);
                        }

                        index++;
                    }
                }
            }
        }

        internal static SqlConnection GetConnection()
        {
            throw new NotImplementedException();
        }
    }
}
