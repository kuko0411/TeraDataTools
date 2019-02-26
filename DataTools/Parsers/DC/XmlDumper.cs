using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DCTools;
using GothosDC;

namespace DataTools.Parsers.DC
{
    public class XmlDumper
    {
        public static void Parse()
        {
            var dc = DCT.GetDataCenter();
            var namesCount = new Dictionary<string, int>();
            var childCount = new Dictionary<string, int>();

            var names = new List<string>();
            var initial = new Dictionary<string, int>();
            Directory.CreateDirectory("xml/");

            foreach (var child in DCT.DataCenter.Root.Children)
            {
                childCount.TryGetValue(child.Name, out var val);
                childCount[child.Name] = val + 1;   
            }


            int i = 0;
            foreach (var child in DCT.DataCenter.Root.Children)
            {
                string filename;

                if (childCount[child.Name] == 1)
                {
                    filename = "xml/" + child.Name;
                }
                else
                {
                    filename = "xml/" + child.Name + "/";
                    Directory.CreateDirectory(filename);

                    filename += child.Name;

                    foreach (var attribute in child.Attributes)
                    {
                        if ((attribute.Name == "id" || attribute.Name.EndsWith("Id")) && attribute.Value.ToString() != "0")
                        {
                            filename += "-" + attribute.Name + "-" + attribute.Value;
                        }
                    }

                    if (namesCount.TryGetValue(filename, out var count))
                    {
                        namesCount[filename] = count + 1;
                        
                        if (count == 1)
                        {
                            var initialIndex = initial[filename];
                            names[initialIndex] += "-0";
                        }
                        
                        filename += "-" + count;
                    }
                    else
                    {
                        namesCount[filename] = 1;
                        initial[filename] = i;
                    }
                }

                names.Add(filename);
                i++;
            }


            foreach (var item in names.Zip(DCT.DataCenter.Root.Children, (s, e) => new {filename = s + ".xml", element = e}))
            {
                var element = ConvertToXElement(item.element);
                element.Save(item.filename);
            }
        }

        private static XElement ConvertToXElement(DataCenterElement obj)
        {
            var element = new XElement(obj.Name);
            foreach (var arg in obj.Attributes)
            {
                element.SetAttributeValue(arg.Name, arg.ValueToString(CultureInfo.InvariantCulture));
            }
            foreach (var child in obj.Children)
            {
                var childElement = ConvertToXElement(child);
                element.Add(childElement);
            }
            return element;
        }
    }
}
