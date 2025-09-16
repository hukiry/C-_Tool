using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
   
    public static class ByteBlockExtensions
    {
        /// <summary>
        /// 读取数据，然后递增Pos
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        
        public static int Read(this ByteBlock byteBlock, byte[] buffer) 
        {
            return byteBlock.Read(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 设置游标到末位
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns></returns>
        
        public static ByteBlock SeekToEnd(this ByteBlock byteBlock) 
        {
            byteBlock.Position = byteBlock.Length;
            return byteBlock;
        }

        /// <summary>
        /// 移动游标
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="byteBlock"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        
        public static ByteBlock Seek(this ByteBlock byteBlock, int position) 
        {
            byteBlock.Position = position;
            return byteBlock;
        }

        /// <summary>
        /// 设置游标到首位
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns></returns>
        
        public static ByteBlock SeekToStart(this ByteBlock byteBlock) 
        {
            byteBlock.Position = 0;
            return byteBlock;
        }

        /// <summary>
        /// 读取数据，然后递增Pos
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int Read(this ByteBlock byteBlock, out byte[] buffer, int length) 
        {
            buffer = new byte[length];
            return byteBlock.Read(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 转换为有效内存
        /// </summary>
        /// <returns></returns>
        public static byte[] ToArray(this ByteBlock byteBlock) 
        {
            return byteBlock.ToArray(0, byteBlock.Len);
        }

        /// <summary>
        /// 从指定位置转化到有效内存
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public static byte[] ToArray(this ByteBlock byteBlock, int offset) 
        {
            return byteBlock.ToArray(offset, byteBlock.Len - offset);
        }

        #region Int16

        /// <summary>
        /// 从当前流位置读取一个<see cref="short"/>值
        /// </summary>
        public static short ReadInt16(this ByteBlock byteBlock)
        {
            short value = HukiryBitConverter.Default.ToInt16(byteBlock.Buffer, byteBlock.Pos);
            byteBlock.Pos += 2;
            return value;
        }

        /// <summary>
        /// 写入<see cref="short"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static ByteBlock Write(this ByteBlock byteBlock, short value)
        {
            byteBlock.Write(HukiryBitConverter.Default.GetBytes(value));
            return byteBlock;
        }

        #endregion Int16

        #region Int32

        /// <summary>
        /// 从当前流位置读取一个<see cref="int"/>值
        /// </summary>
        public static int ReadInt32(this ByteBlock byteBlock)
        {
            int value = HukiryBitConverter.Default.ToInt32(byteBlock.Buffer, byteBlock.Pos);
            byteBlock.Pos += 4;
            return value;
        }

        /// <summary>
        /// 写入<see cref="int"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static ByteBlock Write(this ByteBlock byteBlock, int value) 
        {
            byteBlock.Write(HukiryBitConverter.Default.GetBytes(value));
            return byteBlock;
        }

        #endregion Int32

        #region Int64

        /// <summary>
        /// 从当前流位置读取一个<see cref="long"/>值
        /// </summary>
        public static long ReadInt64(this ByteBlock byteBlock)
        {
            long value = HukiryBitConverter.Default.ToInt64(byteBlock.Buffer, byteBlock.Pos);
            byteBlock.Pos += 8;
            return value;
        }

        /// <summary>
        /// 写入<see cref="long"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static ByteBlock Write(this ByteBlock byteBlock, long value) 
        {
            byteBlock.Write(HukiryBitConverter.Default.GetBytes(value));
            return byteBlock;
        }

        #endregion Int64

        #region UInt16

        /// <summary>
        /// 从当前流位置读取一个<see cref="ushort"/>值
        /// </summary>
        public static ushort ReadUInt16(this ByteBlock byteBlock)
        {
            ushort value = HukiryBitConverter.Default.ToUInt16(byteBlock.Buffer, byteBlock.Pos);
            byteBlock.Pos += 2;
            return value;
        }

        /// <summary>
        /// 写入<see cref="ushort"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static ByteBlock Write(this ByteBlock byteBlock, ushort value)
        {
            byteBlock.Write(HukiryBitConverter.Default.GetBytes(value));
            return byteBlock;
        }

        #endregion UInt16

        #region UInt32

        /// <summary>
        /// 从当前流位置读取一个<see cref="uint"/>值
        /// </summary>
        public static uint ReadUInt32(this ByteBlock byteBlock)
        {
            uint value = HukiryBitConverter.Default.ToUInt32(byteBlock.Buffer, byteBlock.Pos);
            byteBlock.Pos += 4;
            return value;
        }

        /// <summary>
        /// 写入<see cref="uint"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static ByteBlock Write(this ByteBlock byteBlock, uint value)
        {
            byteBlock.Write(HukiryBitConverter.Default.GetBytes(value));
            return byteBlock;
        }

        #endregion UInt32

        #region UInt64

        /// <summary>
        /// 从当前流位置读取一个<see cref="ulong"/>值
        /// </summary>
        public static ulong ReadUInt64(this ByteBlock byteBlock)
        {
            ulong value = HukiryBitConverter.Default.ToUInt64(byteBlock.Buffer, byteBlock.Pos);
            byteBlock.Pos += 8;
            return value;
        }

        /// <summary>
        /// 写入<see cref="ulong"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static ByteBlock Write(this ByteBlock byteBlock, ulong value)
        {
            byteBlock.Write(HukiryBitConverter.Default.GetBytes(value));
            return byteBlock;
        }

        #endregion UInt64

        #region Bool

        /// <summary>
        /// 从当前流位置读取一个<see cref="bool"/>值
        /// </summary>
        public static bool ReadBoolean(this ByteBlock byteBlock)
        {
            bool value = HukiryBitConverter.Default.ToBoolean(byteBlock.Buffer, byteBlock.Pos);
            byteBlock.Pos += 1;
            return value;
        }

        /// <summary>
        /// 写入<see cref="bool"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static ByteBlock Write(this ByteBlock byteBlock, bool value) 
        {
            byteBlock.Write(HukiryBitConverter.Default.GetBytes(value));
            return byteBlock;
        }

        #endregion Boolean

        #region Byte

        /// <summary>
        /// 写入<see cref="byte"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ByteBlock Write(this ByteBlock byteBlock, byte value) 
        {
            byteBlock.Write(new byte[] { value }, 0, 1);
            return byteBlock;
        }

        #endregion Byte

        #region Char

        /// <summary>
        /// 从当前流位置读取一个<see cref="char"/>值
        /// </summary>
        public static char ReadChar(this ByteBlock byteBlock)
        {
            char value = HukiryBitConverter.Default.ToChar(byteBlock.Buffer, byteBlock.Pos);
            byteBlock.Pos += 2;
            return value;
        }

        /// <summary>
        /// 写入<see cref="char"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static ByteBlock Write(this ByteBlock byteBlock, char value)
        {
            byteBlock.Write(HukiryBitConverter.Default.GetBytes(value));
            return byteBlock;
        }

        #endregion Char

        #region String

        /// <summary>
        /// 从当前流位置读取一个<see cref="string"/>值
        /// </summary>
        public static string ReadString(this ByteBlock byteBlock)
        {
            ushort len = byteBlock.ReadUInt16();
            if (len == 0) return string.Empty;
            string str = Encoding.UTF8.GetString(byteBlock.Buffer, byteBlock.Pos, len);
            byteBlock.Pos += len;
            return str;
        }


        /// <summary>
        /// 写入<see cref="string"/>值。
        /// <para>读取时必须使用ReadString</para>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static ByteBlock Write(this ByteBlock byteBlock, string value)
        {
            if (value == null) value = string.Empty;
            byte[] buffer = Encoding.UTF8.GetBytes(value);
            if (buffer.Length > ushort.MaxValue)
            {
                throw new Exception("传输长度超长");
            }
            byteBlock.Write((ushort)buffer.Length);
            byteBlock.Write(buffer);
            return byteBlock;
        }

        #endregion String

        #region Float

        /// <summary>
        /// 从当前流位置读取一个<see cref="float"/>值
        /// </summary>
        public static float ReadFloat(this ByteBlock byteBlock)
        {
            float value = HukiryBitConverter.Default.ToSingle(byteBlock.Buffer, byteBlock.Pos);
            byteBlock.Pos += 4;
            return value;
        }

        /// <summary>
        /// 写入<see cref="float"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static ByteBlock Write(this ByteBlock byteBlock, float value)
        {
            byteBlock.Write(HukiryBitConverter.Default.GetBytes(value));
            return byteBlock;
        }

        #endregion Float

        #region Double

        /// <summary>
        /// 从当前流位置读取一个<see cref="double"/>值
        /// </summary>
        public static double ReadDouble(this ByteBlock byteBlock)
        {
            double value = HukiryBitConverter.Default.ToDouble(byteBlock.Buffer, byteBlock.Pos);
            byteBlock.Pos += 8;
            return value;
        }

        /// <summary>
        /// 写入<see cref="double"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static ByteBlock Write(this ByteBlock byteBlock, double value) 
        {
            byteBlock.Write(HukiryBitConverter.Default.GetBytes(value));
            return byteBlock;
        }

        #endregion Double

        public static void ReadClassList<T>(this ByteBlock byteBlock, List<T> infoList) where T : IProto, new()
        {
            int len = byteBlock.ReadUInt16();
            for (int i = 0; i < len; i++)
            {
                T temp = new T();
                temp.Read(byteBlock);
                infoList.Add(temp);
            }
        }

        public static void WriteClassList<T>(this ByteBlock byteBlock, List<T> infoList) where T : IProto, new()
        {
            byteBlock.Write((ushort)infoList.Count);
            for (int i = 0; i < infoList.Count; i++)
            {
                infoList[i].Write(byteBlock);
            }
        }
    }

    public interface IProto
    {
        void Write(ByteBlock byteBlock);
        void Read(ByteBlock byteBlock);
    }

}
