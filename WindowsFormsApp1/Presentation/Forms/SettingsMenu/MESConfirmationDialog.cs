using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class MESConfirmationDialog : Form
    {
        public MESConfirmationDialog()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
