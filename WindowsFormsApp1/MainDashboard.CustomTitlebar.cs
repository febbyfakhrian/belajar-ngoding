using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    // This partial contains ONLY the custom title bar + Visual Studio–style menu code.
    public partial class MainDashboard : Form
    {
        private Button btnMinimize;
        private Button btnMaximize;
        private Button btnClose;
        private Panel vsHeaderPanel;
        private Label lblTitle;

        // ===== Public entry from MainDashboard.cs =====
        private void SetupCustomHeaderAndMenu()
        {
            try
            {
                // Dark theme dasar body
                ApplyVisualStudioDarkTheme();

                // Buat panel header custom ala VS - DIPANGGIL DULUAN
                CreateVsHeaderPanel();

                // Style menu seperti VS
                SetupVisualStudioMenuStrip();

                // Tombol window (min/max/close) di dalam header
                CreateWindowButtons();
                UpdateMaximizeIcon();

                // Area yang bisa dipakai drag window
                AttachHeaderDragging();

                // PENTING: Set Z-order agar header di paling atas
                if (vsHeaderPanel != null && this.Controls.Contains(vsHeaderPanel))
                {
                    this.Controls.SetChildIndex(vsHeaderPanel, 0);
                }
                
                // Toolbar di bawah header (jika ada)
                if (flowLayoutPanel2 != null && this.Controls.Contains(flowLayoutPanel2))
                {
                    this.Controls.SetChildIndex(flowLayoutPanel2, 1);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SetupCustomHeaderAndMenu] Error: {ex.Message}");
                MessageBox.Show($"Error setup header: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CreateVsHeaderPanel()
        {
            // Panel header lebar penuh (satu-satunya header bar)
            vsHeaderPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30, // Sama dengan VS 2025
                BackColor = Color.FromArgb(45, 45, 48)
            };
            this.Controls.Add(vsHeaderPanel);

            // PINDAHKAN SEMUA CONTROL DARI HEADER KE TOOLBAR
            // Ini akan membersihkan header, hanya tinggal logo + menu + title + window buttons
            MoveAllControlsToToolbar();

            // Pindahkan logo (pictureBox1) jika ada
            if (pictureBox1 != null)
            {
                try
                {
                    pictureBox1.Parent?.Controls.Remove(pictureBox1);
                    vsHeaderPanel.Controls.Add(pictureBox1);

                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox1.Size = new Size(16, 16);
                    pictureBox1.Location = new Point(8, 7);
                    pictureBox1.BackColor = Color.Transparent;
                }
                catch { }
            }

            // Pindahkan menu strip ke samping logo
            if (crownMenuStrip1 != null)
            {
                try
                {
                    crownMenuStrip1.Parent?.Controls.Remove(crownMenuStrip1);
                    vsHeaderPanel.Controls.Add(crownMenuStrip1);

                    crownMenuStrip1.Dock = DockStyle.None;
                    crownMenuStrip1.AutoSize = true; // Biarkan auto size
                    crownMenuStrip1.Height = 30;
                    
                    int menuX = pictureBox1 != null ? pictureBox1.Right + 2 : 8;
                    crownMenuStrip1.Location = new Point(menuX, 0);
                }
                catch { }
            }

            // Label judul di tengah-kanan (mirip "WindowsFormsApp1 - Microsoft Visual Studio")
            lblTitle = new Label
            {
                AutoSize = true,
                ForeColor = Color.FromArgb(241, 241, 241),
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                Text = "Inspection Platform",
                Anchor = AnchorStyles.Top,
                BackColor = Color.Transparent
            };
            vsHeaderPanel.Controls.Add(lblTitle);

            // Posisi awal judul (akan diatur lagi saat resize)
            PositionTitleLabel();

            // Re-position setiap resize
            vsHeaderPanel.Resize += (s, e) =>
            {
                PositionTitleLabel();
                PositionWindowButtonsInHeader();
            };

            // Setup toolbar di bawah header (untuk dropdown dan control lainnya)
            SetupToolbarPanel();
        }

        private void MoveAllControlsToToolbar()
        {
            // Cari semua control yang bukan logo, menu, atau label title
            // Pindahkan ke flowLayoutPanel2 (toolbar)
            
            if (flowLayoutPanel2 == null) return;

            try
            {
                // List control yang TIDAK boleh dipindah
                var keepControls = new[] { "pictureBox1", "crownMenuStrip1", "lblTitle" };

                // Ambil semua control dari parent asli (biasanya dari panel lain)
                foreach (Control ctrl in this.Controls.OfType<Control>().ToList())
                {
                    if (ctrl is Panel panel && panel != vsHeaderPanel && panel != flowLayoutPanel2)
                    {
                        foreach (Control child in panel.Controls.OfType<Control>().ToList())
                        {
                            // Jangan pindah control yang harus stay
                            if (keepControls.Contains(child.Name)) continue;

                            // Pindah ke toolbar
                            if (child is ComboBox || child is Button || child is TextBox)
                            {
                                child.Parent?.Controls.Remove(child);
                                flowLayoutPanel2.Controls.Add(child);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void SetupToolbarPanel()
        {
            if (flowLayoutPanel2 == null) return;

            try
            {
                flowLayoutPanel2.Dock = DockStyle.Top;
                flowLayoutPanel2.Height = 35;
                flowLayoutPanel2.BackColor = Color.FromArgb(37, 37, 38);
                flowLayoutPanel2.Padding = new Padding(8, 5, 8, 5);
                flowLayoutPanel2.WrapContents = false;
                flowLayoutPanel2.AutoScroll = true;
                
                // Standardize semua control di toolbar
                foreach (Control ctrl in flowLayoutPanel2.Controls)
                {
                    if (ctrl is ComboBox cb)
                    {
                        cb.Height = 24;
                        cb.FlatStyle = FlatStyle.Flat;
                        cb.BackColor = Color.FromArgb(51, 51, 55);
                        cb.ForeColor = Color.FromArgb(241, 241, 241);
                        cb.Margin = new Padding(4, 0, 4, 0);
                    }
                    else if (ctrl is Button btn)
                    {
                        btn.Height = 24;
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.BackColor = Color.FromArgb(62, 62, 64);
                        btn.ForeColor = Color.FromArgb(241, 241, 241);
                        btn.FlatAppearance.BorderSize = 0;
                        btn.Margin = new Padding(4, 0, 4, 0);
                    }
                    else if (ctrl is Label lbl)
                    {
                        lbl.ForeColor = Color.FromArgb(241, 241, 241);
                        lbl.AutoSize = true;
                        lbl.Margin = new Padding(8, 4, 4, 0);
                    }
                }
            }
            catch { }
        }

        private int CalculateMenuStripWidth()
        {
            if (crownMenuStrip1 == null) return 400;

            int totalWidth = 0;
            foreach (ToolStripItem item in crownMenuStrip1.Items)
            {
                // Estimasi lebar setiap menu item
                totalWidth += item.Width > 0 ? item.Width : 60; // default 60px per item
            }
            
            // Tambah padding
            return totalWidth + 20;
        }

        private void PositionTitleLabel()
        {
            if (vsHeaderPanel == null || lblTitle == null) return;

            // Sisakan ruang untuk tombol Min/Max/Close (3 x 46 width)
            int captionButtonsWidth = 46 * 3;
            int menuWidth = crownMenuStrip1?.Width ?? 0;
            int logoWidth = pictureBox1 != null ? pictureBox1.Right + 4 : 10;

            // Posisi title di tengah antara menu dan tombol window
            int availableSpace = vsHeaderPanel.Width - logoWidth - menuWidth - captionButtonsWidth;
            int titleX = logoWidth + menuWidth + (availableSpace / 2) - (lblTitle.Width / 2);

            lblTitle.Location = new Point(
                Math.Max(titleX, logoWidth + menuWidth + 20), // minimal 20px dari menu
                8);
        }

        // =====================================================================
        //  VISUAL STUDIO 2019-LIKE DARK THEME FOR FORM HEADER
        // =====================================================================

        private void ApplyVisualStudioDarkTheme()
        {
            // Form base
            try
            {
                this.BackColor = Color.FromArgb(30, 30, 30);
            }
            catch { }

            // Panel/header belakang menu
            try
            {
                if (parrotWidgetPanel1 != null)
                    parrotWidgetPanel1.BackColor = Color.FromArgb(45, 45, 48);
            }
            catch { }

            // Title text & logo
            try { if (bigLabel1 != null) bigLabel1.ForeColor = Color.FromArgb(241, 241, 241); } catch { }
            try { if (pictureBox1 != null) pictureBox1.BackColor = Color.Transparent; } catch { }
        }

        // =====================================================================
        //  MENU STRIP : COLORS & RENDERER LIKE VISUAL STUDIO
        // =====================================================================

        // Core colors (mendekati VS 2019 Dark)
        private readonly Color menuBarBack = Color.FromArgb(45, 45, 48);
        private readonly Color menuText = Color.FromArgb(241, 241, 241);
        private readonly Color menuTextDisabled = Color.FromArgb(153, 153, 153);
        private readonly Color dropdownBack = Color.FromArgb(37, 37, 38);
        private readonly Color dropdownBorder = Color.FromArgb(51, 51, 55);
        private readonly Color itemHoverBack = Color.FromArgb(62, 62, 64);
        private readonly Color itemPressedBack = Color.FromArgb(51, 51, 55);

        private void SetupVisualStudioMenuStrip()
        {
            if (crownMenuStrip1 == null)
                return;

            // General menu strip appearance
            crownMenuStrip1.BackColor = menuBarBack;
            crownMenuStrip1.ForeColor = menuText;
            crownMenuStrip1.Font = new Font("Segoe UI", 9f);
            crownMenuStrip1.Padding = new Padding(0, 0, 0, 0);
            crownMenuStrip1.Margin = Padding.Empty;
            crownMenuStrip1.RenderMode = ToolStripRenderMode.Professional;
            crownMenuStrip1.Renderer = new VisualStudioToolStripRenderer(
                                            new VisualStudio2019ColorTable(
                                                menuBarBack,
                                                dropdownBack,
                                                dropdownBorder,
                                                itemHoverBack,
                                                itemPressedBack));

            // PENTING: Set LayoutStyle agar horizontal dan tidak wrap
            crownMenuStrip1.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            crownMenuStrip1.GripStyle = ToolStripGripStyle.Hidden;
            crownMenuStrip1.CanOverflow = true; // Allow overflow untuk menu yang terlalu panjang

            // Bersihkan menu items yang terlalu banyak jika perlu
            // Rekomendasi: Max 6-7 top-level menu items
            // Jika lebih dari itu, gabungkan ke submenu

            // Top-level items: File / Edit / View / ...
            foreach (ToolStripMenuItem top in crownMenuStrip1.Items.OfType<ToolStripMenuItem>())
            {
                top.ForeColor = menuText;
                top.Margin = new Padding(0, 0, 0, 0);
                top.Padding = new Padding(8, 4, 8, 4); // Padding lebih compact
                top.AutoSize = true;

                // Pastikan dropdown tampil seperti VS
                StyleDropDownRecursive(top);
            }

            // HIDE menu items yang jarang dipakai ke "More" menu jika terlalu panjang
            OptimizeMenuItems();

            crownMenuStrip1.Invalidate();
            crownMenuStrip1.Refresh();
        }

        private void OptimizeMenuItems()
        {
            if (crownMenuStrip1 == null || crownMenuStrip1.Items.Count <= 6)
                return;

            try
            {
                // Jika menu items lebih dari 6, pindahkan yang jarang dipakai ke "More" atau "Tools"
                // Ini optional - bisa di-comment jika tidak perlu

                // Contoh: Keep only File, Edit, View, Tools, Window, Help
                // Move sisanya ke Tools submenu
                
                // Implementation disesuaikan dengan kebutuhan aplikasi Anda
            }
            catch { }
        }

        private void StyleDropDownRecursive(ToolStripMenuItem root)
        {
            if (root.DropDown is ToolStripDropDownMenu dd)
            {
                dd.ShowImageMargin = true;   // kolom kiri untuk icon
                dd.ShowCheckMargin = false;
                dd.BackColor = dropdownBack;
                dd.ForeColor = menuText;
                dd.Padding = new Padding(2, 2, 2, 2);
            }

            foreach (ToolStripItem item in root.DropDownItems)
            {
                // Separator biarkan default
                if (item is ToolStripSeparator)
                    continue;

                item.BackColor = dropdownBack;
                item.ForeColor = item.Enabled ? menuText : menuTextDisabled;

                // Padding supaya text start setelah kolom icon (mirip VS)
                item.Padding = new Padding(24, 4, 10, 4); // 24px icon gutter

                // ====== ICON SECTION - UNCOMMENT UNTUK TAMBAH ICON ======
                // Contoh:
                // if (item.Name == "fileNewMenuItem")
                //     item.Image = Properties.Resources.IconNew_16;
                //
                // if (item.Name == "fileOpenMenuItem")
                //     item.Image = Properties.Resources.IconOpen_16;
                //
                // if (item.Name == "fileSaveMenuItem")
                //     item.Image = Properties.Resources.IconSave_16;
                // =========================================================

                if (item is ToolStripMenuItem mi && mi.HasDropDownItems)
                {
                    StyleDropDownRecursive(mi);
                }
            }
        }

        // =====================================================================
        //  CUSTOM WINDOW BUTTONS (MIN / MAX / CLOSE)
        // =====================================================================

        private void CreateWindowButtons()
        {
            if (btnClose != null) return;

            if (vsHeaderPanel == null)
                CreateVsHeaderPanel();

            btnMinimize = new Button();
            btnMaximize = new Button();
            btnClose = new Button();

            void Setup(Button b, string txt, EventHandler h)
            {
                b.Text = txt;
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 0;
                b.FlatAppearance.MouseDownBackColor = Color.FromArgb(62, 62, 64);
                b.BackColor = Color.FromArgb(45, 45, 48);
                b.ForeColor = Color.FromArgb(241, 241, 241);
                b.Width = 46;  // Sesuai VS 2025
                b.Height = 30;
                b.Margin = Padding.Empty;
                b.Padding = Padding.Empty;
                b.Click += h;
                b.TabStop = false;
                b.Cursor = Cursors.Hand;
                b.TextAlign = ContentAlignment.MiddleCenter;
                b.Font = new Font("Segoe UI", 10f);
                b.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            }

            Setup(btnMinimize, "─", (s, e) => this.WindowState = FormWindowState.Minimized);
            Setup(btnMaximize, "□", (s, e) =>
            {
                this.WindowState = (this.WindowState == FormWindowState.Maximized)
                    ? FormWindowState.Normal
                    : FormWindowState.Maximized;
                UpdateMaximizeIcon();
            });
            Setup(btnClose, "✕", (s, e) => this.Close());

            // Hover effects
            btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.FromArgb(232, 17, 35);
            btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.FromArgb(45, 45, 48);

            btnMinimize.MouseEnter += (s, e) => btnMinimize.BackColor = Color.FromArgb(62, 62, 64);
            btnMinimize.MouseLeave += (s, e) => btnMinimize.BackColor = Color.FromArgb(45, 45, 48);

            btnMaximize.MouseEnter += (s, e) => btnMaximize.BackColor = Color.FromArgb(62, 62, 64);
            btnMaximize.MouseLeave += (s, e) => btnMaximize.BackColor = Color.FromArgb(45, 45, 48);

            // Tambahkan ke header panel
            vsHeaderPanel.Controls.Add(btnClose);
            vsHeaderPanel.Controls.Add(btnMaximize);
            vsHeaderPanel.Controls.Add(btnMinimize);

            PositionWindowButtonsInHeader();
        }

        private void PositionWindowButtonsInHeader()
        {
            if (vsHeaderPanel == null || btnClose == null) return;

            int marginRight = 0;
            int top = 0;

            int xClose = vsHeaderPanel.Width - btnClose.Width - marginRight;
            btnClose.Location = new Point(xClose, top);

            int xMax = xClose - btnMaximize.Width;
            btnMaximize.Location = new Point(xMax, top);

            int xMin = xMax - btnMinimize.Width;
            btnMinimize.Location = new Point(xMin, top);

            btnClose.BringToFront();
            btnMaximize.BringToFront();
            btnMinimize.BringToFront();

            // update posisi title supaya tidak ketabrak tombol
            PositionTitleLabel();
        }

        private void UpdateMaximizeIcon()
        {
            if (btnMaximize == null) return;
            btnMaximize.Text = (this.WindowState == FormWindowState.Maximized) ? "❐" : "□";
        }

        // =====================================================================
        //  DRAGGING WINDOW DARI CUSTOM HEADER
        // =====================================================================

        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private void AttachHeaderDragging()
        {
            // Hanya vsHeaderPanel dan pictureBox yang bisa drag
            // Menu TIDAK bisa drag agar dropdown bisa diklik
            var dragControls = new Control[] { vsHeaderPanel, pictureBox1, lblTitle };
            
            foreach (var c in dragControls.Where(c => c != null))
            {
                c.MouseDown += TitleArea_MouseDown;
                c.MouseMove += TitleArea_MouseMove;
                c.MouseUp += TitleArea_MouseUp;
                c.MouseDoubleClick += TitleArea_MouseDoubleClick;
            }
        }

        private void TitleArea_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = Location;
        }

        private void TitleArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (!dragging) return;

            Point diff = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
            Location = Point.Add(dragFormPoint, new Size(diff));
        }

        private void TitleArea_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            dragging = false;
        }

        private void TitleArea_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            // Double click = maximize/restore
            this.WindowState = (this.WindowState == FormWindowState.Maximized)
                ? FormWindowState.Normal
                : FormWindowState.Maximized;
            UpdateMaximizeIcon();
        }

        // =====================================================================
        //  DEBUG HELPER (opsional)
        // =====================================================================

        private void DebugMenuItems()
        {
            try
            {
                Debug.WriteLine($"[Menu Debug] crownMenuStrip1 items = {crownMenuStrip1?.Items.Count}");
                if (crownMenuStrip1 != null)
                {
                    foreach (ToolStripItem ti in crownMenuStrip1.Items)
                    {
                        Debug.WriteLine($"[Menu Debug] Top: '{ti.Text}' Type={ti.GetType().Name} Bounds={ti.Bounds}");
                        if (ti is ToolStripMenuItem tmi && tmi.DropDownItems.Count > 0)
                        {
                            foreach (ToolStripItem sub in tmi.DropDownItems)
                            {
                                Debug.WriteLine($"  - Sub: '{sub.Text}' Type={sub.GetType().Name} Enabled={sub.Enabled}");
                            }
                        }
                    }
                }
            }
            catch { }
        }
    }

    // ========================================================================
    //  SUPPORT CLASSES FOR VS-STYLE TOOLSTRIP RENDERING
    // ========================================================================

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
            Rectangle bounds = new Rectangle(Point.Empty, item.Size);

            bool topLevel = item.OwnerItem == null && item.Owner is MenuStrip;

            if (item.Selected || item.Pressed)
            {
                Color back;

                if (topLevel)
                {
                    // item top bar
                    back = item.Pressed
                        ? ((VisualStudio2019ColorTable)ColorTable).MenuItemPressedGradientBegin
                        : ((VisualStudio2019ColorTable)ColorTable).MenuItemSelected;
                }
                else
                {
                    back = ((VisualStudio2019ColorTable)ColorTable).MenuItemSelected;
                }

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
            if (!e.Item.Enabled)
                e.TextColor = Color.FromArgb(153, 153, 153);
            else
                e.TextColor = Color.FromArgb(241, 241, 241);

            base.OnRenderItemText(e);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            Rectangle r = e.Item.ContentRectangle;
            int y = r.Top + r.Height / 2;
            using (var p = new Pen(Color.FromArgb(60, 60, 60)))
            {
                // mulai setelah margin icon
                e.Graphics.DrawLine(p, r.Left + 24, y, r.Right - 4, y);
            }
        }
    }
}