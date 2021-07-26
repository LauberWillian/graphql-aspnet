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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Caching;
    using System.Xml;
    using GraphQL.AspNet.Common;

    /// <summary>
    /// A class that read in the generated XML file for using DocFx (triple-slash comments) to be used as descriptions
    /// for types, methods, properties and parameters in lue of <see cref="DescriptionAttribute"/>.
    /// </summary>
    public partial class DocFxReader : IDisposable
    {
        private const string SUMMARY_ELEMENT = "summary";
        private static readonly TimeSpan _defaultCacheTime = TimeSpan.FromSeconds(15);

        private class CachedXmlFile
        {
            public CachedXmlFile(XmlDocument document, TimeSpan expiresAfter)
            {
                this.Document = document;
                this.ExpiresAfter = DateTimeOffset.UtcNow.Add(expiresAfter);
            }

            public XmlDocument Document { get; }

            public DateTimeOffset ExpiresAfter { get; }
        }

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

        private readonly TimeSpan _cacheExpiryTime;

        private MemoryCache _xmlCache;
        private List<string> _fileFolders;
        private HashSet<string> _usedKeys;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocFxReader" /> class.
        /// </summary>
        /// <param name="cacheExpiryTime">The amount of time to hold loaded docFx files
        /// in memory before purging. When null, a default time of 15 seconds is used.</param>
        /// <param name="fileFolders">A set of folders on disk to search for docFx files. The assembly
        /// location is automatically searched for any type or member requested.</param>
        public DocFxReader(
            TimeSpan? cacheExpiryTime = null,
            params string[] fileFolders)
            : this(cacheExpiryTime, (IEnumerable<string>)fileFolders)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocFxReader"/> class.
        /// </summary>
        /// <param name="cacheExpiryTime">The amount of time to hold loaded docFx files
        /// in memory before purging. When null, a default time of 15 seconds is used.</param>
        /// <param name="fileFolders">A set of folders on disk to search for docFx files. The assembly
        /// location is automatically searched for any type or member requested.</param>
        public DocFxReader(
            TimeSpan? cacheExpiryTime = null,
            IEnumerable<string> fileFolders = null)
        {
            _cacheExpiryTime = cacheExpiryTime ?? _defaultCacheTime;
            _xmlCache = new MemoryCache(nameof(DocFxReader) + "Cache");
            _fileFolders = new List<string>(fileFolders ?? Enumerable.Empty<string>());
            _usedKeys = new HashSet<string>();
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
            string cacheKey = location.Name;
            if (cacheKey.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                cacheKey = cacheKey.Substring(0, cacheKey.Length - 4);

            var cacheItem = _xmlCache.GetCacheItem(cacheKey);
            if (cacheItem != null)
            {
                return cacheItem.Value as XmlDocument;
            }

            // if the assembly is not really on disk, just stop
            if (!location.Exists)
                return null;

            var foldersToSearch = new List<string>();
            foldersToSearch.Add(location.Directory.FullName);
            if (_fileFolders.Count > 0)
                foldersToSearch.AddRange(_fileFolders);

            XmlDocument xmlDocument = null;
            foreach (var folder in foldersToSearch)
            {
                var filePath = Path.Combine(folder, $"{cacheKey}.xml");
                if (File.Exists(filePath))
                {
                    try
                    {
                        xmlDocument = new XmlDocument();
                        xmlDocument.Load(filePath);
                    }
                    catch
                    {
                        xmlDocument = null;
                    }
                }

                if (xmlDocument != null)
                    break;
            }

            xmlDocument = xmlDocument ?? new XmlDocument();

            // cache the file or a null reference
            var cachePolicy = new CacheItemPolicy();
            cachePolicy.SlidingExpiration = _cacheExpiryTime;

            _xmlCache.Add(new CacheItem(cacheKey, xmlDocument), cachePolicy);
            _usedKeys.Add(cacheKey);
            return xmlDocument;
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

        /// <summary>
        /// Attempts to loop through the type, the types it inherits from and the interfaces
        /// it implements searching for docfx key that can satify the request for the 'summary' xml element.
        /// This method will automatically follow any 'inheritdoc' elements according to their
        /// specificity.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="member">The individual member on the type if any. When null the type
        /// itself will be inspected for a class/interface level 'summary' element.</param>
        /// <returns>System.String.</returns>
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

        /// <summary>
        /// Determines whether the given node represents a member that defines an inherit
        /// doc statement pointing to another member for its documentation.
        /// </summary>
        /// <param name="node">The node to inspect.</param>
        /// <param name="inheritFrom">The specific cref value in the inheritdoc element, if found. Value is
        /// null when no cref is set.</param>
        /// <returns><c>true</c> if the member does define an inheritdoc element; <c>false</c> otherwise.</returns>
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
            if (isDisposing)
            {
                _usedKeys.Clear();
                _xmlCache.Dispose();
            }
        }

        /// <summary>
        /// Gets the name of the loaded xml file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName { get; private set; }
    }
}