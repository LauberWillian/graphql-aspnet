// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.TypeTemplates
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using GraphQL.AspNet.Common;

    /// <summary>
    /// A class that read in the generated XML file for using DocFx (triple-slash comments) to be used as descriptions
    /// for types, methods, properties and parameters in lue of <see cref="DescriptionAttribute"/>.
    /// </summary>
    public class DocFXReader : IDisposable
    {
        private const string SUMMARY_ELEMENT = "summary";

        /// <summary>
        /// Attempts to create a qualfied DocFx file location for the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly to create a file reference for.</param>
        /// <returns>The fully qualified file path for the expected docFx file.</returns>
        private string CreateFilePathFromAssembly(Assembly assembly)
        {
            Validation.ThrowIfNull(assembly, nameof(assembly));

            var assemblyFile = new FileInfo(assembly.Location);
            if (!assemblyFile.Exists)
                throw new FileNotFoundException("Unable to locate the supplied assembly on disk.", assemblyFile.FullName);

            var fileFolder = assemblyFile.Directory.FullName;

            var fileName = assemblyFile.Name;
            while (fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                fileName = fileName.Substring(0, fileName.Length - 4);

            fileName += ".xml";
            fileName = Path.Combine(fileFolder, fileName);
            return fileName;
        }

        private Dictionary<string, XmlDocument> _xmlDocuments;
        private List<string> _fileFolders;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocFXReader" /> class.
        /// </summary>
        /// <param name="fileFolders">A set of folders on disk to search for docFx files. The assembly
        /// location is automatically searched for any type or member requested.</param>
        public DocFXReader(params string[] fileFolders)
            : this((IEnumerable<string>)fileFolders)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocFXReader"/> class.
        /// </summary>
        /// <param name="fileFolders">A set of folders on disk to search for docFx files. The assembly
        /// location is automatically searched for any type or member requested.</param>
        public DocFXReader(IEnumerable<string> fileFolders = null)
        {
            _xmlDocuments = new Dictionary<string, XmlDocument>();
            _fileFolders = new List<string>(fileFolders ?? Enumerable.Empty<string>());
        }

        /// <summary>
        /// Attempts to load and cache a valid xml document representing the docFx comments
        /// for the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly to retrieve documentation for.</param>
        /// <returns>the xml document containing the documentation or null.</returns>
        private XmlDocument LoadDocument(Assembly assembly)
        {
            // make a key that is the name of the assembly file name (minus extension)
            var location = new FileInfo(assembly.Location);
            string key = location.Name;
            if (key.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                key = key.Substring(0, key.Length - 4);

            if (_xmlDocuments.ContainsKey(key))
                return _xmlDocuments[key];

            // if the assembly is not really on disk, just stop
            if (!location.Exists)
                return null;

            var foldersToSearch = new List<string>();
            foldersToSearch.Add(location.Directory.FullName);
            if (_fileFolders.Count > 0)
                foldersToSearch.AddRange(_fileFolders);

            foreach (var folder in foldersToSearch)
            {
                var filePath = Path.Combine(folder, $"{key}.xml");
                if (File.Exists(filePath))
                {
                    try
                    {
                        var xmlDocument = new XmlDocument();
                        xmlDocument.Load(filePath);

                        _xmlDocuments.Add(key, xmlDocument);
                        return xmlDocument;
                    }
                    catch
                    {
                    }
                }
            }

            // couldn't find the file, no need to ever try again
            _xmlDocuments.Add(key, null);
            return null;
        }

        private string CreateKeyName(Type type, MemberInfo member = null)
        {
            string keyPrefix = "T";
            string keySuffix = null;
            if (member != null)
            {
                if (member is PropertyInfo pi)
                {
                    keyPrefix = "P";
                    keySuffix = pi.Name;
                }
                else if (member is MethodInfo mi)
                {
                    keyPrefix = "M";
                    keySuffix = mi.Name;
                }
            }

            if (!string.IsNullOrWhiteSpace(keySuffix))
                keySuffix = "." + keySuffix;

            return $"{keyPrefix}:{type.FullName}{keySuffix}";
        }

        /// <summary>
        /// Creates a complete set of keys for the given type and optional member in the following order.
        /// Supplied Type => Inherited Class(s) => Implmented Interfaces => Interfaces of Inherited Classes.
        /// </summary>
        /// <param name="type">The type to key off of.</param>
        /// <param name="member">The member of the type, if needed.</param>
        /// <returns>IEnumerable&lt;System.String&gt;.</returns>
        private IEnumerable<(string keyName, Assembly assembly)> GenerateKeyList(Type type, MemberInfo member = null)
        {
            // key/assembly fro the provided type
            yield return (this.CreateKeyName(type, member), type.Assembly);

            var interfacesToWalk = new List<Type>();
            interfacesToWalk.AddRange(type.GetInterfaces());

            // each inherited type
            var inheritsFrom = type.BaseType;
            while (inheritsFrom != null && inheritsFrom != typeof(object))
            {
                yield return (this.CreateKeyName(inheritsFrom, member), inheritsFrom.Assembly);
                interfacesToWalk.AddRange(inheritsFrom.GetInterfaces());
                inheritsFrom = inheritsFrom.BaseType;
            }

            // implemented interfaces
            foreach (var iface in interfacesToWalk)
            {
                yield return (this.CreateKeyName(iface, member), iface.Assembly);
            }
        }

        /// <summary>
        /// Attempts to read the class/interface header summary for the given type.
        /// </summary>
        /// <param name="type">The type to extract a description for.</param>
        /// <returns>System.String.</returns>
        public string ReadDescription(Type type)
        {
            return this.ReadSummaryText(type);
        }

        /// <summary>
        /// Attempts to read the description of a given property or method.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>System.String.</returns>
        public string ReadDescription(MemberInfo member)
        {
            return this.ReadSummaryText(member.DeclaringType, member);
        }

        /// <summary>
        /// Attempts to read the description for a given parameter of a method.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>System.String.</returns>
        public string ReadDescription(ParameterInfo member)
        {
            return null;
        }

        private string ReadSummaryText(Type type, MemberInfo member = null)
        {
            string exactKey = null;
            var keyWasMatched = false;
            foreach (var keySet in this.GenerateKeyList(type, member))
            {
                if (keyWasMatched)
                    break;

                if (exactKey != null)
                {
                    keyWasMatched = exactKey == keySet.keyName;
                    if (!keyWasMatched)
                        continue;
                }

                var doc = this.LoadDocument(keySet.assembly);
                if (doc == null)
                    continue;

                // if a key is not defined then it has no doxFx text (not summary or inheritdoc)
                // is defined, we can safely just stop
                var node = this.FindNode(doc, keySet.keyName);
                if (node == null)
                    continue;

                // try and read the
                var result = this.ReadElementText(node, SUMMARY_ELEMENT)?.Trim();
                if (!string.IsNullOrWhiteSpace(result))
                    return result;

                if (this.IsInheritDoc(node, out string inheritFrom))
                    exactKey = inheritFrom;
            }

            return null;
        }

        private XmlNode FindNode(XmlDocument doc, string key)
        {
            try
            {
                var nodes = doc?.DocumentElement?.SelectNodes($"//member[@name='{key}']");
                if (nodes != null && nodes.Count == 1)
                    return nodes[0];
            }
            catch
            {
            }

            return null;
        }

        private string ReadElementText(XmlNode node, string elementName)
        {
            return node?.SelectSingleNode(elementName)?.InnerText;
        }

        private bool IsInheritDoc(XmlNode node, out string inheritFrom)
        {
            inheritFrom = null;
            var inheritNode = node?.SelectSingleNode("inheritdoc");
            if (inheritNode == null)
                return false;

            try
            {
                inheritFrom = inheritNode.Attributes["cref"]?.Value;
            }
            catch
            {
            }

            return true;
        }

        /// <summary>
        /// Closes this docFx reader, releasing any contained resources.
        /// </summary>
        public void Close()
        {
            foreach (var key in _xmlDocuments.Keys)
                _xmlDocuments[key] = null;

            _xmlDocuments = null;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="isDisposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            this.Close();
        }

        /// <summary>
        /// Gets the name of the loaded xml file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName { get; private set; }
    }
}