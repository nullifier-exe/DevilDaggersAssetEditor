﻿using System.Collections.Generic;
using System.IO;

namespace DevilDaggersAssetCore.Chunks
{
	public abstract class AbstractResourceChunk : AbstractChunk
	{
		public uint Unknown { get; set; }

		protected AbstractResourceChunk(string name, uint startOffset, uint size, uint unknown)
			: base(name, startOffset, size)
		{
			Unknown = unknown;
		}

		// Only overridden by AbstractHeaderedChunk to take header into account.
		public virtual void SetBuffer(byte[] buffer) => Buffer = buffer;

		// Only overridden by AbstractHeaderedChunk to take header into account.
		public virtual byte[] GetBuffer() => Buffer;

		public virtual void Compress(string path)
		{
			Buffer = File.ReadAllBytes(path);
			Size = (uint)Buffer.Length;
		}

		public virtual IEnumerable<FileResult> Extract()
		{
			yield return new FileResult(Name, Buffer);
		}

		public override string ToString() => $"Type: {GetType().Name} | Name: {Name} | Size: {Size}";
	}
}