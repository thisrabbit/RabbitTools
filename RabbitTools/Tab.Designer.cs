namespace RabbitTools
{
    partial class Tab : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public Tab()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TabPabbit = this.Factory.CreateRibbonTab();
            this.GroupLayout = this.Factory.CreateRibbonGroup();
            this.LayoutGrid = this.Factory.CreateRibbonButton();
            this.GroupShape = this.Factory.CreateRibbonGroup();
            this.ShapeProportionate = this.Factory.CreateRibbonButton();
            this.TabPabbit.SuspendLayout();
            this.GroupLayout.SuspendLayout();
            this.GroupShape.SuspendLayout();
            this.SuspendLayout();
            // 
            // TabPabbit
            // 
            this.TabPabbit.Groups.Add(this.GroupLayout);
            this.TabPabbit.Groups.Add(this.GroupShape);
            this.TabPabbit.Label = "兔子工具箱";
            this.TabPabbit.Name = "TabPabbit";
            this.TabPabbit.Position = this.Factory.RibbonPosition.AfterOfficeId("TabInsert");
            // 
            // GroupLayout
            // 
            this.GroupLayout.Items.Add(this.LayoutGrid);
            this.GroupLayout.Label = "布局";
            this.GroupLayout.Name = "GroupLayout";
            // 
            // LayoutGrid
            // 
            this.LayoutGrid.Label = "网格";
            this.LayoutGrid.Name = "LayoutGrid";
            this.LayoutGrid.ScreenTip = "网格布局";
            this.LayoutGrid.SuperTip = "网格状分割大形状并快速任意合并";
            this.LayoutGrid.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.LayoutGrid_Click);
            // 
            // GroupShape
            // 
            this.GroupShape.Items.Add(this.ShapeProportionate);
            this.GroupShape.Label = "形状";
            this.GroupShape.Name = "GroupShape";
            // 
            // ShapeProportionate
            // 
            this.ShapeProportionate.Label = "比例化";
            this.ShapeProportionate.Name = "ShapeProportionate";
            this.ShapeProportionate.ScreenTip = "形状比例化";
            this.ShapeProportionate.SuperTip = "按照指定的数学公式将包含数字的形状的尺寸调整为与内含数字成比例";
            // 
            // Tab
            // 
            this.Name = "Tab";
            this.RibbonType = "Microsoft.PowerPoint.Presentation";
            this.Tabs.Add(this.TabPabbit);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.Tab_Load);
            this.TabPabbit.ResumeLayout(false);
            this.TabPabbit.PerformLayout();
            this.GroupLayout.ResumeLayout(false);
            this.GroupLayout.PerformLayout();
            this.GroupShape.ResumeLayout(false);
            this.GroupShape.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Office.Tools.Ribbon.RibbonTab TabPabbit;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup GroupLayout;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton LayoutGrid;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup GroupShape;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton ShapeProportionate;
    }

    partial class ThisRibbonCollection
    {
        internal Tab Tab
        {
            get { return this.GetRibbon<Tab>(); }
        }
    }
}
