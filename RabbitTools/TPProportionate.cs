using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

namespace RabbitTools
{
    // TODO: Fix content save when window is changed
    public partial class TPProportionate : UserControl
    {
        PowerPoint.Application app = Globals.ThisAddIn.Application;

        /// DRAWING VARIABLE REUSE
        /// ---------------------------------------------------------------------------
        
        Graphics g;
        Pen p;
        Font f = new Font(new FontFamily("arial"), 6);

        /// ---------------------------------------------------------------------------


        /// APP STATE MAINTAINANCE
        /// ---------------------------------------------------------------------------

        // Initial pre-requirements check
        short checkCode = 0b000;

        // Init state of direction change component
        string dir = "DISABLED";

        // Be true if Width == Height for all shapes
        bool uniControl = true;
        
        // Paint mode
        // 'D'(Disabled), 'W'(Width), 'H'(Height), 'U'(UniControl), 'S'(Separately)
        char paintMode = 'D';

        // Show only X or Y curve when both are activated
        // 'D'(Disabled), 'W'(Width), 'H'(Height)
        char XOnlyState = 'D';

        /// --------------------------------------------------------------------------


        /// APP DATA STRUCTURE GLOBAL SAVE
        /// --------------------------------------------------------------------------

        // Save selection for later repaint
        PowerPoint.Selection sel;

        // Writeable SelectionRange (for safe sort)
        PowerPoint.Shape[] wsr;

        // Save numbers inside shapes (asc-sorted)
        double[] nums;

        // Save shapes' data for later calculate
        // data[0] = [minWidthIndex, maxWidthIndex, minHeightIndex, maxHeightIndex]
        float[,] data;

        // When UniControl, data will be saved in position of "Width", 
        // and the original "Height" area will be null
        // [PresetForWidth, PresetForHeight]
        string[] preset = new string[2];

        // Save key points' coordinates (in canvas coords system, unmapped)
        // [WidthLeftCorNerX,   WLCNY, 
        //  WidthLeftConTrolX,  WLCTY,
        //  WidthRightConTrolX, WRCTY,
        //  WidthRightCorNerX,  WRCNY,
        //  HLCNX,              HLCNY,
        //  HLCTX,              HLCTX,
        //  HRCTX,              HRCTY,
        //  HRCNX,              HRCTY]
        int[] curvePoint = new int[16];

        // Save control point handles' coords
        // (in canvas coords system, mapped, center of the circle)
        // for event listening
        // Maximum 4 control points
        // [CURR(=1,2,3,4), WLX, WLY, WRX, WRY, HLX, HLY, HRX, HRY]
        int[] handlePointDrew = new int[9];

        /// ---------------------------------------------------------------------------

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

        // Will not only be invoked by event handler, but also repaint request
        public void HandleWindowSelectionChanged(PowerPoint.Selection sel)
        {
            if (!Globals.ThisAddIn.TPProportionate.Visible)
                return;

            if (sel != null &&
                sel.Type == PowerPoint.PpSelectionType.ppSelectionShapes)
            {
                if (sel.ShapeRange.Count >= 3)
                {
                    // Quick check pr4 when pr1~3 are already checked
                    if (checkCode == 0b0111)
                    {
                        checkCode = GetNumbersFromShapeRange(sel.ShapeRange, true);
                        pr4.Checked = (checkCode & 0b1000) > 0;
                    }
                    else
                    {
                        pr1.Checked = true;
                        checkCode = GetNumbersFromShapeRange(sel.ShapeRange);
                        pr2.Checked = (checkCode & 0b0010) > 0;
                        pr3.Checked = (checkCode & 0b0100) > 0;
                        pr4.Checked = (checkCode & 0b1000) > 0;
                    }

                    Activate(sel);
                }
            }
            else
                Deactivate();
        }

        private short GetNumbersFromShapeRange(PowerPoint.ShapeRange sr, bool quick = false)
        {
            // quick == true means just need a quick check for pr4, 
            // which pr1~3 are already checked and true
            if (!quick)
            {
                // Try parse string inside shapes to number
                nums = new double[sr.Count + 1];
                for (int i = 1; i <= sr.Count; i++)
                {
                    string txt = sr[i].TextEffect.Text;
                    if (!double.TryParse(txt, out nums[i]))
                    {
                        // Doesn't match pr2: shapes contain numbers
                        nums = null;
                        return 0b0001;
                    }
                }

                // Copy read-only selection range so that we can sort them
                wsr = new PowerPoint.Shape[sr.Count + 1];
                for (int i = 1; i <= sr.Count; i++)
                {
                    wsr[i] = sr[i];
                }

                // Selection sort based on number
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

                        nums[i] += nums[minIndex];
                        nums[minIndex] = nums[i] - nums[minIndex];
                        nums[i] -= nums[minIndex];
                    }
                }

                // Doesn't match pr3: min num cann't be the same as max num
                if (nums[1] == nums[sr.Count])
                {
                    nums = null;
                    wsr = null;
                    return 0b0011;
                }

                // Save shapes data for later use
                data = new float[sr.Count + 1, 4];
                for (int i = 1; i <= sr.Count; i++)
                {
                    data[i, 0] = wsr[i].Left;
                    data[i, 1] = wsr[i].Top;
                    // W&H for those nubmer in shape is negative will be negative
                    // (benefits canvas drawing)
                    data[i, 2] = (nums[i] >= 0 ? 1 : -1) * wsr[i].Width;
                    data[i, 3] = (nums[i] >= 0 ? 1 : -1) * wsr[i].Height;

                    // If W=H, then only one curve and one control point pair is enough
                    uniControl &= data[i, 2] == data[i, 3];
                }

                // Make help for coords calculate and canvas drawing
                FindMinMaxdata(sr.Count);
            }

            if (((dir != "L" && dir != "R") && (data[1, 3] >= data[nums.Length - 1, 3])) ||
                ((dir != "T" && dir != "B") && (data[1, 2] >= data[nums.Length - 1, 2])))
            {
                // Doesn't match pr4: min < max
                // Reserved variables for later dir change
                //nums = null;
                //wsr = null;
                //data = null;
                return 0b0111;
            }

            // All pre-requests are valid
            return 0b1111;
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

        bool mouseDown = false;
        private void canvas_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            for (int i = 1; i <= 8; i += 2)
            {
                if (Math.Pow(handlePointDrew[i] - e.X, 2) + Math.Pow(handlePointDrew[i + 1] - e.Y, 2)
                    <= 64)
                {
                    handlePointDrew[0] = i / 2 + 1;
                    mouseDown = true;

                    // Find the first corelated handlePoint, and then break the loop
                    break;
                }    
            }
        }

        int prevX = -1, prevY = -1;
        private void canvas_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!mouseDown)
                return;

            if (prevX != -1 && prevY != -1)
            {
                // offset in [handlePointDrew]
                int offset = handlePointDrew[0] * 2 - 1;

                handlePointDrew[offset] += e.X - prevX;

                // In case out of boundary
                if (handlePointDrew[offset] < 10)
                    handlePointDrew[offset] = 10;
                else if (handlePointDrew[offset] > 240)
                    handlePointDrew[offset] = 240;

                handlePointDrew[offset + 1] += e.Y - prevY;

                if (handlePointDrew[offset + 1] < 10)
                    handlePointDrew[offset + 1] = 10;
                else if (handlePointDrew[offset + 1] > 190)
                    handlePointDrew[offset + 1] = 190;

                // offset in [curvePoint]
                int offsetC = ((handlePointDrew[0] > 2) ? 10 : 2) + (-handlePointDrew[0] % 2 + 1) * 2;

                curvePoint[offsetC] = MapCoord("IX", handlePointDrew[offset]);
                curvePoint[offsetC + 1] = MapCoord("IY", handlePointDrew[offset + 1]);

                HandlePresetChange("");
            }

            prevX = e.X;
            prevY = e.Y;
        }

        private void canvas_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.mouseDown = false;

            prevX = -1;
            prevY = -1;

            handlePointDrew[0] = -1;
        }

        private void DrawCanvasClear(string mode = "refresh")
        {
            if (mode == "refresh")
                g.Clear(ColorTable.BG);
            else if (mode == "init")
            {
                DrawCanvasClear();
                p.Color = ColorTable.Axis;
                p.Width = 1;
                g.DrawLine(p, 0, 0, 250, 200);
            }

        }

        // Map 250 * 200 canvas size to 230 * 180 to avoid drawing on the edge
        // mode = "X", "Y", "IX"(Inverse X), "IY"
        private int MapCoord(string mode, int value)
        {
            switch (mode)
            {
                case "X":
                    return (int)Math.Round((float)value / 250f * 230f + 10f);
                case "Y":
                    return (int)Math.Round((float)value / 200f * 180f + 10f);
                case "IX":
                    return (int)Math.Round(((float)value - 10f) / 230f * 250f);
                case "IY":
                    return (int)Math.Round(((float)value - 10f) / 180f * 200f);
                default:
                    return -1;
            }
        }
        
        // Draw basic num points and y = 0 line, 
        // W points, H points according to different paint mode
        private void DrawCanvasInfo()
        {
            DrawCanvasClear();

            int count = nums.Length - 1;

            // Basic coords calculate
            float YMax, YMin;
            if (dir == "T" || dir == "B")
            {
                YMax = data[(int)data[0, 3], 3];
                YMin = data[(int)data[0, 2], 3];
            }
            else if (dir == "L" || dir == "R")
            {
                YMax = data[(int)data[0, 1], 2];
                YMin = data[(int)data[0, 0], 2];
            }
            else
            {
                YMax = Math.Max(data[(int)data[0, 1], 2], data[(int)data[0, 3], 3]);
                YMin = Math.Min(data[(int)data[0, 0], 2], data[(int)data[0, 2], 3]);
            }
            // End of Basic coords calculate
            
            float YRange = YMax - YMin;
            double XRange = nums[count] - nums[1];

            // Draw num points
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
            // End of Draw num points

            SolidBrush SBSelected;
            short offset = 2;

            // Draw width points or uni points
            if (dir != "T" && dir != "B")
            {
                string axis = "W";

                if (paintMode == 'S' && XOnlyState == 'H')
                    SBSelected = ColorTable.WidthTintBrush;
                else
                    SBSelected = ColorTable.WidthBrush;

                if (dir == "L" || dir == "R")
                    offset = 0;
                else
                {
                    if (paintMode == 'U')
                    {
                        SBSelected = ColorTable.UniBrush;
                        offset = 0;
                        axis = "W&H";
                    }
                }

                for (int i = 1; i <= count; i++)
                {
                    int x = (int)(((nums[i] - nums[1]) / XRange * 250) - offset);
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
            // End of Draw width points

            // Draw height points
            if (dir != "L" && dir != "R" && paintMode != 'U')
            {
                if (dir == "T" || dir == "B")
                    offset = 0;
                else
                    offset = 2;

                if (paintMode == 'S' && XOnlyState == 'W')
                    SBSelected = ColorTable.HeightTintBrush;
                else
                    SBSelected = ColorTable.HeightBrush;

                for (int i = 1; i <= count; i++)
                {
                    int x = (int)(((nums[i] - nums[1]) / XRange * 250) + offset);
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

        // dir = "LX", "LY", "RX", "RY", 
        //       "ILX"(Inverse Left), "ILY", "IRX"(Inverse Right), "IRY"
        private int MapControlPoint(string dir, int value)
        {
            return value;
            
            // Too difficult to map coords, so abandon.
            //switch (dir)
            //{
            //    case "LX":
            //        return value / 3 * 2;
            //    case "LY":
            //        return 200 - (200 - value) / 3 * 2;
            //    case "RX":
            //        return 250 - (250 - value) / 3 * 2;
            //    case "RY":
            //        return value / 3 * 2;
            //    case "ILX":
            //        return 0;
            //    case "ILY":
            //        return 0;
            //    case "IRX":
            //        return 0;
            //    case "IRY":
            //        return 0;
            //    default:
            //        return 0;
            //}
        }

        private void resetHandlePointDrew()
        {
            for (int i = 0; i <= 8; i++)
                handlePointDrew[i] = -1;
        }

        // Draw one pair of control points at a time
        private void DrawCanvasControlHandle(char mode)
        {
            int presetIndex = (mode == 'W' || mode == 'U') ? 0 : 1;
            
            if (preset[presetIndex] != "custom")
                return;

            p.Color = ColorTable.ControlLine;
            p.Width = 4;
            
            if (mode == 'W' || mode == 'U')
            {
                if (XOnlyState == 'H')
                    return;

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

                // Save control points' coords for event handle
                handlePointDrew[1] = MapCoord("X", MapControlPoint("LX", curvePoint[2]));
                handlePointDrew[2] = MapCoord("Y", MapControlPoint("LY", curvePoint[3]));
                handlePointDrew[3] = MapCoord("X", MapControlPoint("RX", curvePoint[4]));
                handlePointDrew[4] = MapCoord("Y", MapControlPoint("RY", curvePoint[5]));
            }
            else if (mode == 'H')
            {
                if (XOnlyState == 'W')
                    return;

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
                    MapCoord("Y", MapControlPoint("RY", curvePoint[13]) - 8),
                    16, 16);
                g.FillEllipse(ColorTable.ControlPointInnerBrush,
                    MapCoord("X", MapControlPoint("RX", curvePoint[12]) - 4),
                    MapCoord("Y", MapControlPoint("RY", curvePoint[13]) - 4),
                    8, 8);

                handlePointDrew[5] = MapCoord("X", MapControlPoint("LX", curvePoint[10]));
                handlePointDrew[6] = MapCoord("Y", MapControlPoint("LY", curvePoint[11]));
                handlePointDrew[7] = MapCoord("X", MapControlPoint("RX", curvePoint[12]));
                handlePointDrew[8] = MapCoord("Y", MapControlPoint("RY", curvePoint[13]));
            }
        }

        // Draw one single curve at a time
        // offset = 2(width), 10(height)
        private void DrawCanvasCurve(char mode)
        {
            int presetIndex = (mode == 'W' || mode == 'U') ? 0 : 1;
            // 0-2, 1-10
            int offset = 8 * presetIndex + 2;
            
            switch (preset[presetIndex])
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

            DrawCanvasControlHandle(mode);

            p.Width = 2;
            if (mode == 'W' || mode == 'U')
            {
                if (XOnlyState == 'H')
                    return;

                p.Color = ColorTable.WidthLine;
                if (mode == 'U')
                    p.Color = ColorTable.UniLine;

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
                if (XOnlyState == 'W')
                    return;

                p.Color = ColorTable.HeightLine;

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
            
            if (checkCode == 0b1111)
            {
                this.sel = sel;

                HandleDirChange(dir == "DISABLED" ? "CTR" : dir, true);
                HandlePresetChange("linear");
                ChangeXOnlyStates((dir.Length == 1 || uniControl) ? 'D' : 'A');

                btnOperate.Enabled = true;
            }
            else if ((checkCode & 0b0100) > 0)
            {
                this.sel = sel;

                HandleDirChange(dir == "DISABLED" ? "CTR" : dir, true);
                HandlePresetChange();
                ChangeXOnlyStates();

                btnOperate.Enabled = false;
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
            checkCode = 0b0000;

            HandleDirChange();

            uniControl = true;

            ChangeXOnlyStates();

            paintMode = 'D';

            HandlePresetChange();

            sel = null;
            wsr = null;

            nums = null;
            data = null;

            for (int i = 0; i <= 15; i++)
                curvePoint[i] = 0;

            resetHandlePointDrew();

            DrawCanvasClear("init");

            btnOperate.Enabled = false;
        }

        private void ChangeDirBtnGroupState(bool state)
        {
            dirTL. Enabled = state;
            dirT.  Enabled = state;
            dirTR. Enabled = state;
            dirL.  Enabled = state;
            dirCTR.Enabled = state;
            dirR.  Enabled = state;
            dirBL. Enabled = state;
            dirB.  Enabled = state;
            dirBR. Enabled = state;
            dirFL. Visible = state;
        }

        private void HandleDirChange(string dir = "DISABLED", bool fromActivate = false)
        {
            this.dir = dir;
            DrawCanvasClear("init");

            if (dir == "DISABLED")
            {
                ChangeDirBtnGroupState(false);
                return;
            }

            ChangeDirBtnGroupState(true);

            switch (dir)
            {
                case "TL":
                    dirFL.Text = "左上";
                    dirFL.Top = 28;
                    dirFL.Left = 48;
                    paintMode = (checkCode & 0b1000) > 0 ? (uniControl ? 'U' : 'S') : 'D';
                    break;
                case "T":
                    dirFL.Text = "上";
                    dirFL.Top = 28;
                    dirFL.Left = 129;
                    paintMode = (checkCode & 0b1000) > 0 ? 'H' : 'D';
                    break;
                case "TR":
                    dirFL.Text = "右上";
                    dirFL.Top = 28;
                    dirFL.Left = 210;
                    paintMode = (checkCode & 0b1000) > 0 ? (uniControl ? 'U' : 'S') : 'D';
                    break;
                case "L":
                    dirFL.Text = "左";
                    dirFL.Top = 57;
                    dirFL.Left = 48;
                    paintMode = (checkCode & 0b1000) > 0 ? 'W' : 'D';
                    break;
                case "R":
                    dirFL.Text = "右";
                    dirFL.Top = 57;
                    dirFL.Left = 210;
                    paintMode = (checkCode & 0b1000) > 0 ? 'W' : 'D';
                    break;
                case "BL":
                    dirFL.Text = "左下";
                    dirFL.Top = 86;
                    dirFL.Left = 48;
                    paintMode = (checkCode & 0b1000) > 0 ? (uniControl ? 'U' : 'S') : 'D';
                    break;
                case "B":
                    dirFL.Text = "下";
                    dirFL.Top = 86;
                    dirFL.Left = 129;
                    paintMode = (checkCode & 0b1000) > 0 ? 'H' : 'D';
                    break;
                case "BR":
                    dirFL.Text = "右下";
                    dirFL.Top = 86;
                    dirFL.Left = 210;
                    paintMode = (checkCode & 0b1000) > 0 ? (uniControl ? 'U' : 'S') : 'D';
                    break;
                case "CTR":
                    dirFL.Text = "中心";
                    dirFL.Top = 57;
                    dirFL.Left = 129;
                    paintMode = (checkCode & 0b1000) > 0 ? (uniControl ? 'U' : 'S') : 'D';
                    break;
            }

            if (!fromActivate)
                HandleWindowSelectionChanged(this.sel);
        }

        private void dirTL_Click(object sender, EventArgs e)
        {
            HandleDirChange("TL");
        }

        private void dirT_Click(object sender, EventArgs e)
        {
            HandleDirChange("T");
        }

        private void dirTR_Click(object sender, EventArgs e)
        {
            HandleDirChange("TR");
        }

        private void dirL_Click(object sender, EventArgs e)
        {
            HandleDirChange("L");
        }

        private void dirCTR_Click(object sender, EventArgs e)
        {
            HandleDirChange("CTR");
        }

        private void dirR_Click(object sender, EventArgs e)
        {
            HandleDirChange("R");
        }

        private void dirBL_Click(object sender, EventArgs e)
        {
            HandleDirChange("BL");
        }

        private void dirB_Click(object sender, EventArgs e)
        {
            HandleDirChange("B");
        }

        private void dirBR_Click(object sender, EventArgs e)
        {
            HandleDirChange("BR");
        }

        private void HandlePresetChange(string preset = null)
        {
            if (paintMode == 'D')
            {
                DrawCanvasClear("init");
                presetLinear.Enabled = false;
                presetLog.   Enabled = false;
                presetPow.   Enabled = false;
                presetCustom.Enabled = false;

                this.preset[0] = null;
                this.preset[1] = null;
                
                return;
            }

            presetLinear.Enabled = true;
            presetLog.   Enabled = true;
            presetPow.   Enabled = true;
            presetCustom.Enabled = true;

            DrawCanvasInfo();

            switch (paintMode)
            {
                case 'S':       // Separately
                    if (XOnlyState == 'A')
                    {
                        if (preset != "")
                            this.preset[0] = preset;
                        DrawCanvasCurve('W');
                        if (preset != "")
                            this.preset[1] = preset;
                        DrawCanvasCurve('H');
                    }
                    else if (XOnlyState == 'W')
                        goto case 'W';
                    else
                        goto case 'H';
                    break;
                case 'W':
                    if (preset != "")
                        this.preset[0] = preset;
                    DrawCanvasCurve('W');
                    break;
                case 'H':
                    if (preset != "")
                        this.preset[1] = preset;
                    DrawCanvasCurve('H');
                    break;
                case 'U':       // UniControl
                    if (preset != "")
                        this.preset[0] = preset;
                    DrawCanvasCurve('U');
                    break;
            }
        }

        private void presetLinear_Click(object sender, EventArgs e)
        {
            HandlePresetChange("linear");
        }

        private void presetLog_Click(object sender, EventArgs e)
        {
            HandlePresetChange("log");
        }

        private void presetPow_Click(object sender, EventArgs e)
        {
            HandlePresetChange("pow");
        }

        private void presetCustom_Click(object sender, EventArgs e)
        {
            HandlePresetChange("custom");
        }

        private void ChangeXOnlyStates(char mode = 'D')
        {
            XOnlyState = mode;

            if (mode == 'D')
            {
                WOnly.Enabled = false;
                WOnly.Checked = false;
                HOnly.Enabled = false;
                HOnly.Checked = false;

                return;
            }

            resetHandlePointDrew();

            DrawCanvasClear();
            DrawCanvasInfo();
            
            switch (mode)
            {
                case 'A':   // All
                    WOnly.Enabled = true;
                    WOnly.Checked = true;
                    HOnly.Enabled = true;
                    HOnly.Checked = true;

                    DrawCanvasCurve('W');
                    DrawCanvasCurve('H');
                    break;
                case 'W':
                    goto case 'H';
                case 'H':
                    DrawCanvasCurve(mode);
                    break;
            }
        }

        bool isChangedManually = false;
        private void WOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (XOnlyState == 'D')
                    return;
            
            if (isChangedManually)
            {
                isChangedManually = false;
            }
            else
            {
                if (!WOnly.Checked && !HOnly.Checked)
                {
                    isChangedManually = true;
                    WOnly.Checked = true;
                }
                else
                {
                    char XOnly;
                    if (WOnly.Checked && HOnly.Checked)
                        XOnly = 'A';
                    else if (WOnly.Checked && !HOnly.Checked)
                        XOnly = 'W';
                    else
                        XOnly = 'H';

                    ChangeXOnlyStates(XOnly);
                }
            } 
        }

        private void HOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (XOnlyState == 'D')
                return;

            if (isChangedManually)
            {
                isChangedManually = false;
            }
            else
            {
                if (!WOnly.Checked && !HOnly.Checked)
                {
                    isChangedManually = true;
                    HOnly.Checked = true;
                }
                else
                {
                    char XOnly;
                    if (WOnly.Checked && HOnly.Checked)
                        XOnly = 'A';
                    else if (WOnly.Checked && !HOnly.Checked)
                        XOnly = 'W';
                    else
                        XOnly = 'H';

                    ChangeXOnlyStates(XOnly);
                }
            }
        }

        // TODO: Add protect zone in case size is set to 0
        private void btnOperate_Click(object sender, EventArgs e)
        {   
            CubicBezierCurve bezier;
            
            switch (sender == btnOperate ? this.dir : sender)
            {
                case "TL":
                    // Little tricky for code reuse
                    btnOperate_Click("T", null);
                    btnOperate_Click("L", null);
                    break;

                case "T":
                    if (uniControl)
                        bezier = new CubicBezierCurve(
                        (this.curvePoint[2] - this.curvePoint[0]) / 250f,
                        (float)(this.curvePoint[1] - this.curvePoint[3]) /
                            (float)(this.curvePoint[1] - this.curvePoint[7]),
                        (this.curvePoint[4] - this.curvePoint[0]) / 250f,
                        (float)(this.curvePoint[1] - this.curvePoint[5]) /
                            (float)(this.curvePoint[1] - this.curvePoint[7]));
                    else
                        bezier = new CubicBezierCurve(
                            (this.curvePoint[10] - this.curvePoint[8]) / 250f,
                            (float)(this.curvePoint[9] - this.curvePoint[11]) /
                                (float)(this.curvePoint[9] - this.curvePoint[15]),
                            (this.curvePoint[12] - this.curvePoint[8]) / 250f,
                            (float)(this.curvePoint[9] - this.curvePoint[13]) /
                                (float)(this.curvePoint[9] - this.curvePoint[15]));

                    for (int i = 2; i <= this.nums.Length - 2; i++)
                    {
                        float prevH = this.wsr[i].Height;
                        
                        this.wsr[i].Height =
                            Math.Abs(this.data[1, 3] +
                            bezier.GetPoint(bezier.GetClosestParam(
                                (float)(nums[i] - nums[1]) / (float)(nums[nums.Length - 1] - nums[1])
                            )).Y *
                            (this.data[nums.Length - 1, 3] - this.data[1, 3]));

                        this.wsr[i].Top -= this.wsr[i].Height - prevH;
                    }
                    break;

                case "TR":
                    btnOperate_Click("T", null);
                    btnOperate_Click("R", null);
                    break;

                case "L":
                    bezier = new CubicBezierCurve(
                        (this.curvePoint[2] - this.curvePoint[0]) / 250f,
                        (float)(this.curvePoint[1] - this.curvePoint[3]) /
                            (float)(this.curvePoint[1] - this.curvePoint[7]),
                        (this.curvePoint[4] - this.curvePoint[0]) / 250f,
                        (float)(this.curvePoint[1] - this.curvePoint[5]) /
                            (float)(this.curvePoint[1] - this.curvePoint[7]));

                    for (int i = 2; i <= this.nums.Length - 2; i++)
                    {
                        float prevW = this.wsr[i].Width;

                        this.wsr[i].Width =
                            Math.Abs(this.data[1, 2] +
                            bezier.GetPoint(bezier.GetClosestParam(
                                (float)(nums[i] - nums[1]) / (float)(nums[nums.Length - 1] - nums[1])
                            )).Y *
                            (this.data[nums.Length - 1, 2] - this.data[1, 2]));

                        this.wsr[i].Left -= this.wsr[i].Width - prevW;
                    }
                    break;

                case "CTR":
                    bezier = new CubicBezierCurve(
                        (this.curvePoint[2] - this.curvePoint[0]) / 250f,
                        (float)(this.curvePoint[1] - this.curvePoint[3]) /
                            (float)(this.curvePoint[1] - this.curvePoint[7]),
                        (this.curvePoint[4] - this.curvePoint[0]) / 250f,
                        (float)(this.curvePoint[1] - this.curvePoint[5]) /
                            (float)(this.curvePoint[1] - this.curvePoint[7]));

                    for (int i = 2; i <= this.nums.Length - 2; i++)
                    {
                        float prevW = this.wsr[i].Width;

                        this.wsr[i].Width =
                            Math.Abs(this.data[1, 2] +
                            bezier.GetPoint(bezier.GetClosestParam(
                                (float)(nums[i] - nums[1]) / (float)(nums[nums.Length - 1] - nums[1])
                            )).Y *
                            (this.data[nums.Length - 1, 2] - this.data[1, 2]));

                        this.wsr[i].Left -= (this.wsr[i].Width - prevW) / 2;
                    }

                    if (!uniControl)
                        bezier = new CubicBezierCurve(
                            (this.curvePoint[10] - this.curvePoint[8]) / 250f,
                            (float)(this.curvePoint[9] - this.curvePoint[11]) /
                                (float)(this.curvePoint[9] - this.curvePoint[15]),
                            (this.curvePoint[12] - this.curvePoint[8]) / 250f,
                            (float)(this.curvePoint[9] - this.curvePoint[13]) /
                                (float)(this.curvePoint[9] - this.curvePoint[15]));

                    for (int i = 2; i <= this.nums.Length - 2; i++)
                    {
                        float prevH = this.wsr[i].Height;

                        this.wsr[i].Height =
                            Math.Abs(this.data[1, 3] +
                            bezier.GetPoint(bezier.GetClosestParam(
                                (float)(nums[i] - nums[1]) / (float)(nums[nums.Length - 1] - nums[1])
                            )).Y *
                            (this.data[nums.Length - 1, 3] - this.data[1, 3]));

                        this.wsr[i].Top -= (this.wsr[i].Height - prevH) / 2;
                    }
                    break;

                case "R":
                    bezier = new CubicBezierCurve(
                        (this.curvePoint[2] - this.curvePoint[0]) / 250f,
                        (float)(this.curvePoint[1] - this.curvePoint[3]) /
                            (float)(this.curvePoint[1] - this.curvePoint[7]),
                        (this.curvePoint[4] - this.curvePoint[0]) / 250f,
                        (float)(this.curvePoint[1] - this.curvePoint[5]) /
                            (float)(this.curvePoint[1] - this.curvePoint[7]));

                    for (int i = 2; i <= this.nums.Length - 2; i++)
                    {
                        float prevW = this.wsr[i].Width;

                        this.wsr[i].Width =
                            Math.Abs(this.data[1, 2] +
                            bezier.GetPoint(bezier.GetClosestParam(
                                (float)(nums[i] - nums[1]) / (float)(nums[nums.Length - 1] - nums[1])
                            )).Y *
                            (this.data[nums.Length - 1, 2] - this.data[1, 2]));
                    }
                    break;

                case "BL":
                    btnOperate_Click("B", null);
                    btnOperate_Click("L", null);
                    break;

                case "B":
                    if (uniControl)
                        bezier = new CubicBezierCurve(
                        (this.curvePoint[2] - this.curvePoint[0]) / 250f,
                        (float)(this.curvePoint[1] - this.curvePoint[3]) /
                            (float)(this.curvePoint[1] - this.curvePoint[7]),
                        (this.curvePoint[4] - this.curvePoint[0]) / 250f,
                        (float)(this.curvePoint[1] - this.curvePoint[5]) /
                            (float)(this.curvePoint[1] - this.curvePoint[7]));
                    else
                        bezier = new CubicBezierCurve(
                            (this.curvePoint[10] - this.curvePoint[8]) / 250f,
                            (float)(this.curvePoint[9] - this.curvePoint[11]) /
                                (float)(this.curvePoint[9] - this.curvePoint[15]),
                            (this.curvePoint[12] - this.curvePoint[8]) / 250f,
                            (float)(this.curvePoint[9] - this.curvePoint[13]) /
                                (float)(this.curvePoint[9] - this.curvePoint[15]));

                    for (int i = 2; i <= this.nums.Length - 2; i++)
                    {
                        this.wsr[i].Height =
                            Math.Abs(this.data[1, 3] +
                            bezier.GetPoint(bezier.GetClosestParam(
                                (float)(nums[i] - nums[1]) / (float)(nums[nums.Length - 1] - nums[1])
                            )).Y * 
                            (this.data[nums.Length - 1, 3] - this.data[1, 3]));
                    }
                    break;

                case "BR":
                    btnOperate_Click("B", null);
                    btnOperate_Click("R", null);
                    break;
            }

            // Only draw once, don't draw when decompose 2 dir into 2 * 1 dir
            if (sender == btnOperate)
            {
                // Draw after-operation curve to show effect
                GetNumbersFromShapeRange(app.ActiveWindow.Selection.ShapeRange);
                HandlePresetChange("");
            }
        }
    }
}
