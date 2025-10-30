using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class LoginForm : Form
    {
        private IServiceProvider _provider;
        private readonly string _connectionString;
        public LoginForm(IServiceProvider provider, string connectionString)
        {
            _provider = provider;
            _connectionString = connectionString;
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void foreverButton1_Click(object sender, EventArgs e)
        {
            var sqliteConnection = new SQLiteConnection(_connectionString);
            sqliteConnection.Open();
            using (var cmd = sqliteConnection.CreateCommand())
            {
                cmd.CommandText = "SELECT id, badge_number, password FROM users WHERE badge_number = @BadgeNumber AND password = @Password LIMIT 1;";
                cmd.Parameters.AddWithValue("@BadgeNumber", metroTextBox1.Text);
                cmd.Parameters.AddWithValue("@Password", metroTextBox2.Text);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Successful login
                        sqliteConnection.Close();
                        this.Hide();
                        var dashboard = new MainDashboard(_provider);
                        dashboard.Show();
                    }
                    else
                    {
                        MessageBox.Show("Invalid badge number or password.");
                    }
                }
            }
        }
    }
}
