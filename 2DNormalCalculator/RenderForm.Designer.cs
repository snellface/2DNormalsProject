namespace _2DNormalCalculator
{
    partial class RenderForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.centerImageCheckBox = new System.Windows.Forms.CheckBox();
            this.displayColorsCheckBox = new System.Windows.Forms.CheckBox();
            this.setZoomUpDown = new System.Windows.Forms.NumericUpDown();
            this.xnaFrame = new _2DNormalCalculator.XNAFrame();
            this.singleReRenderButton = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.setZoomUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.xnaFrame);
            this.panel1.Location = new System.Drawing.Point(12, 45);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(323, 203);
            this.panel1.TabIndex = 1;
            // 
            // centerImageCheckBox
            // 
            this.centerImageCheckBox.AutoSize = true;
            this.centerImageCheckBox.Location = new System.Drawing.Point(12, 12);
            this.centerImageCheckBox.Name = "centerImageCheckBox";
            this.centerImageCheckBox.Size = new System.Drawing.Size(88, 17);
            this.centerImageCheckBox.TabIndex = 2;
            this.centerImageCheckBox.Text = "Center image";
            this.centerImageCheckBox.UseVisualStyleBackColor = true;
            // 
            // displayColorsCheckBox
            // 
            this.displayColorsCheckBox.AutoSize = true;
            this.displayColorsCheckBox.Checked = true;
            this.displayColorsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.displayColorsCheckBox.Location = new System.Drawing.Point(106, 12);
            this.displayColorsCheckBox.Name = "displayColorsCheckBox";
            this.displayColorsCheckBox.Size = new System.Drawing.Size(91, 17);
            this.displayColorsCheckBox.TabIndex = 3;
            this.displayColorsCheckBox.Text = "Display colors";
            this.displayColorsCheckBox.UseVisualStyleBackColor = true;
            this.displayColorsCheckBox.CheckedChanged += new System.EventHandler(this.displayColorsCheckBox_CheckedChanged);
            // 
            // setZoomUpDown
            // 
            this.setZoomUpDown.Location = new System.Drawing.Point(203, 11);
            this.setZoomUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.setZoomUpDown.Name = "setZoomUpDown";
            this.setZoomUpDown.Size = new System.Drawing.Size(46, 20);
            this.setZoomUpDown.TabIndex = 4;
            this.setZoomUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.setZoomUpDown.ValueChanged += new System.EventHandler(this.setZoomUpDown_ValueChanged);
            // 
            // xnaFrame
            // 
            this.xnaFrame.BackColor = System.Drawing.Color.Magenta;
            this.xnaFrame.Location = new System.Drawing.Point(0, 0);
            this.xnaFrame.Name = "xnaFrame";
            this.xnaFrame.RefreshMode = _2DNormalCalculator.XNAFrame.eRefreshMode.Always;
            this.xnaFrame.Size = new System.Drawing.Size(320, 200);
            this.xnaFrame.TabIndex = 0;
            this.xnaFrame.MouseMove += new System.Windows.Forms.MouseEventHandler(this.xnaFrame_MouseMove);
            this.xnaFrame.MouseDown += new System.Windows.Forms.MouseEventHandler(this.xnaFrame_MouseDown);
            this.xnaFrame.MouseUp += new System.Windows.Forms.MouseEventHandler(this.xnaFrame_MouseUp);
            // 
            // singleReRenderButton
            // 
            this.singleReRenderButton.Location = new System.Drawing.Point(260, 8);
            this.singleReRenderButton.Name = "singleReRenderButton";
            this.singleReRenderButton.Size = new System.Drawing.Size(75, 23);
            this.singleReRenderButton.TabIndex = 5;
            this.singleReRenderButton.Text = "Rerender";
            this.singleReRenderButton.UseVisualStyleBackColor = true;
            this.singleReRenderButton.Click += new System.EventHandler(this.singleReRenderButton_Click);
            // 
            // RenderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(347, 260);
            this.Controls.Add(this.singleReRenderButton);
            this.Controls.Add(this.setZoomUpDown);
            this.Controls.Add(this.displayColorsCheckBox);
            this.Controls.Add(this.centerImageCheckBox);
            this.Controls.Add(this.panel1);
            this.Name = "RenderForm";
            this.Text = "Renderer";
            this.Load += new System.EventHandler(this.RenderForm_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TestForm_FormClosed);
            this.Resize += new System.EventHandler(this.RenderForm_Resize);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.setZoomUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private XNAFrame xnaFrame;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox centerImageCheckBox;
        private System.Windows.Forms.CheckBox displayColorsCheckBox;
        private System.Windows.Forms.NumericUpDown setZoomUpDown;
        private System.Windows.Forms.Button singleReRenderButton;
    }
}