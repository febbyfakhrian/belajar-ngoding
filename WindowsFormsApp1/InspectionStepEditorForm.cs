using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApp1.Inspection;

namespace WindowsFormsApp1.Forms
{
    /// <summary>
    /// Form untuk edit/create inspection step
    /// </summary>
    public partial class InspectionStepEditorForm : Form
    {
        public InspectionStep EditedStep { get; private set; }
        private bool isEditMode;

        // Common Controls
        private TextBox txtStepName;
        private ComboBox cmbInspectionType;
        private CheckBox chkEnabled;
        private NumericUpDown numRoiX, numRoiY, numRoiWidth, numRoiHeight;
        private Panel pnlSpecificConfig;
        private Button btnSelectROI;

        // Pass/Fail Controls
        private ComboBox cmbOperator;
        private TextBox txtExpectedValue;
        private NumericUpDown numTolerance;
        private CheckBox chkCritical;

        public InspectionStepEditorForm(InspectionStep existingStep)
        {
            isEditMode = existingStep != null;
            EditedStep = existingStep ?? new InspectionStep
            {
                StepName = "New Step",
                InspectionType = InspectionType.LabelOCR
            };

            InitializeComponent();
            InitializeCustomControls();
            LoadStepData();
        }

        private void InitializeComponent()
        {
            this.Text = isEditMode ? "Edit Inspection Step" : "Add Inspection Step";
            this.Size = new Size(800, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(37, 37, 38);
            this.ForeColor = Color.FromArgb(241, 241, 241);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void InitializeCustomControls()
        {
            int yPos = 20;

            // Step Name
            AddLabel("Step Name:", 20, ref yPos);
            txtStepName = new TextBox
            {
                Location = new Point(150, yPos - 20),
                Width = 600,
                BackColor = Color.FromArgb(51, 51, 55),
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            this.Controls.Add(txtStepName);
            yPos += 15;

            // Inspection Type
            AddLabel("Inspection Type:", 20, ref yPos);
            cmbInspectionType = new ComboBox
            {
                Location = new Point(150, yPos - 20),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(51, 51, 55),
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            cmbInspectionType.DataSource = Enum.GetValues(typeof(InspectionType));
            cmbInspectionType.SelectedIndexChanged += CmbInspectionType_SelectedIndexChanged;
            this.Controls.Add(cmbInspectionType);

            // Enabled Checkbox
            chkEnabled = new CheckBox
            {
                Text = "Enabled",
                Location = new Point(460, yPos - 20),
                AutoSize = true,
                Checked = true,
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            this.Controls.Add(chkEnabled);
            yPos += 15;

            // ROI Section
            yPos += 10;
            AddLabel("ROI (Region of Interest):", 20, ref yPos, true);

            AddLabel("X:", 20, ref yPos);
            numRoiX = CreateNumericUpDown(80, yPos - 20, 0, 10000, 0);

            AddLabel("Y:", 200, yPos - 20);
            numRoiY = CreateNumericUpDown(240, yPos - 20, 0, 10000, 0);

            AddLabel("Width:", 380, yPos - 20);
            numRoiWidth = CreateNumericUpDown(450, yPos - 20, 1, 10000, 640);

            AddLabel("Height:", 580, yPos - 20);
            numRoiHeight = CreateNumericUpDown(650, yPos - 20, 1, 10000, 480);
            yPos += 15;

            btnSelectROI = CreateButton("Select ROI from Image", 20, yPos);
            btnSelectROI.Width = 200;
            btnSelectROI.Click += BtnSelectROI_Click;
            yPos += 40;

            // Specific Config Panel
            yPos += 10;
            AddLabel("Inspection Configuration:", 20, ref yPos, true);
            pnlSpecificConfig = new Panel
            {
                Location = new Point(20, yPos),
                Width = 750,
                Height = 200,
                BackColor = Color.FromArgb(45, 45, 48),
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };
            this.Controls.Add(pnlSpecificConfig);
            yPos += 210;

            // Pass/Fail Criteria
            AddLabel("Pass/Fail Criteria:", 20, ref yPos, true);

            AddLabel("Operator:", 20, ref yPos);
            cmbOperator = new ComboBox
            {
                Location = new Point(150, yPos - 20),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(51, 51, 55),
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            cmbOperator.DataSource = Enum.GetValues(typeof(ComparisonOperator));
            this.Controls.Add(cmbOperator);
            yPos += 15;

            AddLabel("Expected Value:", 20, ref yPos);
            txtExpectedValue = new TextBox
            {
                Location = new Point(150, yPos - 20),
                Width = 300,
                BackColor = Color.FromArgb(51, 51, 55),
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            this.Controls.Add(txtExpectedValue);
            yPos += 15;

            AddLabel("Tolerance:", 20, ref yPos);
            numTolerance = CreateNumericUpDown(150, yPos - 20, 0, 100, 0);

            chkCritical = new CheckBox
            {
                Text = "Critical (Stop on Fail)",
                Location = new Point(300, yPos - 20),
                AutoSize = true,
                Checked = true,
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            this.Controls.Add(chkCritical);
            yPos += 40;

            // Buttons
            Button btnSave = CreateButton("Save", 550, yPos);
            btnSave.Click += BtnSave_Click;

            Button btnCancel = CreateButton("Cancel", 660, yPos);
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
        }

        private void CmbInspectionType_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSpecificConfigPanel((InspectionType)cmbInspectionType.SelectedItem);
        }

        private void LoadSpecificConfigPanel(InspectionType type)
        {
            pnlSpecificConfig.Controls.Clear();
            int y = 10;

            switch (type)
            {
                case InspectionType.LabelOCR:
                    LoadLabelOcrConfig(ref y);
                    break;
                case InspectionType.ScrewDetection:
                    LoadScrewDetectionConfig(ref y);
                    break;
                case InspectionType.BarcodeReading:
                    LoadBarcodeConfig(ref y);
                    break;
                case InspectionType.ColorDetection:
                    LoadColorDetectionConfig(ref y);
                    break;
                case InspectionType.DefectDetection:
                    LoadDefectDetectionConfig(ref y);
                    break;
                case InspectionType.Measurement:
                    LoadMeasurementConfig(ref y);
                    break;
            }
        }

        private void LoadLabelOcrConfig(ref int y)
        {
            if (EditedStep.LabelOcrConfig == null)
                EditedStep.LabelOcrConfig = new LabelOcrConfig();

            AddConfigLabel("Expected Text:", 10, ref y);
            var txtExpected = AddConfigTextBox(150, y - 20, EditedStep.LabelOcrConfig.ExpectedText);
            txtExpected.Tag = "LabelOcr_ExpectedText";

            AddConfigLabel("Min Confidence:", 10, ref y);
            var numConf = AddConfigNumeric(150, y - 20, 0, 1, 0.01, (double)EditedStep.LabelOcrConfig.MinConfidence);
            numConf.Tag = "LabelOcr_MinConfidence";
            numConf.DecimalPlaces = 2;

            var chkCase = new CheckBox
            {
                Text = "Case Sensitive",
                Location = new Point(300, y - 20),
                Checked = EditedStep.LabelOcrConfig.CaseSensitive,
                Tag = "LabelOcr_CaseSensitive",
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            pnlSpecificConfig.Controls.Add(chkCase);

            var chkPartial = new CheckBox
            {
                Text = "Allow Partial Match",
                Location = new Point(450, y - 20),
                Checked = EditedStep.LabelOcrConfig.AllowPartialMatch,
                Tag = "LabelOcr_AllowPartialMatch",
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            pnlSpecificConfig.Controls.Add(chkPartial);
            y += 20;
        }

        private void LoadScrewDetectionConfig(ref int y)
        {
            if (EditedStep.ScrewDetectionConfig == null)
                EditedStep.ScrewDetectionConfig = new ScrewDetectionConfig();

            AddConfigLabel("Expected Count:", 10, ref y);
            var numExpected = AddConfigNumeric(150, y - 20, 0, 1000, 1, EditedStep.ScrewDetectionConfig.ExpectedCount);
            numExpected.Tag = "Screw_ExpectedCount";

            AddConfigLabel("Min Count:", 10, ref y);
            var numMin = AddConfigNumeric(150, y - 20, 0, 1000, 1, EditedStep.ScrewDetectionConfig.MinCount);
            numMin.Tag = "Screw_MinCount";

            AddConfigLabel("Max Count:", 400, y - 20);
            var numMax = AddConfigNumeric(500, y - 20, 0, 1000, 1, EditedStep.ScrewDetectionConfig.MaxCount);
            numMax.Tag = "Screw_MaxCount";
            y += 20;

            AddConfigLabel("Detection Model:", 10, ref y);
            var txtModel = AddConfigTextBox(150, y - 20, EditedStep.ScrewDetectionConfig.DetectionModel);
            txtModel.Tag = "Screw_DetectionModel";
        }

        private void LoadBarcodeConfig(ref int y)
        {
            if (EditedStep.BarcodeReadingConfig == null)
                EditedStep.BarcodeReadingConfig = new BarcodeReadingConfig();

            AddConfigLabel("Barcode Type:", 10, ref y);
            var cmbType = new ComboBox
            {
                Location = new Point(150, y - 20),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = Enum.GetValues(typeof(BarcodeType)),
                SelectedItem = EditedStep.BarcodeReadingConfig.BarcodeType,
                Tag = "Barcode_Type",
                BackColor = Color.FromArgb(51, 51, 55),
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            pnlSpecificConfig.Controls.Add(cmbType);
            y += 20;

            AddConfigLabel("Expected Pattern (Regex):", 10, ref y);
            var txtPattern = AddConfigTextBox(200, y - 20, EditedStep.BarcodeReadingConfig.ExpectedPattern);
            txtPattern.Width = 500;
            txtPattern.Tag = "Barcode_Pattern";
        }

        private void LoadColorDetectionConfig(ref int y)
        {
            if (EditedStep.ColorDetectionConfig == null)
                EditedStep.ColorDetectionConfig = new ColorDetectionConfig { ExpectedColorRGB = new ColorRGB() };

            AddConfigLabel("Color Name:", 10, ref y);
            var txtName = AddConfigTextBox(150, y - 20, EditedStep.ColorDetectionConfig.ExpectedColorName);
            txtName.Tag = "Color_Name";

            AddConfigLabel("RGB - R:", 10, ref y);
            var numR = AddConfigNumeric(80, y - 20, 0, 255, 1, EditedStep.ColorDetectionConfig.ExpectedColorRGB.R);
            numR.Tag = "Color_R";

            AddConfigLabel("G:", 200, y - 20);
            var numG = AddConfigNumeric(240, y - 20, 0, 255, 1, EditedStep.ColorDetectionConfig.ExpectedColorRGB.G);
            numG.Tag = "Color_G";

            AddConfigLabel("B:", 360, y - 20);
            var numB = AddConfigNumeric(400, y - 20, 0, 255, 1, EditedStep.ColorDetectionConfig.ExpectedColorRGB.B);
            numB.Tag = "Color_B";
            y += 20;

            AddConfigLabel("Tolerance:", 10, ref y);
            var numTol = AddConfigNumeric(150, y - 20, 0, 100, 1, (decimal)EditedStep.ColorDetectionConfig.ColorTolerance);
            numTol.Tag = "Color_Tolerance";
        }

        private void LoadDefectDetectionConfig(ref int y)
        {
            if (EditedStep.DefectDetectionConfig == null)
                EditedStep.DefectDetectionConfig = new DefectDetectionConfig();

            AddConfigLabel("Max Allowed Defects:", 10, ref y);
            var numMax = AddConfigNumeric(180, y - 20, 0, 100, 1, (decimal)EditedStep.DefectDetectionConfig.MaxAllowedDefects);
            numMax.Tag = "Defect_MaxAllowed";

            AddConfigLabel("Min Defect Size (px):", 10, ref y);
            var numMin = AddConfigNumeric(180, y - 20, 1, 1000, 1, (decimal)EditedStep.DefectDetectionConfig.MinDefectSize);
            numMin.Tag = "Defect_MinSize";
        }

        private void LoadMeasurementConfig(ref int y)
        {
            if (EditedStep.MeasurementConfig == null)
                EditedStep.MeasurementConfig = new MeasurementConfig();

            AddConfigLabel("Measurement Type:", 10, ref y);
            var cmbType = new ComboBox
            {
                Location = new Point(180, y - 20),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = Enum.GetValues(typeof(MeasurementType)),
                SelectedItem = EditedStep.MeasurementConfig.MeasurementType,
                Tag = "Measurement_Type",
                BackColor = Color.FromArgb(51, 51, 55),
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            pnlSpecificConfig.Controls.Add(cmbType);
            y += 20;

            AddConfigLabel("Expected Value:", 10, ref y);
            var numExp = AddConfigNumeric(180, y - 20, 0, 10000, 0.1, (decimal)EditedStep.MeasurementConfig.ExpectedValue);
            numExp.Tag = "Measurement_Expected";
            numExp.DecimalPlaces = 2;

            AddConfigLabel("Min:", 10, ref y);
            var numMin = AddConfigNumeric(80, y - 20, 0, 10000, 0.1, (decimal)EditedStep.MeasurementConfig.MinValue);
            numMin.Tag = "Measurement_Min";
            numMin.DecimalPlaces = 2;

            AddConfigLabel("Max:", 250, y - 20);
            var numMax = AddConfigNumeric(300, y - 20, 0, 10000, 0.1, (decimal)EditedStep.MeasurementConfig.MaxValue);
            numMax.Tag = "Measurement_Max";
            numMax.DecimalPlaces = 2;
        }

        private void BtnSelectROI_Click(object sender, EventArgs e)
        {
            MessageBox.Show("ROI Selection from Image will be implemented with image viewer.\nFor now, please enter coordinates manually.",
                "ROI Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStepName.Text))
            {
                MessageBox.Show("Please enter a step name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Save common properties
            EditedStep.StepName = txtStepName.Text;
            EditedStep.InspectionType = (InspectionType)cmbInspectionType.SelectedItem;
            EditedStep.IsEnabled = chkEnabled.Checked;

            EditedStep.ROI.X = (int)numRoiX.Value;
            EditedStep.ROI.Y = (int)numRoiY.Value;
            EditedStep.ROI.Width = (int)numRoiWidth.Value;
            EditedStep.ROI.Height = (int)numRoiHeight.Value;

            EditedStep.PassCriteria.Operator = (ComparisonOperator)cmbOperator.SelectedItem;
            EditedStep.PassCriteria.ExpectedValue = txtExpectedValue.Text;
            EditedStep.PassCriteria.Tolerance = (double)numTolerance.Value;
            EditedStep.PassCriteria.IsCritical = chkCritical.Checked;

            // Save specific config from panel controls
            SaveSpecificConfig();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void SaveSpecificConfig()
        {
            foreach (Control ctrl in pnlSpecificConfig.Controls)
            {
                if (ctrl.Tag == null) continue;
                string tag = ctrl.Tag.ToString();

                // Label OCR
                if (tag.StartsWith("LabelOcr_"))
                {
                    if (tag == "LabelOcr_ExpectedText") EditedStep.LabelOcrConfig.ExpectedText = ((TextBox)ctrl).Text;
                    if (tag == "LabelOcr_MinConfidence") EditedStep.LabelOcrConfig.MinConfidence = (double)((NumericUpDown)ctrl).Value;
                    if (tag == "LabelOcr_CaseSensitive") EditedStep.LabelOcrConfig.CaseSensitive = ((CheckBox)ctrl).Checked;
                    if (tag == "LabelOcr_AllowPartialMatch") EditedStep.LabelOcrConfig.AllowPartialMatch = ((CheckBox)ctrl).Checked;
                }
                // Screw Detection
                else if (tag.StartsWith("Screw_"))
                {
                    if (tag == "Screw_ExpectedCount") EditedStep.ScrewDetectionConfig.ExpectedCount = (int)((NumericUpDown)ctrl).Value;
                    if (tag == "Screw_MinCount") EditedStep.ScrewDetectionConfig.MinCount = (int)((NumericUpDown)ctrl).Value;
                    if (tag == "Screw_MaxCount") EditedStep.ScrewDetectionConfig.MaxCount = (int)((NumericUpDown)ctrl).Value;
                    if (tag == "Screw_DetectionModel") EditedStep.ScrewDetectionConfig.DetectionModel = ((TextBox)ctrl).Text;
                }
                // Barcode
                else if (tag.StartsWith("Barcode_"))
                {
                    if (tag == "Barcode_Type") EditedStep.BarcodeReadingConfig.BarcodeType = (BarcodeType)((ComboBox)ctrl).SelectedItem;
                    if (tag == "Barcode_Pattern") EditedStep.BarcodeReadingConfig.ExpectedPattern = ((TextBox)ctrl).Text;
                }
                // Color
                else if (tag.StartsWith("Color_"))
                {
                    if (tag == "Color_Name") EditedStep.ColorDetectionConfig.ExpectedColorName = ((TextBox)ctrl).Text;
                    if (tag == "Color_R") EditedStep.ColorDetectionConfig.ExpectedColorRGB.R = (int)((NumericUpDown)ctrl).Value;
                    if (tag == "Color_G") EditedStep.ColorDetectionConfig.ExpectedColorRGB.G = (int)((NumericUpDown)ctrl).Value;
                    if (tag == "Color_B") EditedStep.ColorDetectionConfig.ExpectedColorRGB.B = (int)((NumericUpDown)ctrl).Value;
                    if (tag == "Color_Tolerance") EditedStep.ColorDetectionConfig.ColorTolerance = (double)((NumericUpDown)ctrl).Value;
                }
                // Defect
                else if (tag.StartsWith("Defect_"))
                {
                    if (tag == "Defect_MaxAllowed") EditedStep.DefectDetectionConfig.MaxAllowedDefects = (double)((NumericUpDown)ctrl).Value;
                    if (tag == "Defect_MinSize") EditedStep.DefectDetectionConfig.MinDefectSize = (double)((NumericUpDown)ctrl).Value;
                }
                // Measurement
                else if (tag.StartsWith("Measurement_"))
                {
                    if (tag == "Measurement_Type") EditedStep.MeasurementConfig.MeasurementType = (MeasurementType)((ComboBox)ctrl).SelectedItem;
                    if (tag == "Measurement_Expected") EditedStep.MeasurementConfig.ExpectedValue = (double)((NumericUpDown)ctrl).Value;
                    if (tag == "Measurement_Min") EditedStep.MeasurementConfig.MinValue = (double)((NumericUpDown)ctrl).Value;
                    if (tag == "Measurement_Max") EditedStep.MeasurementConfig.MaxValue = (double)((NumericUpDown)ctrl).Value;
                }
            }
        }

        private void LoadStepData()
        {
            txtStepName.Text = EditedStep.StepName;
            cmbInspectionType.SelectedItem = EditedStep.InspectionType;
            chkEnabled.Checked = EditedStep.IsEnabled;

            numRoiX.Value = EditedStep.ROI.X;
            numRoiY.Value = EditedStep.ROI.Y;
            numRoiWidth.Value = EditedStep.ROI.Width;
            numRoiHeight.Value = EditedStep.ROI.Height;

            cmbOperator.SelectedItem = EditedStep.PassCriteria.Operator;
            txtExpectedValue.Text = EditedStep.PassCriteria.ExpectedValue;
            numTolerance.Value = (decimal)EditedStep.PassCriteria.Tolerance;
            chkCritical.Checked = EditedStep.PassCriteria.IsCritical;

            LoadSpecificConfigPanel(EditedStep.InspectionType);
        }

        // Helper methods
        private void AddLabel(string text, int x, ref int y, bool bold = false)
        {
            var lbl = new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", bold ? 10 : 9, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            this.Controls.Add(lbl);
            y += bold ? 30 : 25;
        }

        private void AddConfigLabel(string text, int x, ref int y)
        {
            var lbl = new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            pnlSpecificConfig.Controls.Add(lbl);
            y += 25;
        }

        private NumericUpDown CreateNumericUpDown(int x, int y, decimal min, decimal max, decimal value)
        {
            var num = new NumericUpDown
            {
                Location = new Point(x, y),
                Width = 100,
                Minimum = min,
                Maximum = max,
                Value = value,
                BackColor = Color.FromArgb(51, 51, 55),
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            this.Controls.Add(num);
            return num;
        }

        private NumericUpDown AddConfigNumeric(int x, int y, decimal min, decimal max, decimal increment, decimal value)
        {
            var num = new NumericUpDown
            {
                Location = new Point(x, y),
                Width = 100,
                Minimum = min,
                Maximum = max,
                Increment = increment,
                Value = value,
                BackColor = Color.FromArgb(51, 51, 55),
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            pnlSpecificConfig.Controls.Add(num);
            return num;
        }

        private TextBox AddConfigTextBox(int x, int y, string text)
        {
            var txt = new TextBox
            {
                Location = new Point(x, y),
                Width = 200,
                Text = text ?? "",
                BackColor = Color.FromArgb(51, 51, 55),
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            pnlSpecificConfig.Controls.Add(txt);
            return txt;
        }

        private Button CreateButton(string text, int x, int y)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Width = 100,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            this.Controls.Add(btn);
            return btn;
        }
    }
}