﻿using DevilDaggersAssetCore.BinaryFileHandlers;
using DevilDaggersAssetCore.Chunks;
using DevilDaggersAssetCore.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DevilDaggersAssetCore.Compression
{
	public static class Compressor
	{
		private static readonly byte ddIdentifier = 66;

		public static byte[] Compress(string inputPath)
		{
			byte[] sourceFileBytes = File.ReadAllBytes(inputPath);

			ResourceFileHandler handler = new ResourceFileHandler(BinaryFileType.Dd);
			handler.ValidateFile(sourceFileBytes);
			byte[] tocBuffer = handler.ReadTocBuffer(sourceFileBytes);
			List<AbstractChunk> chunks = handler.ReadChunks(tocBuffer).Where(c => c.Size != 0).ToList(); // Filter empty chunks (garbage in core file TOC buffer).
			foreach (AbstractResourceChunk chunk in chunks)
			{
				byte[] buf = new byte[chunk.Size];
				Buffer.BlockCopy(sourceFileBytes, (int)chunk.StartOffset, buf, 0, (int)chunk.Size);
				chunk.SetBuffer(buf);
			}

			List<CompressedChunk> compressedChunks = new List<CompressedChunk>();
			foreach (AbstractResourceChunk chunk in chunks)
			{
				ChunkInfo chunkInfo = ChunkInfo.All.FirstOrDefault(c => c.ChunkType == chunk.GetType());
				CompressedChunk compressedChunk = (CompressedChunk)Activator.CreateInstance(chunkInfo.CompressedType, (byte)chunkInfo.BinaryTypes[0], chunk.Name, (uint)chunk.Buffer.Length);
				compressedChunk.Compress(chunk);
				compressedChunks.Add(compressedChunk);
			}

			using MemoryStream tocStream = new MemoryStream();
			using MemoryStream dataStream = new MemoryStream();
			foreach (CompressedChunk compressedChunk in compressedChunks)
			{
				tocStream.Write(Encoding.Default.GetBytes($"{compressedChunk.Name}\0"), 0, compressedChunk.Name.Length + 1); // + 1 to include null terminator.
				tocStream.Write(new[] { compressedChunk.Type }, 0, sizeof(byte));
				tocStream.Write(BitConverter.GetBytes(compressedChunk.Buffer.Length), 0, sizeof(uint));

				dataStream.Write(compressedChunk.Buffer, 0, compressedChunk.Buffer.Length);
			}

			byte[] header = new byte[3];
			Buffer.BlockCopy(new byte[1] { ddIdentifier }, 0, header, 0, 1);
			Buffer.BlockCopy(BitConverter.GetBytes((ushort)tocStream.Length), 0, header, 1, sizeof(ushort));

			using MemoryStream finalStream = new MemoryStream();
			finalStream.Write(header, 0, header.Length);
			finalStream.Write(tocStream.ToArray(), 0, (int)tocStream.Length);
			finalStream.Write(dataStream.ToArray(), 0, (int)dataStream.Length);

			return finalStream.ToArray();
		}

		public static byte[] Extract(string inputPath)
		{
			byte[] sourceFileBytes = File.ReadAllBytes(inputPath);

			if (sourceFileBytes.Length == 0)
				throw new Exception("Empty file.");
			if (sourceFileBytes[0] != ddIdentifier)
				throw new Exception($"Invalid file format. The magic number value is incorrect:\n\nHeader value 1: {sourceFileBytes[0]} should be {ddIdentifier}");

			ushort cddTocLength = BitConverter.ToUInt16(sourceFileBytes, 1);
			byte[] cddTocBuffer = new byte[cddTocLength];
			Buffer.BlockCopy(sourceFileBytes, 3, cddTocBuffer, 0, cddTocLength);

			List<CompressedChunk> compressedChunks = ReadChunks(cddTocBuffer);

			int cddDataStart = 3 + cddTocLength;
			List<AbstractResourceChunk> chunks = new List<AbstractResourceChunk>();
			foreach (CompressedChunk compressedChunk in compressedChunks)
			{
				compressedChunk.Buffer = new byte[compressedChunk.Size];
				Buffer.BlockCopy(sourceFileBytes, cddDataStart, compressedChunk.Buffer, 0, (int)compressedChunk.Size);
				AbstractResourceChunk chunk = compressedChunk.Extract();
				chunks.Add(chunk);
			}

			ResourceFileHandler handler = new ResourceFileHandler(BinaryFileType.Dd);
			handler.CreateTocStream(chunks, out byte[] tocBuffer, out Dictionary<AbstractResourceChunk, long> startOffsetBytePositions);
			byte[] assetBuffer = handler.CreateAssetStream(BinaryFileType.Dd.ToString().ToLower(), chunks, tocBuffer, startOffsetBytePositions);
			return handler.CreateBinary(tocBuffer, assetBuffer);
		}

		private static List<CompressedChunk> ReadChunks(byte[] tocBuffer)
		{
			List<CompressedChunk> compressedChunks = new List<CompressedChunk>();

			int i = 0;
			while (i < tocBuffer.Length - 5) // TODO: Might still get out of range maybe... (5 bytes per chunk, but name length is variable)
			{
				string name = Utils.ReadNullTerminatedString(tocBuffer, i);
				i += name.Length + 1; // + 1 to include null terminator.

				byte type = tocBuffer[i];
				i += sizeof(byte);

				uint size = BitConverter.ToUInt32(tocBuffer, i);
				i += sizeof(uint);

				// TODO: Use Activator.CreateInstance.
				CompressedChunk compressedChunk;
				if (type == ChunkInfo.Texture.BinaryTypes[0])
					compressedChunk = new TextureCompressedChunk(type, name, size);
				else if (type == ChunkInfo.Model.BinaryTypes[0])
					compressedChunk = new ModelCompressedChunk(type, name, size);
				else if (type == ChunkInfo.Shader.BinaryTypes[0])
					compressedChunk = new ShaderCompressedChunk(type, name, size);
				// TODO: Implement class hierarchy distinction between CompressedChunk and a new AbstractCompressedChunk.
				else
					compressedChunk = new CompressedChunk(type, name, size);

				compressedChunks.Add(compressedChunk);
			}

			return compressedChunks;
		}
	}
}