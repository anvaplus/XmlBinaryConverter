using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml;
using XmlBinaryConverter.XmlTools;

namespace XmlBinaryConverter.Compiler
{
    public class CompoundField : ACompilerInfoField
    {
        private readonly List<ACompilerInfoField> _fields;
        private uint _binarySize;
        private readonly string _cTypeName;

        public override uint Alignment => 4;

        public override uint Size
        {
            get
            {
                uint padding = 0;
                if (Number == 1 && _binarySize % Alignment != 0)
                    padding = Alignment - (_binarySize % Alignment);
                return (_binarySize + padding) * Number;
            }
        }

        public List<string> CDefinition
        {
            get
            {
                List<string> result = new List<string>
                {
                    $"//! Xpath {XPath} Size {Size / Number}", "typedef struct {"
                };

                foreach (var field in _fields)
                {
                    if (field is CompoundField)
                    {
                        CompoundField cf = field as CompoundField;
                        result.InsertRange(0, cf.CDefinition);
                    }
                    result.AddRange(field.CDeclaration);
                }
                result.Add($"}} {_cTypeName};");
                result.Add("");
                return result;
            }
        }

        public override string ToString()
        {
            return
                $"Xpath {XPath} Offset {Offset} Size {Size} Number {Number} Alignment {Alignment} Type {this.GetType()}";
        }


        public override List<string> CDeclaration
        {
            get
            {
                List<string> result = new List<string>
                {
                    $"\t//! {this.ToString()}",
                    $"\t{_cTypeName} {CName}[{Number}];"
                };
                return result;
            }
        }

        public uint GetBinarySize() { return _binarySize; }

        protected uint Number;

        public CompoundField(string name, uint number) : base(name)
        {
            _fields = new List<ACompilerInfoField>();
            this.Number = number;
            string[] x = CName.Split('.');
            _cTypeName = x[x.Length - 1];
            _cTypeName += "_t";
        }

        public override byte[] GetBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager)
        {
            byte[] result = new byte[_binarySize * Number];
            for (int i = 0; i < Number; i++)
            {
                byte[] result2 = new byte[_binarySize];
                foreach (var field in _fields)
                {
                    string completeXPath;
                    if (field is PaddingField)
                        continue;
                    if (Number > 1)
                    {
                        string[] parts = field.XPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        string pre = '/' + parts[0];
                        string post = "";
                        for (int j = 1; j < parts.Length; j++)
                        {
                            post += '/' + parts[j];
                        }
                        completeXPath = $"{xPrefix}{pre}[{i + 1}]{post}";
                    }
                    else
                    {
                        completeXPath = string.Format("{0}{2}", xPrefix, i + 1, field.XPath);
                    }

                    XmlNode subNode = node.SelectSingleNode(completeXPath, nsmanager);
                    if (subNode == null)
                    {
                        continue;
                    }


                    byte[] tmp = field.GetBytes(xPrefix + field.XPath, subNode, nsmanager);


                    if (tmp == null)
                    {
                        continue;
                    }

                    Array.Copy(tmp, 0, result2, field.Offset, tmp.Length);
                }

                Array.Copy(result2, 0, result, result2.Length * i, result2.Length);
            }
            return result;
        }

        public override string FromBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager, byte[] bytes)
        {
            for (int i = 0; i < Number; i++)
            {
                byte[] tmp = new byte[_binarySize];
                Array.Copy(bytes, Offset + (tmp.Length * i), tmp, 0, tmp.Length);
                foreach (var field in _fields)
                {
                    string completeXPath;
                    if (field is PaddingField)
                        continue;
                    if (Number > 1)
                    {
                        string[] parts = field.XPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        string pre = '/' + parts[0];
                        string post = "";
                        for (int j = 1; j < parts.Length; j++)
                        {
                            post += '/' + parts[j];
                        }
                        completeXPath = $"{xPrefix}{pre}[{i + 1}]{post}";
                    }
                    else
                    {
                        completeXPath = string.Format("{0}{2}", xPrefix, i + 1, field.XPath);
                    }


                    string value = field.FromBytes(xPrefix + field.XPath, node, nsmanager, tmp);
                    XmlUtilities.MakeXPath(node as XmlDocument, completeXPath, nsmanager, value);
                }
            }
            return "";
        }

        public void AddField(ACompilerInfoField field)
        {
            uint skip = (_binarySize % field.Alignment);
            if (skip != 0)
            {
                var pad = new PaddingField(field.Alignment - skip);
                pad.Offset = _binarySize;
                _fields.Add(pad);
                _binarySize += pad.Size;
            }

            field.Offset = _binarySize;
            _fields.Add(field);
            _binarySize += field.Size;
        }
    }

    public class Int32Field : ACompilerInfoField
    {
        public Int32Field(string Name) : base(Name)
        {

        }

        public override List<string> CDeclaration
        {
            get
            {
                List<string> result = new List<String>
                {
                    $"\t//! {this}",
                    $"\tint32_t {CName};"
                };
                return result;
            }
        }

        public override uint Alignment => 4;
        public override uint Size => 4;

        public override string FromBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager, byte[] bytes)
        {
            return BitConverter.ToInt32(bytes, (int)Offset).ToString();
        }

        public override byte[] GetBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager)
        {
            return BitConverter.GetBytes(Int32.Parse(node.InnerText));
        }
    }

    public class UInt32Field : ACompilerInfoField
    {
        public UInt32Field(string Name) : base(Name)
        {
        }

        public override List<string> CDeclaration
        {
            get
            {
                List<string> result = new List<string>
                {
                    $"\t//! {this}",
                    $"\t{"uint32_t"} {CName};"
                };
                return result;
            }
        }

        public override uint Alignment => 4;
        public override uint Size => 4;

        public override string FromBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager, byte[] bytes)
        {
            return BitConverter.ToUInt32(bytes, (int)Offset).ToString();
        }

        public override byte[] GetBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager)
        {
            return BitConverter.GetBytes(UInt32.Parse(node.InnerText));
        }
    }

    public class Int16Field : ACompilerInfoField
    {
        public Int16Field(string Name) : base(Name)
        {
        }

        public override List<string> CDeclaration
        {
            get
            {
                List<string> result = new List<string>
                {
                    $"\t//! {this.ToString()}",
                    $"\tint16_t {CName};"
                };
                return result;
            }
        }

        public override uint Alignment => 2;

        public override uint Size => 2;

        public override string FromBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager, byte[] bytes)
        {
            return BitConverter.ToInt16(bytes, (int)Offset).ToString();
        }

        public override byte[] GetBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager)
        {
            return BitConverter.GetBytes(Int16.Parse(node.InnerText));
        }
    }

    public class UInt16Field : ACompilerInfoField
    {
        public UInt16Field(string Name) : base(Name)
        {

        }

        public override List<string> CDeclaration
        {
            get
            {
                List<string> result = new List<string>
                {
                    $"\t//! {this}",
                    $"\tuint16_t {CName};"
                };
                return result;
            }
        }

        public override uint Alignment => 2;

        public override uint Size => 2;

        public override string FromBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager, byte[] bytes)
        {
            return BitConverter.ToUInt16(bytes, (int)Offset).ToString();
        }

        public override byte[] GetBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager)
        {
            return BitConverter.GetBytes(ushort.Parse(node.InnerText));
        }
    }

    public class Int64Field : ACompilerInfoField
    {
        public Int64Field(string Name) : base(Name)
        {

        }

        public override List<string> CDeclaration
        {
            get
            {
                List<string> result = new List<String>
                {$"\t//! {this.ToString()}", $"\tint64_t {CName};"};
                return result;
            }
        }

        public override uint Alignment => 8;
        public override uint Size => 8;

        public override string FromBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager, byte[] bytes)
        {
            return BitConverter.ToInt64(bytes, (int)Offset).ToString();
        }

        public override byte[] GetBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager)
        {
            return BitConverter.GetBytes(long.Parse(node.InnerText));
        }
    }

    public class UInt64Field : ACompilerInfoField
    {
        public UInt64Field(string Name) : base(Name)
        {

        }

        public override List<string> CDeclaration
        {
            get
            {
                List<string> result = new List<string>
                {
                    $"\t//! {this}",
                    $"\tuint64_t {CName};"
                };
                return result;
            }
        }

        public override uint Alignment => 8;
        public override uint Size => 8;

        public override string FromBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager, byte[] bytes)
        {
            return BitConverter.ToUInt64(bytes, (int)Offset).ToString();
        }

        public override byte[] GetBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager)
        {
            return BitConverter.GetBytes(ulong.Parse(node.InnerText));
        }
    }

    public class PaddingField : ACompilerInfoField
    {
        private static uint _id = 0;

        public PaddingField(uint size) : base("padding_" + _id)
        {
            _id++;
            Size = size;
        }

        public override List<string> CDeclaration
        {
            get
            {
                List<string> result = new List<string>
                {
                    $"\tuint8_t {CName}[{Size}];"
                };
                return result;
            }
        }

        public override uint Alignment => 1;

        public override uint Size { get; }

        public override String FromBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager, byte[] bytes)
        {
            return "";
        }

        public override byte[] GetBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager)
        {
            return null;
        }
    }

    public class HexBinaryField : ACompilerInfoField
    {
        public HexBinaryField(string Name, uint Size) : base(Name)
        {
            this.Size = Size;
        }

        public override List<string> CDeclaration
        {
            get
            {
                List<string> result = new List<string>
                {
                    $"\t//! {this}",
                    $"\tuint8_t {CName}[{Size}];"
                };
                return result;
            }
        }

        public override uint Alignment => 4;

        public override uint Size { get; }

        public override byte[] GetBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager)
        {
            return ConvertHexStringToByteArray(node.InnerText);
        }

        public override string FromBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager, byte[] bytes)
        {
            return ConvertByteArrayToHexString(bytes, (int)Offset, (int)Size);
        }

        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new XmlBinaryConverterException($"The binary key cannot have an odd number of digits: {hexString}");
            }

            byte[] HexAsBytes = new byte[hexString.Length / 2];
            for (int index = 0; index < HexAsBytes.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                HexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber);
            }

            return HexAsBytes;
        }

        public static string ConvertByteArrayToHexString(byte[] ba)
        {
            return ConvertByteArrayToHexString(ba, 0, ba.Length);
        }

        public static string ConvertByteArrayToHexString(byte[] ba, int index)
        {
            return ConvertByteArrayToHexString(ba, index, ba.Length - index);
        }

        public static string ConvertByteArrayToHexString(byte[] ba, int index, int length)
        {
            string hex = BitConverter.ToString(ba, index, length);
            return hex.Replace("-", "");
        }
    }

    public class BooleanField : ACompilerInfoField
    {
        public BooleanField(string Name) : base(Name)
        {

        }

        public override List<string> CDeclaration
        {
            get
            {
                List<string> result = new List<string>
                {
                    $"\t//! {this}",
                    $"\tuint8_t {CName};"
                };
                return result;
            }
        }

        public override uint Alignment => 1;

        public override uint Size => 1;

        public override string FromBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager, byte[] bytes)
        {
            return BitConverter.ToBoolean(bytes, (int)Offset).ToString().ToLower();
        }

        public override byte[] GetBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager)
        {
            return BitConverter.GetBytes(bool.Parse(node.InnerText));
        }
    }

    public class ByteField : ACompilerInfoField
    {
        public ByteField(string Name) : base(Name)
        {

        }

        public override List<string> CDeclaration
        {
            get
            {
                List<string> result = new List<string>
                {
                    $"\t//! {this}",
                    $"\tuint8_t {CName};"
                };
                return result;
            }
        }

        public override uint Alignment => 1;
        public override uint Size => 1;

        public override string FromBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager, byte[] bytes)
        {
            byte b = bytes[Offset];
            return b.ToString();
        }

        public override byte[] GetBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager)
        {
            byte[] tmp1 = BitConverter.GetBytes(byte.Parse(node.InnerText));
            byte[] tmp = new byte[1];
            tmp[0] = tmp1[0];
            return tmp;
        }
    }

    public class SByteField : ACompilerInfoField
    {
        public SByteField(string Name) : base(Name)
        {

        }

        public override List<string> CDeclaration
        {
            get
            {
                List<string> result = new List<string>
                {
                    $"\t//! {this}",
                    $"\tint8_t {CName};"
                };
                return result;
            }
        }

        public override uint Alignment => 1;
        public override uint Size => 1;

        public override String FromBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager, byte[] bytes)
        {
            sbyte b = (sbyte)bytes[Offset];
            return b.ToString();
        }

        public override byte[] GetBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager)
        {
            byte[] tmp1 = BitConverter.GetBytes(SByte.Parse(node.InnerText));
            byte[] tmp = new byte[Size];
            tmp[0] = tmp1[0];
            return tmp;
        }
    }

    public class StringField : ACompilerInfoField
    {
        uint _size;

        public StringField(string Name, uint Size) : base(Name)
        {
            this._size = Size;
        }

        public override List<string> CDeclaration
        {
            get
            {
                List<string> result = new List<string>
                {
                    $"\t//! {this}",
                    $"\tchar {CName}[{Size}];"
                };
                return result;
            }
        }

        public override uint Alignment => 1;

        public override uint Size => _size;

        public override string FromBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager, byte[] bytes)
        {
            string converted = Encoding.ASCII.GetString(bytes, (int)Offset, (int)Size);
            int position = converted.IndexOf((char)0);
            if (position == -1)
            {
                throw new XmlBinaryConverterException($"Unterminated string on {XPath} node");
            }

            return position == 0
                ? ""
                : converted.Substring(0, position);
        }

        public override byte[] GetBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager)
        {
            byte[] converted = Encoding.ASCII.GetBytes(node.InnerText);
            byte[] result = new byte[Size];
            Array.Copy(converted, result, Math.Min(Size, converted.Length));
            return result;
        }
    }

    public class StringEnumField : ACompilerInfoField
    {
        private readonly List<string> _values;

        public StringEnumField(string Name, List<string> values) : base(Name)
        {
            this._values = values;
        }


        public override List<string> CDeclaration
        {
            get
            {
                List<string> result = new List<string>
                {
                    $"\t//! {this}",
                    $"\tuint32_t {CName};"
                };
                return result;
            }
        }

        public override uint Alignment => 4;
        public override uint Size => 4;

        public override byte[] GetBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager)
        {
            int position = -1;
            for (int i = 0; i < _values.Count; i++)
            {
                if (_values[i] == node.InnerText)
                {
                    position = i;
                    break;
                }
            }

            if (position == -1)
            {
                throw new XmlBinaryConverterException(
                    $"Node {node.InnerText} on path {XPath} in invalid according to restricted values list");
            }

            return BitConverter.GetBytes(position);
        }

        public override String FromBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager, byte[] bytes)
        {
            int position = BitConverter.ToInt32(bytes, (int)Offset);

            if ((position < 0) || (position >= _values.Count))
            {
                throw new XmlBinaryConverterException(
                    $"Value {position} on path {XPath} in invalid according to restricted values list");
            }

            return _values[position].ToString();
        }
    }

    public class DateTimeField : ACompilerInfoField
    {
        private readonly string _format;

        public DateTimeField(string Name, String format) : base(Name)
        {
            this._format = format;
        }

        public override List<string> CDeclaration
        {
            get
            {
                List<string> result = new List<string>
                {
                    $"\t//! {this.ToString()}",
                    $"\tchar {CName}[{Size}];"
                };
                return result;
            }
        }
        public override uint Alignment => 4;

        //add 1 byte for c-style string termination
        public override uint Size => (uint)_format.Length + 1;

        public override byte[] GetBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager)
        {
            DateTime n = DateTime.Parse(node.InnerText);
            String s = n.ToString(_format);
            byte[] result = Encoding.ASCII.GetBytes(s);
            Debug.Assert(result.Length == _format.Length);
            return result;
        }

        public override string FromBytes(string xPrefix, XmlNode node, XmlNamespaceManager nsmanager, byte[] bytes)
        {
            string converted = Encoding.ASCII.GetString(bytes, (int)Offset, _format.Length);
            DateTime n = DateTime.ParseExact(converted, _format, CultureInfo.InvariantCulture);
            // This is W3C format string
            return n.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");
        }
    }
}
