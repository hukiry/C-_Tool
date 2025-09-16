
using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Hukiry.ByteManager
{
    /// <summary>
    /// 字节块集合
    /// </summary>
    [DebuggerDisplay("Count = {bytesQueue.Count}")]
    internal class BytesQueue
    {
        internal int size;

        internal BytesQueue(int size)
        {
            this.size = size;
        }

        /// <summary>
        /// 占用空间
        /// </summary>
        public long FullSize => this.size * this.bytesQueue.Count;

        private readonly ConcurrentQueue<byte[]> bytesQueue = new ConcurrentQueue<byte[]>();

        internal long referenced;

        /// <summary>
        /// 获取当前实例中的空闲的Block
        /// </summary>
        /// <returns></returns>
        public bool TryGet(out byte[] bytes)
        {
            this.referenced++;
            return this.bytesQueue.TryDequeue(out bytes);
        }

        /// <summary>
        /// 向当前集合添加Block
        /// </summary>
        /// <param name="bytes"></param>
        public void Add(byte[] bytes)
        {
            this.bytesQueue.Enqueue(bytes);
        }

        internal void Clear()
        {
            for (int i = 0; i < bytesQueue.Count; i++)
            {
                bytesQueue.TryDequeue(out byte[] bytes);
            }
        }
    }
}