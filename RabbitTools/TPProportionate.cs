using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Office = Microsoft.Office.Core;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;


namespace RabbitTools
{
    public partial class TPProportionate : UserControl
    {
        PowerPoint.Application app = Globals.ThisAddIn.Application;
        Graphics g;
        double[] nums;
        // data[0] = [minWidthIndex, maxWidthIndex, minHeightIndex, maxHeightIndex]
        float[,] data;
        PowerPoint.Selection sel;
        // Writeable SelectionRange
        PowerPoint.Shape[] wsr;
        string dir;
        // preset = 0(clear), 1(linear), 2(log), 3(pow), 4(custom)
        int preset;
        Pen p;

        public TPProportionate()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);

            g = canvas.CreateGraphics();

            app.WindowSelectionChange +=
                new PowerPoint.EApplication_WindowSelectionChangeEventHandler(HandleWindowSelectionChanged);
        }

        public void HandleWindowSelectionChanged(PowerPoint.Selection sel)
        {
            if (!Globals.ThisAddIn.TPProportionate.Visible)
                return;

            if (sel.Type == PowerPoint.PpSelectionType.ppSelectionShapes)
            {
                if (sel.ShapeRange.Count >= 2)
                {
                    pr1.Checked = true;
                    int rCode = GetNumbersFromShapeRange(sel.ShapeRange);
                    pr2.Checked = (rCode & 0b001) > 0;
                    pr3.Checked = (rCode & 0b010) > 0;
                    pr4.Checked = (rCode & 0b100) > 0;
                    Activate(sel);
                }
            }
            else
                Deactivate();
        }

        private int GetNumbersFromShapeRange(PowerPoint.ShapeRange sr)
        {
            nums = new double[sr.Count+1];
            for (int i = 1; i <= sr.Count; i++)
            {
                string txt = sr[i].TextEffect.Text;
                if (!double.TryParse(txt, out nums[i]))
                {
                    nums = null;
                    return 0b000;
                }
            }

            wsr = new PowerPoint.Shape[sr.Count + 1];
            for (int i = 1; i <= sr.Count; i++)
            {
                wsr[i] = sr[i];
            }
            // sort
            for (int i = 1; i <= sr.Count; i++)
            {
                int minIndex = i;
                for (int j = i + 1; j <= sr.Count; j++)
                {
                    minIndex = nums[minIndex] > nums[j] ? j : minIndex;
                }

                if (minIndex != i)
                {
                    PowerPoint.Shape tmp = wsr[minIndex];
                    wsr[minIndex] = wsr[i];
                    wsr[i] = tmp;
                    tmp = null;

                    nums[i] += nums[minIndex];
                    nums[minIndex] = nums[i] - nums[minIndex];
                    nums[i] -= nums[minIndex];
                }
            }

            if (nums[1] == nums[sr.Count])
            {
                nums = null;
                wsr = null;
                return 0b001;
            }

            data = new float[sr.Count + 1, 4];
            for (int i = 1; i <= sr.Count; i++)
            {
                data[i, 0] = wsr[i].Left;
                data[i, 1] = wsr[i].Top;
                data[i, 2] = (nums[i] >= 0 ? 1 : -1) * wsr[i].Width;
                data[i, 3] = (nums[i] >= 0 ? 1 : -1) * wsr[i].Height;
            }

            FindMinMaxdata(sr.Count);

            if (((dir != "L" && dir != "R") && (data[0, 2] == data[0, 3])) ||
                ((dir != "T" && dir != "B") && (data[0, 0] == data[0, 1])))
            {
                nums = null;
                wsr = null;
                data = null;
                return 0b011;
            }

            return 0b111;
        }

        private void FindMinMaxdata(int count)
        {
            int minWI = 1;
            int maxWI = 1;
            int minHI = 1;
            int maxHI = 1;
            for (int i = 1; i <= count; i++)
            {
                minWI = data[minWI, 2] > data[i, 2] ? i : minWI;
                maxWI = data[maxWI, 2] < data[i, 2] ? i : maxWI;
                minHI = data[minHI, 3] > data[i, 3] ? i : minHI;
                maxHI = data[maxHI, 3] < data[i, 3] ? i : maxHI;
            }
            data[0, 0] = minWI;
            data[0, 1] = maxWI;
            data[0, 2] = minHI;
            data[0, 3] = maxHI;
        }

        private void DrawCanvasClear()
        {
            g.Clear(Color.FromArgb(230, 230, 230));
        }

        // mode = 'x', 'y'
        private int ConvertCoord(char mode, int value)
        {
            if (mode == 'x')
            {
                return (int)((float)value / 250f * 230f + 10f);
            }
            else
            {
                return (int)((float)value / 200 * 180 + 10);
            }
        }
        
        private void DrawCanvasInfo()
        {
            DrawCanvasClear();

            p = new Pen(Color.LightGray, 1);

            // TODO: 添加“W”和“H”

            int count = nums.Length - 1;
            float YMax = Math.Max(data[(int)data[0, 1], 2], data[(int)data[0, 3], 3]);
            float YMin = Math.Min(data[(int)data[0, 3], 3], data[(int)data[0, 2], 3]);
            float YRange = YMax - YMin;
            double XRange = nums[count] - nums[1];

            // Draw number points
            int y0;
            if (YMin >= 0)
                y0 = 200;
            else if (YMax <= 0)
                y0 = 0;
            else
                y0 = (int)(YMax / YRange * 200);

            g.DrawLine(p, 0, ConvertCoord('y', y0), 250, ConvertCoord('y', y0));
            
            p.Color = Color.DarkGray;
            p.Width = 4;
            int convertedY0 = ConvertCoord('y', y0);
            for (int i = 1; i <= count; i++)
            {
                g.DrawPie(p, 
                    ConvertCoord('x',
                        (int)((nums[i] - nums[1]) / XRange * 250)), 
                    convertedY0, 4, 4, 0, 360);
            }

            // Draw width curve
            if (this.dir != "T" && this.dir != "B")
            {
                p.Color = Color.FromArgb(200, 237, 125, 49);
                p.Width = 2;

                for (int i = 1; i <= count; i++)
                {
                    int y = (int)((Math.Max(data[(int)data[0, 1], 2], data[(int)data[0, 3], 3]) - data[i, 2]) / YRange * 200);
                    g.DrawPie(p,
                        ConvertCoord('x',
                            (int)((nums[i] - nums[1]) / XRange * 250) - 2),
                        ConvertCoord('y', y), 4, 4, 0, 360);
                }
            }

            // Draw height Curve
            if (this.dir != "L" && this.dir != "R")
            {
                p.Color = Color.FromArgb(200, 68, 114, 196);

                for (int i = 1; i <= count; i++)
                {
                    int y = (int)((Math.Max(data[(int)data[0, 1], 2], data[(int)data[0, 3], 3]) - data[i, 3]) / YRange * 200);
                    g.DrawPie(p,
                        ConvertCoord('x',
                            (int)((nums[i] - nums[1]) / XRange * 250) + 2),
                        ConvertCoord('y', y), 4, 4, 0, 360);
                }
            }
        }

        private void DrawCanvasLinear()
        {

        }

        private void DrawCanvasLog()
        {

        }

        private void DrawCanvasPow()
        {

        }

        private void DrawCanvasCustom()
        {

        }

        private void Activate(PowerPoint.Selection sel)
        {
            
            if (pr1.Checked && pr2.Checked && pr3.Checked && pr4.Checked)
            {
                btnOperate.Enabled = true;
                DrawCanvasInfo();
                this.sel = sel;
            }
            else if (pr1.Checked || pr2.Checked || pr3.Checked || pr4.Checked)
            {
                this.sel = sel;
            }
            else
            {
                this.sel = null;
                btnOperate.Enabled = false;
            }
        }

        private void Deactivate()
        {
            pr1.Checked = false;
            pr2.Checked = false;
            pr3.Checked = false;
            pr4.Checked = false;
            nums = null;
            data = null;
            sel = null;
            wsr = null;
            DrawCanvasClear();
            preset = 0;
            btnOperate.Enabled = false;
        }

        private void HandleDirFLChange(string dir)
        {
            this.dir = dir;
            DrawCanvasClear();

            if (sel != null)
            {
                HandleWindowSelectionChanged(sel);
            }
            
            switch (dir)
            {
                case "TL":
                    dirFL.Text = "左上";
                    dirFL.Top = 28;
                    dirFL.Left = 48;
                    break;
                case "T":
                    dirFL.Text = "上";
                    dirFL.Top = 28;
                    dirFL.Left = 129;
                    break;
                case "TR":
                    dirFL.Text = "右上";
                    dirFL.Top = 28;
                    dirFL.Left = 210;
                    break;
                case "L":
                    dirFL.Text = "左";
                    dirFL.Top = 57;
                    dirFL.Left = 48;
                    break;
                case "R":
                    dirFL.Text = "右";
                    dirFL.Top = 57;
                    dirFL.Left = 210;
                    break;
                case "BL":
                    dirFL.Text = "左下";
                    dirFL.Top = 86;
                    dirFL.Left = 48;
                    break;
                case "B":
                    dirFL.Text = "下";
                    dirFL.Top = 86;
                    dirFL.Left = 129;
                    break;
                case "BR":
                    dirFL.Text = "右下";
                    dirFL.Top = 86;
                    dirFL.Left = 210;
                    break;
                default:
                    dirFL.Text = "中心";
                    dirFL.Top = 57;
                    dirFL.Left = 129;
                    break;
            }
        }

        private void canvas_Click(object sender, EventArgs e)
        {
        }

        private void dirTL_Click(object sender, EventArgs e)
        {
            HandleDirFLChange("TL");
        }

        private void dirT_Click(object sender, EventArgs e)
        {
            HandleDirFLChange("T");
        }

        private void dirTR_Click(object sender, EventArgs e)
        {
            HandleDirFLChange("TR");
        }

        private void dirL_Click(object sender, EventArgs e)
        {
            HandleDirFLChange("L");
        }

        private void dirCTR_Click(object sender, EventArgs e)
        {
            HandleDirFLChange("CTR");
        }

        private void dirR_Click(object sender, EventArgs e)
        {
            HandleDirFLChange("R");
        }

        private void dirBL_Click(object sender, EventArgs e)
        {
            HandleDirFLChange("BL");
        }

        private void dirB_Click(object sender, EventArgs e)
        {
            HandleDirFLChange("B");
        }

        private void dirBR_Click(object sender, EventArgs e)
        {
            HandleDirFLChange("BR");
        }

        private void presetLinear_Click(object sender, EventArgs e)
        {

        }

        private void presetLog_Click(object sender, EventArgs e)
        {

        }

        private void presetPow_Click(object sender, EventArgs e)
        {

        }

        private void presetCustom_Click(object sender, EventArgs e)
        {

        }

        private void btnOperate_Click(object sender, EventArgs e)
        {

        }
    }
}
