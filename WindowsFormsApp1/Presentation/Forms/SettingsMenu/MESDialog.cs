using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class MESDialog : Form
    {
        public MESDialog()
        {
            InitializeComponent();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void onlineRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            MESConfirmationDialog mesConfirmationDialog = new MESConfirmationDialog();

            mesConfirmationDialog.ShowDialog();
        }
    }
}
