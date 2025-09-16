
namespace HukiryPropertySheet
{
    partial class FileInfoPropertySheet
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

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.labSize = new System.Windows.Forms.Label();
            this.labPath = new System.Windows.Forms.Label();
            this.labCreateTime = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.labFileName = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "文件预览图";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(19, 36);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(150, 150);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(188, 115);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "创建时间：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 217);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "文件大小：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(17, 268);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 5;
            this.label5.Text = "文件路径：";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(19, 318);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 26);
            this.button1.TabIndex = 6;
            this.button1.Text = "拷贝文件路径";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // labSize
            // 
            this.labSize.AutoSize = true;
            this.labSize.Location = new System.Drawing.Point(88, 217);
            this.labSize.Name = "labSize";
            this.labSize.Size = new System.Drawing.Size(0, 12);
            this.labSize.TabIndex = 7;
            // 
            // labPath
            // 
            this.labPath.AutoSize = true;
            this.labPath.Location = new System.Drawing.Point(88, 268);
            this.labPath.Name = "labPath";
            this.labPath.Size = new System.Drawing.Size(0, 12);
            this.labPath.TabIndex = 8;
            // 
            // labCreateTime
            // 
            this.labCreateTime.AutoSize = true;
            this.labCreateTime.Location = new System.Drawing.Point(259, 115);
            this.labCreateTime.Name = "labCreateTime";
            this.labCreateTime.Size = new System.Drawing.Size(0, 12);
            this.labCreateTime.TabIndex = 9;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Location = new System.Drawing.Point(190, 36);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(47, 42);
            this.pictureBox2.TabIndex = 10;
            this.pictureBox2.TabStop = false;
            // 
            // labFileName
            // 
            this.labFileName.AutoSize = true;
            this.labFileName.Location = new System.Drawing.Point(259, 49);
            this.labFileName.Name = "labFileName";
            this.labFileName.Size = new System.Drawing.Size(0, 12);
            this.labFileName.TabIndex = 11;
            // 
            // FileInfoPropertySheet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labFileName);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.labCreateTime);
            this.Controls.Add(this.labPath);
            this.Controls.Add(this.labSize);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.Name = "FileInfoPropertySheet";
            this.Size = new System.Drawing.Size(339, 390);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label labSize;
        private System.Windows.Forms.Label labPath;
        private System.Windows.Forms.Label labCreateTime;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label labFileName;
    }
}
