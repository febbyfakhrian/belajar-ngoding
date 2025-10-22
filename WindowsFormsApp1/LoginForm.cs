using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void foreverButton1_Click(object sender, EventArgs e)
        {
            if (metroTextBox1.Text == "user" && metroTextBox2.Text == "password")
            {
                //_ = new MainDashboard
                //{
                //    Visible = true
                //};
                this.Hide();
            }
            else
            {
                MessageBox.Show("Wrong username or password");
            }
        }
    }
}
