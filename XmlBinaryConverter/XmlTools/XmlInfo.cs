namespace XmlBinaryConverter.XmlTools
{
	public sealed class XmlInfo
	{
		#region Singleton Pattern

		private static XmlInfo _instance;
		public static XmlInfo GetInstance => _instance ??= new XmlInfo();
		private XmlInfo() { }

		#endregion

		private string XsdPath { get; set; }

		private string XmlPath { get; set; }

		private string XmlMainElementName { get; set; }

		private string XmlNamespaceUri { get; set; }

		private string XmlNamespacePrefix { get; set; }


		public string GetXsdPath()
		{
			return XsdPath;
		}

		public void SetXsdPath(string xsdPath)
		{
			XsdPath = xsdPath;
		}

		public string GetXmlPath()
		{
			return XmlPath;
		}

		public void SetXmlPath(string xmlPath)
		{
			XmlPath = xmlPath;
		}

		public string GetXmlMainElementName()
		{
			return XmlMainElementName;
		}

		public void SetXmlMainElementName(string xmlMainElementName)
		{
			XmlMainElementName = xmlMainElementName;
		}

		public string GetXmlNamespaceUri()
		{
			return XmlNamespaceUri;
		}

		public void SetXmlNamespaceUri(string xmlNamespaceUri)
		{
			XmlNamespaceUri = xmlNamespaceUri;
		}

		public string GetXmlNamespacePrefix()
		{
			return XmlNamespacePrefix;
		}

		public void SetXmlNamespacePrefix(string xmlNamespacePrefix)
		{
			XmlNamespacePrefix = xmlNamespacePrefix;
		}


		public void ClearData()
		{
			SetXsdPath(string.Empty);
			SetXmlPath(string.Empty);
			SetXmlNamespaceUri(string.Empty);
			SetXmlNamespacePrefix(string.Empty);
			SetXmlMainElementName(string.Empty);
		}

	}
}
