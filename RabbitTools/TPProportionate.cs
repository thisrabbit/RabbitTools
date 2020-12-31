using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Office = Microsoft.Office.Core;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;


namespace RabbitTools
{
    // TODO: Fix initial selection and content save when window is changed
    public partial class TPProportionate : UserControl
    {
        PowerPoint.Application app = Globals.ThisAddIn.Application;
        Graphics g;
        Pen p;
        double[] nums;
        // data[0] = [minWidthIndex, maxWidthIndex, minHeightIndex, maxHeightIndex]
        float[,] data;
        PowerPoint.Selection sel;
        // Writeable SelectionRange
        PowerPoint.Shape[] wsr;
        string dir = "CTR";
        // [PresetForWidth, PresetForHeight]
        string[] preset = new string[2];
        // Be true if Width == Height for all shapes
        bool uniControl = true;
        ///[WidthLeftCorNerX,   WLCNY, 
        /// WidthLeftConTrolX,  WLCTY,
        /// WidthRightConTrolX, WRCTY,
        /// WidthRightCorNerX,  WRCNY,
        /// HLCNX,              HLCNY,
        /// HLCTX,              HLCTX,
        /// HRCTX,              HRCTY,
        /// HRCNX,              HRCTY]
        int[] curvePoint = new int[16];

        public TPProportionate()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);

            g = canvas.CreateGraphics();
            p = new Pen(Color.LightGray, 1);

            app.WindowSelectionChange +=
                new PowerPoint.EApplication_WindowSelectionChangeEventHandler(HandleWindowSelectionChanged);
        }

        public void HandleWindowSelectionChanged(PowerPoint.Selection sel)
        {
            if (!Globals.ThisAddIn.TPProportionate.Visible)
                return;

            if (sel.Type == PowerPoint.PpSelectionType.ppSelectionShapes)
            {
                if (sel.ShapeRange.Count >= 3)
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

                uniControl &= data[i, 2] == data[i, 3];
            }

            FindMinMaxdata(sr.Count);

            if (((dir != "L" && dir != "R") && (data[0, 2] >= data[0, 3])) ||
                ((dir != "T" && dir != "B") && (data[0, 0] >= data[0, 1])))
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

        private void DrawCanvasClear(string mode)
        {
            if (mode == "refresh")
            {
                g.Clear(Color.FromArgb(230, 230, 230));
            }
            else if (mode == "init")
            {
                DrawCanvasClear("refresh");
                p.Color = Color.LightGray;
                p.Width = 1;
                g.DrawLine(p, 0, 0, 250, 200);
            }
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
                return (int)((float)value / 200f * 180f + 10f);
            }
        }
        
        private void DrawCanvasInfo()
        {
            DrawCanvasClear("refresh");

            int count = nums.Length - 1;
            float YMax, YMin;
            if (this.dir == "T" || this.dir == "B")
            {
                YMax = data[(int)data[0, 3], 3];
                YMin = data[(int)data[0, 2], 3];
            }
            else if (this.dir == "L" || this.dir == "R")
            {
                YMax = data[(int)data[0, 1], 2];
                YMin = data[(int)data[0, 0], 2];
            }
            else
            {
                YMax = Math.Max(data[(int)data[0, 1], 2], data[(int)data[0, 3], 3]);
                YMin = Math.Min(data[(int)data[0, 0], 2], data[(int)data[0, 2], 3]);
            }
            
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
            
            int convertedY0 = ConvertCoord('y', y0);

            p.Color = Color.LightGray;
            p.Width = 1;

            g.DrawLine(p, 0, convertedY0, 250, convertedY0);
            
            p.Color = Color.DarkGray;
            p.Width = 4;
            for (int i = 1; i <= count; i++)
            {
                g.DrawPie(p, 
                    ConvertCoord('x',
                        (int)((nums[i] - nums[1]) / XRange * 250)), 
                    convertedY0, 4, 4, 0, 360);
            }

            char currentXOnlyState = TestCurrentXOnlyState();

            int offset = 2;
            string axis = "W";
            // Draw width points
            if (this.dir != "T" && this.dir != "B")
            {
                if (currentXOnlyState == 'H')
                    p.Color = Color.FromArgb(50, 237, 125, 49);
                else
                    p.Color = Color.FromArgb(200, 237, 125, 49);

                if (this.dir == "L" || this.dir == "R")
                    offset = 0;
                else
                {
                    if (currentXOnlyState == 'U')
                    {
                        p.Color = Color.FromArgb(200, 206, 49, 237);
                        offset = 0;
                        axis = "W&H";
                    }
                }

                p.Width = 2;

                for (int i = 1; i <= count; i++)
                {
                    int x = (int)((nums[i] - nums[1]) / XRange * 250) - offset;
                    int y = (int)((YMax - data[i, 2]) / YRange * 200);

                    if (i == 1)
                    {
                        curvePoint[0] = x;
                        curvePoint[1] = y;
                    }
                    else if (i == count)
                    {
                        curvePoint[6] = x;
                        curvePoint[7] = y;
                    }

                    g.DrawPie(p,
                        ConvertCoord('x', x),
                        ConvertCoord('y', y),
                        4, 4, 0, 360);
                }

                g.DrawString(axis, 
                    new Font(new FontFamily("arial"), 6), new SolidBrush(p.Color), 
                    0, y0 <= 100 ? 190 : 0);
            }

            // Draw height points
            if (this.dir != "L" && this.dir != "R")
            {
                if (this.dir != "T" && this.dir != "B" && uniControl)
                    return;

                if (this.dir == "T" || this.dir == "B")
                    offset = 0;
                else
                    offset = 2;
                
                if (currentXOnlyState == 'W')
                    p.Color = Color.FromArgb(50, 68, 114, 196);
                else
                    p.Color = Color.FromArgb(200, 68, 114, 196);
                p.Width = 2;

                for (int i = 1; i <= count; i++)
                {
                    int x = (int)((nums[i] - nums[1]) / XRange * 250) + offset;
                    int y = (int)((YMax - data[i, 3]) / YRange * 200);

                    if (i == 1)
                    {
                        curvePoint[8] = x;
                        curvePoint[9] = y;
                    }
                    else if (i == count)
                    {
                        curvePoint[14] = x;
                        curvePoint[15] = y;
                    }

                    g.DrawPie(p,
                        ConvertCoord('x', x),
                        ConvertCoord('y', y),
                        4, 4, 0, 360);
                }

                g.DrawString("H", new Font(new FontFamily("arial"), 6), new SolidBrush(p.Color), 243, y0 <= 100 ? 190 : 0);
            }
        }

        private void DrawCanvasControlHandle()
        {
            // TODO: fill this function & add uniControl print
        }

        // Draw one single curve at a time
        // offset = 2(width), 10(height)
        private void DrawCanvasCurve(char mode, short offset, string preset)
        {
            switch (preset)
            {
                case "log":
                    curvePoint[offset + 0] = curvePoint[offset - 2];
                    curvePoint[offset + 1] = curvePoint[offset - 1];
                    curvePoint[offset + 2] = (int)((curvePoint[offset + 4] + curvePoint[offset - 2]) * 0.58);
                    curvePoint[offset + 3] = curvePoint[offset + 5];
                    break;
                case "pow":
                    curvePoint[offset + 0] = (int)((curvePoint[offset + 4] + curvePoint[offset - 2]) * 0.42);
                    curvePoint[offset + 1] = curvePoint[offset - 1];
                    curvePoint[offset + 2] = curvePoint[offset + 4];
                    curvePoint[offset + 3] = curvePoint[offset + 5];
                    break;
                case "custom":
                    break;
                default:
                    curvePoint[offset + 0] = (int)((curvePoint[offset + 4] + curvePoint[offset - 2]) * 0.5);
                    curvePoint[offset + 1] = (int)((curvePoint[offset - 1] + curvePoint[offset + 5]) * 0.5);
                    curvePoint[offset + 2] = curvePoint[offset + 0];
                    curvePoint[offset + 3] = curvePoint[offset + 1];
                    break;
            }

            if (mode == 'W' || (mode == 'U' && this.dir.Length == 1))
            {
                p.Color = Color.FromArgb(150, 237, 125, 49);
                if (mode== 'U' && (this.dir == "T" || this.dir == "B"))
                    p.Color = Color.FromArgb(150, 68, 114, 196);

                p.Width = 2;

                g.DrawBezier(p, 
                    ConvertCoord('x', curvePoint[0]),
                    ConvertCoord('y', curvePoint[1]),
                    ConvertCoord('x', curvePoint[2]),
                    ConvertCoord('y', curvePoint[3]),
                    ConvertCoord('x', curvePoint[4]),
                    ConvertCoord('y', curvePoint[5]),
                    ConvertCoord('x', curvePoint[6]),
                    ConvertCoord('y', curvePoint[7]));
            }
            else if (mode == 'H')
            {
                p.Color = Color.FromArgb(150, 68, 114, 196);
                p.Width = 2;

                g.DrawBezier(p,
                    ConvertCoord('x', curvePoint[8]),
                    ConvertCoord('y', curvePoint[9]),
                    ConvertCoord('x', curvePoint[10]),
                    ConvertCoord('y', curvePoint[11]),
                    ConvertCoord('x', curvePoint[12]),
                    ConvertCoord('y', curvePoint[13]),
                    ConvertCoord('x', curvePoint[14]),
                    ConvertCoord('y', curvePoint[15]));
            }
            else
            {
                p.Color = Color.FromArgb(150, 206, 49, 237);
                p.Width = 2;

                g.DrawBezier(p,
                    ConvertCoord('x', curvePoint[0]),
                    ConvertCoord('y', curvePoint[1]),
                    ConvertCoord('x', curvePoint[2]),
                    ConvertCoord('y', curvePoint[3]),
                    ConvertCoord('x', curvePoint[4]),
                    ConvertCoord('y', curvePoint[5]),
                    ConvertCoord('x', curvePoint[6]),
                    ConvertCoord('y', curvePoint[7]));
            }
        }

        private void Activate(PowerPoint.Selection sel)
        {
            
            if (pr1.Checked && pr2.Checked && pr3.Checked && pr4.Checked)
            {
                btnOperate.Enabled = true;
                ChangeXOnlyStates('E');
                DrawCanvasInfo();
                HandlePresetChange(TestCurrentXOnlyState(), "");

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
            uniControl = true;
            nums = null;
            data = null;
            sel = null;
            wsr = null;
            DrawCanvasClear("init");
            ChangeXOnlyStates('D');
            HandlePresetChange('C', "");
            HandleDirFLChange("CTR");
            btnOperate.Enabled = false;
        }

        private void HandleDirFLChange(string dir)
        {
            this.dir = dir;
            DrawCanvasClear("init");
            
            switch (dir)
            {
                case "TL":
                    dirFL.Text = "左上";
                    dirFL.Top = 28;
                    dirFL.Left = 48;
                    ChangeXOnlyStates('E');
                    break;
                case "T":
                    dirFL.Text = "上";
                    dirFL.Top = 28;
                    dirFL.Left = 129;
                    ChangeXOnlyStates('D');
                    break;
                case "TR":
                    dirFL.Text = "右上";
                    dirFL.Top = 28;
                    dirFL.Left = 210;
                    ChangeXOnlyStates('E');
                    break;
                case "L":
                    dirFL.Text = "左";
                    dirFL.Top = 57;
                    dirFL.Left = 48;
                    ChangeXOnlyStates('D');
                    break;
                case "R":
                    dirFL.Text = "右";
                    dirFL.Top = 57;
                    dirFL.Left = 210;
                    ChangeXOnlyStates('D');
                    break;
                case "BL":
                    dirFL.Text = "左下";
                    dirFL.Top = 86;
                    dirFL.Left = 48;
                    ChangeXOnlyStates('E');
                    break;
                case "B":
                    dirFL.Text = "下";
                    dirFL.Top = 86;
                    dirFL.Left = 129;
                    ChangeXOnlyStates('D');
                    break;
                case "BR":
                    dirFL.Text = "右下";
                    dirFL.Top = 86;
                    dirFL.Left = 210;
                    ChangeXOnlyStates('E');
                    break;
                case "CTR":
                    dirFL.Text = "中心";
                    dirFL.Top = 57;
                    dirFL.Left = 129;
                    ChangeXOnlyStates('E');
                    break;
            }

            if (sel != null)
            {
                HandleWindowSelectionChanged(sel);
                HandlePresetChange(TestCurrentXOnlyState(), "");
            }
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

        private void HandlePresetChange(char mode, string preset)
        {
            if (this.sel != null)
                DrawCanvasInfo();
            
            switch (mode)
            {
                case 'A':       // All
                    this.preset[0] = preset == "" ? this.preset[0] : preset;
                    DrawCanvasCurve('W', 2, this.preset[0]);
                    this.preset[1] = preset == "" ? this.preset[1] : preset;
                    DrawCanvasCurve('H', 10, this.preset[1]);
                    break;
                case 'W':
                    this.preset[0] = preset == "" ? this.preset[0] : preset;
                    DrawCanvasCurve('W', 2, this.preset[0]);
                    break;
                case 'H':
                    this.preset[1] = preset == "" ? this.preset[1] : preset;
                    DrawCanvasCurve('H', 10, this.preset[1]);
                    break;
                case 'U':       // UniControl
                    this.preset[0] = preset == "" ? this.preset[0] : preset;
                    DrawCanvasCurve('U', 2, this.preset[0]);
                    break;
                case 'C':       // Clear
                    this.preset[0] = "linear";
                    this.preset[1] = "linear";
                    break;
            }
        }

        private void presetLinear_Click(object sender, EventArgs e)
        {
            HandlePresetChange(TestCurrentXOnlyState(), "linear");
        }

        private void presetLog_Click(object sender, EventArgs e)
        {
            HandlePresetChange(TestCurrentXOnlyState(), "log");
        }

        private void presetPow_Click(object sender, EventArgs e)
        {
            HandlePresetChange(TestCurrentXOnlyState(), "pow");
        }

        private void presetCustom_Click(object sender, EventArgs e)
        {
            HandlePresetChange(TestCurrentXOnlyState(), "custom");
        }

        private void btnOperate_Click(object sender, EventArgs e)
        {

        }

        private char TestCurrentXOnlyState()
        {
            if (WOnly.Enabled && HOnly.Enabled)
            {
                if (WOnly.Checked && HOnly.Checked)
                    return 'A';
                else if (WOnly.Checked && !HOnly.Checked)
                    return 'W';
                else
                    return 'H';
            }
            else
            {
                if (uniControl)
                    return 'U';
                else if (this.dir == "L" || this.dir == "R")
                    return 'W';
                else
                    return 'H';
            }
        }

        private void ChangeXOnlyStates(char mode)
        {
            switch (mode)
            {
                case 'E':
                    if (this.dir != "T" && this.dir != "B" && 
                        this.dir != "L" && this.dir != "R" && !uniControl)
                    {
                        WOnly.Enabled = true;
                        WOnly.Checked = true;
                        HOnly.Enabled = true;
                        HOnly.Checked = true;
                        return;
                    }
                    goto default;
                default:    // Disable
                    WOnly.Enabled = false;
                    WOnly.Checked = false;
                    HOnly.Enabled = false;
                    HOnly.Checked = false;
                    break;
            }
        }

        bool isChangedManually = false;
        private void WOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (WOnly.Enabled && !isChangedManually)
            {
                if (!WOnly.Checked && !HOnly.Checked)
                {
                    isChangedManually = true;
                    WOnly.Checked = true;
                }

                DrawCanvasInfo();
                HandlePresetChange(TestCurrentXOnlyState(), "");
            } 
            else if (isChangedManually)
            {
                isChangedManually = false;
            }
        }

        private void HOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (HOnly.Enabled && !isChangedManually)
            {
                if (!WOnly.Checked && !HOnly.Checked)
                {
                    isChangedManually = true;
                    HOnly.Checked = true;
                }

                DrawCanvasInfo();
                HandlePresetChange(TestCurrentXOnlyState(), "");
            }
            else if (isChangedManually)
            {
                isChangedManually = false;
            }
        }
    }
}
