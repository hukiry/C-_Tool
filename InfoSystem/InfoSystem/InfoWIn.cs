using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ComputerInfo;

namespace InfoSystem
{
    public partial class InfoWIn : Form
    {
        public static string _fileName = null;
        Main mainForm;
        public InfoWIn()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        public InfoWIn(string info, Main mainForm) : this()
        {
            this.mainForm = mainForm;

            this.richTextBox1.Text = info;
            // 若Main窗口已经关闭后执行代码又引发异常，此时再调用此窗口句柄即为无效句柄，将报错，加此判断
            this.Focus();
        }

        private void 复制到剪贴板ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.richTextBox1.SelectAll();
            this.richTextBox1.Copy();
            MessageBox.Show("文本内容已复制，您可以粘贴发送了。");
        }

        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FIleNameInputWin fIleNameInputWin = new FIleNameInputWin();
            var res = fIleNameInputWin.ShowDialog();
            if (res == DialogResult.OK)
            {
                // 使用rtf写字板文件，而不是txt记事本,txt在win7可能不会换行
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), 
                     _fileName + ".rtf");
                // FileMode.Create存在则覆
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    using (StreamWriter stw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        stw.Write(this.richTextBox1.Text);
                    }
                }
                MessageBox.Show("已在您的桌面创建了存储了配置信息的文本文件。");
            }
        }

        private void 重命名ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RenameFileShowForm show = new RenameFileShowForm();
            show.action = (dirTex) =>
            {
                //打开目录
                FolderBrowserDialog folder = new FolderBrowserDialog();
                if (folder.ShowDialog() == DialogResult.OK)
                {
                    dirTex.Text = folder.SelectedPath;
                }
            };

            var res = show.ShowDialog(this);
            if (res == DialogResult.OK)
            {
                MessageBox.Show("执行成功！");
            }
        }

        private void InfoWIn_Load(object sender, EventArgs e)
        {
            mainForm.Hide();
        }


    }
}
