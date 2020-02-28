using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using XmlBinaryConverter.XmlTools;

namespace XmlBinaryConverter.Compiler
{
    public class Compiler
    {
        private readonly XmlInfo _xmlInfo = XmlInfo.GetInstance;

        public CompoundField Info { get; protected set; }

        private readonly XmlSchema _schema;
        // private readonly XmlInfo _xmlInfo = XmlInfo.GetInstance;

        void StrictValidationCallback(object sender, ValidationEventArgs e)
        {
            throw e.Exception;
        }


        public Compiler(XmlSchema schema)
        {
            this._schema = schema;

            // The Compile method will throw errors encountered on compiling the schema
            XmlSchemaSet xset = new XmlSchemaSet();
            xset.Add(_schema);
            xset.ValidationEventHandler += StrictValidationCallback;
            xset.Compile();

            XmlSchemaElement e = null;
            foreach (XmlSchemaElement elem in _schema.Elements.Values)
            {
                if (elem.Name == $"{_xmlInfo.GetXmlMainElementName()}")
                {
                    e = elem;
                    break;
                }
            }

            if (e == null)
            {
                throw new XmlBinaryConverterException($"No element named {_xmlInfo.GetXmlMainElementName()} can be found in the schema");
            }

            Info = new CompoundField($"{_xmlInfo.GetXmlMainElementName()}", 1);
            ProcessSchemaObject(Info, null, e, 1);
        }


        private void ProcessSchemaObject(CompoundField compilerInfo, string ancestor, XmlSchemaObject obj, uint num)
        {
            for (uint i = 1; i <= num; i++)
            {
                string _ancestor = ancestor;

                if (obj is XmlSchemaElement elem)
                {
                    ProcessElement(compilerInfo, _ancestor, elem, i, num);
                }
                else if (obj is XmlSchemaChoice choice)
                {
                    ProcessChoice(compilerInfo, _ancestor, choice, i, num);
                }
                else if (obj is XmlSchemaSequence sequence)
                {
                    ProcessSequence(compilerInfo, _ancestor, sequence, i, num);
                }
                else
                {
                    throw new XmlBinaryConverterException($"Object {obj} is not supported for schema generation");
                }
            }
        }

        private void ProcessElement(CompoundField compilerInfo, String ancestor, XmlSchemaElement elem, uint index, uint cardinality)
        {
            //output.WriteLine(feed() + "Processing element \"{0}\" of type \"{1}\" [{2}..{3}]", prefix + elem.Name, elem.SchemaTypeName, elem.MinOccurs, elem.MaxOccurs);

            bool skipBinarization = false;
            bool restrictToSingleScanner = false;

            if (elem.UnhandledAttributes != null)
            {
                foreach (var e in elem.UnhandledAttributes)
                {
                    if (e.LocalName == "skipBinarization")
                    {
                        //output.WriteLine("{0}Do not binarize {1} (marked skipBinarization)", feed(), elem.Name);
                        skipBinarization = true;
                    }
                    else if (e.LocalName == "restrictToSingleScanner")
                    {
                        //output.WriteLine("{0}Restrict {1} MaxOccurs to 1 (marked skipBinarization)", feed(), elem.Name);
                        restrictToSingleScanner = true;
                    }
                    else
                    {
                        throw new XmlBinaryConverterException($"Attribute {e.LocalName} is not supported for schema generation");
                    }

                }
            }

            if (skipBinarization)
            {
                return;
            }

            string prefix = ancestor == null ? "" : ancestor + ".";

            if (elem.ElementSchemaType is XmlSchemaComplexType)
            {
                XmlSchemaComplexType ct = elem.ElementSchemaType as XmlSchemaComplexType;

                foreach (DictionaryEntry obj in ct.AttributeUses)
                {
                    //output.WriteLine(feed() + "Attribute: {0}  ", (obj.Value as XmlSchemaAttribute).Name);
                }

                prefix += elem.Name;
                if (cardinality > 1)
                {
                    prefix += "[" + index + "]";
                    throw new XmlBinaryConverterException("MaxOccurs greater than 1 is not supported on element nodes");
                }

                if (!restrictToSingleScanner && elem.MaxOccurs > 1)
                {
                    throw new XmlBinaryConverterException("MaxOccurs greater than 1 is not supported on element nodes");
                }

                ProcessSchemaObject(compilerInfo, prefix, ct.ContentTypeParticle, restrictToSingleScanner ? 1 : (uint)elem.MaxOccurs);
            }
            else if (elem.ElementSchemaType is XmlSchemaSimpleType)
            {
                Binarize(compilerInfo, prefix, elem, index, cardinality);
            }
            else
            {
                throw new XmlBinaryConverterException($"Element {elem} schema type is not supported for schema generation");
            }
        }

        private void ProcessSequence(CompoundField compilerInfo, String ancestor, XmlSchemaSequence sequence, uint index, uint cardinality)
        {
            //output.WriteLine(feed() + "Processing sequence [{0}..{1}]", sequence.MinOccurs, sequence.MaxOccurs);

            if (sequence.MaxOccurs == 1)
            {
                ProcessItemCollection(compilerInfo, ancestor, sequence.Items, 1);
                return;
            }

            CompoundField arg = new CompoundField(ancestor, (uint)sequence.MaxOccurs);
            ProcessItemCollection(arg, null, sequence.Items, 1);
            compilerInfo.AddField(arg);

            /*CompoundField arg = new CompoundField(ancestor, (uint)sequence.MaxOccurs);
            ProcessItemCollection(arg, null, sequence.Items, 1);
            compilerInfo.AddField(arg);*/
        }

        private void ProcessChoice(CompoundField compilerInfo, String ancestor, XmlSchemaChoice choice, uint index, uint cardinality)
        {
            //output.WriteLine(feed() + "Processing choice [{0}..{1}]", choice.MinOccurs, choice.MaxOccurs);
            if (choice.MaxOccurs > 1)
                throw new XmlBinaryConverterException("MaxOccurs greater than 1 is not supported on choice nodes");

            ProcessItemCollection(compilerInfo, ancestor, choice.Items, (uint)choice.MaxOccurs);
        }

        private void ProcessItemCollection(CompoundField compilerInfo, String ancestor, XmlSchemaObjectCollection objs, uint num)
        {
            foreach (XmlSchemaObject obj in objs)
            {
                //index++;
                ProcessSchemaObject(compilerInfo, ancestor, obj, num);
                //index--;
            }
        }

        private static void Binarize(CompoundField compilerInfo, string prefix, XmlSchemaElement elem, uint index, uint cardinality)
        {
            XmlSchemaDatatypeVariety dataType = elem.ElementSchemaType.Datatype.Variety;
            string name = prefix + elem.Name;
            if (cardinality > 1)
            {
                throw new XmlBinaryConverterException("cardinality greater than 1 is not supported");
                //name += "[" + index + "]";
            }

            ACompilerInfoField cif = null;
            IEnumerable<XmlSchemaLengthFacet> lengthFacets;
            IEnumerable<XmlSchemaMaxLengthFacet> maxLengthFacet;
            IEnumerable<XmlSchemaEnumerationFacet> enumerationFacets;

            switch (elem.ElementSchemaType.TypeCode)
            {
                case XmlTypeCode.String:
                    try
                    {
                        var simpleType = elem.ElementSchemaType as XmlSchemaSimpleType;
                        var restriction = simpleType.Content as XmlSchemaSimpleTypeRestriction;
                        maxLengthFacet = restriction.Facets.OfType<XmlSchemaMaxLengthFacet>();
                        enumerationFacets = restriction.Facets.OfType<XmlSchemaEnumerationFacet>();
                    }
                    catch (Exception e)
                    {
                        throw new XmlBinaryConverterException($"Unexpected exception while processing element {name}");
                    }

                    if (maxLengthFacet.Count() != 0)
                    {
                        cif = new StringField(name, uint.Parse(maxLengthFacet.ElementAt(0).Value));
                    }
                    else if (enumerationFacets.Count() != 0)
                    {
                        List<string> values = enumerationFacets.Select(e => e.Value).ToList();
                        cif = new StringEnumField(name, values);
                    }
                    else
                    {
                        throw new XmlBinaryConverterException(
                            $"String {name} have unrestricted length and thus is not supported for schema generation");
                    }
                    break;
                case XmlTypeCode.Int:
                    cif = new Int32Field(name);
                    break;
                case XmlTypeCode.UnsignedInt:
                    cif = new UInt32Field(name);
                    break;
                case XmlTypeCode.Short:
                    cif = new Int16Field(name);
                    break;
                case XmlTypeCode.UnsignedShort:
                    cif = new UInt16Field(name);
                    break;
                case XmlTypeCode.Byte:
                    cif = new SByteField(name);
                    break;
                case XmlTypeCode.UnsignedByte:
                    cif = new ByteField(name);
                    break;
                case XmlTypeCode.UnsignedLong:
                    cif = new UInt64Field(name);
                    break;
                case XmlTypeCode.Boolean:
                    cif = new BooleanField(name);
                    break;
                case XmlTypeCode.HexBinary:
                    try
                    {
                        var simpleType = elem.ElementSchemaType as XmlSchemaSimpleType;
                        var restriction = simpleType.Content as XmlSchemaSimpleTypeRestriction;
                        lengthFacets = restriction.Facets.OfType<XmlSchemaLengthFacet>();
                    }
                    catch (Exception e)
                    {
                        throw new XmlBinaryConverterException($"Unexpected exception while processing element {name}");
                    }

                    if (lengthFacets.Count() != 0)
                    {
                        uint size = uint.Parse(lengthFacets.ElementAt(0).Value);
                        cif = new HexBinaryField(name, size);
                    }
                    else
                    {
                        throw new XmlBinaryConverterException(
                            $"HexBinary {name} have unrestricted length and thus is not supported for schema generation");
                    }
                    break;
                case XmlTypeCode.DateTime:
                    cif = new DateTimeField(name, "yyyyMMddHHmmss");
                    break;
                default:
                    {
                        throw new XmlBinaryConverterException($"{name} have unsupported type {elem.ElementSchemaType.TypeCode}");
                    }
            }

            if (cif == null)
            {
                throw new XmlBinaryConverterException($"Internal error: unable to create a conversion structure for {name}");
            }

            compilerInfo.AddField(cif);
        }

    }
}
