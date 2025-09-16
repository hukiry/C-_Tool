using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 轻压Install
{
    public partial class Form1 : Form
    {
        private string dirPath = "C:\\Program Files\\Hukiry";
        private bool isDelete = false;
        private bool isFinish = false;
        private Thread th;

        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            this.textBox1.Text = dirPath;

        }

        //点击安装
        private void buttonInstall_Click(object sender, EventArgs e)
        {
           
            if (isFinish)
            {
                Application.Exit();
                return;
            }
            if (string.IsNullOrEmpty(this.textBox1.Text))
            {
                MessageBox.Show(this, "安装路径不可为空，请重新输入！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            this.button2.Enabled = false;
            this.textBox1.Enabled = false;
            this.checkBox1.Enabled = false;
            this.button1.Enabled = false;
            this.button3.Enabled = false;

            this.th = new Thread(new ParameterizedThreadStart(this.EnableRegistry));
            this.th.IsBackground = true;
            this.th.Start(this);

        }

        private void EnableRegistry(object obj)
        {
            try
            {
                string softName = "qingya";
                InstallManager.ins.EnableRegistry(this, dirPath, dirPath + $"\\{softName}.exe", this.isDelete, softName);
            }
            catch (Exception ex)
            {
                this.button2.Enabled = true;
                this.textBox1.Enabled = true;
                this.checkBox1.Enabled = true;
                this.button3.Enabled = true;
                this.button1.Enabled = true;
                MessageBox.Show(this, ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void InistallFinish()
        {
            this.button1.Text = "完成！";
            this.button2.Enabled = false;
            this.textBox1.Enabled = false;
            this.checkBox1.Enabled = true;
            this.button1.Enabled = true;
            this.button3.Enabled = true;
            this.isFinish = true;
        }

        //预览目录
        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.ShowNewFolderButton = true;
            folder.RootFolder = Environment.SpecialFolder.Desktop;
            folder.Description = "打开安装目录";
            if (folder.ShowDialog(this) == DialogResult.OK)
            {
                this.textBox1.Text = folder.SelectedPath;
                this.dirPath = folder.SelectedPath;
               
                this.checkBox1.Checked = true;
                this.isDelete = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.ExitThread();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            isDelete = !isDelete;
        }
    }
}
