using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Office = Microsoft.Office.Core;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Microsoft.Office.Interop.PowerPoint;
using Microsoft.VisualStudio.Tools.Applications.Runtime;
using System.Numerics;

namespace RabbitTools
{
    public partial class TaskPaneGrid : UserControl
    {
        public TaskPaneGrid()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);

            app.WindowSelectionChange +=
                new PowerPoint.EApplication_WindowSelectionChangeEventHandler(HandleWindowSelectionChanged);
        }

        PowerPoint.Application app = Globals.ThisAddIn.Application;
        
        public void HandleWindowSelectionChanged(PowerPoint.Selection sel)
        {
            if (sel.Type == PowerPoint.PpSelectionType.ppSelectionShapes)
            {
                //if (sel.HasChildShapeRange)
                //{
                //    label12.Text = "请不要选择组合形状";
                //    deactivate();
                //}
                //else
                if (sel.ShapeRange.Count > 1)
                {
                    activate(sel.ShapeRange.Count);
                    canvas = sel.ShapeRange;
                    label12.Text = "选择了多个形状";
                }
                else
                {
                    activate(1);
                    canvas = sel.ShapeRange;
                    textBox1.Text = canvas.Width.ToString();
                    textBox2.Text = canvas.Height.ToString();
                    label12.Text = "以该形状为绘制区域";
                }
            }
            else
            {
                deactivate();
            }
        }

        private void activate(int selectedCount)
        {
            deactivate();
            button3.Enabled = true;
            if (selectedCount > 1)
            {
                button2.Enabled = true;
            }
            else
            {
                button1.Enabled = true;
            }
        }

        private void deactivate()
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;

            label12.Text = "未选择形状";

            textBox1.Text = "";
            textBox2.Text = "";

            canvas = null;
        }

        PowerPoint.ShapeRange canvas;

        // btn-Generate
        private void button1_Click(object sender, EventArgs e)
        {
            float canX = canvas[1].Left;
            float canY = canvas[1].Top;
            float canW = canvas[1].Width;
            float canH = canvas[1].Height;

            int countW = int.Parse(numericUpDown2.Value.ToString());
            int countH = int.Parse(numericUpDown1.Value.ToString());
            float marginW = float.Parse(numericUpDown4.Value.ToString());
            float marginH = float.Parse(numericUpDown3.Value.ToString());

            float oneW = (canW - (countW - 1) * marginW) / countW;
            float oneH = (canH - (countH - 1) * marginH) / countH;

            oneW = oneW <= 0 ? 1 : oneW;
            oneH = oneH <= 0 ? 1 : oneH;

            for (int i = 0; i < countW; i++)
            {
                for (int j = 0; j < countH; j++)
                {
                    PowerPoint.Shape dup = canvas[1].Duplicate()[1];
                    dup.Width = oneW;
                    dup.Height = oneH;
                    dup.Left = canX + (oneW + marginW) * i;
                    dup.Top = canY + (oneH + marginH) * j;
                    dup.Select(Office.MsoTriState.msoFalse);
                }
            }

            canvas.Delete();
        }

        // btn-merge
        private void button2_Click(object sender, EventArgs e)
        {
            float posX = canvas[1].Left;
            float posY = canvas[1].Top;
            float width = canvas[canvas.Count].Left - posX + canvas[canvas.Count].Width;
            float height = canvas[canvas.Count].Top - posY + canvas[canvas.Count].Height;

            List<ComparableVector2> topLeft = new List<ComparableVector2>();
            List<ComparableVector2> bottomRight = new List<ComparableVector2>();

            for (int i = 1; i <= canvas.Count; i++)
            {
                topLeft.Add(new ComparableVector2(canvas[i].Left, canvas[i].Top));
                bottomRight.Add(new ComparableVector2(canvas[i].Left + canvas[i].Width,
                    canvas[i].Top + canvas[i].Height));
            }
            ComparableVector2 minTL = topLeft.Min();
            ComparableVector2 maxBR = bottomRight.Max();

            PowerPoint.Shape dup = canvas[1].Duplicate()[1];
            dup.Left = minTL.X;
            dup.Top = minTL.Y;
            dup.Width = maxBR.X - minTL.X;
            dup.Height = maxBR.Y - minTL.Y;
            canvas.Delete();
            dup.Select();
        }

        // btn-unify
        private void button3_Click(object sender, EventArgs e)
        {
            PowerPoint.ShapeRange shapes = app.ActiveWindow.Selection.ShapeRange;

            for (int i = 1; i <= shapes.Count; i++)
            {
                PowerPoint.Adjustments adj = shapes[i].Adjustments;
                if (adj.Count == 0)
                {
                    shapes[i].AutoShapeType = Office.MsoAutoShapeType.msoShapeRoundedRectangle;
                    adj = shapes[i].Adjustments;
                }
                adj[1] = float.Parse(numericUpDown5.Value.ToString()) / 
                    (shapes[i].Width >= shapes[i].Height ? shapes[i].Height : shapes[i].Width);
            }
        }
    }
}
