using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace XmlBinaryConverter.Interfaces
{
	internal interface ISchemaCompiler
	{
		XmlNamespaceManager ExtendNamespaceManager(XmlDocument xmlDoc);

		// Get the header file for c/c++ environment
		List<string> GetHeaderFile();

		List<ValidationEventArgs> ValidateXml(XmlDocument xmlDoc);

		byte[] Compile(XmlDocument xmlDoc);

		XmlDocument Decompile(byte[] data);
	}
}
