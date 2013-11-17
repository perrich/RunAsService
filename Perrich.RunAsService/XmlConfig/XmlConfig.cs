using System;
using System.Collections.Generic;
using System.Xml;

namespace Perrich.RunAsService.XmlConfig
{
    /// <summary>
    /// The class which represents a configuration xml file
    /// </summary>
    public class XmlConfig
    {
        /// <summary>
        /// The node separator in an XPath query
        /// </summary>
        public static readonly String XPathSeparator = "/";

        private readonly XmlDocument _xmldoc;

        private String _fileName;

        /// <summary>
        /// Create an XmlConfig from an empty xml file 
        /// containing only the rootelement named as 'xml'
        /// </summary>
        public XmlConfig()
        {
            _xmldoc = new XmlDocument();
            LoadXmlFromString("<xml></xml>");
        }

        /// <summary>
        /// Create an XmlConfig from an existing file, or create a new one
        /// </summary>
        /// <param name="loadfromfile">
        /// Path and filename from where to load the xml file
        /// </param>
        public XmlConfig(string loadfromfile)
        {
            _xmldoc = new XmlDocument();
            LoadXmlFromFile(loadfromfile);
        }

        /// <summary>
        ///  Get the first configuration item which matches the xpath request
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public XmlConfigItem GetItem(String xpath)
        {
            xpath = CompleteXPathWithRootName(xpath);
            return CreateXmlConfigItem(_xmldoc.SelectSingleNode(xpath));
        }

        /// <summary>
        /// Get configuration items which match the xpath request
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public IList<XmlConfigItem> GetItems(String xpath)
        {
            xpath = CompleteXPathWithRootName(xpath);
            IList<XmlConfigItem> list = new List<XmlConfigItem>();
            var nList = _xmldoc.SelectNodes(xpath);
            if (nList != null)
            {
                foreach (XmlNode xNode in nList)
                {
                    var item = CreateXmlConfigItem(xNode);
                    if (item != null)
                    {
                        list.Add(item);
                    }

                }
            }

            return list;
        }

        /// <summary>
        /// Create an configuration item from an Xml node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static XmlConfigItem CreateXmlConfigItem(XmlNode node)
        {
            if (node == null)
            {
                return new XmlConfigItem(String.Empty);
            }
            var result = node.InnerXml;

            if (String.IsNullOrEmpty(result) && node.Attributes != null)
            {
                var xmlattrib = node.Attributes.GetNamedItem("value");
                result = xmlattrib != null ? xmlattrib.Value : String.Empty;
            }

            return new XmlConfigItem(result);
        }

        /// <summary>
        /// Automaticaly add the root node name to the xpath request if possible
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        private String CompleteXPathWithRootName(String xpath)
        {
            if (xpath != null && _xmldoc.DocumentElement != null && !xpath.StartsWith(XPathSeparator) && !xpath.StartsWith(_xmldoc.DocumentElement.Name + XPathSeparator))
            {
                xpath = _xmldoc.DocumentElement.Name + XPathSeparator + xpath;
            }
            return xpath;
        }

        /// <summary>
        /// Load a new XmlDocument from a file
        /// </summary>
        /// <param name="filename">
        /// Path and filename from where to load the xml file
        /// </param>
        /// <remarks>
        /// Throws an XmlException if file does not exist
        /// </remarks>
        public void LoadXmlFromFile(string filename)
        {
            _fileName = filename;
            try
            {
                _xmldoc.Load(filename);
            }
            catch (Exception e)
            {
                throw new XmlException(string.Format("XML loading failed for file '{0}': {1}", _fileName, e.Message));
            }
        }

        /// <summary>
        /// Load a new XmlDocument from a string
        /// </summary>
        /// <param name="xml">
        /// XML string
        /// </param>
        public void LoadXmlFromString(string xml)
        {
            _xmldoc.LoadXml(xml);
            _fileName = null;
        }

        /// <summary>
        /// Reset to an empty XmlDocument
        /// </summary>
        /// <param name="rootelement">
        /// Name of root element
        /// </param>
        public void ResetXml(string rootelement)
        {
            LoadXmlFromString(String.Format("<{0}></{0}>", rootelement));
        }

        /// <summary>
        /// If loaded from a file reload the file
        /// </summary>
        /// <returns>
        /// True on success
        /// False on failure, probably due to file was not loaded from a file but from a String
        /// </returns>
        public bool Reload()
        {
            if (_fileName != null)
            {
                LoadXmlFromFile(_fileName);
                return true;
            }
            return false;
        }
    }
}