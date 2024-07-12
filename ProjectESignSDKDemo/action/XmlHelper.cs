using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ProjectESignSDKDemo.action
{
    public class XmlHelper
    {
        public static XmlElement FindNodeWithAttributeValueIn(XmlNodeList nodes, string attr, string value)
        {
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes != null && node.Attributes[attr] != null && node.Attributes[attr].Value == value)
                {
                    return (XmlElement)node;
                }
            }

            return null;
        }

        internal static CanonicalXmlNodeList GetPropagatedAttributes(XmlElement elem)
        {
            if (elem == null)
            {
                return null;
            }

            CanonicalXmlNodeList canonicalXmlNodeList = new CanonicalXmlNodeList();
            XmlNode xmlNode = elem;
            if (xmlNode == null)
            {
                return null;
            }

            bool flag = true;
            while (xmlNode != null)
            {
                if (!(xmlNode is XmlElement xmlElement))
                {
                    xmlNode = xmlNode.ParentNode;
                    continue;
                }

                XmlElement xmlElement2 = xmlElement;
                if (!IsCommittedNamespace(xmlElement2, xmlElement2.Prefix, xmlElement.NamespaceURI))
                {
                    XmlElement xmlElement3 = xmlElement;
                    if (!IsRedundantNamespace(xmlElement3, xmlElement3.Prefix, xmlElement.NamespaceURI))
                    {
                        string name = ((xmlElement.Prefix.Length > 0) ? ("xmlns:" + xmlElement.Prefix) : "xmlns");
                        XmlAttribute xmlAttribute = elem.OwnerDocument.CreateAttribute(name);
                        xmlAttribute.Value = xmlElement.NamespaceURI;
                        canonicalXmlNodeList.Add(xmlAttribute);
                    }
                }

                if (xmlElement.HasAttributes)
                {
                    foreach (XmlAttribute attribute in xmlElement.Attributes)
                    {
                        if (flag && attribute.LocalName == "xmlns")
                        {
                            XmlAttribute xmlAttribute3 = elem.OwnerDocument.CreateAttribute("xmlns");
                            xmlAttribute3.Value = attribute.Value;
                            canonicalXmlNodeList.Add(xmlAttribute3);
                            flag = false;
                        }
                        else if (attribute.Prefix == "xmlns" || attribute.Prefix == "xml")
                        {
                            canonicalXmlNodeList.Add(attribute);
                        }
                        else if (attribute.NamespaceURI.Length > 0 && !IsCommittedNamespace(xmlElement, attribute.Prefix, attribute.NamespaceURI) && !IsRedundantNamespace(xmlElement, attribute.Prefix, attribute.NamespaceURI))
                        {
                            string name2 = ((attribute.Prefix.Length > 0) ? ("xmlns:" + attribute.Prefix) : "xmlns");
                            XmlAttribute xmlAttribute4 = elem.OwnerDocument.CreateAttribute(name2);
                            xmlAttribute4.Value = attribute.NamespaceURI;
                            canonicalXmlNodeList.Add(xmlAttribute4);
                        }
                    }
                }

                xmlNode = xmlNode.ParentNode;
            }

            return canonicalXmlNodeList;
        }

        internal static bool IsCommittedNamespace(XmlElement element, string prefix, string value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            string name = ((prefix.Length > 0) ? ("xmlns:" + prefix) : "xmlns");
            return element.HasAttribute(name) && element.GetAttribute(name) == value;
        }

        internal static bool IsRedundantNamespace(XmlElement element, string prefix, string value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            for (XmlNode parentNode = element.ParentNode; parentNode != null; parentNode = parentNode.ParentNode)
            {
                if (parentNode is XmlElement element2 && HasNamespace(element2, prefix, value))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool HasNamespace(XmlElement element, string prefix, string value)
        {
            return IsCommittedNamespace(element, prefix, value) || (element.Prefix == prefix && element.NamespaceURI == value);
        }

        internal static void AddNamespaces(XmlElement elem, CanonicalXmlNodeList namespaces)
        {
            if (namespaces == null)
            {
                return;
            }

            foreach (XmlNode @namespace in namespaces)
            {
                string text = ((@namespace.Prefix.Length > 0) ? (@namespace.Prefix + ":" + @namespace.LocalName) : @namespace.LocalName);
                if (!elem.HasAttribute(text) && (!text.Equals("xmlns") || elem.Prefix.Length != 0))
                {
                    XmlAttribute xmlAttribute = elem.OwnerDocument.CreateAttribute(text);
                    xmlAttribute.Value = @namespace.Value;
                    elem.SetAttributeNode(xmlAttribute);
                }
            }
        }
    }

    internal class CanonicalXmlNodeList : XmlNodeList, IList, ICollection, IEnumerable
    {
        private readonly ArrayList m_nodeArray;

        public override int Count => m_nodeArray.Count;

        public bool IsFixedSize => m_nodeArray.IsFixedSize;

        public bool IsReadOnly => m_nodeArray.IsReadOnly;

        object IList.this[int index]
        {
            get
            {
                return m_nodeArray[index];
            }
            set
            {
                ArrayList nodeArray = m_nodeArray;
                if (!(value is XmlNode))
                {
                    throw new ArgumentException("Cryptography_Xml_IncorrectObjectType");
                }

                nodeArray[index] = value;
            }
        }

        public object SyncRoot => m_nodeArray.SyncRoot;

        public bool IsSynchronized => m_nodeArray.IsSynchronized;

        internal CanonicalXmlNodeList()
        {
            m_nodeArray = new ArrayList();
        }

        public override XmlNode Item(int index)
        {
            return (XmlNode)m_nodeArray[index];
        }

        public override IEnumerator GetEnumerator()
        {
            return m_nodeArray.GetEnumerator();
        }

        public int Add(object value)
        {
            if (!(value is XmlNode))
            {
                throw new ArgumentException("Cryptography_Xml_IncorrectObjectType");
            }

            return m_nodeArray.Add(value);
        }

        public void Clear()
        {
            m_nodeArray.Clear();
        }

        public bool Contains(object value)
        {
            return m_nodeArray.Contains(value);
        }

        public int IndexOf(object value)
        {
            return m_nodeArray.IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            if (!(value is XmlNode))
            {
                throw new ArgumentException("Cryptography_Xml_IncorrectObjectType");
            }

            m_nodeArray.Insert(index, value);
        }

        public void Remove(object value)
        {
            m_nodeArray.Remove(value);
        }

        public void RemoveAt(int index)
        {
            m_nodeArray.RemoveAt(index);
        }

        public void CopyTo(Array array, int index)
        {
            m_nodeArray.CopyTo(array, index);
        }

        internal static CanonicalXmlNodeList GetPropagatedAttributes(XmlElement elem)
        {
            if (elem == null)
            {
                return null;
            }

            CanonicalXmlNodeList canonicalXmlNodeList = new CanonicalXmlNodeList();
            XmlNode xmlNode = elem;
            if (xmlNode == null)
            {
                return null;
            }

            bool flag = true;
            while (xmlNode != null)
            {
                if (!(xmlNode is XmlElement xmlElement))
                {
                    xmlNode = xmlNode.ParentNode;
                    continue;
                }

                XmlElement xmlElement2 = xmlElement;
                if (!IsCommittedNamespace(xmlElement2, xmlElement2.Prefix, xmlElement.NamespaceURI))
                {
                    XmlElement xmlElement3 = xmlElement;
                    if (!IsRedundantNamespace(xmlElement3, xmlElement3.Prefix, xmlElement.NamespaceURI))
                    {
                        string name = ((xmlElement.Prefix.Length > 0) ? ("xmlns:" + xmlElement.Prefix) : "xmlns");
                        XmlAttribute xmlAttribute = elem.OwnerDocument.CreateAttribute(name);
                        xmlAttribute.Value = xmlElement.NamespaceURI;
                        canonicalXmlNodeList.Add(xmlAttribute);
                    }
                }

                if (xmlElement.HasAttributes)
                {
                    foreach (XmlAttribute attribute in xmlElement.Attributes)
                    {
                        if (flag && attribute.LocalName == "xmlns")
                        {
                            XmlAttribute xmlAttribute3 = elem.OwnerDocument.CreateAttribute("xmlns");
                            xmlAttribute3.Value = attribute.Value;
                            canonicalXmlNodeList.Add(xmlAttribute3);
                            flag = false;
                        }
                        else if (attribute.Prefix == "xmlns" || attribute.Prefix == "xml")
                        {
                            canonicalXmlNodeList.Add(attribute);
                        }
                        else if (attribute.NamespaceURI.Length > 0 && !IsCommittedNamespace(xmlElement, attribute.Prefix, attribute.NamespaceURI) && !IsRedundantNamespace(xmlElement, attribute.Prefix, attribute.NamespaceURI))
                        {
                            string name2 = ((attribute.Prefix.Length > 0) ? ("xmlns:" + attribute.Prefix) : "xmlns");
                            XmlAttribute xmlAttribute4 = elem.OwnerDocument.CreateAttribute(name2);
                            xmlAttribute4.Value = attribute.NamespaceURI;
                            canonicalXmlNodeList.Add(xmlAttribute4);
                        }
                    }
                }

                xmlNode = xmlNode.ParentNode;
            }

            return canonicalXmlNodeList;
        }

        public static XmlElement FindNodeWithAttributeValueIn(XmlNodeList nodes, string attr, string value)
        {
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes != null && node.Attributes[attr] != null && node.Attributes[attr].Value == value)
                {
                    return (XmlElement)node;
                }
            }

            return null;
        }

        internal static bool IsCommittedNamespace(XmlElement element, string prefix, string value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            string name = ((prefix.Length > 0) ? ("xmlns:" + prefix) : "xmlns");
            return element.HasAttribute(name) && element.GetAttribute(name) == value;
        }

        internal static bool IsRedundantNamespace(XmlElement element, string prefix, string value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            for (XmlNode parentNode = element.ParentNode; parentNode != null; parentNode = parentNode.ParentNode)
            {
                if (parentNode is XmlElement element2 && HasNamespace(element2, prefix, value))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool HasNamespace(XmlElement element, string prefix, string value)
        {
            return IsCommittedNamespace(element, prefix, value) || (element.Prefix == prefix && element.NamespaceURI == value);
        }

        internal static void AddNamespaces(XmlElement elem, CanonicalXmlNodeList namespaces)
        {
            if (namespaces == null)
            {
                return;
            }

            foreach (XmlNode @namespace in namespaces)
            {
                string text = ((@namespace.Prefix.Length > 0) ? (@namespace.Prefix + ":" + @namespace.LocalName) : @namespace.LocalName);
                if (!elem.HasAttribute(text) && (!text.Equals("xmlns") || elem.Prefix.Length != 0))
                {
                    XmlAttribute xmlAttribute = elem.OwnerDocument.CreateAttribute(text);
                    xmlAttribute.Value = @namespace.Value;
                    elem.SetAttributeNode(xmlAttribute);
                }
            }
        }
    }
}
