using System.Collections.Generic;
using System.Xml;
using XmlBinaryConverter.XmlTools;

namespace XmlBinaryConverter.Compiler
{
	public abstract class ACompilerInfoField
	{
		private readonly XmlInfo _xmlInfo = XmlInfo.GetInstance;

		protected ACompilerInfoField(string name)
		{
			var prefix = _xmlInfo.GetXmlNamespacePrefix();

			XPath = "";
			foreach (string s in name.Split('.'))
			{
				XPath += "/" + (prefix == "" ? "" : prefix + ":") + s;
			}

			this.CName = XPath.Replace("/" + prefix + ":", "_").Replace("[", "").Replace("]", "_").Replace($"_{_xmlInfo.GetXmlMainElementName()}_", "").Replace("__", "");
		}

		public string XPath { get; }

		public abstract uint Alignment { get; }

		public abstract uint Size { get; }

		public uint Offset { get; set; }

		public string CName { get; }

		public abstract List<string> CDeclaration { get; }

		public override string ToString()
		{
			return $"Xpath {XPath} Offset {Offset} Size {Size} Alignment {Alignment} Type {this.GetType().ToString()}";
		}

		public abstract byte[] GetBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager);

		public abstract string FromBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager, byte[] bytes);
	}
}
