namespace TestPLC20170302
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonInitial = new System.Windows.Forms.Button();
            this.buttonReset = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxRail2Pos = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxRail1Pos = new System.Windows.Forms.TextBox();
            this.OutPutTextBox = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonInitial
            // 
            this.buttonInitial.Location = new System.Drawing.Point(13, 9);
            this.buttonInitial.Name = "buttonInitial";
            this.buttonInitial.Size = new System.Drawing.Size(75, 23);
            this.buttonInitial.TabIndex = 0;
            this.buttonInitial.Text = "连接初始化";
            this.buttonInitial.UseVisualStyleBackColor = true;
            this.buttonInitial.Click += new System.EventHandler(this.buttonInitial_Click);
            // 
            // buttonReset
            // 
            this.buttonReset.Enabled = false;
            this.buttonReset.Location = new System.Drawing.Point(130, 9);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(75, 23);
            this.buttonReset.TabIndex = 0;
            this.buttonReset.Text = "系统复位";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(130, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "导轨2位置：";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // textBoxRail2Pos
            // 
            this.textBoxRail2Pos.Location = new System.Drawing.Point(130, 58);
            this.textBoxRail2Pos.Name = "textBoxRail2Pos";
            this.textBoxRail2Pos.Size = new System.Drawing.Size(100, 21);
            this.textBoxRail2Pos.TabIndex = 2;
            this.textBoxRail2Pos.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "导轨1位置：";
            this.label2.Click += new System.EventHandler(this.label1_Click);
            // 
            // textBoxRail1Pos
            // 
            this.textBoxRail1Pos.Location = new System.Drawing.Point(13, 58);
            this.textBoxRail1Pos.Name = "textBoxRail1Pos";
            this.textBoxRail1Pos.Size = new System.Drawing.Size(100, 21);
            this.textBoxRail1Pos.TabIndex = 2;
            this.textBoxRail1Pos.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // OutPutTextBox
            // 
            this.OutPutTextBox.Location = new System.Drawing.Point(13, 105);
            this.OutPutTextBox.Name = "OutPutTextBox";
            this.OutPutTextBox.ReadOnly = true;
            this.OutPutTextBox.Size = new System.Drawing.Size(223, 110);
            this.OutPutTextBox.TabIndex = 3;
            this.OutPutTextBox.Text = "先打开两台控制柜并上电";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 90);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 1;
            this.label3.Text = "输出：";
            this.label3.Click += new System.EventHandler(this.label1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(248, 227);
            this.Controls.Add(this.OutPutTextBox);
            this.Controls.Add(this.textBoxRail1Pos);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxRail2Pos);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.buttonInitial);
            this.Name = "Form1";
            this.Text = "导轨调试（先打开两台控制柜并上电）";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonInitial;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxRail2Pos;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxRail1Pos;
        private System.Windows.Forms.RichTextBox OutPutTextBox;
        private System.Windows.Forms.Label label3;
    }
}

