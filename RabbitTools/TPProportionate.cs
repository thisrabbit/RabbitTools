using System;
using System.Drawing;
using System.Windows.Forms;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

namespace RabbitTools
{
    // TODO: Fix initial selection and content save when window is changed
    public partial class TPProportionate : UserControl
    {
        PowerPoint.Application app = Globals.ThisAddIn.Application;

        Graphics g;
        Pen p;
        Font f = new Font(new FontFamily("arial"), 6);
        bool mouseDown = false;

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
            p = new Pen(ColorTable.Axis, 1);

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

        private void canvas_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.mouseDown = true;
        }

        private void canvas_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!mouseDown)
                return;


        }

        private void canvas_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.mouseDown = false;
        }

        private void DrawCanvasClear(string mode)
        {
            if (mode == "refresh")
            {
                g.Clear(ColorTable.BG);
            }
            else if (mode == "init")
            {
                DrawCanvasClear("refresh");
                p.Color = ColorTable.Axis;
                p.Width = 1;
                g.DrawLine(p, 0, 0, 250, 200);
            }
        }

        // mode = "X", "Y", "IX"(Inverse X), "IY"
        private int MapCoord(string mode, int value)
        {
            switch (mode)
            {
                case "X":
                    return (int)((float)value / 250f * 230f + 10f);
                case "Y":
                    return (int)((float)value / 200f * 180f + 10f);
                case "IX":
                    return 0;
                case "IY":
                    return 0;
                default:
                    return 0;
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
            
            int MapedY0 = MapCoord("Y", y0);

            p.Color = ColorTable.Axis;
            p.Width = 1;
            g.DrawLine(p, 0, MapedY0, 250, MapedY0);
            
            for (int i = 1; i <= count; i++)
            {
                g.FillEllipse(ColorTable.AxisPointBrush,
                    MapCoord("X",
                        (int)((nums[i] - nums[1]) / XRange * 250)) - 5,
                    MapedY0 - 5, 10, 10);
            }

            char currentXOnlyState = TestCurrentXOnlyState();

            SolidBrush SBSelected;
            int offset = 2;
            // Draw width points
            if (this.dir != "T" && this.dir != "B")
            {
                string axis = "W";

                if (currentXOnlyState == 'H')
                    SBSelected = ColorTable.WidthTintBrush;
                else
                    SBSelected = ColorTable.WidthBrush;

                if (this.dir == "L" || this.dir == "R")
                    offset = 0;
                else
                {
                    if (currentXOnlyState == 'U')
                    {
                        SBSelected = ColorTable.UniBrush;
                        offset = 0;
                        axis = "W&H";
                    }
                }

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

                    g.FillEllipse(SBSelected,
                        MapCoord("X", x) - 4,
                        MapCoord("Y", y) - 4,
                        8, 8);
                }

                g.DrawString(axis, f, SBSelected, 0, y0 <= 100 ? 190 : 0);
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
                    SBSelected = ColorTable.HeightTintBrush;
                else
                    SBSelected = ColorTable.HeightBrush;

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

                    g.FillEllipse(SBSelected,
                        MapCoord("X", x) - 4,
                        MapCoord("Y", y) - 4,
                        8, 8);
                }

                g.DrawString("H", f, SBSelected, 243, y0 <= 100 ? 190 : 0);
            }
        }

        /// dir = "LX", "LY", "RX", "RY", 
        ///       "ILX"(Inverse Left), "ILY", "IRX"(Inverse Right), "IRY"
        private int MapControlPoint(string dir, int value)
        {
            return value;
            
            // Too difficult to map coords, so abandon.
            switch (dir)
            {
                case "LX":
                    return value / 3 * 2;
                case "LY":
                    return 200 - (200 - value) / 3 * 2;
                case "RX":
                    return 250 - (250 - value) / 3 * 2;
                case "RY":
                    return value / 3 * 2;
                case "ILX":
                    return 0;
                case "ILY":
                    return 0;
                case "IRX":
                    return 0;
                case "IRY":
                    return 0;
                default:
                    return 0;
            }
        }

        // Draw one pair of control points at a time
        private void DrawCanvasControlHandle(char mode, char compiledMode, string preset)
        {
            if (preset != "custom")
                return;

            p.Color = ColorTable.ControlLine;
            p.Width = 4;
            
            if (compiledMode == 'W' || compiledMode == 'U')
            {
                g.DrawLine(p, 
                    MapCoord("X", curvePoint[0]), 
                    MapCoord("Y", curvePoint[1]), 
                    MapCoord("X", MapControlPoint("LX", curvePoint[2])), 
                    MapCoord("Y", MapControlPoint("LY", curvePoint[3])));
                g.DrawLine(p,
                    MapCoord("X", MapControlPoint("RX", curvePoint[4])),
                    MapCoord("Y", MapControlPoint("RY", curvePoint[5])),
                    MapCoord("X", curvePoint[6]),
                    MapCoord("Y", curvePoint[7]));
                g.FillEllipse(ColorTable.ControlPointOuterBrush, 
                    MapCoord("X", MapControlPoint("LX", curvePoint[2]) - 8), 
                    MapCoord("Y", MapControlPoint("LY", curvePoint[3]) - 8),
                    16, 16);
                g.FillEllipse(ColorTable.ControlPointInnerBrush,
                    MapCoord("X", MapControlPoint("LX", curvePoint[2]) - 4),
                    MapCoord("Y", MapControlPoint("LY", curvePoint[3]) - 4),
                    8, 8);
                g.FillEllipse(ColorTable.ControlPointOuterBrush,
                    MapCoord("X", MapControlPoint("RX", curvePoint[4]) - 8),
                    MapCoord("Y", MapControlPoint("RY", curvePoint[5]) - 8),
                    16, 16);
                g.FillEllipse(ColorTable.ControlPointInnerBrush,
                    MapCoord("X", MapControlPoint("RX", curvePoint[4]) - 4),
                    MapCoord("Y", MapControlPoint("RY", curvePoint[5]) - 4),
                    8, 8);
            }
            else if (compiledMode == 'H')
            {
                g.DrawLine(p,
                    MapCoord("X", curvePoint[8]),
                    MapCoord("Y", curvePoint[9]),
                    MapCoord("X", MapControlPoint("LX", curvePoint[10])),
                    MapCoord("Y", MapControlPoint("LY", curvePoint[11])));
                g.DrawLine(p,
                    MapCoord("X", MapControlPoint("RX", curvePoint[12])),
                    MapCoord("Y", MapControlPoint("RY", curvePoint[13])),
                    MapCoord("X", curvePoint[14]),
                    MapCoord("Y", curvePoint[15]));
                g.FillEllipse(ColorTable.ControlPointOuterBrush,
                    MapCoord("X", MapControlPoint("LX", curvePoint[10]) - 8),
                    MapCoord("Y", MapControlPoint("LY", curvePoint[11]) - 8),
                    16, 16);
                g.FillEllipse(ColorTable.ControlPointInnerBrush,
                    MapCoord("X", MapControlPoint("LX", curvePoint[10]) - 4),
                    MapCoord("Y", MapControlPoint("LY", curvePoint[11]) - 4),
                    8, 8);
                g.FillEllipse(ColorTable.ControlPointOuterBrush,
                    MapCoord("X", MapControlPoint("RX", curvePoint[12]) - 8),
                    MapCoord("Y", MapControlPoint("YX", curvePoint[13]) - 8),
                    16, 16);
                g.FillEllipse(ColorTable.ControlPointInnerBrush,
                    MapCoord("X", MapControlPoint("RX", curvePoint[12]) - 4),
                    MapCoord("Y", MapControlPoint("YX", curvePoint[13]) - 4),
                    8, 8);
            }
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

            if (mode == 'W' || mode == 'U')
            {
                if (mode == 'U')
                {
                    if (this.dir == "T" || this.dir == "B")
                    {
                        DrawCanvasControlHandle(mode, 'H', preset);
                    }
                    else if (this.dir.Length >= 2)
                    {
                        DrawCanvasControlHandle(mode, 'U', preset);
                    }
                }
                else
                {
                    DrawCanvasControlHandle(mode, 'W', preset);
                }

                p.Color = ColorTable.WidthLine;
                if (mode == 'U')
                {
                    if (this.dir == "T" || this.dir == "B")
                    {
                        p.Color = ColorTable.HeightLine;
                    }
                    else if (this.dir.Length >= 2)
                    {
                        p.Color = ColorTable.UniLine;
                    }
                }

                p.Width = 2;

                g.DrawBezier(p,
                    MapCoord("X", curvePoint[0]),
                    MapCoord("Y", curvePoint[1]),
                    MapCoord("X", curvePoint[2]),
                    MapCoord("Y", curvePoint[3]),
                    MapCoord("X", curvePoint[4]),
                    MapCoord("Y", curvePoint[5]),
                    MapCoord("X", curvePoint[6]),
                    MapCoord("Y", curvePoint[7]));
            }
            else if (mode == 'H')
            {
                DrawCanvasControlHandle(mode, 'H', preset);
                
                p.Color = ColorTable.HeightLine;
                p.Width = 2;

                g.DrawBezier(p,
                    MapCoord("X", curvePoint[8]),
                    MapCoord("Y", curvePoint[9]),
                    MapCoord("X", curvePoint[10]),
                    MapCoord("Y", curvePoint[11]),
                    MapCoord("X", curvePoint[12]),
                    MapCoord("Y", curvePoint[13]),
                    MapCoord("X", curvePoint[14]),
                    MapCoord("Y", curvePoint[15]));
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

        private void btnOperate_Click(object sender, EventArgs e)
        {

        }
    }
}
