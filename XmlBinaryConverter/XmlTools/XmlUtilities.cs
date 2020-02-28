using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using XmlBinaryConverter.Compiler;

namespace XmlBinaryConverter.XmlTools
{
	public class XmlUtilities
	{
		private static readonly XmlInfo XmlInfo = XmlInfo.GetInstance;

		public static XmlNode MakeXPath(XmlDocument doc, string xpath, XmlNamespaceManager nsManager, string value)
		{
			return _makeXPath(doc, doc as XmlNode, xpath, nsManager, value);
		}

		private static XmlNode _makeXPath(XmlDocument doc, XmlNode parent, string xpath, XmlNamespaceManager nsManager, string value)
		{
			// grab the next node name in the xpath; or return parent if empty
			string[] partsOfXPath = xpath.Trim('/').Split('/');
			string nextNodeInXPath = partsOfXPath.First();
			if (string.IsNullOrEmpty(nextNodeInXPath))
			{
				return parent;
			}

			// rejoin the remainder of the array as an xpath expression and recurse
			string rest = string.Join("/", partsOfXPath.Skip(1).ToArray());

			// get or create the node from the name
			XmlNode node = parent.SelectSingleNode(nextNodeInXPath, nsManager);
			if (node == null)
			{
				XmlElement elem = null;
				nextNodeInXPath = nextNodeInXPath.Replace("mib:", "");

				if (nextNodeInXPath.Contains("["))
				{
					// this is terrible but actually works if xpath expressions are in increasing order and by construction... is ok!
					string[] ss = nextNodeInXPath.Split(new char[] { '[', ']' });
					uint index = uint.Parse(ss[1]);
					elem = doc.CreateElement(ss[0], $"{XmlInfo.GetXmlNamespaceUri()}");
				}
				else
				{
					elem = doc.CreateElement(nextNodeInXPath, $"{XmlInfo.GetXmlNamespaceUri()}");
				}

				if (rest == "")
				{
					elem.InnerText = value;
				}

				node = parent.AppendChild(elem);
			}

			return _makeXPath(doc, node, rest, nsManager, value);
		}


		public static Tuple<bool, string> ValidateXmlAgainstXsd(string xml, SchemaCompiler schemaCompiler)
		{
			try
			{
				// load Xml document
				XmlDocument doc = new XmlDocument();
				doc.Load(XmlReader.Create(new StringReader(xml)));

				// validate against XSD
				var list = schemaCompiler.ValidateXml(doc);
				if (list.Any(e => e.Severity == XmlSeverityType.Error))
				{
					return new Tuple<bool, string>(false, $"XSD validation fail: {list[0].Message}");
				}
			}
			catch (Exception e)
			{
				// the XML file is not valid
				return new Tuple<bool, string>(false, $"XML not valid: {e}");
			}

			return new Tuple<bool, string>(true, string.Empty);
		}

		public static bool IsValidPath(string path)
		{
			if (path == string.Empty)
			{
				return false;
			}

			Regex driveCheck = new Regex(@"^[a-zA-Z]:\\$");
			if (!driveCheck.IsMatch(path.Substring(0, 3)))
			{
				return false;
			}

			string strTheseAreInvalidFileNameChars = new string(Path.GetInvalidPathChars());
			strTheseAreInvalidFileNameChars += @":/?*" + "\"";
			Regex containsABadCharacter = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");

			if (containsABadCharacter.IsMatch(path.Substring(3, path.Length - 3)))
			{
				return false;
			}

			return true;
		}

		public static bool FileExist(string path)
		{
			var fullPath = Path.GetFullPath(path);
			return File.Exists(fullPath);
		}


	}
}
