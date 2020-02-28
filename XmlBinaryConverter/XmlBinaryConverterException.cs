using System;

namespace XmlBinaryConverter
{
	public class XmlBinaryConverterException : Exception
	{
		public XmlBinaryConverterException()
		{
			this.Message = "XML Binary Converter Exception";
		}

		public XmlBinaryConverterException(string message)
		{
			this.Message = message;
		}

		public sealed override string Message { get; }
	}
}
