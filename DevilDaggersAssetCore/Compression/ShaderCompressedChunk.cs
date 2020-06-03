using DevilDaggersAssetCore.Chunks;
using DevilDaggersAssetCore.Headers;
using DevilDaggersAssetCore.Info;
using System;
using Buf = System.Buffer;

namespace DevilDaggersAssetCore.Compression
{
	public class ShaderCompressedChunk : CompressedChunk
	{
		public ShaderCompressedChunk(byte type, string name, uint size)
			: base(type, name, size)
		{
		}

		// TODO: Use C# 9 covariant returns.
		public override AbstractResourceChunk Extract()
		{
			ShaderChunk modelChunk = new ShaderChunk(Name, 0U, 0U, 0U)
			{
				Buffer = Buffer
			};

			int nameLength = Name.Length;
			int vertexSize = 0; // TODO
			int fragmentSize = 0; // TODO

			byte[] headerBuffer = new byte[ChunkInfo.Shader.HeaderInfo.FixedSize.Value];
			Buf.BlockCopy(BitConverter.GetBytes(nameLength), 0, headerBuffer, 0, sizeof(uint));
			Buf.BlockCopy(BitConverter.GetBytes(vertexSize), 0, headerBuffer, 4, sizeof(uint));
			Buf.BlockCopy(BitConverter.GetBytes(fragmentSize), 0, headerBuffer, 8, sizeof(uint));

			modelChunk.Header = new ShaderHeader(headerBuffer);

			return modelChunk;
		}
	}
}