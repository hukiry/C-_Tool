
using System;
using System.Runtime.CompilerServices;

namespace Server
{
    /// <summary>
    /// 大小端类型
    /// </summary>
    public enum EndianType
    {
        /// <summary>
        /// 小端模式
        /// </summary>
        Little,

        /// <summary>
        /// 大端模式
        /// </summary>
        Big
    }

    /// <summary>
    /// 将基数据类型转换为指定端的一个字节数组，
    /// 或将一个字节数组转换为指定端基数据类型。
    /// </summary>
    public class HukiryBitConverter
    {
        /// <summary>
        /// 以大端
        /// </summary>
        public static HukiryBitConverter BigEndian;

        /// <summary>
        /// 以小端
        /// </summary>
        public static HukiryBitConverter LittleEndian;

        static HukiryBitConverter()
        {
            BigEndian = new HukiryBitConverter(EndianType.Big);
            LittleEndian = new HukiryBitConverter(EndianType.Little);
            DefaultEndianType = EndianType.Little;
        }

        /// <summary>
        /// 以默认小端，可通过<see cref="TouchSocketBitConverter.DefaultEndianType"/>重新指定默认端。
        /// </summary>
        public static HukiryBitConverter Default { get; private set; }

        private static EndianType m_defaultEndianType;

        /// <summary>
        /// 默认大小端切换。
        /// </summary>
        public static EndianType DefaultEndianType
        {
            get => m_defaultEndianType;
            set
            {
                m_defaultEndianType = value;
                switch (value)
                {
                    case EndianType.Little:
                        Default = LittleEndian;
                        break;

                    case EndianType.Big:
                        Default = BigEndian;
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="endianType"></param>
        public HukiryBitConverter(EndianType endianType)
        {
            this.EndianType = endianType;
        }

        /// <summary>
        /// 指定大小端。
        /// </summary>
        public EndianType EndianType { get; private set; }

        /// <summary>
        /// 判断当前系统是否为设置的大小端
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSameOfSet()
        {
            return !(HukiryBit.IsLittleEndian ^ (this.EndianType == EndianType.Little));
            //return true;
        }

        #region ushort

        /// <summary>
        /// 转换为指定端2字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(ushort value)
        {
            var bytes = HukiryBit.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }


        /// <summary>
        /// 转换为指定端模式的2字节转换为UInt16数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public ushort ToUInt16(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return HukiryBit.ToUInt16(buffer, offset);
            }
            else
            {
                var bytes = new byte[2];
                Array.Copy(buffer, offset, bytes, 0, 2);
                Array.Reverse(bytes);
                return HukiryBit.ToUInt16(bytes, 0);
            }
        }

        #endregion ushort

        #region ulong

        /// <summary>
        /// 转换为指定端8字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(ulong value)
        {
            var bytes = HukiryBit.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        ///  转换为指定端模式的Ulong数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public ulong ToUInt64(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return HukiryBit.ToUInt64(buffer, offset);
            }
            else
            {
                var bytes = new byte[8];
                Array.Copy(buffer, offset, bytes, 0, 8);
                Array.Reverse(bytes);
                return HukiryBit.ToUInt64(bytes, 0);
            }

        }

        #endregion ulong

        #region bool

        /// <summary>
        /// 转换为指定端1字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(bool value)
        {
            return HukiryBit.GetBytes(value);
        }

        /// <summary>
        ///  转换为指定端模式的bool数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public bool ToBoolean(byte[] buffer, int offset)
        {
            return HukiryBit.ToBoolean(buffer, offset);
        }

        #endregion bool

        #region char

        /// <summary>
        /// 转换为指定端2字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(char value)
        {
            var bytes = HukiryBit.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        ///  转换为指定端模式的Char数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public char ToChar(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return HukiryBit.ToChar(buffer, offset);
            }
            else
            {
                var bytes = new byte[2];
                Array.Copy(buffer, offset, bytes, 0, bytes.Length);
                Array.Reverse(bytes);
                return HukiryBit.ToChar(bytes, 0);
            }
        }

        #endregion char

        #region short

        /// <summary>
        /// 转换为指定端2字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(short value)
        {
            var bytes = HukiryBit.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        ///  转换为指定端模式的Short数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public short ToInt16(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return HukiryBit.ToInt16(buffer, offset);
            }
            else
            {
                var bytes = new byte[2];
                Array.Copy(buffer, offset, bytes, 0, bytes.Length);
                Array.Reverse(bytes);
                return HukiryBit.ToInt16(bytes, 0);
            }

        }

        #endregion short

        #region int

        /// <summary>
        /// 转换为指定端4字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(int value)
        {
            var bytes = HukiryBit.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        ///  转换为指定端模式的int数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int ToInt32(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return HukiryBit.ToInt32(buffer, offset);
            }
            else
            {
                var bytes = new byte[4];
                Array.Copy(buffer, offset, bytes, 0, bytes.Length);
                Array.Reverse(bytes);
                return HukiryBit.ToInt32(bytes, 0);
            }
        }

        #endregion int

        #region long

        /// <summary>
        /// 转换为指定端8字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(long value)
        {
            var bytes = HukiryBit.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        ///  转换为指定端模式的long数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public long ToInt64(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return HukiryBit.ToInt64(buffer, offset);
            }
            else
            {
                var bytes = new byte[8];
                Array.Copy(buffer, offset, bytes, 0, bytes.Length);
                Array.Reverse(bytes);
                return HukiryBit.ToInt64(bytes, 0);
            }

        }

        #endregion long

        #region uint

        /// <summary>
        /// 转换为指定端4字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(uint value)
        {
            var bytes = HukiryBit.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        ///  转换为指定端模式的Uint数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public uint ToUInt32(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return HukiryBit.ToUInt32(buffer, offset);
            }
            else
            {
                var bytes = new byte[4];
                Array.Copy(buffer, offset, bytes, 0, bytes.Length);
                Array.Reverse(bytes);
                return HukiryBit.ToUInt32(bytes, 0);
            }
        }

        #endregion uint

        #region float

        /// <summary>
        /// 转换为指定端4字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(float value)
        {
            var bytes = HukiryBit.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        ///  转换为指定端模式的float数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public float ToSingle(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return HukiryBit.ToSingle(buffer, offset);
            }
            else
            {
                var bytes = new byte[4];
                Array.Copy(buffer, offset, bytes, 0, bytes.Length);
                Array.Reverse(bytes);
                return HukiryBit.ToSingle(bytes, 0);
            }
        }

        #endregion float

        #region double

        /// <summary>
        /// 转换为指定端8字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(double value)
        {
            var bytes = HukiryBit.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        ///  转换为指定端模式的double数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public double ToDouble(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return HukiryBit.ToDouble(buffer, offset);
            }
            else
            {
                var bytes = new byte[8];
                Array.Copy(buffer, offset, bytes, 0, bytes.Length);
                Array.Reverse(bytes);
                return HukiryBit.ToDouble(bytes, 0);
            }
        }

        #endregion long
    }
}