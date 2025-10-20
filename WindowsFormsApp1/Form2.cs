using System;
using System.Data;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            this.Load += Form2_Load;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            // 1. Buat DataTable dummy
            var dt = new DataTable();
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Serial Number", typeof(string));
            dt.Columns.Add("Status", typeof(string));
            dt.Columns.Add("Device Model", typeof(string));
            dt.Columns.Add("IPv4 Address", typeof(string));
            dt.Rows.Add(1, "SN-001", "Online", "Model A", "192.168.0.10");


            // 2. Konfigurasi DataGridView
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.Columns.Clear();

            // 3. Tambah kolom manual
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ID",
                HeaderText = "ID",
                Width = 50
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Serial Number",
                HeaderText = "Serial Number",
                Width = 120
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Status",
                HeaderText = "Status",
                Width = 80
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Device Model",
                HeaderText = "Device Model",
                Width = 100
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "IPv4 Address",
                HeaderText = "IPv4 Address",
                Width = 100
            });

            var imgCol = new DataGridViewImageColumn
            {
                Name = "Setting",
                HeaderText = "Setting",
                Image = Properties.Resources.SettingIcon,
                ImageLayout = DataGridViewImageCellLayout.Normal,  // use Normal, not Zoom
                Width = 40
            };
            dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.NotSet;
            dataGridView1.AlternatingRowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.NotSet;

            dataGridView1.Columns.Add(imgCol);

            // 4. Bind ke DataTable
            dataGridView1.DataSource = dt;               // :contentReference[oaicite:13]{index=13}

            // 6. Pastikan kolom Setting berada di paling kanan
            imgCol.DisplayIndex = dataGridView1.Columns.Count - 1;

            // 7. (Opsional) Tangani klik pada ikon Setting
            dataGridView1.CellClick += DataGridView1_CellClick;
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridView1.Columns[e.ColumnIndex].Name == "Setting")
            {
                CameraDialog settingCameraDialog = new CameraDialog();
                settingCameraDialog.ShowDialog();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
