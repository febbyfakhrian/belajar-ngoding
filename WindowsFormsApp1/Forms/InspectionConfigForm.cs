using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WindowsFormsApp1.Inspection;

namespace WindowsFormsApp1.Forms
{
    /// <summary>
    /// Form untuk konfigurasi Inspection Project tanpa coding
    /// </summary>
    public partial class InspectionConfigForm : Form
    {
        private InspectionProject currentProject;
        private InspectionStep selectedStep;
        private bool isEditing = false;

        // Controls
        private TextBox txtProjectName;
        private TextBox txtDescription;
        private ListBox lstSteps;
        private Button btnAddStep;
        private Button btnEditStep;
        private Button btnDeleteStep;
        private Button btnSaveProject;
        private Button btnLoadProject;
        private Button btnNewProject;
        private Panel pnlStepDetails;
        private Label lblStatus;

        public InspectionConfigForm()
        {
            InitializeComponent();
            InitializeCustomControls();
            NewProject();
        }

        private void InitializeComponent()
        {
            this.Text = "Inspection Configuration - No Code Setup";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.FromArgb(241, 241, 241);
        }

        private void InitializeCustomControls()
        {
            // ===== LEFT PANEL - Project Info =====
            Panel pnlLeft = new Panel
            {
                Dock = DockStyle.Left,
                Width = 300,
                BackColor = Color.FromArgb(37, 37, 38),
                Padding = new Padding(10)
            };
            this.Controls.Add(pnlLeft);

            Label lblProjectInfo = new Label
            {
                Text = "PROJECT INFORMATION",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            pnlLeft.Controls.Add(lblProjectInfo);

            Label lblName = new Label
            {
                Text = "Project Name:",
                Location = new Point(10, 40),
                AutoSize = true,
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            pnlLeft.Controls.Add(lblName);

            txtProjectName = new TextBox
            {
                Location = new Point(10, 65),
                Width = 280,
                BackColor = Color.FromArgb(51, 51, 55),
                ForeColor = Color.FromArgb(241, 241, 241),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlLeft.Controls.Add(txtProjectName);

            Label lblDesc = new Label
            {
                Text = "Description:",
                Location = new Point(10, 95),
                AutoSize = true,
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            pnlLeft.Controls.Add(lblDesc);

            txtDescription = new TextBox
            {
                Location = new Point(10, 120),
                Width = 280,
                Height = 80,
                Multiline = true,
                BackColor = Color.FromArgb(51, 51, 55),
                ForeColor = Color.FromArgb(241, 241, 241),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlLeft.Controls.Add(txtDescription);

            // Buttons
            btnNewProject = CreateStyledButton("New Project", 10, 210);
            btnNewProject.Click += BtnNewProject_Click;
            pnlLeft.Controls.Add(btnNewProject);

            btnLoadProject = CreateStyledButton("Load Project", 150, 210);
            btnLoadProject.Click += BtnLoadProject_Click;
            pnlLeft.Controls.Add(btnLoadProject);

            btnSaveProject = CreateStyledButton("Save Project", 10, 250);
            btnSaveProject.Click += BtnSaveProject_Click;
            pnlLeft.Controls.Add(btnSaveProject);

            // ===== STEPS LIST =====
            Label lblSteps = new Label
            {
                Text = "INSPECTION STEPS",
                Location = new Point(10, 300),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            pnlLeft.Controls.Add(lblSteps);

            lstSteps = new ListBox
            {
                Location = new Point(10, 330),
                Width = 280,
                Height = 300,
                BackColor = Color.FromArgb(51, 51, 55),
                ForeColor = Color.FromArgb(241, 241, 241),
                BorderStyle = BorderStyle.FixedSingle
            };
            lstSteps.SelectedIndexChanged += LstSteps_SelectedIndexChanged;
            pnlLeft.Controls.Add(lstSteps);

            btnAddStep = CreateStyledButton("Add Step", 10, 640);
            btnAddStep.Width = 90;
            btnAddStep.Click += BtnAddStep_Click;
            pnlLeft.Controls.Add(btnAddStep);

            btnEditStep = CreateStyledButton("Edit", 105, 640);
            btnEditStep.Width = 90;
            btnEditStep.Click += BtnEditStep_Click;
            pnlLeft.Controls.Add(btnEditStep);

            btnDeleteStep = CreateStyledButton("Delete", 200, 640);
            btnDeleteStep.Width = 90;
            btnDeleteStep.Click += BtnDeleteStep_Click;
            pnlLeft.Controls.Add(btnDeleteStep);

            // ===== RIGHT PANEL - Step Details =====
            pnlStepDetails = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 45, 48),
                Padding = new Padding(20),
                AutoScroll = true
            };
            this.Controls.Add(pnlStepDetails);

            Label lblWelcome = new Label
            {
                Text = "Select or Add Inspection Step to Configure",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.FromArgb(153, 153, 153)
            };
            pnlStepDetails.Controls.Add(lblWelcome);

            // ===== STATUS BAR =====
            lblStatus = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                Text = "Ready"
            };
            this.Controls.Add(lblStatus);
        }

        private Button CreateStyledButton(string text, int x, int y)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Width = 135,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9, FontStyle.Regular)
            };
        }

        // ===== EVENT HANDLERS =====

        private void BtnNewProject_Click(object sender, EventArgs e)
        {
            NewProject();
        }

        private void BtnLoadProject_Click(object sender, EventArgs e)
        {
            var projects = InspectionConfigManager.GetAllProjects();
            if (projects.Count == 0)
            {
                MessageBox.Show("No saved projects found.", "Load Project", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Show project selector dialog
            using (var selector = new ProjectSelectorForm(projects))
            {
                if (selector.ShowDialog() == DialogResult.OK)
                {
                    var project = InspectionConfigManager.LoadProject(selector.SelectedFileName);
                    if (project != null)
                    {
                        LoadProject(project);
                        lblStatus.Text = $"Project '{project.ProjectName}' loaded successfully";
                    }
                }
            }
        }

        private void BtnSaveProject_Click(object sender, EventArgs e)
        {
            SaveProject();
        }

        private void BtnAddStep_Click(object sender, EventArgs e)
        {
            using (var stepEditor = new InspectionStepEditorForm(null))
            {
                if (stepEditor.ShowDialog() == DialogResult.OK)
                {
                    var newStep = stepEditor.EditedStep;
                    newStep.StepOrder = currentProject.Steps.Count + 1;
                    currentProject.Steps.Add(newStep);
                    RefreshStepsList();
                    lblStatus.Text = $"Step '{newStep.StepName}' added";
                }
            }
        }

        private void BtnEditStep_Click(object sender, EventArgs e)
        {
            if (selectedStep == null)
            {
                MessageBox.Show("Please select a step to edit.", "Edit Step", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var stepEditor = new InspectionStepEditorForm(selectedStep))
            {
                if (stepEditor.ShowDialog() == DialogResult.OK)
                {
                    RefreshStepsList();
                    ShowStepDetails(selectedStep);
                    lblStatus.Text = $"Step '{selectedStep.StepName}' updated";
                }
            }
        }

        private void BtnDeleteStep_Click(object sender, EventArgs e)
        {
            if (selectedStep == null)
            {
                MessageBox.Show("Please select a step to delete.", "Delete Step", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete step '{selectedStep.StepName}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                currentProject.Steps.Remove(selectedStep);
                ReorderSteps();
                RefreshStepsList();
                pnlStepDetails.Controls.Clear();
                selectedStep = null;
                lblStatus.Text = "Step deleted";
            }
        }

        private void LstSteps_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstSteps.SelectedIndex >= 0 && lstSteps.SelectedIndex < currentProject.Steps.Count)
            {
                selectedStep = currentProject.Steps[lstSteps.SelectedIndex];
                ShowStepDetails(selectedStep);
            }
        }

        // ===== HELPER METHODS =====

        private void NewProject()
        {
            currentProject = new InspectionProject
            {
                ProjectName = "New Inspection Project",
                Description = "Enter project description here"
            };

            txtProjectName.Text = currentProject.ProjectName;
            txtDescription.Text = currentProject.Description;

            RefreshStepsList();
            pnlStepDetails.Controls.Clear();

            lblStatus.Text = "New project created";
        }

        private void LoadProject(InspectionProject project)
        {
            currentProject = project;
            txtProjectName.Text = project.ProjectName;
            txtDescription.Text = project.Description;
            RefreshStepsList();
        }

        private void SaveProject()
        {
            if (string.IsNullOrWhiteSpace(txtProjectName.Text))
            {
                MessageBox.Show("Please enter a project name.", "Save Project", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            currentProject.ProjectName = txtProjectName.Text;
            currentProject.Description = txtDescription.Text;

            bool success = InspectionConfigManager.SaveProject(currentProject);

            if (success)
            {
                lblStatus.Text = $"Project '{currentProject.ProjectName}' saved successfully";
                lblStatus.BackColor = Color.FromArgb(16, 124, 16);
                MessageBox.Show("Project saved successfully!", "Save Project", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblStatus.Text = "Failed to save project";
                lblStatus.BackColor = Color.FromArgb(232, 17, 35);
                MessageBox.Show("Failed to save project.", "Save Project", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshStepsList()
        {
            lstSteps.Items.Clear();
            foreach (var step in currentProject.Steps.OrderBy(s => s.StepOrder))
            {
                string enabledMark = step.IsEnabled ? "✓" : "✗";
                lstSteps.Items.Add($"{step.StepOrder}. [{enabledMark}] {step.StepName} ({step.InspectionType})");
            }
        }

        private void ReorderSteps()
        {
            int order = 1;
            foreach (var step in currentProject.Steps.OrderBy(s => s.StepOrder))
            {
                step.StepOrder = order++;
            }
        }

        private void ShowStepDetails(InspectionStep step)
        {
            pnlStepDetails.Controls.Clear();

            int yPos = 20;

            AddDetailLabel("STEP DETAILS", ref yPos, true);
            AddDetailLabel($"Name: {step.StepName}", ref yPos);
            AddDetailLabel($"Type: {step.InspectionType}", ref yPos);
            AddDetailLabel($"Enabled: {(step.IsEnabled ? "Yes" : "No")}", ref yPos);

            yPos += 10;
            AddDetailLabel("ROI (Region of Interest):", ref yPos, true);
            AddDetailLabel($"X: {step.ROI.X}, Y: {step.ROI.Y}, Width: {step.ROI.Width}, Height: {step.ROI.Height}", ref yPos);

            yPos += 10;
            AddDetailLabel("Configuration:", ref yPos, true);

            // Show specific config based on inspection type
            switch (step.InspectionType)
            {
                case InspectionType.LabelOCR:
                    if (step.LabelOcrConfig != null)
                    {
                        AddDetailLabel($"Expected Text: {step.LabelOcrConfig.ExpectedText}", ref yPos);
                        AddDetailLabel($"Min Confidence: {step.LabelOcrConfig.MinConfidence:P0}", ref yPos);
                        AddDetailLabel($"Case Sensitive: {step.LabelOcrConfig.CaseSensitive}", ref yPos);
                    }
                    break;

                case InspectionType.ScrewDetection:
                    if (step.ScrewDetectionConfig != null)
                    {
                        AddDetailLabel($"Expected Count: {step.ScrewDetectionConfig.ExpectedCount}", ref yPos);
                        AddDetailLabel($"Min/Max Count: {step.ScrewDetectionConfig.MinCount} - {step.ScrewDetectionConfig.MaxCount}", ref yPos);
                        AddDetailLabel($"Detection Model: {step.ScrewDetectionConfig.DetectionModel}", ref yPos);
                    }
                    break;

                case InspectionType.BarcodeReading:
                    if (step.BarcodeReadingConfig != null)
                    {
                        AddDetailLabel($"Barcode Type: {step.BarcodeReadingConfig.BarcodeType}", ref yPos);
                        AddDetailLabel($"Expected Pattern: {step.BarcodeReadingConfig.ExpectedPattern}", ref yPos);
                    }
                    break;

                case InspectionType.ColorDetection:
                    if (step.ColorDetectionConfig != null)
                    {
                        AddDetailLabel($"Expected Color: {step.ColorDetectionConfig.ExpectedColorName}", ref yPos);
                        AddDetailLabel($"Tolerance: {step.ColorDetectionConfig.ColorTolerance}", ref yPos);
                    }
                    break;
            }

            yPos += 10;
            AddDetailLabel("Pass/Fail Criteria:", ref yPos, true);
            AddDetailLabel($"Operator: {step.PassCriteria.Operator}", ref yPos);
            AddDetailLabel($"Expected Value: {step.PassCriteria.ExpectedValue}", ref yPos);
            AddDetailLabel($"Critical: {step.PassCriteria.IsCritical}", ref yPos);
        }

        private void AddDetailLabel(string text, ref int yPos, bool isBold = false)
        {
            var label = new Label
            {
                Text = text,
                Location = new Point(20, yPos),
                AutoSize = true,
                ForeColor = Color.FromArgb(241, 241, 241),
                Font = new Font("Segoe UI", isBold ? 10 : 9, isBold ? FontStyle.Bold : FontStyle.Regular)
            };
            pnlStepDetails.Controls.Add(label);
            yPos += isBold ? 35 : 25;
        }
    }
}