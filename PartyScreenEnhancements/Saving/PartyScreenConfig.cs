﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using HarmonyLib;
using PartyScreenEnhancements.Comparers;

namespace PartyScreenEnhancements.Saving
{
    /// <summary>
    ///     Primary Setting class, used for both storing values as well as serialization to XML
    ///     Makes use of <see cref="ExtraSettings" />
    /// </summary>
    public static class PartyScreenConfig
    {
        internal const double VERSION = 1.02;

        internal static Dictionary<string, int> PathsToUpgrade = new();
        internal static Dictionary<string, int> PrisonersToRecruit = new();

        internal static PartySort DefaultSorter =
            new TypeComparer(new TrueTierComparer(new AlphabetComparer(null, false), true), false);

        internal static ExtraSettings ExtraSettings = new();

        private static readonly string _FILENAME = Directories.GetConfigPathForFile("PartyScreenEnhancements.xml");

        // Used to reset Sorters to their initial state in case some changes were made.
        private static bool _upgradedVersion = true;

        public static void Initialize()
        {
            if (!File.Exists(_FILENAME))
            {
                Save();
            }
            else
            {
                Load();
                if (_upgradedVersion) Save();
            }
        }

        public static void Save()
        {
            try
            {
                var xmlDocument = new XmlDocument();

                var xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDocument.InsertBefore(xmlDeclaration, xmlDocument.DocumentElement);
                var modNode = xmlDocument.CreateElement("PartyScreenConfig");
                xmlDocument.AppendChild(modNode);

                var options = xmlDocument.CreateElement("Options");
                var version = xmlDocument.CreateElement("Version");
                version.InnerText = VERSION.ToString();

                var node = xmlDocument.CreateNode(XmlNodeType.Text, "test", null);

                options.AppendChild(version);
                options.AppendChild(node);

                node = node.ReplaceWithSerializationOf(ExtraSettings);

                modNode.AppendChild(options);

                addDictionaryToXML(ref PathsToUpgrade, ref xmlDocument, ref modNode, "UpgradePaths");
                addDictionaryToXML(ref PrisonersToRecruit, ref xmlDocument, ref modNode, nameof(PrisonersToRecruit));

                xmlDocument.Save(_FILENAME);
            }
            catch (Exception e)
            {
                FileLog.Log(e.ToString());
            }
        }

        private static void addDictionaryToXML(ref Dictionary<string, int> dictionary, ref XmlDocument document,
            ref XmlElement parent, string name)
        {
            var el = new XElement(name,
                dictionary.Select(kv => new XElement(kv.Key, kv.Value)));

            var element = document.ReadNode(el.CreateReader()) as XmlElement;

            parent.AppendChild(element);
        }

        public static void Load()
        {
            if (!File.Exists(_FILENAME)) return;

            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(_FILENAME);

                foreach (var obj in xmlDocument.DocumentElement.ChildNodes)
                {
                    var xmlNode = (XmlNode)obj;
                    if (xmlNode.Name == "UpgradePaths")
                    {
                        var rootElement = XElement.Parse(xmlNode.OuterXml);
                        PathsToUpgrade = rootElement.Elements()
                            .ToDictionary(key => key.Name.LocalName, val => int.Parse(val.Value));
                    }

                    if (xmlNode.Name == nameof(PrisonersToRecruit))
                    {
                        var rootElement = XElement.Parse(xmlNode.OuterXml);
                        PrisonersToRecruit = rootElement.Elements()
                            .ToDictionary(key => key.Name.LocalName, val => int.Parse(val.Value));
                    }

                    if (xmlNode.Name == "Options")
                    {
                        var options = XElement.Parse(xmlNode.OuterXml);
                        foreach (var element in options.Elements())
                        {
                            if (element.Name == "Version")
                                if (double.Parse(element.Value) == VERSION)
                                    _upgradedVersion = false;

                            if (element.Name == nameof(ExtraSettings))
                                try
                                {
                                    using (var writer = element.CreateNavigator().ReadSubtree())
                                    {
                                        var test = new XmlSerializer(typeof(ExtraSettings));
                                        ExtraSettings = test.Deserialize(writer) as ExtraSettings;
                                    }
                                }
                                catch (Exception e)
                                {
                                    throw new XmlException(
                                        "Could not load Sorter.xml from PartyScreenEnhancements, please try again!" +
                                        e);
                                }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                Save();
            }
        }
    }

    public static class XmlNodeExtensions
    {
        public static XmlNode ReplaceWithSerializationOf<T>(this XmlNode node, T replacement)
        {
            if (node == null)
                throw new ArgumentNullException();
            var parent = node.ParentNode;
            var serializer = new XmlSerializer(replacement == null ? typeof(T) : replacement.GetType());

            using (var writer = node.CreateNavigator().InsertAfter())
            {
                writer.WriteWhitespace("");

                var ns = new XmlSerializerNamespaces();
                ns.Add(node.GetNamespaceOfPrefix(node.NamespaceURI), node.NamespaceURI);

                serializer.Serialize(writer, replacement, ns);
            }

            var nextNode = node.NextSibling;
            parent.RemoveChild(node);
            return nextNode;
        }
    }
}