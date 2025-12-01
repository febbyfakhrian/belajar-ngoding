using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static WindowsFormsApp1.Presentation.Flow.DiagramConfigurationForm;

namespace WindowsFormsApp1
{
    // Partial KHUSUS untuk styling header + menu + toolbar ala Visual Studio.
    public partial class MainDashboard : Form
    {
        // Warna-warna mirip Visual Studio 2019 Dark
        private readonly Color _vsMenuBarBack = Color.FromArgb(45, 45, 48);
        private readonly Color _vsMenuText = Color.FromArgb(241, 241, 241);
        private readonly Color _vsMenuTextDisabled = Color.FromArgb(153, 153, 153);
        private readonly Color _vsDropDownBack = Color.FromArgb(37, 37, 38);
        private readonly Color _vsDropDownBorder = Color.FromArgb(51, 51, 55);
        private readonly Color _vsItemHoverBack = Color.FromArgb(62, 62, 64);
        private readonly Color _vsItemPressedBack = Color.FromArgb(51, 51, 55);

        // --- Custom titlebar state ---
        private Button _btnClose;
        private Button _btnMaxRestore;
        private Button _btnMinimize;
        private Panel _windowButtonsHost;
        private Panel _windowButtonsPanel;

        private bool _draggingWindow;
        private Point _dragStartCursor;
        private Point _dragStartForm;


        /// <summary>
        /// Dipanggil dari Form1_Load di MainDashboard.cs
        /// </summary>

        private void SetupCustomHeaderAndMenu()
        {
            // Form borderless → kita ambil alih titlebar
            this.FormBorderStyle = FormBorderStyle.None;
            Debug.WriteLine("[SetupCustomHeaderAndMenu] start");

            if (tableLayoutPanel2 != null &&
                tableLayoutPanel8 != null &&
                crownMenuStrip1 != null &&
                flowLayoutPanel2 != null)
            {
                // 1) Header 2 baris (menu + toolbar) + tema
                SetupToolbarLayout();
                ApplyVisualStudioMenuTheme();

                // 2) Tombol di dalam header (kanan baris menu)
                CreateWindowButtons();
            }

            // 3) Tombol overlay tambahan di pojok kanan atas
            InitWindowButtonsOverlay();

            // 4) Drag dari header
            HookCustomTitlebarDrag();

            //Debug.WriteLine("[SetupCustomHeaderAndMenu] done");
        }


        // =====================================================================
        //  CUSTOM TITLEBAR: WINDOW BUTTONS + DRAG
        // =====================================================================
        private void InitWindowButtonsOverlay()
        {
            if (_windowButtonsPanel != null)
                return; // sudah dibuat

            _windowButtonsPanel = new Panel
            {
                Size = new Size(90, 24),
                BackColor = _vsMenuBarBack,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            _btnMinimize = BuildWindowButton("_", OnClickMinimize);
            _btnMaxRestore = BuildWindowButton("❐", OnClickMaxRestore);
            _btnClose = BuildWindowButton("X", OnClickClose);

            flow.Controls.Add(_btnMinimize);
            flow.Controls.Add(_btnMaxRestore);
            flow.Controls.Add(_btnClose);

            _windowButtonsPanel.Controls.Add(flow);

            // TEMPATKAN DI FORM LANGSUNG (overlay)
            this.Controls.Add(_windowButtonsPanel);
            PositionWindowButtons();

            // Saat form di-resize, tombol ikut geser
            this.Resize -= MainDashboard_ResizeForWindowButtons;
            this.Resize += MainDashboard_ResizeForWindowButtons;
        }

        private void MainDashboard_ResizeForWindowButtons(object sender, EventArgs e)
        {
            PositionWindowButtons();
        }

        private void PositionWindowButtons()
        {
            if (_windowButtonsPanel == null)
                return;

            // pojok kanan atas client area
            _windowButtonsPanel.Location = new Point(
                this.ClientSize.Width - _windowButtonsPanel.Width,
                0);

            _windowButtonsPanel.BringToFront();
        }

        private void CreateWindowButtons()
        {
            if (tableLayoutPanel8 == null || crownMenuStrip1 == null)
                return;

            if (_windowButtonsHost == null)
            {
                _windowButtonsHost = new Panel
                {
                    Dock = DockStyle.Fill,
                    Margin = Padding.Empty,
                    Padding = Padding.Empty,
                    BackColor = _vsMenuBarBack
                };

                var buttonsFlow = new FlowLayoutPanel
                {
                    Dock = DockStyle.Right,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    Margin = new Padding(0),
                    Padding = new Padding(0)
                };

                _btnMinimize = BuildWindowButton("_", OnClickMinimize);
                _btnMaxRestore = BuildWindowButton("❐", OnClickMaxRestore);
                _btnClose = BuildWindowButton("X", OnClickClose);

                buttonsFlow.Controls.Add(_btnMinimize);
                buttonsFlow.Controls.Add(_btnMaxRestore);
                buttonsFlow.Controls.Add(_btnClose);

                _windowButtonsHost.Controls.Add(buttonsFlow);
              
            }

            tableLayoutPanel8.SuspendLayout();

            // 2 kolom: kiri = menu, kanan = tombol window
            tableLayoutPanel8.ColumnCount = 2;
            tableLayoutPanel8.ColumnStyles.Clear();
            tableLayoutPanel8.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // menu
            tableLayoutPanel8.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F)); // tombol

            tableLayoutPanel8.Controls.Clear();

            crownMenuStrip1.Dock = DockStyle.Fill;
            crownMenuStrip1.Margin = Padding.Empty;

            tableLayoutPanel8.Controls.Add(crownMenuStrip1, 0, 0);
            tableLayoutPanel8.Controls.Add(_windowButtonsHost, 1, 0);

            tableLayoutPanel8.ResumeLayout();
        }

        private Button BuildWindowButton(string text, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.White,
                BackColor = _vsMenuBarBack,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(30, 22),
                Margin = new Padding(0),
                TabStop = false
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;

            btn.MouseEnter += (s, e) =>
            {
                if (text == "X")
                    btn.BackColor = Color.FromArgb(232, 17, 35);   // merah untuk close
                else
                    btn.BackColor = _vsItemHoverBack;
            };

            btn.MouseLeave += (s, e) => { btn.BackColor = _vsMenuBarBack; };

            return btn;
        }

        private void OnClickClose(object sender, EventArgs e) => Close();

        private void OnClickMinimize(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void OnClickMaxRestore(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
                if (_btnMaxRestore != null) _btnMaxRestore.Text = "❐";
            }
            else
            {
                WindowState = FormWindowState.Maximized;
                if (_btnMaxRestore != null) _btnMaxRestore.Text = "❏";
            }
        }

        private void HookCustomTitlebarDrag()
        {
            // Biar area header (menu + toolbar) bisa buat drag window
            AttachDragHandler(tableLayoutPanel8);
            AttachDragHandler(crownMenuStrip1);
            AttachDragHandler(flowLayoutPanel2);
        }

        private void AttachDragHandler(Control c)
        {
            if (c == null) return;

            c.MouseDown += Titlebar_MouseDown;
            c.MouseMove += Titlebar_MouseMove;
            c.MouseUp += Titlebar_MouseUp;
        }

        private void Titlebar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            _draggingWindow = true;
            _dragStartCursor = Cursor.Position;
            _dragStartForm = Location;
        }

        private void Titlebar_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_draggingWindow) return;

            var diff = new Size(
                Cursor.Position.X - _dragStartCursor.X,
                Cursor.Position.Y - _dragStartCursor.Y);

            Location = new Point(_dragStartForm.X + diff.Width,
                                 _dragStartForm.Y + diff.Height);
        }

        private void Titlebar_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _draggingWindow = false;
        }

        // =====================================================================
        //  LAYOUT: MENU (ROW 0) + TOOLBAR (ROW 1)
        // =====================================================================
        private void SetupToolbarLayout()
        {

            if (tableLayoutPanel2 == null ||
                tableLayoutPanel8 == null ||
                flowLayoutPanel2 == null ||
                crownMenuStrip1 == null)
                return;

            tableLayoutPanel1.SetColumnSpan(tableLayoutPanel2, tableLayoutPanel1.ColumnCount);
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel8.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            tableLayoutPanel1.SetColumnSpan(tableLayoutPanel2, tableLayoutPanel1.ColumnCount);
           
            // ===== PANEL HEADER UTAMA =====
            tableLayoutPanel2.BackColor = _vsMenuBarBack;
            tableLayoutPanel2.Margin = Padding.Empty;
            tableLayoutPanel2.Padding = Padding.Empty;
            tableLayoutPanel2.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            tableLayoutPanel2.Dock = DockStyle.Top;   // nempel di atas form
            /*tableLayoutPanel2.Height = 52;   */           // 24 + 28 kira-kira

            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Clear();
            tableLayoutPanel2.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100F));

            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Clear();
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            // row 0 = menu (titlebar)
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            // row 1 = toolbar
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));

            tableLayoutPanel2.Controls.Clear();
            tableLayoutPanel2.Controls.Add(tableLayoutPanel8, 0, 0);
            tableLayoutPanel2.Controls.Add(flowLayoutPanel2, 0, 1);

            // ===== ROW 0 : tableLayoutPanel8 (menu + window buttons) =====
            tableLayoutPanel8.BackColor = _vsMenuBarBack;
            tableLayoutPanel8.Margin = Padding.Empty;
            tableLayoutPanel8.Padding = Padding.Empty;
            tableLayoutPanel8.Dock = DockStyle.Fill;

            // Menu benar-benar nempel kiri
            crownMenuStrip1.Dock = DockStyle.Fill;
            crownMenuStrip1.GripStyle = ToolStripGripStyle.Hidden;
            crownMenuStrip1.AutoSize = false;
            crownMenuStrip1.Margin = Padding.Empty;
            crownMenuStrip1.Padding = new Padding(6, 1, 0, 1);   // kiri 6px

            // ===== ROW 1 : flowLayoutPanel2 (toolbar) =====
            flowLayoutPanel2.Dock = DockStyle.Fill;
            flowLayoutPanel2.BackColor = _vsMenuBarBack;
            flowLayoutPanel2.WrapContents = false;
            flowLayoutPanel2.AutoSize = false;

            // RATANYA DI SINI → kiri 6px, margin 0
            flowLayoutPanel2.Margin = new Padding(0);
            flowLayoutPanel2.Padding = new Padding(6, 2, 0, 0);

            // Biar semua control di toolbar rapi & sejajar kiri
            foreach (Control ctrl in flowLayoutPanel2.Controls)
            {
                ctrl.Margin = new Padding(2, 2, 2, 2);
            }

            flowLayoutPanel2.ResumeLayout();
            tableLayoutPanel8.ResumeLayout();
            tableLayoutPanel2.ResumeLayout();
        }

        // =====================================================================
        //  STYLING MENU STRIP & DROPDOWN
        // =====================================================================
        private void ApplyVisualStudioMenuTheme()
        {
            if (crownMenuStrip1 == null)
                return;

            crownMenuStrip1.BackColor = _vsMenuBarBack;
            crownMenuStrip1.ForeColor = _vsMenuText;
            crownMenuStrip1.Font = new Font("Segoe UI", 9f);
            crownMenuStrip1.Padding = new Padding(4, 1, 0, 1);
            crownMenuStrip1.RenderMode = ToolStripRenderMode.Professional;
            crownMenuStrip1.Renderer = new VisualStudioToolStripRenderer(
                new VisualStudio2019ColorTable(
                    _vsMenuBarBack,
                    _vsDropDownBack,
                    _vsDropDownBorder,
                    _vsItemHoverBack,
                    _vsItemPressedBack));

            // Top level: File / Setting / Tools / System / Debug
            foreach (ToolStripMenuItem top in crownMenuStrip1.Items.OfType<ToolStripMenuItem>())
            {
                top.ForeColor = _vsMenuText;
                top.Margin = Padding.Empty;
                top.Padding = new Padding(8, 0, 8, 0);
                top.AutoSize = true;

                StyleDropDownRecursive(top);
            }

            crownMenuStrip1.Invalidate();
            crownMenuStrip1.Refresh();
        }

        private void StyleDropDownRecursive(ToolStripMenuItem root)
        {
            if (root.DropDown is ToolStripDropDownMenu dd)
            {
                dd.ShowImageMargin = true;
                dd.ShowCheckMargin = false;
                dd.BackColor = _vsDropDownBack;
                dd.ForeColor = _vsMenuText;
                dd.Padding = Padding.Empty;
            }

            foreach (ToolStripItem item in root.DropDownItems)
            {
                if (item is ToolStripSeparator)
                    continue;

                item.BackColor = _vsDropDownBack;
                item.ForeColor = item.Enabled ? _vsMenuText : _vsMenuTextDisabled;

                // Gutter 24px di kiri untuk icon (VS-style)
                item.Padding = new Padding(24, 2, 10, 2);

                if (item is ToolStripMenuItem mi && mi.HasDropDownItems)
                    StyleDropDownRecursive(mi);
            }
        }
    }

    // ============================== SUPPORT CLASS ==============================

    internal sealed class VisualStudio2019ColorTable : ProfessionalColorTable
    {
        private readonly Color _menuBarBack;
        private readonly Color _dropDownBack;
        private readonly Color _border;
        private readonly Color _itemHover;
        private readonly Color _itemPressed;

        public VisualStudio2019ColorTable(
            Color menuBarBack,
            Color dropDownBack,
            Color border,
            Color itemHover,
            Color itemPressed)
        {
            _menuBarBack = menuBarBack;
            _dropDownBack = dropDownBack;
            _border = border;
            _itemHover = itemHover;
            _itemPressed = itemPressed;
            UseSystemColors = false;
        }

        public override Color MenuStripGradientBegin => _menuBarBack;
        public override Color MenuStripGradientEnd => _menuBarBack;
        public override Color ToolStripGradientBegin => _menuBarBack;
        public override Color ToolStripGradientMiddle => _menuBarBack;
        public override Color ToolStripGradientEnd => _menuBarBack;
        public override Color ToolStripDropDownBackground => _dropDownBack;

        public override Color ImageMarginGradientBegin => _dropDownBack;
        public override Color ImageMarginGradientMiddle => _dropDownBack;
        public override Color ImageMarginGradientEnd => _dropDownBack;

        public override Color MenuBorder => _border;
        public override Color MenuItemBorder => _border;
        public override Color MenuItemSelected => _itemHover;
        public override Color MenuItemSelectedGradientBegin => _itemHover;
        public override Color MenuItemSelectedGradientEnd => _itemHover;
        public override Color MenuItemPressedGradientBegin => _itemPressed;
        public override Color MenuItemPressedGradientEnd => _itemPressed;
    }

    internal sealed class VisualStudioToolStripRenderer : ToolStripProfessionalRenderer
    {
        public VisualStudioToolStripRenderer(ProfessionalColorTable colorTable)
            : base(colorTable)
        {
            RoundedEdges = false;
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var item = e.Item;
            var g = e.Graphics;
            var bounds = new Rectangle(Point.Empty, item.Size);

            bool topLevel = item.OwnerItem == null && item.Owner is MenuStrip;
            var ct = ColorTable as VisualStudio2019ColorTable;

            if (item.Selected || item.Pressed)
            {
                Color back = topLevel
                    ? (item.Pressed ? ct.MenuItemPressedGradientBegin
                                    : ct.MenuItemSelected)
                    : ct.MenuItemSelected;

                using (var b = new SolidBrush(back))
                    g.FillRectangle(b, bounds);
            }
            else
            {
                using (var b = new SolidBrush(item.Owner.BackColor))
                    g.FillRectangle(b, bounds);
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Enabled
                ? Color.FromArgb(241, 241, 241)
                : Color.FromArgb(153, 153, 153);

            base.OnRenderItemText(e);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            Rectangle r = e.Item.ContentRectangle;
            int y = r.Top + r.Height / 2;
            using (var p = new Pen(Color.FromArgb(60, 60, 60)))
            {
                // Mulai setelah area icon (24px)
                e.Graphics.DrawLine(p, r.Left + 24, y, r.Right - 4, y);
            }
        }
    }
}
