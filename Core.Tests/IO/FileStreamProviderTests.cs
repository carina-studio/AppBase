using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace CarinaStudio.IO
{
	/// <summary>
	/// Tests of <see cref="FileStreamProvider"/>.
	/// </summary>
	[TestFixture]
	class FileStreamProviderTests : BaseStreamProviderTests
	{
		// Fields.
		readonly List<string> tempFilePaths = new();


		/// <summary>
		/// Clear all generated temporary files.
		/// </summary>
		[OneTimeTearDown]
		public void ClearTempFiles()
		{
			lock (this.tempFilePaths)
			{
				foreach (var filePath in this.tempFilePaths)
					Global.RunWithoutError(() => System.IO.File.Delete(filePath));
				this.tempFilePaths.Clear();
			}
		}


		// Create instance.
		protected override IStreamProvider CreateInstance(byte[] data)
		{
			var filePath = Path.GetTempFileName();
			lock (this.tempFilePaths)
				this.tempFilePaths.Add(filePath);
			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Write))
				stream.Write(data);
			return new FileStreamProvider(filePath);
		}


		// Get written data.
		protected override byte[] GetWrittenData(IStreamProvider provider)
		{
			using var stream = new FileStream(((FileStreamProvider)provider).FileName, FileMode.Open, FileAccess.Read);
			return stream.ReadAllBytes();
		}
	}
}
