

using System;
using System.IO;
using System.Threading;

namespace Server
{
    /// <summary>
    /// 字节块流
    /// </summary>
    public sealed class ByteBlock : Stream, IDisposable
    {
        internal long m_length;
        internal bool m_using;
        private static float m_ratio = 1.5f;
        private byte[] m_buffer;
        private bool m_holding;
        private readonly bool m_needDis;
        private long m_position;
        private int m_dis = 1;

        /// <summary>
        ///  构造函数
        /// </summary>
        /// <param name="byteSize"></param>
        /// <param name="equalSize"></param>
        public ByteBlock(int byteSize = 1024 * 64)
        {
            this.m_needDis = true;
            this.m_buffer = new byte[byteSize];
            this.m_using = true;
        }

        /// <summary>
        /// 实例化一个已知内存的对象。且该内存不会被回收。
        /// </summary>
        /// <param name="bytes"></param>
        public ByteBlock(byte[] bytes)
        {
            this.m_buffer = bytes ?? throw new ArgumentNullException(nameof(bytes));
            this.m_length = bytes.Length;
            this.m_using = true;
        }

        /// <summary>
        /// 字节实例
        /// </summary>
        public byte[] Buffer => this.m_buffer;

        /// <summary>
        /// 仅当内存块可用，且<see cref="CanReadLen"/>>0时为True。
        /// </summary>
         public override bool CanRead => this.m_using && this.CanReadLen > 0;

        /// <summary>
        /// 还能读取的长度，计算为<see cref="Len"/>与<see cref="Pos"/>的差值。
        /// </summary>
         public int CanReadLen => this.Len - this.Pos;

        /// <summary>
        /// 还能读取的长度，计算为<see cref="Len"/>与<see cref="Pos"/>的差值。
        /// </summary>
        //public long CanReadLength => this.m_length - this.m_position;

        /// <summary>
        /// 支持查找
        /// </summary>
        public override bool CanSeek => this.m_using;

        /// <summary>
        /// 可写入
        /// </summary>
         public override bool CanWrite => this.m_using;
        /// <summary>
        /// Int真实长度
        /// </summary>
        public int Len => (int)this.m_length;

        /// <summary>
        /// 真实长度
        /// </summary>
        public override long Length => this.m_length;

        /// <summary>
        /// int型流位置
        /// </summary>
        
        public int Pos
        {
            get => (int)this.m_position;
            set => this.m_position = value;
        }

        /// <summary>
        /// 流位置
        /// </summary>
        
        public override long Position
        {
            get => this.m_position;
            set => this.m_position = value;
        }
      

        private void Dis()
        {
            this.m_holding = false;
            this.m_using = false;
            this.m_position = 0;
            this.m_length = 0;
            this.m_buffer = null;
        }

        /// <summary>
        /// 清空所有内存数据
        /// </summary>
        /// <exception cref="ObjectDisposedException">内存块已释放</exception>
        
        public void Clear()
        {
            if (!this.m_using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            Array.Clear(this.m_buffer, 0, this.m_buffer.Length);
            base.Dispose(true);
        }

        /// <summary>
        /// 无实际效果
        /// </summary>
        
        public override void Flush(){}

        /// <summary>
        /// 读取数据，然后递增Pos
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        
        public override int Read(byte[] buffer, int offset, int length)
        {
            if (!this.m_using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            int len = this.m_length - this.m_position > length ? length : this.CanReadLen;
            Array.Copy(this.m_buffer, this.m_position, buffer, offset, len);
            this.m_position += len;
            return len;
        }

        /// <summary>
        /// 从当前流位置读取一个<see cref="byte"/>值
        /// </summary>
        public override int ReadByte()
        {
            byte value = this.m_buffer[this.m_position];
            this.m_position++;
            return value;
        }

        /// <summary>
        /// 将内存块初始化到刚申请的状态。
        /// <para>仅仅重置<see cref="Position"/>和<see cref="Length"/>属性。</para>
        /// </summary>
        /// <exception cref="ObjectDisposedException">内存块已释放</exception>
        
        public void Reset()
        {
            if (!this.m_using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            this.m_position = 0;
            this.m_length = 0;
        }

        /// <summary>
        /// 设置流位置
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!this.m_using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            switch (origin)
            {
                case SeekOrigin.Begin:
                    this.m_position = offset;
                    break;

                case SeekOrigin.Current:
                    this.m_position += offset;
                    break;

                case SeekOrigin.End:
                    this.m_position = this.m_length + offset;
                    break;
            }
            return this.m_position;
        }

        /// <summary>
        /// 重新设置容量
        /// </summary>
        /// <param name="size">新尺寸</param>
        /// <param name="retainedData">是否保留元数据</param>
        /// <exception cref="ObjectDisposedException"></exception>
        
        public void SetCapacity(int size, bool retainedData = false)
        {
            if (!this.m_using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            byte[] bytes = new byte[size];

            if (retainedData)
            {
                Array.Copy(this.m_buffer, 0, bytes, 0, this.m_buffer.Length);
            }
            this.m_buffer = bytes;
        }
        /// <summary>
        /// 设置实际长度
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        
        public override void SetLength(long value)
        {
            if (!this.m_using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (value > this.m_buffer.Length)
            {
                throw new Exception("设置值超出容量");
            }
            this.m_length = value;
        }

        /// <summary>
        /// 从指定位置转化到指定长度的有效内存
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        
        public byte[] ToArray(int offset, int length)
        {
            if (!this.m_using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            byte[] buffer = new byte[length];
            Array.Copy(this.m_buffer, offset, buffer, 0, buffer.Length);
            return buffer;
        }
        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count == 0)
            {
                return;
            }
            if (!this.m_using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (this.m_buffer.Length - this.m_position < count)
            {
                int need = this.m_buffer.Length + count - ((int)(this.m_buffer.Length - this.m_position));
                int lend = this.m_buffer.Length;
                while (need > lend)
                {
                    lend = (int)(lend * m_ratio);
                }
                this.SetCapacity(lend, true);
            }
            Array.Copy(buffer, offset, this.m_buffer, this.m_position, count);
            this.m_position += count;
            this.m_length = Math.Max(this.m_position, this.m_length);
        }
        
        public void Write(byte[] buffer)
        {
            this.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        
        protected sealed override void Dispose(bool disposing)
        {
            if (this.m_holding)
            {
                return;
            }

            if (this.m_needDis)
            {
                if (Interlocked.Decrement(ref this.m_dis) == 0)
                {
                    GC.SuppressFinalize(this);
                    this.Dis();
                }
            }

            base.Dispose(disposing);
        }
    }
}