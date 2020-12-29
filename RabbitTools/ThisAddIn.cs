using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;

namespace RabbitTools
{
    public partial class ThisAddIn
    {
        private TaskPaneGrid controlOfPaneGrid;
        private Microsoft.Office.Tools.CustomTaskPane taskPaneGrid;
        public Microsoft.Office.Tools.CustomTaskPane TaskPaneGrid
        {
            get
            {
                return taskPaneGrid;
            }
        }

        private TPProportionate controlOfTPProportionate;
        private Microsoft.Office.Tools.CustomTaskPane tpProportionate;
        public Microsoft.Office.Tools.CustomTaskPane TPProportionate
        {
            get
            {
                return tpProportionate;
            }
        }

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            controlOfPaneGrid = new TaskPaneGrid();
            taskPaneGrid = this.CustomTaskPanes.Add(controlOfPaneGrid, "网格布局工具");
            taskPaneGrid.Width = 270;

            controlOfTPProportionate = new TPProportionate();
            tpProportionate = this.CustomTaskPanes.Add(controlOfTPProportionate, "尺寸比例化工具");
            tpProportionate.Width = 290;
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region VSTO 生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
