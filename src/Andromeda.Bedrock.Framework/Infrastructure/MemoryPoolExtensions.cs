using System;
using System.Buffers;

namespace Andromeda.Bedrock.Framework.Infrastructure
{
    internal static class MemoryPoolExtensions
    {
        /// <summary>
        /// Computes a minimum segment size
        /// </summary>
        /// <param name="pool"></param>
        /// <returns></returns>
        public static int GetMinimumSegmentSize(this MemoryPool<byte> pool) => pool == null ? 4096 : Math.Min(4096, pool.MaxBufferSize);

        // 1/2 of a segment
        public static int GetMinimumAllocSize(this MemoryPool<byte> pool) => pool.GetMinimumSegmentSize() / 2;
    }
}
