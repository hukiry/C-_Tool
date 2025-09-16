using Microsoft.VisualBasic.Devices;
using System;
using System.IO;
using System.Windows.Forms;
namespace ComputerInfo
{
    public partial class RenameFileShowForm : Form
    {
        public Action<TextBox> action;
        public RenameFileShowForm()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            //开始执行
            string dirPath = this.textBox1.Text.Trim();
            if (!string.IsNullOrEmpty(dirPath) && Directory.Exists(dirPath))
            {
                string oldTex = this.textBox2.Text.Trim();
                string newTex = this.textBox3.Text.Trim();
                Computer myComputer = new Computer();
                string[] array = Directory.GetFiles(dirPath, "*.*");
                if (array != null && array.Length > 0)
                {
                    foreach (var oldpath in array)
                    {
                        string newPath = Path.GetFileName(oldpath);
                        string newName = newPath.Replace(oldTex, newTex);
                        if (newPath != newName)
                        {
                            myComputer.FileSystem.RenameFile(oldpath, newName);
                        }
                    }
                }
                //myComputer.FileSystem.RenameDirectory(fullDirPath, newName);
            }
            else
            {
                MessageBox.Show("文本提示", "目录路径不正确", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.textBox1.Clear();
            this.textBox2.Clear();
            this.textBox3.Clear();
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //打开目录
            FolderSelectDialog dialog = new FolderSelectDialog();
            if (dialog.ShowDialog(this.Handle))
            {
                this.textBox1.Text = dialog.FileName;
            }

        }

    }
}
