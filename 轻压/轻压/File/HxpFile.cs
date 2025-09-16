using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Filejieyasuo
{
    [CompilationRelaxations(1)]
    [CompilerGenerated()]
    [CompilerGlobalScope()]
    class HxpFile
    {
       
        public static HxpFile ins { get; } = new HxpFile();

        /// <summary>
        /// 密文：最大不能超过8个字符
        /// </summary>
        private string codeStr = "hukiry";
        public const string tempPng = "_temp.png";

        private const string FLAG = "HXP";
        private bool isDir = false;
       
        private Dictionary<string, string> fileList = new Dictionary<string, string>();

        private string compressDirName = null;
        public void AddCompressionFile(string fileFullPath, string dirName, bool isDir = true)
        {
            if (Path.GetFileName(fileFullPath).Equals(tempPng)) return;

            this.isDir = isDir;
            int num = fileFullPath.IndexOf("\\" + dirName);
            if(compressDirName == null)
                compressDirName = fileFullPath.Substring(0, num+dirName.Length+1);
            string lastPath = string.Empty;
            if (isDir)
            {
                lastPath = fileFullPath.Substring(num + 1);
            }
            else
            {
                lastPath = fileFullPath.Substring(num + 2 + dirName.Length);
            }

            lastPath= lastPath.Replace(':', '_').Replace('*', '_')
                .Replace('?', '_').Replace('"', '_')
                .Replace('<', '_').Replace('>', '_').Replace('|', '_');
            this.fileList[fileFullPath] = lastPath;
        }

        public void Clear() {
            this.fileList.Clear();
        }

        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="form"></param>
        /// <param name="saveFile">保存的压缩文件</param>
        /// <param name="dirName">压缩的目录</param>
        /// <param name="customCode"></param>
        public void Compression(Form1 form, string saveFile,string dirName, string customCode = null)
        {
            form.LoadTempPng(compressDirName);
            int num = 0;
            using (FileStream fileStream = new FileStream(saveFile, FileMode.Create, FileAccess.Write))
            {
                
                //写入头：不加密
                byte[] array_code = this.StringToBytes(FLAG);
                fileStream.Write(array_code, 0, array_code.Length);

                form.WirteImage(fileStream);

                array_code = new byte[1] { 0 };
                //写入密码标识符
                if (!string.IsNullOrEmpty(customCode))
                {
                    array_code[0] = 1;
                    codeStr = customCode;
                }
                fileStream.Write(array_code, 0, array_code.Length);

                byte[] codeBuffer = this.StringToBytes(codeStr);
                //写入密码大小
                array_code = BitConverter.GetBytes((ushort)codeBuffer.Length);
                fileStream.Write(array_code, 0, array_code.Length);
                //写入密码
                array_code = this.SerializableCode(codeBuffer, true);
                fileStream.Write(array_code, 0, array_code.Length);
                int itemCount = this.fileList.Keys.Count;
                foreach (string fileName in this.fileList.Keys)
                {
                    FileInfo fileInfo = new FileInfo(fileName);
                    form.showPath.Text = fileInfo.FullName;
                    if (!fileInfo.Exists)
                    {
                        num++; 
                        continue;
                    }

                    //读取文件
                    using (FileStream fileStream2 = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
                    {
                        string shortFilePath = this.isDir? this.fileList[fileInfo.FullName]:Path.Combine(dirName, this.fileList[fileInfo.FullName]);
                        byte[] pathBuffer = this.StringToBytes(shortFilePath);
                        //写入路径尺寸
                        array_code = BitConverter.GetBytes((ushort)pathBuffer.Length);
                        fileStream.Write(array_code, 0, array_code.Length);

                        if (fileStream2.Length > long.MaxValue)
                        {
                            form.timer2.Enabled = false;
                            if (MessageBox.Show(form, "此文件超过4G，是否停止压缩！", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.OK)
                            {
                                return;
                            }
                        }
                        //写入文件大小
                        array_code = BitConverter.GetBytes((long)fileStream2.Length);
                        fileStream.Write(array_code, 0, array_code.Length);

                        //写入路径
                        array_code = this.SerializableCode(pathBuffer);
                        fileStream.Write(array_code, 0, array_code.Length);
                        //每次最大读1M
                        array_code = new byte[this.GetWriteReadSize(fileStream2.Length, true)];
                        form.ShowProssbor(1, fileStream2.Position, fileStream2.Length);
                        //写入内容
                        do
                        {
                            int count = fileStream2.Read(array_code, 0, array_code.Length);
                            if (count > 0)
                            {
                                array_code = this.SerializableCode(array_code);
                                fileStream.Write(array_code, 0, count);
                                form.ShowProssbor(1, fileStream2.Position, fileStream2.Length);

                                float delayValue = fileStream2.Position / (float)fileStream2.Length;
                                form.ShowProssbor(2, num + delayValue, itemCount);
                            }
                        } while (fileStream2.Position < fileStream2.Length);
                    }
                    num++;
                    form.ShowProssbor(2, num, itemCount);
                    form.CloseForm(1);
                }
            }
            form.timer1.Enabled = false;
        }

        /// <summary>
        /// 解压
        /// </summary>
        public void Decompression(Form1 form, string saveDirectory, string customCode, Action<bool> passwodError = null)
        {
            string path = form.textBox1.Text.Trim();

            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                //读取头
                byte[] array = new byte[FLAG.Length];
                fileStream.Read(array, 0, array.Length);
                string tagName = this.BytesToString(array);
                form.ReadImage(fileStream);

                if (tagName.Equals(FLAG))
                {
                    //读取密码文件
                    array = new byte[1];
                    fileStream.Read(array, 0, array.Length);
                    bool isHas = array[0] == 1;
                    if (isHas) codeStr = customCode;

                    //读取密码文件
                    array = new byte[2];
                    fileStream.Read(array, 0, array.Length);
                    int codeLen = BitConverter.ToUInt16(array, 0);

                    array = new byte[codeLen];
                    fileStream.Read(array, 0, array.Length);
                    string addCode = this.BytesToString(this.SerializableCode(array, true));
                    //如果等于默认密码继续，如果不
                    if (addCode != codeStr)
                    {
                        MessageBox.Show(form, "您输入的密码有误，请重新输入", "系统提示", MessageBoxButtons.OK);
                        passwodError?.Invoke(true);
                        return;
                    }
                    passwodError?.Invoke(false);
                    Thread.Sleep(300);

                    while (fileStream.Position < fileStream.Length)
                    {

                        //读取路径大小
                        array = new byte[2];
                        fileStream.Read(array, 0, array.Length);
                        int pathSize = BitConverter.ToUInt16(array, 0);
                        //读取文件大小
                        array = new byte[8];
                        fileStream.Read(array, 0, array.Length);
                        long fileSize = BitConverter.ToInt64(array, 0);

                        //读取路径
                        array = new byte[pathSize];
                        fileStream.Read(array, 0, array.Length);
                        string readPath = this.BytesToString(this.SerializableCode(array));
                        //写入完整路径
                        string writeFilePath = Path.Combine(saveDirectory, readPath);

                        //创建文件目录
                        form.listBox2.Items.Insert(0, readPath);
                        form.showCurrentPath.Text = writeFilePath;
                        this.CreateDirectory(writeFilePath);

                        form.SaveTempPng(Path.Combine(saveDirectory ,readPath.Substring(0,readPath.IndexOf('\\'))));

                        List<int> arrayList = GetReadSizeArray(fileSize);
                        int arraryCount = arrayList.Count;
                        try
                        {
                            using (FileStream fileStream2 = new FileStream(writeFilePath, FileMode.Create, FileAccess.Write))
                            {
                                int index = 0;
                                if (arraryCount > 0)
                                {
                                    while (index < arraryCount)
                                    {
                                        array = new byte[arrayList[index]];
                                        int count = fileStream.Read(array, 0, array.Length);
                                        fileStream2.Write(this.SerializableCode(array), 0, count);
                                        index++;
                                        form.ShowProssbor(11, index, arraryCount);
                                        form.ShowProssbor(22, fileStream.Position, fileStream.Length);
                                    }
                                }
                                else
                                {
                                    index++;
                                    form.ShowProssbor(11, index, arraryCount);
                                }
                            }
                            form.ShowProssbor(22, fileStream.Position, fileStream.Length);
                            form.CloseForm(2);
                        }
                        catch
                        {

                            if (MessageBox.Show("解压错误，是否继续解压！", "系统错误", MessageBoxButtons.YesNo) == DialogResult.OK)
                            {
                                int index = 0;
                                while (index < arraryCount)
                                {
                                    array = new byte[arrayList[index]];
                                    int count = fileStream.Read(array, 0, array.Length);
                                    index++;
                                    form.ShowProssbor(11, index, arraryCount);
                                    form.ShowProssbor(22, fileStream.Position, fileStream.Length);
                                }
                            }
                            form.ShowProssbor(22, fileStream.Position, fileStream.Length);
                            form.CloseForm(2);
                        }
                    }
                }
                else
                {
                    passwodError?.Invoke(false);
                    FileManager.Instance.DecompressionFile(form, saveDirectory);
                }

            }
            form.timer2.Enabled = false;

        }

        private int GetWriteReadSize(long size, bool isCompression)
        {
            //每次最大读1M
            int curSize = 1024 * 1024;
            if (size > curSize * 1000)//大于1000M
            {
                curSize = isCompression ? curSize * 6 : curSize * 7;
            }
            else if (size > curSize * 500)//大于500M
            {
                curSize = curSize * 4;
            }
            else if (size > curSize * 100)//大于100M
            {
                curSize = curSize * 2;
            }
            return curSize;
        }

        //记录每次读取的数量
        private List<int> GetReadSizeArray(long size)
        {
            List<int> temp = new List<int>();
            //每次最大读1M
            int len = this.GetWriteReadSize(size, false);
            int count = (int)(size / len);
            int lastSize = (int)(size % len);

            for (int i = 0; i < count; i++)
                temp.Add(len);
            
            if (lastSize > 0)
                temp.Add(lastSize);

            return temp;
        }
        
        private void CreateDirectory(string realPath)
        {
            FileInfo fileInfo = new FileInfo(realPath);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }
        }

        public string BytesToString(byte[] buffer)
        {
            return Encoding.Default.GetString(buffer);
        }

        private byte[] StringToBytes(string tag)
        {
            return Encoding.Default.GetBytes(tag);
        }

        private byte[] SerializableCode(byte[] buffer, bool isCode = false)
        {
            if (string.IsNullOrEmpty(this.codeStr)) return buffer;
            byte[] bufferTemp = new byte[buffer.Length];
            int len = buffer.Length;
            int codeLen = this.codeStr.Length;
            int index = 0;
            for (int i = 0; i < len; i++)
            {
                byte result = buffer[i];
                if (i % 2 == 0 || isCode)
                {
                    byte b = (byte)this.codeStr[index % codeLen];
                    result ^= b;
                    index++;
                }
                bufferTemp[i] = result;
            }
            return bufferTemp;

        }
    }
}
