using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WindowsFormsApp1.Inspection;

namespace WindowsFormsApp1.Forms
{
    /// <summary>
    /// Form untuk select project yang akan di-load
    /// </summary>
    public partial class ProjectSelectorForm : Form
    {
        public string SelectedFileName { get; private set; }
        private List<ProjectMetadata> projects;
        private ListView lstProjects;

        public ProjectSelectorForm(List<ProjectMetadata> projectList)
        {
            projects = projectList;
            InitializeComponent();
            InitializeCustomControls();
            LoadProjects();
        }

        private void InitializeComponent()
        {
            this.Text = "Select Inspection Project";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(37, 37, 38);
            this.ForeColor = Color.FromArgb(241, 241, 241);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void InitializeCustomControls()
        {
            // Title
            Label lblTitle = new Label
            {
                Text = "SELECT PROJECT TO LOAD",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            this.Controls.Add(lblTitle);

            // ListView untuk projects
            lstProjects = new ListView
            {
                Location = new Point(20, 60),
                Size = new Size(740, 430),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = Color.FromArgb(51, 51, 55),
                ForeColor = Color.FromArgb(241, 241, 241),
                BorderStyle = BorderStyle.FixedSingle,
                MultiSelect = false
            };

            // Columns
            lstProjects.Columns.Add("Project Name", 200);
            lstProjects.Columns.Add("Description", 250);
            lstProjects.Columns.Add("Steps", 60);
            lstProjects.Columns.Add("Modified", 150);
            lstProjects.Columns.Add("Created", 150);

            lstProjects.DoubleClick += (s, e) => LoadSelectedProject();

            this.Controls.Add(lstProjects);

            // Buttons
            Button btnLoad = new Button
            {
                Text = "Load",
                Location = new Point(560, 510),
                Width = 100,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnLoad.Click += (s, e) => LoadSelectedProject();
            this.Controls.Add(btnLoad);

            Button btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(670, 510),
                Width = 100,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(62, 62, 64),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);

            // Delete button
            Button btnDelete = new Button
            {
                Text = "Delete",
                Location = new Point(20, 510),
                Width = 100,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(232, 17, 35),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnDelete.Click += BtnDelete_Click;
            this.Controls.Add(btnDelete);
        }

        private void LoadProjects()
        {
            lstProjects.Items.Clear();

            foreach (var proj in projects.OrderByDescending(p => p.ModifiedDate))
            {
                var item = new ListViewItem(proj.ProjectName);
                item.SubItems.Add(proj.Description ?? "");
                item.SubItems.Add(proj.StepCount.ToString());
                item.SubItems.Add(proj.ModifiedDate.ToString("yyyy-MM-dd HH:mm"));
                item.SubItems.Add(proj.CreatedDate.ToString("yyyy-MM-dd HH:mm"));
                item.Tag = proj.FileName;

                lstProjects.Items.Add(item);
            }
        }

        private void LoadSelectedProject()
        {
            if (lstProjects.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a project.", "Load Project",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SelectedFileName = lstProjects.SelectedItems[0].Tag.ToString();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (lstProjects.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a project to delete.", "Delete Project",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var fileName = lstProjects.SelectedItems[0].Tag.ToString();
            var projectName = lstProjects.SelectedItems[0].Text;

            var result = MessageBox.Show(
                $"Are you sure you want to delete project '{projectName}'?\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                if (InspectionConfigManager.DeleteProject(fileName))
                {
                    lstProjects.Items.Remove(lstProjects.SelectedItems[0]);
                    MessageBox.Show("Project deleted successfully.", "Delete Project",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to delete project.", "Delete Project",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}