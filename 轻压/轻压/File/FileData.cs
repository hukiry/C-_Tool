using System;
using System.Text;
using System.Windows.Forms;

namespace Filejieyasuo
{
    class FileData : SingletonClass<FileData>
    {
        public static byte[] FileHead(string tag)
        {
            return Encoding.Default.GetBytes(tag);
        }
        public byte[] ComputerSize(string str, int length)
        {
            byte[] result;
            byte[] bytes = Encoding.Default.GetBytes(str);
            if (bytes.Length <= length)
            {
                byte[] array = new byte[length];
                bytes.CopyTo(array, 0);
                for (int i = bytes.Length; i < array.Length; i++)
                {
                    array[i] = Encoding.Default.GetBytes(new char[]
                    {
                        ' '
                    })[0];
                }
                result = array;
            }
            else
            {
                byte[] array = new byte[length];
                if (MessageBox.Show("解压失败原因：字符串路径不能超过256个！是否要退出？") == DialogResult.OK)
                {
                    Application.ExitThread();
                }
                else
                {
                    Array.Copy(bytes, 0, array, 0, length);
                }
                return array;
            }
            return result;
        }

    }
}
