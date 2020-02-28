using System;
using System.IO;
using System.Xml;
using XmlBinaryConverter.Compiler;
using XmlBinaryConverter.XmlTools;
using static XmlBinaryConverter.Constants;

namespace XmlBinaryConverter
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			XmlInfo xmlInfo = XmlInfo.GetInstance;

			if (args == null)
			{
				Console.WriteLine("Bad args parameter!");
				return;
			}

			if (args.Length == 0)
			{
				var interactiveMode = true;
				while (interactiveMode)
				{
					var consoleModeResult = ConsoleInteractiveMode();

					if (consoleModeResult)
					{
						Console.WriteLine("----");
						Console.WriteLine("Everything went as planned! Reload: Y/N ?");

						var readKey = Console.ReadLine();
						if (readKey == null || readKey.ToUpper() != "Y")
						{
							interactiveMode = false;
						}

					}
					else
					{
						Console.WriteLine("----");
						Console.WriteLine("Something went wrong! Reload: Y/N ?");

						var readKey = Console.ReadLine();
						if (readKey == null || readKey.ToUpper() != "Y")
						{
							interactiveMode = false;
						}

					}
				}

				Console.WriteLine("Press any key to exit...");
				Console.Read();
				return;
			}

			if (args[0] == "-help")
			{
				ShowArgumentsHelp();
			}

			if (IsConsoleInInteractiveMode(args))
			{
				Console.WriteLine($"Time start : {DateTime.Now}");
				Console.WriteLine("----");
				Console.WriteLine("");

				var consoleArgumentsModeResult = ConsoleArgumentsMode(args, true);

				if (consoleArgumentsModeResult)
				{
					Console.WriteLine("");
					Console.WriteLine("Everything went as planned!");
					Console.WriteLine("----");
					Console.WriteLine($"Time end : {DateTime.Now}");
					return;
				}

				Console.WriteLine("");
				Console.WriteLine("Something went wrong!");
				Console.WriteLine("----");
				Console.WriteLine($"Time end : {DateTime.Now}");
				Console.WriteLine("Press any key to exit...");
				Console.Read();
			}
			else
			{
				// save output to file
				// if the args are not valid an error file will be created
				using StreamWriter writer = new StreamWriter($"{FileManager.StandardPath()}\\log.txt");

				Console.SetOut(writer);

				Console.WriteLine($"Time start : {DateTime.Now}");
				Console.WriteLine("----");
				Console.WriteLine("");

				var consoleArgumentsModeResult = ConsoleArgumentsMode(args, false);

				if (!consoleArgumentsModeResult)
				{
					Console.WriteLine("");
					Console.WriteLine("----");
					Console.WriteLine("Something went wrong!");
					Console.WriteLine("----");
					Console.WriteLine("");
				}

				Console.WriteLine("");
				Console.WriteLine("----");
				Console.WriteLine($"Time end : {DateTime.Now}");
			}
		}


		private static Tuple<bool, string> LoadXsdData(string xsdPath = null)
		{
			string desktopPath = FileManager.StandardPath();
			string path;
			XmlInfo xmlInfo = XmlInfo.GetInstance;

			if (xsdPath == null)
			{
				Console.WriteLine("Insert XSD path: ");
				path = Console.ReadLine();
			}
			else
			{
				path = xsdPath;
			}

			if (!XmlUtilities.IsValidPath(path))
			{
				return new Tuple<bool, string>(false, "Error! XSD path not valid");
			}

			if (!XmlUtilities.FileExist(path))
			{
				return new Tuple<bool, string>(false, "Error! XSD file not exist");
			}

			xmlInfo.SetXsdPath(path);

			return new Tuple<bool, string>(true, string.Empty);
		}

		private static Tuple<bool, string> LoadXmlData(string xmlPath = null, string xmlns = null, string xmlPrefix = null, string xmlMainElementName = null)
		{
			var setXmlPath = SetXmlPath(xmlPath);
			if (!setXmlPath.Item1)
			{
				return new Tuple<bool, string>(false, setXmlPath.Item2);
			}

			SetXmlNamespaceUri(xmlns);
			SetXmlNamespacePrefix(xmlPrefix);
			SetXmlMainElementName(xmlMainElementName);

			return new Tuple<bool, string>(true, string.Empty);
		}

		private static Tuple<bool, string> SetXmlPath(string xmlPath = null)
		{
			string desktopPath = FileManager.StandardPath();
			string path;
			XmlInfo xmlInfo = XmlInfo.GetInstance;

			if (xmlPath == null)
			{
				Console.WriteLine("Insert XML path: ");
				path = Console.ReadLine();
			}
			else
			{
				path = xmlPath;
			}

			if (!XmlUtilities.IsValidPath(path))
			{
				return new Tuple<bool, string>(false, "Error! XML path not valid");
			}

			if (!XmlUtilities.FileExist(path))
			{
				return new Tuple<bool, string>(false, "Error! XNL file not exist");
			}

			xmlInfo.SetXmlPath(path);

			return new Tuple<bool, string>(true, string.Empty);
		}

		private static void SetXmlNamespaceUri(string xmlns = null)
		{
			XmlInfo xmlInfo = XmlInfo.GetInstance;
			if (xmlns == null)
			{
				Console.WriteLine("Insert target namespace URI (XMLNS): (Ex. http://tempuri.org/Test.xsd) ");
				xmlns = Console.ReadLine();
			}

			xmlInfo.SetXmlNamespaceUri(xmlns);

		}

		private static void SetXmlNamespacePrefix(string xmlPrefix = null)
		{
			XmlInfo xmlInfo = XmlInfo.GetInstance;
			if (xmlPrefix == null)
			{
				Console.WriteLine("Insert XML namespace prefix: ");
				xmlPrefix = Console.ReadLine();
			}

			xmlInfo.SetXmlNamespacePrefix(xmlPrefix);
		}

		private static void SetXmlMainElementName(string xmlMainElementName = null)
		{
			XmlInfo xmlInfo = XmlInfo.GetInstance;
			if (xmlMainElementName == null)
			{
				Console.WriteLine("Insert XML main element name:");
				xmlMainElementName = Console.ReadLine();
			}

			xmlInfo.SetXmlMainElementName(xmlMainElementName);
		}

		private static Tuple<bool, string> ValidateXml()
		{
			XmlInfo xmlInfo = XmlInfo.GetInstance;
			var schemaCompiler = new SchemaCompiler(new StreamReader(xmlInfo.GetXsdPath()));
			var xmlString = File.ReadAllText(xmlInfo.GetXmlPath());

			var validateXml = XmlUtilities.ValidateXmlAgainstXsd(xmlString, schemaCompiler);

			if (!validateXml.Item1)
			{
				return new Tuple<bool, string>(false, validateXml.Item2);
			}

			return new Tuple<bool, string>(true, string.Empty);
		}

		private static Tuple<bool, string> SaveHeaderAndBiFile(string savePath = null)
		{
			var path = FileManager.StandardPath();
			var xmlInfo = XmlInfo.GetInstance;
			var schemaCompiler = new SchemaCompiler(new StreamReader(xmlInfo.GetXsdPath()));
			var xmlString = File.ReadAllText(xmlInfo.GetXmlPath());

			if (savePath != null)
			{
				path = savePath;
			}

			try
			{
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}

				File.WriteAllLines(Path.Combine(path, "headerFile.h"), schemaCompiler.GetHeaderFile());

				// load Xml document
				XmlDocument doc = new XmlDocument();
				doc.Load(XmlReader.Create(new StringReader(xmlString)));
				var compiledByteArray = schemaCompiler.Compile(doc);

				File.WriteAllBytes($"{path}/binFile.bin", compiledByteArray);
			}
			catch (Exception ex)
			{
				return new Tuple<bool, string>(false, $"Error on Write files :  {ex.Message}");
			}

			return new Tuple<bool, string>(true, $"Files correctly saved on: {path}");
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private static bool ConsoleInteractiveMode()
		{
			XmlInfo xmlInfo = XmlInfo.GetInstance;
			xmlInfo.ClearData();

			var xsdData = LoadXsdData();
			if (!xsdData.Item1)
			{
				Console.WriteLine(xsdData.Item2);
				return false;
			}

			var xmlData = LoadXmlData();
			if (!xmlData.Item1)
			{
				Console.WriteLine(xmlData.Item2);
				return false;
			}

			if (string.Equals(xmlInfo.GetXsdPath(), xmlInfo.GetXmlPath()))
			{
				Console.WriteLine("Error! XSD and XML path are the same");
				return false;
			}

			var validateXml = ValidateXml();
			if (!validateXml.Item1)
			{
				Console.WriteLine("XML validation against XSD schema failed!");
				Console.WriteLine(validateXml.Item2);
				return false;
			}

			Console.WriteLine(SaveHeaderAndBiFile().Item2);
			return true;
		}

		private static bool ConsoleArgumentsMode(string[] args, bool interactiveMode)
		{
			string xsdPath = null;
			string xmlPath = null;
			string xmlns = null;
			string xmlPrefix = null;
			string xmlMainElementName = null;
			string savePath = null;

			for (int i = 0; i < args.Length; i++)
			{
				switch (args[i])
				{
					case xsdPath_opt:
						xsdPath = args[i + 1];
						continue;
					case xmlPath_opt:
						xmlPath = args[i + 1];
						continue;
					case xmlns_opt:
						xmlns = args[i + 1];
						continue;
					case xmlpx_opt:
						xmlPrefix = args[i + 1];
						continue;
					case xmlMainElement_opt:
						xmlMainElementName = args[i + 1];
						continue;
					case output_opt:
						savePath = args[i + 1];
						continue;
					case interactive_opt:
						continue;
				}
			}


			if (!interactiveMode && (xsdPath == null || xmlPath == null || xmlns == null || xmlPrefix == null ||
									 xmlMainElementName == null))
			{
				Console.WriteLine("Wrong Arguments! Application not in interactive mode. Mandatory arguments: [ -xsd | -xml | -xns | -xpx | -xen ]");
				Console.WriteLine("Help mode: XmlBinaryConverter -help");
				Console.WriteLine("Interactive mode: XmlBinaryConverter -it");
				return false;
			}


			XmlInfo xmlInfo = XmlInfo.GetInstance;
			xmlInfo.ClearData();

			var xsdData = LoadXsdData(xsdPath);
			if (!xsdData.Item1)
			{
				Console.WriteLine(xsdData.Item2);
				return false;
			}

			var xmlData = LoadXmlData(xmlPath, xmlns, xmlPrefix, xmlMainElementName);
			if (!xmlData.Item1)
			{
				Console.WriteLine(xmlData.Item2);
				return false;
			}

			if (string.Equals(xmlInfo.GetXsdPath(), xmlInfo.GetXmlPath()))
			{
				Console.WriteLine("Error! XSD and XML path are the same");
				return false;
			}

			var validateXml = ValidateXml();
			if (!validateXml.Item1)
			{
				Console.WriteLine("XML validation against XSD schema failed!");
				Console.WriteLine(validateXml.Item2);
				return false;
			}

			Console.WriteLine(SaveHeaderAndBiFile(savePath).Item2);
			return true;

		}

		private static bool IsConsoleInInteractiveMode(string[] args)
		{
			foreach (var arg in args)
			{
				switch (arg)
				{
					case interactive_opt:
						return true;
				}
			}

			return false;
		}


		private static void ShowArgumentsHelp()
		{

			Console.WriteLine();
			Console.WriteLine("Usage: XmlBinaryConverter [OPTIONS] COMMAND");

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine("Options:");

			Console.WriteLine($"{tab,alignTab}" + $"{help_opt,alignLeft}" + $"{help_desc,alignRight}");
			Console.WriteLine($"{tab,alignTab}" + $"{xsdPath_opt,alignLeft}" + $"{xsdPath_desc,alignRight}");
			Console.WriteLine($"{tab,alignTab}" + $"{xmlPath_opt,alignLeft}" + $"{xmlPath_desc,alignRight}");
			Console.WriteLine($"{tab,alignTab}" + $"{xmlns_opt,alignLeft}" + $"{xmlns_desc,alignRight}");
			Console.WriteLine($"{tab,alignTab}" + $"{xmlpx_opt,alignLeft}" + $"{xmlpx_desc,alignRight}");
			Console.WriteLine($"{tab,alignTab}" + $"{xmlMainElement_opt,alignLeft}" + $"{xmlMainElement_desc,alignRight}");
			Console.WriteLine($"{tab,alignTab}" + $"{output_opt,alignLeft}" + $"{output_opt_desc,alignRight}");
			Console.WriteLine($"{tab,alignTab}" + $"{interactive_opt,alignLeft}" + $"{interactive_opt_desc,alignRight}");


			Console.Read();

		}

	}
}