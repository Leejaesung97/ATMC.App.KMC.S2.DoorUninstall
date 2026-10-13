
using ATMC.Common;

namespace ATMC.App.KMC.S2.DoorUninstall
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.tblLayout = new System.Windows.Forms.TableLayoutPanel();
            this.sv_Main = new ATMC.Common.ViewCardSceneViewer();
            this.dashboard = new ATMC.Common.ViewCardWebView2();
            this.statusbar = new Abeo.Controls.ZeroCode.FlatStatusBar();
            this.resultGrid = new Abeo.Controls.AdvancedDataGrid();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.logfileWatcher = new LotusAPI.Utils.LogFileWatcher(this.components);
            this.titlebar = new ATMC.Common.TitleBar();
            this.PANEL = new Abeo.Controls.ZeroCode.FlatTitlePanel();
            this.CycleInfo = new ATMC.Common.CycleInfoView();
            this.tblLayout.SuspendLayout();
            this.titlebar.SuspendLayout();
            this.SuspendLayout();
            // 
            // tblLayout
            // 
            this.tblLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(22)))), ((int)(((byte)(33)))));
            this.tblLayout.ColumnCount = 2;
            this.tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 568F));
            this.tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblLayout.Controls.Add(this.sv_Main, 0, 0);
            this.tblLayout.Controls.Add(this.dashboard, 1, 0);
            this.tblLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblLayout.Location = new System.Drawing.Point(0, 32);
            this.tblLayout.Margin = new System.Windows.Forms.Padding(0);
            this.tblLayout.Name = "tblLayout";
            this.tblLayout.RowCount = 1;
            this.tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblLayout.Size = new System.Drawing.Size(1770, 918);
            this.tblLayout.TabIndex = 4;
            // 
            // sv_Main
            // 
            this.sv_Main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sv_Main.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(77)))));
            this.sv_Main.DetailFont = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.sv_Main.DetailForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(146)))), ((int)(((byte)(161)))), ((int)(((byte)(162)))));
            this.sv_Main.DetailVisible = false;
            this.sv_Main.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.sv_Main.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(146)))), ((int)(((byte)(161)))), ((int)(((byte)(162)))));
            this.sv_Main.Label = "SCENE";
            this.sv_Main.LabelFont = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sv_Main.LabelForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(146)))), ((int)(((byte)(161)))), ((int)(((byte)(162)))));
            this.sv_Main.LabelVisible = false;
            this.sv_Main.Location = new System.Drawing.Point(0, 0);
            this.sv_Main.Margin = new System.Windows.Forms.Padding(0);
            this.sv_Main.Name = "sv_Main";
            this.sv_Main.Padding = new System.Windows.Forms.Padding(1);
            this.sv_Main.Size = new System.Drawing.Size(1202, 918);
            this.sv_Main.TabIndex = 0;
            // 
            // dashboard
            // 
            this.dashboard.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dashboard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(77)))));
            this.dashboard.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.dashboard.IndexFilePath = "src/dashboard/index.html";
            this.dashboard.Label = "DASHBOARD";
            this.dashboard.LabelFont = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dashboard.LabelForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(146)))), ((int)(((byte)(161)))), ((int)(((byte)(162)))));
            this.dashboard.LabelVisible = false;
            this.dashboard.Location = new System.Drawing.Point(1204, 0);
            this.dashboard.Margin = new System.Windows.Forms.Padding(2, 0, 0, 0);
            this.dashboard.Name = "dashboard";
            this.dashboard.Padding = new System.Windows.Forms.Padding(1);
            this.dashboard.Size = new System.Drawing.Size(566, 918);
            this.dashboard.SourceDir = "web";
            this.dashboard.TabIndex = 1;
            // 
            // statusbar
            // 
            this.statusbar.Animate = false;
            this.statusbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(11)))), ((int)(((byte)(22)))));
            this.statusbar.ClockColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(141)))), ((int)(((byte)(211)))));
            this.statusbar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusbar.FixedHeight = 24;
            this.statusbar.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.statusbar.FontResourceMonitor = new System.Drawing.Font("Consolas", 10F);
            this.statusbar.FontRTC = new System.Drawing.Font("Segoe UI", 10F);
            this.statusbar.FontStatus = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusbar.FontVersion = new System.Drawing.Font("Segoe UI", 10F);
            this.statusbar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(146)))), ((int)(((byte)(161)))), ((int)(((byte)(162)))));
            this.statusbar.FreeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(54)))), ((int)(((byte)(66)))));
            this.statusbar.HighUseColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.statusbar.Location = new System.Drawing.Point(0, 950);
            this.statusbar.LowUseColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.statusbar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.statusbar.MediumUseColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(196)))), ((int)(((byte)(13)))));
            this.statusbar.Name = "statusbar";
            this.statusbar.RTCWidth = 192;
            this.statusbar.ShowRunningGirl = false;
            this.statusbar.Size = new System.Drawing.Size(1770, 24);
            // 
            // 
            // 
            this.statusbar.StatusItems.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(33)))), ((int)(((byte)(44)))));
            this.statusbar.StatusItems.BackColorOFF = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.statusbar.StatusItems.BackColorON = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.statusbar.StatusItems.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(44)))), ((int)(((byte)(55)))));
            this.statusbar.StatusItems.Dock = System.Windows.Forms.DockStyle.Left;
            this.statusbar.StatusItems.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusbar.StatusItems.ForeColor = System.Drawing.Color.Black;
            this.statusbar.StatusItems.ForeColorOFF = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(22)))), ((int)(((byte)(33)))));
            this.statusbar.StatusItems.ForeColorON = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(22)))), ((int)(((byte)(33)))));
            this.statusbar.StatusItems.Items = "";
            this.statusbar.StatusItems.ItemWidth = 64;
            this.statusbar.StatusItems.Location = new System.Drawing.Point(60, 0);
            this.statusbar.StatusItems.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.statusbar.StatusItems.Name = "statusItems";
            this.statusbar.StatusItems.ShowBorder = true;
            this.statusbar.StatusItems.Size = new System.Drawing.Size(0, 24);
            this.statusbar.StatusItems.Style = Abeo.Controls.ZeroCode.StatusItemPanel.StyleEnum.Flat;
            this.statusbar.StatusItems.TabIndex = 8;
            this.statusbar.StatusItems.ItemDoubleClickedEvent += new Abeo.Controls.ZeroCode.StatusItemPanel.ItemDoubleClickedEventHandler(this.statusbar_StatusItems_ItemDoubleClickedEvent);
            this.statusbar.StripeBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(11)))), ((int)(((byte)(22)))));
            this.statusbar.StripeForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(33)))), ((int)(((byte)(44)))));
            this.statusbar.StripeInterval = 100;
            this.statusbar.StripeMotionEnable = false;
            this.statusbar.StripeWidth = 8;
            this.statusbar.TabIndex = 0;
            // 
            // resultGrid
            // 
            this.resultGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(33)))), ((int)(((byte)(44)))));
            this.resultGrid.CellBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(43)))), ((int)(((byte)(55)))));
            this.resultGrid.CellColorMode = false;
            this.resultGrid.CellTextPadding = 3;
            this.resultGrid.ColumnBorderOnly = false;
            this.resultGrid.DataSource = null;
            this.resultGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultGrid.ExpandLastColumn = true;
            this.resultGrid.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.resultGrid.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(231)))), ((int)(((byte)(212)))));
            this.resultGrid.HeaderBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(54)))), ((int)(((byte)(67)))));
            this.resultGrid.HeaderFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.resultGrid.HeaderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(141)))), ((int)(((byte)(212)))));
            this.resultGrid.ItemBackColorEven = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(54)))), ((int)(((byte)(67)))));
            this.resultGrid.ItemBackColorOdd = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(43)))), ((int)(((byte)(55)))));
            this.resultGrid.ItemForeColorEven = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(44)))), ((int)(((byte)(55)))));
            this.resultGrid.ItemForeColorOdd = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(55)))), ((int)(((byte)(66)))));
            this.resultGrid.Location = new System.Drawing.Point(0, 70);
            this.resultGrid.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.resultGrid.Name = "resultGrid";
            this.resultGrid.NumberFormat = "0.000";
            this.resultGrid.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(245)))), ((int)(((byte)(226)))));
            this.resultGrid.ShowHeader = true;
            this.resultGrid.ShowSelection = false;
            this.resultGrid.Size = new System.Drawing.Size(385, 170);
            this.resultGrid.StretchRows = false;
            this.resultGrid.TabIndex = 2;
            this.resultGrid.UniformColumnSize = false;
            // 
            // logfileWatcher
            // 
            this.logfileWatcher.Interval = 3600000;
            this.logfileWatcher.LogDirectory = "./Logs/";
            this.logfileWatcher.LogLevel = 127;
            // 
            // titlebar
            // 
            this.titlebar.Auto = false;
            this.titlebar.Dock = System.Windows.Forms.DockStyle.Top;
            this.titlebar.FixedHeight = 32;
            this.titlebar.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.titlebar.Location = new System.Drawing.Point(0, 0);
            this.titlebar.Name = "titlebar";
            this.titlebar.ShowRunningGirl = false;
            this.titlebar.Size = new System.Drawing.Size(1770, 32);
            this.titlebar.StripeBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(11)))), ((int)(((byte)(22)))));
            this.titlebar.StripeForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(33)))), ((int)(((byte)(44)))));
            this.titlebar.TabIndex = 10;
            this.titlebar.HelpButtonClickEvent += new System.EventHandler(this.titlebar_HelpButtonClickEvent);
            this.titlebar.AutoManualDoubleClickEvent += new System.EventHandler(this.titlebar_AutoManualDoubleClickEvent);
            // 
            // PANEL
            // 
            this.PANEL.Animate = false;
            this.PANEL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(22)))), ((int)(((byte)(33)))));
            this.PANEL.Dock = System.Windows.Forms.DockStyle.Top;
            this.PANEL.FixedHeight = 32;
            this.PANEL.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.PANEL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(98)))), ((int)(((byte)(123)))), ((int)(((byte)(131)))));
            this.PANEL.Location = new System.Drawing.Point(0, 1);
            this.PANEL.Name = "PANEL";
            this.PANEL.ShowRunningGirl = false;
            this.PANEL.Size = new System.Drawing.Size(1770, 32);
            this.PANEL.StripeBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(22)))), ((int)(((byte)(33)))));
            this.PANEL.StripeForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(33)))), ((int)(((byte)(44)))));
            this.PANEL.StripeInterval = 100;
            this.PANEL.StripeMotionEnable = false;
            this.PANEL.StripeWidth = 8;
            this.PANEL.TabIndex = 0;
            // 
            // CycleInfo
            // 
            this.CycleInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(22)))), ((int)(((byte)(33)))));
            this.CycleInfo.Dock = System.Windows.Forms.DockStyle.Right;
            this.CycleInfo.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CycleInfo.Items = "MODEL\r\nBODY_NO\r\nSEQ_NO";
            this.CycleInfo.LabelAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.CycleInfo.LabelBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(33)))), ((int)(((byte)(44)))));
            this.CycleInfo.LabelForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(146)))), ((int)(((byte)(161)))), ((int)(((byte)(162)))));
            this.CycleInfo.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(44)))), ((int)(((byte)(55)))));
            this.CycleInfo.Location = new System.Drawing.Point(917, 0);
            this.CycleInfo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.CycleInfo.Name = "CycleInfo";
            this.CycleInfo.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.CycleInfo.Size = new System.Drawing.Size(853, 32);
            this.CycleInfo.TabIndex = 5;
            this.CycleInfo.ValueAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.CycleInfo.ValueBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(22)))), ((int)(((byte)(33)))));
            this.CycleInfo.ValueForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(232)))), ((int)(((byte)(213)))));
            // 
            // FormMain
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(231)))), ((int)(((byte)(212)))));
            this.ClientSize = new System.Drawing.Size(1770, 974);
            this.Controls.Add(this.tblLayout);
            this.Controls.Add(this.statusbar);
            this.Controls.Add(this.titlebar);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(33)))), ((int)(((byte)(44)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MinimumSize = new System.Drawing.Size(1764, 943);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormMain";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.tblLayout.ResumeLayout(false);
            this.titlebar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Abeo.Controls.ZeroCode.FlatStatusBar statusbar;
        private Abeo.Controls.AdvancedDataGrid resultGrid;
        private LotusAPI.Utils.LogFileWatcher logfileWatcher;
        private System.Windows.Forms.ToolTip toolTip1;
        private TitleBar titlebar;
        private System.Windows.Forms.TableLayoutPanel tblLayout;
        private ViewCardSceneViewer sv_Main;
        private ViewCardWebView2 dashboard;
        private Abeo.Controls.ZeroCode.FlatTitlePanel PANEL;
        private CycleInfoView CycleInfo;
    }
}

