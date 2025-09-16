
using System;

namespace Hukiry.ByteManager
{
    public interface IProto
    {
        void Write(ByteBlock byteBlock);
        void Read(ByteBlock byteBlock);
    }

    /// <summary>
    /// 字节块流
    /// </summary>
    public interface IByteBlock : IWrite, IDisposable
    {
        /// <summary>
        /// 字节实例
        /// </summary>
        byte[] Buffer { get; }

        /// <summary>
        /// 仅当内存块可用，且<see cref="CanReadLen"/>>0时为True。
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        /// 剩余的长度，准确掌握该值，可以避免内存扩展，计算为<see cref="Capacity"/>与<see cref="Pos"/>的差值。
        /// </summary>
        int FreeLength { get; }

        /// <summary>
        /// 还能读取的长度，计算为<see cref="Len"/>与<see cref="Pos"/>的差值。
        /// </summary>
        int CanReadLen { get; }

        /// <summary>
        /// 还能读取的长度，计算为<see cref="Len"/>与<see cref="Pos"/>的差值。
        /// </summary>
        long CanReadLength { get; }

        /// <summary>
        /// 容量
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// 表示持续性持有，为True时，Dispose将调用无效。
        /// </summary>
        bool Holding { get; }

        /// <summary>
        /// Int真实长度
        /// </summary>
        int Len { get; }

        /// <summary>
        /// 真实长度
        /// </summary>
        long Length { get; }

        /// <summary>
        /// int型流位置
        /// </summary>
        int Pos { get; set; }

        /// <summary>
        /// 流位置
        /// </summary>
        long Position { get; set; }

        /// <summary>
        /// 使用状态
        /// </summary>
        bool Using { get; }

        /// <summary>
        /// 直接完全释放，游离该对象，然后等待GC
        /// </summary>
        void AbsoluteDispose();

        /// <summary>
        /// 清空所有内存数据
        /// </summary>
        /// <exception cref="ObjectDisposedException">内存块已释放</exception>
        void Clear();

        /// <summary>
        /// 将内存块初始化到刚申请的状态。
        /// <para>仅仅重置<see cref="Position"/>和<see cref="Length"/>属性。</para>
        /// </summary>
        /// <exception cref="ObjectDisposedException">内存块已释放</exception>
        void Reset();

        /// <summary>
        /// 读取数据，然后递增Pos
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        int Read(byte[] buffer, int offset, int length);

        /// <summary>
        /// 读取一个字节
        /// </summary>
        /// <returns></returns>
        int ReadByte();
        /// <summary>
        /// 写入多个字节
        /// </summary>
        /// <returns></returns>
        void Write(byte[] buffer);

        /// <summary>
        /// 重新设置容量
        /// </summary>
        /// <param name="size">新尺寸</param>
        /// <param name="retainedData">是否保留元数据</param>
        /// <exception cref="ObjectDisposedException"></exception>
        void SetCapacity(int size, bool retainedData = false);

        /// <summary>
        /// 设置持续持有属性，当为True时，调用Dispose会失效，表示该对象将长期持有，直至设置为False。
        /// 当为False时，会自动调用Dispose。
        /// </summary>
        /// <param name="holding"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        void SetHolding(bool holding);

        /// <summary>
        /// 设置实际长度
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        void SetLength(long value);

        /// <summary>
        /// 从指定位置转化到指定长度的有效内存
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        byte[] ToArray(int offset, int length);
    }
}