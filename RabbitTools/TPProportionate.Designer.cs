
namespace RabbitTools
{
    partial class TPProportionate
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.step1 = new System.Windows.Forms.Label();
            this.pr2 = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.pr1 = new System.Windows.Forms.CheckBox();
            this.pr4 = new System.Windows.Forms.CheckBox();
            this.pr3 = new System.Windows.Forms.CheckBox();
            this.step2 = new System.Windows.Forms.Label();
            this.canvas = new System.Windows.Forms.PictureBox();
            this.label8 = new System.Windows.Forms.Label();
            this.presetLinear = new System.Windows.Forms.Button();
            this.presetLog = new System.Windows.Forms.Button();
            this.presetPow = new System.Windows.Forms.Button();
            this.presetCustom = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.HOnly = new System.Windows.Forms.CheckBox();
            this.WOnly = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.dirFL = new System.Windows.Forms.Label();
            this.dirCTR = new System.Windows.Forms.Button();
            this.dirBR = new System.Windows.Forms.Button();
            this.dirBL = new System.Windows.Forms.Button();
            this.dirTR = new System.Windows.Forms.Button();
            this.dirTL = new System.Windows.Forms.Button();
            this.dirR = new System.Windows.Forms.Button();
            this.dirL = new System.Windows.Forms.Button();
            this.dirB = new System.Windows.Forms.Button();
            this.step3 = new System.Windows.Forms.Label();
            this.dirT = new System.Windows.Forms.Button();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnOperate = new System.Windows.Forms.Button();
            this.step4 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.canvas)).BeginInit();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // step1
            // 
            this.step1.AutoSize = true;
            this.step1.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.step1.Location = new System.Drawing.Point(0, 0);
            this.step1.Margin = new System.Windows.Forms.Padding(0);
            this.step1.Name = "step1";
            this.step1.Size = new System.Drawing.Size(207, 17);
            this.step1.TabIndex = 1;
            this.step1.Text = "步骤1：选择所有需要调整尺寸的形状";
            // 
            // pr2
            // 
            this.pr2.AutoCheck = false;
            this.pr2.AutoSize = true;
            this.pr2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.pr2.Location = new System.Drawing.Point(6, 40);
            this.pr2.Name = "pr2";
            this.pr2.Size = new System.Drawing.Size(144, 16);
            this.pr2.TabIndex = 21;
            this.pr2.Text = "所选形状内均包含数字";
            this.pr2.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.pr1);
            this.groupBox1.Controls.Add(this.pr4);
            this.groupBox1.Controls.Add(this.pr3);
            this.groupBox1.Controls.Add(this.pr2);
            this.groupBox1.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.groupBox1.Location = new System.Drawing.Point(18, 20);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(250, 106);
            this.groupBox1.TabIndex = 22;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "进行下一步要满足的条件：";
            // 
            // pr1
            // 
            this.pr1.AutoCheck = false;
            this.pr1.AutoSize = true;
            this.pr1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.pr1.Location = new System.Drawing.Point(6, 20);
            this.pr1.Name = "pr1";
            this.pr1.Size = new System.Drawing.Size(126, 16);
            this.pr1.TabIndex = 26;
            this.pr1.Text = "选择了至少3个形状";
            this.pr1.UseVisualStyleBackColor = true;
            // 
            // pr4
            // 
            this.pr4.AutoCheck = false;
            this.pr4.AutoSize = true;
            this.pr4.ForeColor = System.Drawing.SystemColors.ControlText;
            this.pr4.Location = new System.Drawing.Point(6, 84);
            this.pr4.Name = "pr4";
            this.pr4.Size = new System.Drawing.Size(240, 16);
            this.pr4.TabIndex = 25;
            this.pr4.Text = "已为最大值和最小值形状指定不同的尺寸";
            this.pr4.UseVisualStyleBackColor = true;
            // 
            // pr3
            // 
            this.pr3.AutoCheck = false;
            this.pr3.AutoSize = true;
            this.pr3.ForeColor = System.Drawing.SystemColors.ControlText;
            this.pr3.Location = new System.Drawing.Point(6, 62);
            this.pr3.Name = "pr3";
            this.pr3.Size = new System.Drawing.Size(156, 16);
            this.pr3.TabIndex = 24;
            this.pr3.Text = "所选形状的数字不全一样";
            this.pr3.UseVisualStyleBackColor = true;
            // 
            // step2
            // 
            this.step2.AutoSize = true;
            this.step2.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.step2.Location = new System.Drawing.Point(0, 0);
            this.step2.Margin = new System.Windows.Forms.Padding(0);
            this.step2.Name = "step2";
            this.step2.Size = new System.Drawing.Size(147, 17);
            this.step2.TabIndex = 23;
            this.step2.Text = "步骤3：指定尺寸变化函数";
            // 
            // canvas
            // 
            this.canvas.Location = new System.Drawing.Point(18, 49);
            this.canvas.Name = "canvas";
            this.canvas.Size = new System.Drawing.Size(250, 200);
            this.canvas.TabIndex = 24;
            this.canvas.TabStop = false;
            this.canvas.MouseDown += new System.Windows.Forms.MouseEventHandler(this.canvas_MouseDown);
            this.canvas.MouseMove += new System.Windows.Forms.MouseEventHandler(this.canvas_MouseMove);
            this.canvas.MouseUp += new System.Windows.Forms.MouseEventHandler(this.canvas_MouseUp);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label8.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label8.Location = new System.Drawing.Point(15, 22);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(56, 17);
            this.label8.TabIndex = 25;
            this.label8.Text = "使用预设";
            // 
            // presetLinear
            // 
            this.presetLinear.Enabled = false;
            this.presetLinear.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.presetLinear.Location = new System.Drawing.Point(77, 20);
            this.presetLinear.Name = "presetLinear";
            this.presetLinear.Size = new System.Drawing.Size(40, 23);
            this.presetLinear.TabIndex = 26;
            this.presetLinear.Text = "线性";
            this.presetLinear.UseVisualStyleBackColor = false;
            this.presetLinear.Click += new System.EventHandler(this.presetLinear_Click);
            // 
            // presetLog
            // 
            this.presetLog.Enabled = false;
            this.presetLog.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.presetLog.Location = new System.Drawing.Point(123, 20);
            this.presetLog.Name = "presetLog";
            this.presetLog.Size = new System.Drawing.Size(40, 23);
            this.presetLog.TabIndex = 27;
            this.presetLog.Text = "对数";
            this.presetLog.UseVisualStyleBackColor = true;
            this.presetLog.Click += new System.EventHandler(this.presetLog_Click);
            // 
            // presetPow
            // 
            this.presetPow.Enabled = false;
            this.presetPow.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.presetPow.Location = new System.Drawing.Point(169, 20);
            this.presetPow.Name = "presetPow";
            this.presetPow.Size = new System.Drawing.Size(40, 23);
            this.presetPow.TabIndex = 28;
            this.presetPow.Text = "指数";
            this.presetPow.UseVisualStyleBackColor = true;
            this.presetPow.Click += new System.EventHandler(this.presetPow_Click);
            // 
            // presetCustom
            // 
            this.presetCustom.Enabled = false;
            this.presetCustom.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.presetCustom.Location = new System.Drawing.Point(214, 20);
            this.presetCustom.Name = "presetCustom";
            this.presetCustom.Size = new System.Drawing.Size(54, 23);
            this.presetCustom.TabIndex = 30;
            this.presetCustom.Text = "自定义";
            this.presetCustom.UseVisualStyleBackColor = true;
            this.presetCustom.Click += new System.EventHandler(this.presetCustom_Click);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.label1);
            this.panel3.Controls.Add(this.HOnly);
            this.panel3.Controls.Add(this.WOnly);
            this.panel3.Controls.Add(this.step2);
            this.panel3.Controls.Add(this.canvas);
            this.panel3.Controls.Add(this.presetCustom);
            this.panel3.Controls.Add(this.presetPow);
            this.panel3.Controls.Add(this.label8);
            this.panel3.Controls.Add(this.presetLog);
            this.panel3.Controls.Add(this.presetLinear);
            this.panel3.Location = new System.Drawing.Point(6, 293);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(268, 275);
            this.panel3.TabIndex = 31;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label1.Location = new System.Drawing.Point(149, 254);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 17);
            this.label1.TabIndex = 33;
            this.label1.Text = "仅显示";
            // 
            // HOnly
            // 
            this.HOnly.AutoSize = true;
            this.HOnly.Enabled = false;
            this.HOnly.Location = new System.Drawing.Point(234, 254);
            this.HOnly.Name = "HOnly";
            this.HOnly.Size = new System.Drawing.Size(36, 16);
            this.HOnly.TabIndex = 32;
            this.HOnly.Text = "高";
            this.HOnly.UseVisualStyleBackColor = true;
            this.HOnly.CheckedChanged += new System.EventHandler(this.HOnly_CheckedChanged);
            // 
            // WOnly
            // 
            this.WOnly.AutoSize = true;
            this.WOnly.Enabled = false;
            this.WOnly.Location = new System.Drawing.Point(199, 254);
            this.WOnly.Name = "WOnly";
            this.WOnly.Size = new System.Drawing.Size(36, 16);
            this.WOnly.TabIndex = 31;
            this.WOnly.Text = "宽";
            this.WOnly.UseVisualStyleBackColor = true;
            this.WOnly.CheckedChanged += new System.EventHandler(this.WOnly_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.step1);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Location = new System.Drawing.Point(6, 10);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(268, 129);
            this.panel1.TabIndex = 32;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.dirFL);
            this.panel2.Controls.Add(this.dirCTR);
            this.panel2.Controls.Add(this.dirBR);
            this.panel2.Controls.Add(this.dirBL);
            this.panel2.Controls.Add(this.dirTR);
            this.panel2.Controls.Add(this.dirTL);
            this.panel2.Controls.Add(this.dirR);
            this.panel2.Controls.Add(this.dirL);
            this.panel2.Controls.Add(this.dirB);
            this.panel2.Controls.Add(this.step3);
            this.panel2.Controls.Add(this.dirT);
            this.panel2.Location = new System.Drawing.Point(6, 159);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(268, 113);
            this.panel2.TabIndex = 32;
            // 
            // dirFL
            // 
            this.dirFL.AutoSize = true;
            this.dirFL.Location = new System.Drawing.Point(129, 57);
            this.dirFL.Name = "dirFL";
            this.dirFL.Size = new System.Drawing.Size(29, 12);
            this.dirFL.TabIndex = 35;
            this.dirFL.Text = "中心";
            // 
            // dirCTR
            // 
            this.dirCTR.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.dirCTR.Location = new System.Drawing.Point(100, 52);
            this.dirCTR.Name = "dirCTR";
            this.dirCTR.Size = new System.Drawing.Size(23, 23);
            this.dirCTR.TabIndex = 34;
            this.dirCTR.Text = "◯";
            this.dirCTR.UseVisualStyleBackColor = true;
            this.dirCTR.Click += new System.EventHandler(this.dirCTR_Click);
            // 
            // dirBR
            // 
            this.dirBR.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.dirBR.Location = new System.Drawing.Point(181, 81);
            this.dirBR.Name = "dirBR";
            this.dirBR.Size = new System.Drawing.Size(23, 23);
            this.dirBR.TabIndex = 33;
            this.dirBR.Text = "↘";
            this.dirBR.UseVisualStyleBackColor = true;
            this.dirBR.Click += new System.EventHandler(this.dirBR_Click);
            // 
            // dirBL
            // 
            this.dirBL.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.dirBL.Location = new System.Drawing.Point(19, 81);
            this.dirBL.Name = "dirBL";
            this.dirBL.Size = new System.Drawing.Size(23, 23);
            this.dirBL.TabIndex = 32;
            this.dirBL.Text = "↙";
            this.dirBL.UseVisualStyleBackColor = true;
            this.dirBL.Click += new System.EventHandler(this.dirBL_Click);
            // 
            // dirTR
            // 
            this.dirTR.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.dirTR.Location = new System.Drawing.Point(181, 23);
            this.dirTR.Name = "dirTR";
            this.dirTR.Size = new System.Drawing.Size(23, 23);
            this.dirTR.TabIndex = 31;
            this.dirTR.Text = "↗";
            this.dirTR.UseVisualStyleBackColor = true;
            this.dirTR.Click += new System.EventHandler(this.dirTR_Click);
            // 
            // dirTL
            // 
            this.dirTL.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.dirTL.Location = new System.Drawing.Point(19, 23);
            this.dirTL.Name = "dirTL";
            this.dirTL.Size = new System.Drawing.Size(23, 23);
            this.dirTL.TabIndex = 30;
            this.dirTL.Text = "↖";
            this.dirTL.UseVisualStyleBackColor = true;
            this.dirTL.Click += new System.EventHandler(this.dirTL_Click);
            // 
            // dirR
            // 
            this.dirR.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.dirR.Location = new System.Drawing.Point(181, 52);
            this.dirR.Name = "dirR";
            this.dirR.Size = new System.Drawing.Size(23, 23);
            this.dirR.TabIndex = 29;
            this.dirR.Text = "→";
            this.dirR.UseVisualStyleBackColor = true;
            this.dirR.Click += new System.EventHandler(this.dirR_Click);
            // 
            // dirL
            // 
            this.dirL.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.dirL.Location = new System.Drawing.Point(19, 52);
            this.dirL.Name = "dirL";
            this.dirL.Size = new System.Drawing.Size(23, 23);
            this.dirL.TabIndex = 28;
            this.dirL.Text = "←";
            this.dirL.UseVisualStyleBackColor = true;
            this.dirL.Click += new System.EventHandler(this.dirL_Click);
            // 
            // dirB
            // 
            this.dirB.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.dirB.Location = new System.Drawing.Point(100, 81);
            this.dirB.Name = "dirB";
            this.dirB.Size = new System.Drawing.Size(23, 23);
            this.dirB.TabIndex = 27;
            this.dirB.Text = "↓";
            this.dirB.UseVisualStyleBackColor = true;
            this.dirB.Click += new System.EventHandler(this.dirB_Click);
            // 
            // step3
            // 
            this.step3.AutoSize = true;
            this.step3.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.step3.Location = new System.Drawing.Point(0, 0);
            this.step3.Margin = new System.Windows.Forms.Padding(0);
            this.step3.Name = "step3";
            this.step3.Size = new System.Drawing.Size(147, 17);
            this.step3.TabIndex = 23;
            this.step3.Text = "步骤2：指定尺寸变化方向";
            // 
            // dirT
            // 
            this.dirT.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.dirT.Location = new System.Drawing.Point(100, 23);
            this.dirT.Name = "dirT";
            this.dirT.Size = new System.Drawing.Size(23, 23);
            this.dirT.TabIndex = 26;
            this.dirT.Text = "↑";
            this.dirT.UseVisualStyleBackColor = true;
            this.dirT.Click += new System.EventHandler(this.dirT_Click);
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.btnOperate);
            this.panel4.Controls.Add(this.step4);
            this.panel4.Location = new System.Drawing.Point(6, 588);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(268, 49);
            this.panel4.TabIndex = 35;
            // 
            // btnOperate
            // 
            this.btnOperate.Enabled = false;
            this.btnOperate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOperate.Location = new System.Drawing.Point(18, 20);
            this.btnOperate.Name = "btnOperate";
            this.btnOperate.Size = new System.Drawing.Size(75, 23);
            this.btnOperate.TabIndex = 24;
            this.btnOperate.Text = "调整";
            this.btnOperate.UseVisualStyleBackColor = true;
            this.btnOperate.Click += new System.EventHandler(this.btnOperate_Click);
            // 
            // step4
            // 
            this.step4.AutoSize = true;
            this.step4.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.step4.Location = new System.Drawing.Point(0, 0);
            this.step4.Margin = new System.Windows.Forms.Padding(0);
            this.step4.Name = "step4";
            this.step4.Size = new System.Drawing.Size(123, 17);
            this.step4.TabIndex = 23;
            this.step4.Text = "步骤4：进行尺寸调整";
            // 
            // TPProportionate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel3);
            this.Name = "TPProportionate";
            this.Size = new System.Drawing.Size(285, 808);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.canvas)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label step1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label step2;
        private System.Windows.Forms.PictureBox canvas;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button presetLinear;
        private System.Windows.Forms.Button presetLog;
        private System.Windows.Forms.Button presetPow;
        private System.Windows.Forms.Button presetCustom;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox pr2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button dirCTR;
        private System.Windows.Forms.Button dirBR;
        private System.Windows.Forms.Button dirBL;
        private System.Windows.Forms.Button dirTR;
        private System.Windows.Forms.Button dirTL;
        private System.Windows.Forms.Button dirR;
        private System.Windows.Forms.Button dirL;
        private System.Windows.Forms.Button dirB;
        private System.Windows.Forms.Label step3;
        private System.Windows.Forms.Button dirT;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label step4;
        private System.Windows.Forms.Button btnOperate;
        private System.Windows.Forms.Label dirFL;
        private System.Windows.Forms.CheckBox pr3;
        private System.Windows.Forms.CheckBox pr4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox HOnly;
        private System.Windows.Forms.CheckBox WOnly;
        private System.Windows.Forms.CheckBox pr1;
    }
}
