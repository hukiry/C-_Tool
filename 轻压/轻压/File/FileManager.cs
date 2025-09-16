using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Filejieyasuo
{
    class FileManager : SingletonClass<FileManager>
    {
        public class ObjectData
        {
            public Form1 form1;

            public string dirName;

            public List<FileSystemInfo> fileSystems = new List<FileSystemInfo> ();
        }
        public bool IsSip = false;


        public Dictionary<string, string> dir = new Dictionary<string, string>();
        public bool isFinishLoadFile = false;
        #region 压缩文件
        /// <summary>
        ///压缩
        /// </summary>
        /// <param name="obj"></param>
        public void CompressionFile(object obj, string saveFile, string ext)
        {
            CompressionBigFile(obj, saveFile, ext);
        }

        private void ShowProssbor(ProgressBar bar, Label txt, long count, long sum)
        {
            float showTime = (float)count / (float)sum;
            bar.Value = (int)(showTime * 1000);
            txt.Text = string.Format("{0:f1}%", (showTime * 100));
        }
        /// <summary>
        /// 存入到字典集合
        /// </summary>
        /// <param name="f"></param>
        private void CompressionBigFile(object obj, string saveFile, string ext)
        {
            Form1 form = obj as Form1;
            int num = 0;
            int SizeLength = IsSip ? FileStruct.FilePathBufferSip : FileStruct.FilePathBufferHxp;
            using (FileStream fileStream = new FileStream(saveFile, FileMode.Create, FileAccess.Write))
            {
                byte[] array_code = FileData.FileHead(FileStruct.UseCode);
                fileStream.Write(array_code, 0, array_code.Length);  //头

                byte[] array = FileData.FileHead(ext);
                array = this.UseCode(array);
                fileStream.Write(array, 0, array.Length);  //头
                foreach (string fileName in form.listBox1.Items)
                {
                    num++;
                    FileInfo fileInfo = new FileInfo(fileName);
                    form.showPath.Text = fileInfo.FullName;
                    using (FileStream fileStream2 = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
                    {
                        byte[] array2 = SingletonClass<FileData>.Instance.ComputerSize(fileStream2.Length.ToString(), FileStruct.FileSizeBuffer);
                        byte[] array3;
                        byte[] array5 = SingletonClass<FileData>.Instance.ComputerSize(fileInfo.Name, SizeLength);
                        array2 = this.UseCode(array2);
                        fileStream.Write(array2, 0, array2.Length); //文件大小
                        if (this.dir.Count > 0)
                        {
                            if (!IsSip)
                            {
                                array5 = Encoding.Default.GetBytes(this.dir[fileInfo.FullName]);
                                array3 = SingletonClass<FileData>.Instance.ComputerSize(array5.Length.ToString(), FileStruct.PathSizeBuffer);
                                array3 = this.UseCode(array3);
                                fileStream.Write(array3, 0, array3.Length); //路径大小 
                            }
                            else
                            {
                                array5 = SingletonClass<FileData>.Instance.ComputerSize(this.dir[fileInfo.FullName], FileStruct.FilePathBufferSip);
                            }
                        }
                        array5 = this.UseCode(array5);
                        fileStream.Write(array5, 0, array5.Length); //路径字节
                        byte[] array4 = new byte[1024 * 1024];
                        List<byte[]> list = new List<byte[]>();
                        while (fileStream2.Position < fileStream2.Length)
                        {
                            int count = fileStream2.Read(array4, 0, array4.Length);
                            array4 = this.UseCode(array4);
                            fileStream.Write(array4, 0, count);
                            this.ShowProssbor(form.ComProgressBar1, form.textParent1, fileStream2.Position, fileStream2.Length);
                        }
                    }
                    this.ShowProssbor(form.ComProgressBar2, form.textParent2, (long)num, (long)form.listBox1.Items.Count);
                    this.CloseForm(form.ComProgressBar2, form.ComProgressBar1);
                }
            }
            form.timer1.Enabled = false;
        }
        #endregion

        #region 打开文件
        public void OpenDirectoryPath(object obj)
        {
            HxpFile.ins.Clear();
            FileManager.ObjectData objectData = obj as FileManager.ObjectData;
            this.dir.Clear();
            objectData.form1.dirDescription.Enabled = false;
            objectData.form1.dirPath.Enabled = false;
            objectData.form1.button4.Enabled = false;
            objectData.form1.button1.Enabled = false;

            foreach (var item in objectData.fileSystems)
            {
                if (item is DirectoryInfo)
                {
                    this.LoadListBox(objectData.form1.ShowCountLable, objectData.form1.listBox1, (item as DirectoryInfo), item.Name);
                }
                else
                {
                    FileInfo fileInfo = new FileInfo(item.FullName);
                    HxpFile.ins.AddCompressionFile(fileInfo.FullName, objectData.dirName);
                    string value = this.DealWith(fileInfo.FullName, objectData.dirName);
                    if (!this.dir.ContainsKey(fileInfo.FullName))
                    {
                        this.dir.Add(fileInfo.FullName, value);
                    }
                    objectData.form1.listBox1.Items.Insert(0, fileInfo.FullName);
                    objectData.form1.ShowCountLable.Text = "文件个数: " + objectData.form1.listBox1.Items.Count.ToString();
                }
            }
           

            objectData.form1.dirDescription.Enabled = true;
            objectData.form1.dirPath.Enabled = true;
            objectData.form1.button4.Enabled = true;
            objectData.form1.button1.Enabled = true;
        }

        public void AddFile(Form1 form1, string fileFullPath, string dirName)
        {
            FileInfo fileInfo = new FileInfo(fileFullPath);
            HxpFile.ins.AddCompressionFile(fileInfo.FullName, dirName);
            string value = this.DealWith(fileInfo.FullName, dirName);
            if (!this.dir.ContainsKey(fileInfo.FullName))
            {
                this.dir.Add(fileInfo.FullName, value);
                form1.listBox1.Items.Insert(0, fileInfo.FullName);
            }
            form1.ShowCountLable.Text = "文件个数: " + form1.listBox1.Items.Count.ToString();
        }

        private void LoadListBox(Label lab, ListBox listBox, DirectoryInfo di, string name)
        {
            FileSystemInfo[] fileSystemInfos = di.GetFileSystemInfos();
            FileSystemInfo[] array = fileSystemInfos;
            for (int i = 0; i < array.Length; i++)
            {
                FileSystemInfo fileSystemInfo = array[i];
                if (fileSystemInfo is FileInfo)
                {
                   
                    FileInfo fileInfo = new FileInfo(fileSystemInfo.FullName);
                    HxpFile.ins.AddCompressionFile(fileInfo.FullName, name);
                    string value = this.DealWith(fileInfo.FullName, name);
                    if (!this.dir.ContainsKey(fileInfo.FullName))
                    {
                        this.dir.Add(fileInfo.FullName, value);
                    }
                    listBox.Items.Insert(0, fileInfo.FullName);
                    lab.Text = "文件个数: " + listBox.Items.Count.ToString();
                }
                else
                {
                    DirectoryInfo di2 = new DirectoryInfo(fileSystemInfo.FullName);
                    this.LoadListBox(lab, listBox, di2, name);
                }
            }
        }

        private string DealWith(string p, string name)
        {
            int num = p.IndexOf(name);
            return p.Substring(num, p.Length - num);
        }
        /// <summary>
        /// 打开解压文件路径
        /// </summary>
        /// <param name="form1"></param>
        public void OpenFilePath(Form1 form1)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "sip;hxp|*.sip;*.hxp";
            open.Multiselect = false;
            open.Title = "打开文件";
            if (open.ShowDialog() == DialogResult.OK)
            {
                form1.textBox1.Text = open.FileName;
            }
        }

        #endregion
        #region 解压文件

        /// <summary>
        /// 解压文件
        /// </summary>
        /// <param name="obj"></param> 
        public void DecompressionFile(object obj, string saveDirectory)
        {
            Form1 f = obj as Form1;
            DecompressionBigFile(obj, saveDirectory);
        }
        /// <summary>
        /// 解压大文件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="saveDirectory">保存的文件目录</param>
        private void DecompressionBigFile(object obj, string saveDirectory)
        {
            bool isDeCode = false;
            try
            {
                Form1 form = obj as Form1;
                string path = form.textBox1.Text.Trim();
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                DECODE:
                    byte[] array = new byte[FileStruct.HeadBuffer];
                    fileStream.Read(array, 0, array.Length);
                    array = this.UseCode(array, isDeCode);
                    string tagName = Encoding.Default.GetString(array);

                    if (tagName == FileStruct.UseCode)
                    {
                        isDeCode = true;
                        goto DECODE;
                    }

                    if (tagName == FileStruct.TagSip || tagName == FileStruct.TagHXP)
                    {
                        int SizeLength = tagName == FileStruct.TagSip ? FileStruct.FilePathBufferSip : FileStruct.FilePathBufferHxp;
                        while (fileStream.Position < fileStream.Length)
                        {
                            byte[] array2 = new byte[FileStruct.FileSizeBuffer];
                            byte[] arrayPath = new byte[FileStruct.PathSizeBuffer];
                            byte[] array3 = new byte[SizeLength];
                            //文件大小
                            int count = fileStream.Read(array2, 0, array2.Length);
                            array2 = this.UseCode(array2, isDeCode);
                            string str = Encoding.Default.GetString(array2, 0, count);
                            int contenSize = int.Parse(str.Trim());

                            string text = "";
                            string text2 = "";
                            int pathSize = 0;
                            if (tagName != FileStruct.TagSip)
                            {
                                //路径尺寸大小
                                int length = fileStream.Read(arrayPath, 0, arrayPath.Length);
                                arrayPath = this.UseCode(arrayPath, isDeCode);
                                pathSize = int.Parse(Encoding.Default.GetString(arrayPath, 0, length).Trim());

                                //分配路径大小字节
                                array3 = new byte[pathSize];
                                //读取路径字符
                                count = fileStream.Read(array3, 0, array3.Length);
                                array3 = this.UseCode(array3, isDeCode);
                                text = Encoding.Default.GetString(array3, 0, count).Trim().Replace("?", " ").Replace("？", " ");
                                text2 = saveDirectory + "\\" + text;
                            }
                            else
                            {
                                //文件路径
                                count = fileStream.Read(array3, 0, array3.Length);
                                array3 = this.UseCode(array3, isDeCode);
                                text = Encoding.Default.GetString(array3, 0, count).Trim().Replace("?", " ").Replace("？", " ");
                                text2 = saveDirectory + "\\" + text;
                            }
                            form.listBox2.Items.Insert(0, text2);
                            form.showCurrentPath.Text = text2;
                            if (!Directory.Exists(text2))
                            {
                                this.CreateDirectory(text2);
                            }
                            List<int> count2 = this.GetCount(contenSize);
                            using (FileStream fileStream2 = new FileStream(text2, FileMode.Create, FileAccess.Write))
                            {
                                for (int i = 0; i < count2.Count; i++)
                                {
                                    byte[] array4 = new byte[count2[i]];
                                    count = fileStream.Read(array4, 0, array4.Length);
                                    array4 = this.UseCode(array4, isDeCode);
                                    fileStream2.Write(array4, 0, count);
                                    this.ShowProssbor(form.progressBar1, form.label2, (long)(i + 1), (long)count2.Count);
                                }
                            }
                            this.ShowProssbor(form.progressBar2, form.label3, fileStream.Position, fileStream.Length);
                            this.CloseForm(form.progressBar2, form.progressBar1);
                        }
                    }
                    else if (MessageBox.Show("  文件错误，Error code:246行\r\n程序正在退出...", "系统提示", MessageBoxButtons.OK) == DialogResult.OK)
                    {
                        Application.Exit();
                    }
                    else
                    {
                        Application.Exit();
                    }
                }
                form.timer2.Enabled = false;
            }
            catch (System.Exception ex)
            {

                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK);
            }
        }

        private void CreateDirectory(string realPath)
        {
            FileInfo fileInfo = new FileInfo(realPath);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }
        }

        private List<int> GetCount(int contenSize)
        {
            byte[] array = new byte[1024 * 1024];
            List<int> list = new List<int>();
            int item = contenSize % array.Length;
            for (int i = 0; i < contenSize / array.Length; i++)
            {
                list.Add(array.Length);
            }
            list.Add(item);
            return list;
        }

        private void CloseForm(ProgressBar sum, ProgressBar current)
        {
            if (sum.Value >= 1000)
            {
                if (current.Value >= 1000)
                {
                    Thread.Sleep(1000);
                    Application.ExitThread();
                    Application.Exit();

                }
            }
        }
        #endregion

        private byte[] UseCode(byte[] buffer, bool isDeCode = true)
        {
            byte[] bufferTemp = new byte[buffer.Length];
            int len = buffer.Length;
            for (int i = 0; i < len; i++)
            {
                bufferTemp[i] = (byte)(255 - buffer[i]);
            }
            return isDeCode ? bufferTemp : buffer;
        }
    }
   

  
}