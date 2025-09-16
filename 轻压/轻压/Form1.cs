using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Linq;

namespace Filejieyasuo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            //显示任务栏图标
            this.ShowInTaskbar = false;
        }
        private Thread th = null;
        /// <summary>
        /// 解压到的目录，或者压缩的目录
        /// </summary>
        private string dirName = "";

        /// <summary>
        /// 压缩或解压的文件
        /// </summary>
        private string path;

        private int count = 0;

        private int count2;

        private int countLabel = 0;

        private string passWord = string.Empty;
        private string tempPngPath = string.Empty;

        private uint imgSize = 0;
        private byte[] imgBuffer = null;
        private string[][] labelString = new string[][]
		{
			new string[]
			{
				"当前进度.",
				"当前进度.",
				"当前进度..",
				"当前进度..",
				"当前进度...",
				"当前进度..."
			},
			new string[]
			{
				"总进度.",
				"总进度.",
				"总进度..",
				"总进度..",
				"总进度...",
				"总进度..."
			}
		};
        private void Form1_Load(object sender, EventArgs e)
        {
            this.dirPath.BackColor = Color.Red;

            string commandLine = Environment.CommandLine;
            string[] array = commandLine.Split(new char[]
			{
				'"'
			});

            if (array.Length > 3)
            {
                string path=array[3];
                if (File.Exists(path)&&Path.GetExtension(path).EndsWith("hxp"))
                {
                    //解压
                    this.textBox1.Text = path;
                    this.tabControl1.SelectedTab = this.tabPage2;
                }
                else
                {
                    this.dirPath.Text = array[3];
                    if (Directory.Exists(this.dirPath.Text))
                        this.dirName = this.dirPath.Text;
                    else
                        this.dirName = Path.GetDirectoryName(this.dirPath.Text);
                    this.tabControl1.SelectedTab = this.tabPage1;                   
                    dirPath_TextChanged(null, null);
                    this.button4.Visible = false;
                    button4_Click(null, null);
                }
            }

            this.FormClosing+=Form1_FormClosing;
            this.textBoxPassword.TextChanged += TextBoxPassword_TextChanged;
            this.decomTextBox.TextChanged += TextBoxPassword_TextChanged;
            this.notifyIcon1.ContextMenuStrip = new ContextMenuStrip();
            ToolStripItem v = this.notifyIcon1.ContextMenuStrip.Items.Add("Fang");
            v.Click += V_Click;

            v = this.notifyIcon1.ContextMenuStrip.Items.Add("打开安装目录");
            v.Click += V_Click2;

            v = this.notifyIcon1.ContextMenuStrip.Items.Add("使用文件图标");
            v.Click += V_Click3;

            this.AllowDrop = true;
            this.DragEnter += Form1_DragEnter;
            this.DragDrop += Form1_DragDrop;
            this.pictureBox1.AllowDrop = true;

            this.pictureBox2.Visible = false;
            this.LoadPictureBox(this.textBox1.Text);
        }


        private void PictureBox1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
            
        }

        private void PictureBox1_DragDrop(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
            string[] objs = e.Data.GetData(DataFormats.FileDrop) as string[];
            foreach (var item in objs)
            {
                var ext = Path.GetExtension(item).ToLower();
                if (ext.EndsWith("png") || ext.EndsWith("jpg") || ext.EndsWith("jpeg") || ext.EndsWith("tga") || ext.EndsWith("bmp"))
                {
                    this.pictureBox1.Image = Image.FromFile(item);
                 
                    this.ReadImageBuffer(item);
                    break;
                }
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] objs = e.Data.GetData(DataFormats.FileDrop) as string[];
            foreach (var item in objs)
            {
                FileInfo fileInfo = new FileInfo(item);
                if (fileInfo.Exists)
                {
                    FileManager.Instance.AddFile(this, item, fileInfo.Directory.Name);
                }
                else
                {
                    DirectoryInfo diInfo = new DirectoryInfo(item);
                    if (diInfo.Exists)
                    {
                        FileInfo[] fileInfos = diInfo.GetFiles("*.*", SearchOption.AllDirectories);
                        foreach (var info in fileInfos)
                        {
                            FileManager.Instance.AddFile(this, info.FullName, fileInfo.Directory.Name);
                        }
                    }
                }
            }
            e.Effect = DragDropEffects.Copy;

        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void LoadPictureBox(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    //读取头
                    byte[] array = new byte[3];
                    fileStream.Read(array, 0, array.Length);
                    string tagName = HxpFile.ins.BytesToString(array);
                    this.ReadImage(fileStream);
                }
            }
        }


        private void V_Click3(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "png|*.png|jpg|*.jpg|ico|*.ico|bmp|*.bmp|tif|*.tif|exif|*.exif|webp|*.webp|emf|*.emf|wmf|*.wmf";//(*.txt)|*.txt|所有文件(*.*)|*.*””
            open.Multiselect = false;
            open.Title = "打开文件";
            if (open.ShowDialog() == DialogResult.OK)
            {
                this.ReadImageBuffer(open.FileName);
            }
        }

        #region 缩略图

        public void LoadTempPng(string dirPath)
        {
            if (this.imgSize <= 0)
            {
                string pngPath = dirPath + "/" + HxpFile.tempPng;
                if (File.Exists(pngPath))
                {
                    this.imgBuffer = File.ReadAllBytes(pngPath);
                    this.imgSize = (uint)this.imgBuffer.Length;
                    File.Delete(pngPath);
                }
            }
        }

        //密码之前
        public void WirteImage(FileStream fileStream)
        {
            if (this.imgSize > 0)
            {
                //写入文件标识
                byte[] array_code = new byte[1] { 11 };
                fileStream.Write(array_code, 0, array_code.Length);
                array_code = BitConverter.GetBytes(this.imgSize);
                fileStream.Write(array_code, 0, array_code.Length);
                fileStream.Write(this.imgBuffer, 0, this.imgBuffer.Length);

            }
            else
            {   //写入文件标识
                byte[] array_code = new byte[1] { 10 };
                fileStream.Write(array_code, 0, array_code.Length);
            }
        }

        //密码之前
        public void ReadImage(FileStream fileStream)
        {
            //读取文件标识
            byte[] array_code = new byte[1];
            fileStream.Read(array_code, 0, array_code.Length);
            if (array_code[0] >= 10)//有图片标识
            {
                if (array_code[0] == 11)//有图片
                {
                    //读取文件大小
                    array_code = new byte[4];
                    fileStream.Read(array_code, 0, array_code.Length);
                    uint size = BitConverter.ToUInt32(array_code, 0);
                    //读取图片大小
                    array_code = new byte[size];
                    fileStream.Read(array_code, 0, array_code.Length);
                    Image img = Image.FromStream(new MemoryStream(array_code));

                    this.pictureBox2.Image = img;
                    this.pictureBox2.Visible = true;
                }
            }
            else
            {
                fileStream.Position = fileStream.Position - 1;
            }
        }

        //保存缩略图到指定目录
        public void SaveTempPng(string dirPath)
        {
            string tempPngPath = dirPath + "/" + HxpFile.tempPng;
            if (this.pictureBox2.Image != null)
            {
                this.pictureBox2.Image.Save(tempPngPath, ImageFormat.Png);
                File.SetAttributes(tempPngPath, FileAttributes.Hidden);
            }
        }

        private void ReadImageBuffer(string pngFile)
        {
            Image img = Image.FromFile(pngFile);

            Bitmap bitmap = this.GetThumbnail(img, out bool isSizeOk);
            if (isSizeOk == false)
            {
                MessageBox.Show("系统提示", "输入的图片尺寸宽和高不能小于 64", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string fileName = "temp.png";
            bitmap.Save(fileName, ImageFormat.Png);

            if (File.Exists(fileName))
            {
                using (FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read))
                {
                    this.imgSize = (uint)fs.Length;
                    this.imgBuffer = new byte[fs.Length];
                    fs.Read(this.imgBuffer, 0, this.imgBuffer.Length);
                }
                File.Delete(fileName);
            }
        }

        private Bitmap GetThumbnail(Image img, out bool isSizeOk)
        {
            int width = 256;
            if (img.Width < width || img.Height < width)
                width = width / 2;//128
            if (img.Width < width || img.Height < width)
                width = width / 2;//64
            
            if (img.Width < width || img.Height < width)
            {
                isSizeOk = false;
                return null;
            }

            isSizeOk = true;
            float rate = img.Width > img.Height ? img.Height / (float)img.Width : img.Width / (float)img.Height;
            Size thumbnailSize = img.Width > img.Height ? new Size(width, (int)(width * rate)) : new Size((int)(width * rate), width);
            var bitmap = new Bitmap(width, width);
            var iamge = img.GetThumbnailImage(thumbnailSize.Width, thumbnailSize.Height, null, IntPtr.Zero);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                graphics.DrawImage(iamge, 0, 0, thumbnailSize.Width, thumbnailSize.Height);
            }
            return bitmap;
        }


        /// <summary>
        /// 绘制字符串
        /// </summary>
        /// <param name="y">绘制的坐标位置</param>
        /// <returns></returns>
        private Bitmap DrawString(string s, int y, int width = 256)
        {
            var bitmap = new Bitmap(width, width);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                graphics.FillRectangle(Brushes.White, new Rectangle(Point.Empty, new Size(width, width)));
                graphics.DrawString(s, new Font("黑体", 9), Brushes.Gray, new Point(0, y));
            }
            return bitmap;
        } 
        #endregion

        private void V_Click2(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Application.StartupPath);
        }

        private void V_Click(object sender, EventArgs e)
        {
            this.notifyIcon1.ShowBalloonTip(3, "系统提示", "您点击了此项哦", ToolTipIcon.Info);
        }

        private void TextBoxPassword_TextChanged(object sender, EventArgs e)
        {
            TextBox obj = sender as TextBox;
            passWord = obj.Text.Trim();
            if (passWord.Length > 8)
            {
                obj.Text = passWord.Substring(0, 8);
                passWord = obj.Text;
                MessageBox.Show(this, "密码不能超过8个", "系统提示", MessageBoxButtons.OK);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (th != null)
            {
              
                Application.ExitThread();
                Application.Exit();
            }
        }

        #region 压缩
        private void button1_Click(object sender, EventArgs e)
        {

            if (this.listBox1.Items.Count > 0)
            {

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "sip|*.sip|hxp|*.hxp";
                saveFileDialog.Title = "保存指定的文件";
                saveFileDialog.FilterIndex = 0;
                saveFileDialog.AddExtension = true;
                if (File.Exists(this.dirPath.Text))
                    saveFileDialog.InitialDirectory = Path.GetDirectoryName(this.dirPath.Text);
                else
                    saveFileDialog.InitialDirectory = new DirectoryInfo(this.dirPath.Text).Parent.FullName;
                saveFileDialog.FileName = this.dirName;


                saveFileDialog.FilterIndex = FileManager.Instance.IsSip ? 1 : 2;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    this.textParent1.Visible = true;
                    this.textParent2.Visible = true;
                    this.ComProgressBar1.Visible = true;
                    this.ComProgressBar2.Visible = true;
                    this.button1.Visible = false;
                    this.label6.Visible = true;
                    this.label7.Visible = true;
                    this.timer1.Enabled = true;
                    this.dirDescription.Visible = false;
                    this.dirPath.Visible = false;
                    this.textBoxPassword.Visible = false;
                    this.labelPassword.Visible = false;

                    this.dirName = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
                    this.path = saveFileDialog.FileName.Replace(FileStruct.TagSip, FileStruct.TagHXP);

                    this.th = new Thread(new ParameterizedThreadStart(this.CompressionFile));
                    this.th.IsBackground = true;
                    this.th.Start(this);

                }
                else
                {
                    this.dirDescription.Visible = true;
                    this.dirPath.Visible = true;
                }
            }
        }

        private void CompressionFile(object obj)
        {
            try
            {
                HxpFile.ins.Compression(this, path, this.dirName, passWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK);
            }
        }

	    #endregion 
        #region 解压
        private void button2_Click(object sender, EventArgs e)
        {
            FileManager.Instance.OpenFilePath(this);

            this.LoadPictureBox(this.textBox1.Text.Trim());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (this.textBox1.Text.Length <= 0 || !File.Exists(textBox1.Text.Trim())) return;

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;
            folderBrowserDialog.SelectedPath = Path.GetDirectoryName(this.textBox1.Text);
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {

                path = folderBrowserDialog.SelectedPath;
                th = new Thread(DecompressionFile);
                th.IsBackground = true;
                th.Start(this);
            }

        }

        private void ShowView()
        {
            this.progressBar2.Visible = true;
            this.label3.Visible = true;
            this.progressBar1.Visible = true;
            this.label2.Visible = true;
            this.label8.Visible = true;
            this.label9.Visible = true;
            this.timer2.Enabled = true;

            this.decomTextBox.Visible = false;
            this.label10.Visible = false;
            this.button3.Visible = false;
            this.Refresh();
        }

        private void DecompressionFile(object obj)
        {
            try
            {
                Form1 form = obj as Form1;
                HxpFile.ins.Decompression(form, form.path, form.passWord, (isErrorPwd)=> {
                    if (isErrorPwd)
                    {
                        form.th.Abort();
                    }
                    else
                    {
                        form.ShowView();
                        
                    }
                });
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK);
            }
        }
        #endregion
        #region 计时

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.count++;
            this.ShowLabel();
            this.showTimeText.Text = this.GetTime(this.count);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            this.count2++;
            this.ShowLabel();
            this.showSecond.Text = this.GetTime(this.count2);
        }

        private void ShowLabel()
        {
            this.countLabel++;
            this.label6.Text = this.labelString[0][this.countLabel.Clamp(this.countLabel % this.labelString[0].Length, 0, this.labelString[0].Length - 1)];
            this.label7.Text = this.labelString[1][this.countLabel.Clamp(this.countLabel % this.labelString[0].Length, 0, this.labelString[1].Length - 1)];
            this.label8.Text = this.labelString[0][this.countLabel.Clamp(this.countLabel % this.labelString[0].Length, 0, this.labelString[0].Length - 1)];
            this.label9.Text = this.labelString[1][this.countLabel.Clamp(this.countLabel % this.labelString[0].Length, 0, this.labelString[1].Length - 1)];
        }

        private string GetTime(int count)
        {
            float num = (float)count / 10f;
            string result;
            if (count / 10 > 60)
            {
                int num2 = count / 600;
                num -= (float)(num2 * 60);
                result = string.Format("已用:{0:d2}分{1:f1}秒", num2, num);
            }
            else
            {
                result = string.Format("已用:{0:f1}秒", num);
            }
            return result;
        } 
        #endregion

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "所有文件|*.*";
            open.Multiselect = true;
            open.RestoreDirectory = true;
            open.CheckFileExists = true;
            open.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            open.Title = "打开文件";
            string[] arrayPath = this.dirPath.Text?.Split('|').Where(p=>!string.IsNullOrWhiteSpace(p))?.ToArray();
            if (sender!=null)
            {
                if (open.ShowDialog() == DialogResult.OK)
                {
                    arrayPath =  open.FileNames;
                    this.dirPath.Text = string.Join("|", arrayPath);
                   
                }
            }

            if (arrayPath!=null&&arrayPath.Length>0)
            {
                this.ShowCountLable.Visible = true;
                this.listBox1.Items.Clear();
                FileManager.ObjectData objectData = new FileManager.ObjectData();
                objectData.form1 = this;
                string singleName = string.Empty,multiName = string.Empty;
                foreach (var item in arrayPath)
                {
                    if (File.Exists(item))
                    {
                        singleName = Path.GetFileNameWithoutExtension(item);
                        multiName = new FileInfo(item).Directory.Name;
                        objectData.fileSystems.Add(new FileInfo(item));
                    }
                    else if (Directory.Exists(item))
                    {
                        singleName = new DirectoryInfo(item).Name;
                        multiName = new DirectoryInfo(item).Parent.Name;
                        objectData.fileSystems.Add(new DirectoryInfo(item));
                    }
                }

                if (objectData.fileSystems.Count == 1)
                {
                    this.dirName = singleName;
                }
                else
                {
                    this.dirName = multiName;
                }


                objectData.dirName = this.dirName;
                new Thread(new ParameterizedThreadStart(SingletonClass<FileManager>.Instance.OpenDirectoryPath))
                {
                    IsBackground = true
                }.Start(objectData);
            }
        }

        #region 鼠标事件
        [DllImport("user32.dll")]
        public static extern bool SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        [DllImport("user32.dll")]
        internal static extern IntPtr WindowFromPoint(Point Point);

        [DllImport("user32.dll")]
        internal static extern bool GetCursorPos(out Point lpPoint);

        public static IntPtr GetMouseWindow()
        {
            Point point;
            Form1.GetCursorPos(out point);
            return Form1.WindowFromPoint(point);
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public new static void MouseMove(int x, int y)
        {
            Form1.mouse_event(1, x, y, 0, 0);
        }

        public new static void MouseClick()
        {

            Form1.mouse_event(6, 0, 0, 0, 0);
        }

        public new static void MouseDown()
        {
            MessageBox.Show(Form1.mouse_event(2, 0, 0, 0, 0).ToString());
        }

        public new static void MouseUp()
        {
            Form1.mouse_event(4, 0, 0, 0, 0);
        }
        
        #endregion

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            FileManager.Instance.IsSip = true;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            FileManager.Instance.IsSip = false;
        }

        private void dirPath_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.dirPath.Text) || !Directory.Exists(this.dirPath.Text.Trim()))
            {
                this.dirPath.BackColor = Color.Red;
            }
            else {
                this.dirPath.BackColor = Color.LightGreen;
            }
        }

        #region 视图执行
        public void ShowProssbor(int state, float count, float sum)
        {
            ProgressBar bar = this.ComProgressBar1;
            Label txt = this.label2;
            if (state > 10)
            {
                bar = state == 11 ? this.progressBar1 : this.progressBar2;
                txt = state == 11 ? this.label2 : this.label3;
            }
            else
            {
                bar = state == 1 ? this.ComProgressBar1 : this.ComProgressBar2;
                txt = state == 1 ? this.textParent1 : this.textParent2;
            }


            float showTime = count >= sum ? 1 : count / sum;
            bar.Value = (int)(showTime * 1000);
            txt.Text = string.Format("{0:f1}%", (showTime * 100));
            bar.Refresh();
            txt.Refresh();
        }

        public void CloseForm(int state)
        { 
            bool isFinish = this.ComProgressBar2.Value >= 1000 && this.ComProgressBar1.Value >= 1000;
            if (state == 2)
            {
                isFinish = this.progressBar1.Value >= 1000 && this.progressBar2.Value >= 1000;
            }

            if (isFinish)
            {
                Thread.Sleep(500);
                Application.ExitThread();
                Application.Exit();
            }
        }
        #endregion
    }
}

/*图片格式分类
 1 Webp格式
2 BMP格式
3 PCX格式
4 TIF格式
5 GIF格式
6 JPEG格式
7 TGA格式
8 EXIF格式
9 FPX格式
10 SVG格式
11 PSD格式
12 CDR格式
13 PCD格式
14 DXF格式
15 UFO格式
16 EPS格式
17 AI格式
18 PNG格式
19 HDRI格式
20 RAW格式
▪ 储存和优势
▪ 适用性
▪ 弊端
21 WMF格式
22 FLIC格式
23 EMF格式
24 ICO格式
▪ 应用途径
▪ 制作流程
25 avif格式
26 APNG格式
 */