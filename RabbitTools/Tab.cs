using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;

namespace RabbitTools
{
    public partial class Tab
    {
        private void Tab_Load(object sender, RibbonUIEventArgs e)
        {

        }

        private void LayoutGrid_Click(object sender, RibbonControlEventArgs e)
        {
            Globals.ThisAddIn.TaskPaneGrid.Visible = true;
        }
    }
}
