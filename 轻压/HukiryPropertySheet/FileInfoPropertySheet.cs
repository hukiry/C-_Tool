using HukiryPropertySheet.Properties;
using SharpShell.Attributes;
using SharpShell.SharpPropertySheet;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HukiryPropertySheet
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.FileExtension, ".hxp", ".sip")]
    public partial class FileInfoPropertySheet : SharpPropertyPage
    {
        private string currentFilePath;

        public FileInfoPropertySheet()
        {
            InitializeComponent();
            PageTitle = "HXP FileInfo";
            PageIcon = Resources.sip;
        }

        protected override void OnPropertyPageInitialised(SharpPropertySheet parent)
        {
            //  Store the file path.
            this.currentFilePath = parent.SelectedItemPaths.FirstOrDefault();

            MessageBox.Show("test hxp");
            //  Load the file times into the dialog.
            LoadFileInfoPropertySheet();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(this.currentFilePath);
        }

        private void LoadFileInfoPropertySheet()
        {
            try
            {
                this.labFileName.Text = Path.GetFileName(this.currentFilePath);
                this.labPath.Text = this.currentFilePath;
                FileInfo fi = new FileInfo(this.currentFilePath);
                this.labCreateTime.Text = fi.CreationTime.ToLongDateString();
                using (FileStream fs = new FileStream(this.currentFilePath, FileMode.Open))
                {
                    if (fs.Length > 1024 * 1024 * 1024)
                    {
                        this.labSize.Text = string.Format("{0:0.2f}G", fs.Length / (float)(1024 * 1024 * 1024));
                    }
                    else if (fs.Length > 1024 * 1024)
                    {
                        this.labSize.Text = string.Format("{0:0.2f}M", fs.Length / (float)(1024 * 1024));
                    }
                    else if (fs.Length > 1024)
                    {
                        this.labSize.Text = string.Format("{0:0.2f}K", fs.Length / (float)1024);
                    }
                    else
                    {
                        this.labSize.Text = string.Format("{0:0.2f}B", fs.Length);
                    }
                }

                this.pictureBox2.Image = GetThumbnail(Properties.Resources.sip.ToBitmap(), 16);

                this.LoadPreview();
            }
            catch 
            {
            }
        }

        private void LoadPreview()
        {

            using (FileStream fileStream = new FileStream(this.currentFilePath, FileMode.Open, FileAccess.Read))
            {
                //读取头
                byte[] array = new byte[3];
                fileStream.Read(array, 0, array.Length);
                this.ReadImage(fileStream);
            }

        }

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
                    this.pictureBox1.Image = Image.FromStream(new MemoryStream(array_code));
                }
            }
        }

        private Bitmap GetThumbnail(Image img, uint width)
        {
            var thumbnailSize = new Size((int)width, (int)width);
            var bitmap = new Bitmap(thumbnailSize.Width, thumbnailSize.Height,
                                    PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                graphics.DrawImage(img, 0, 0, thumbnailSize.Width, thumbnailSize.Height);
            }
            return bitmap;
        }
    }
}
