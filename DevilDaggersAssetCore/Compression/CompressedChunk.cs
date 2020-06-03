using DevilDaggersAssetCore.Chunks;
using DevilDaggersAssetCore.Info;
using System;
using System.Linq;

namespace DevilDaggersAssetCore.Compression
{
	public class CompressedChunk
	{
		public byte Type { get; }
		public string Name { get; }
		public uint Size { get; }

		public byte[] Buffer { get; set; }

		public CompressedChunk(byte type, string name, uint size)
		{
			Type = type;
			Name = name;
			Size = size;
		}

		public virtual void Compress(AbstractResourceChunk chunk) => Buffer = chunk.Buffer;

		public virtual AbstractResourceChunk Extract()
		{
			ChunkInfo chunkInfo = ChunkInfo.All.FirstOrDefault(c => c.BinaryTypes[0] == Type);

			AbstractResourceChunk chunk = (AbstractResourceChunk)Activator.CreateInstance(chunkInfo.ChunkType, Name, 0U/*Don't know start offset yet.*/, 0U/*Don't know size yet.*/, 0U);

			chunk.Buffer = Buffer;
			chunk.Size = (uint)Buffer.Length;
			return chunk;
		}
	}
}