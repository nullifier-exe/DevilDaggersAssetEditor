using DevilDaggersAssetCore.Chunks;
using DevilDaggersAssetCore.Headers;
using DevilDaggersAssetCore.Info;
using System;
using Buf = System.Buffer;

namespace DevilDaggersAssetCore.Compression
{
	public class ModelCompressedChunk : CompressedChunk
	{
		public ModelCompressedChunk(byte type, string name, uint size)
			: base(type, name, size)
		{
		}

		// TODO: Use C# 9 covariant returns.
		public override AbstractResourceChunk Extract()
		{
			ModelChunk modelChunk = new ModelChunk(Name, 0U, 0U, 0U)
			{
				Buffer = Buffer
			};

			int vertexCount = 0; // TODO

			byte[] headerBuffer = new byte[ChunkInfo.Model.HeaderInfo.FixedSize.Value];
			Buf.BlockCopy(BitConverter.GetBytes((uint)vertexCount), 0, headerBuffer, 0, sizeof(uint));
			Buf.BlockCopy(BitConverter.GetBytes((uint)vertexCount), 0, headerBuffer, 4, sizeof(uint));
			Buf.BlockCopy(BitConverter.GetBytes((ushort)288), 0, headerBuffer, 8, sizeof(ushort));

			modelChunk.Header = new ModelHeader(headerBuffer);

			return modelChunk;
		}
	}
}