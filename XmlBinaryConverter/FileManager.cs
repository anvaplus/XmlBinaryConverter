using System;
using System.IO;

namespace XmlBinaryConverter
{
	internal class FileManager
	{
		public static string StandardPath()
		{
			var result = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

			try
			{
				result = Path.Combine(result, "XmlBinaryConverter");
			}
			catch (Exception)
			{
				// In case XmlBinaryConverter does not exists fall back to MyDocuments
			}

			// check/create standard documents directory
			if (!Directory.Exists(result))
			{
				Directory.CreateDirectory(result);
			}

			return result;
		}
	}
}
