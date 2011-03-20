//-----------------------------------------------------------------------
// <copyright file="DataContractBinaryPlistSerializer.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Runtime.Serialization.Plists
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;

    /// <summary>
    /// Serializes data contracts to and from the binary plist format.
    /// </summary>
    public sealed class DataContractBinaryPlistSerializer
    {
        private Type rootType;
        private bool isDictionary;
        private Dictionary<Type, TypeCacheItem> typeCache;

        /// <summary>
        /// Initializes a new instance of the DataContractBinaryPlistSerializer class.
        /// </summary>
        /// <param name="type">The type of the instances that are serialized or de-serialized.</param>
        public DataContractBinaryPlistSerializer(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type", "type cannot be null.");
            }

            this.isDictionary = typeof(IDictionary).IsAssignableFrom(type);

            if (!this.isDictionary && type.IsCollection())
            {
                throw new ArgumentException("root type cannot be a collection unless it is an IDictionary implementation.", "type");
            }

            if (type.IsPrimitiveOrEnum())
            {
                throw new ArgumentException("type must be an implementation of IDictionary or a complex object type.", "type");
            }

            this.rootType = type;
            this.typeCache = new Dictionary<Type, TypeCacheItem>();
        }

        /// <summary>
        /// Reads an object from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The de-serialized object.</returns>
        public object ReadObject(Stream stream)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes the complete content of the given object to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="graph">The object to write.</param>
        public void WriteObject(Stream stream, object graph)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream", "stream cannot be null.");
            }

            if (graph == null)
            {
                throw new ArgumentNullException("graph", "graph cannot be null.");
            }

            Type type = graph.GetType();

            if (!this.rootType.IsAssignableFrom(type))
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The object specified is of type {0}, which is not assignable to this instance's root type, {1}.", type, this.rootType));
            }

            new BinaryPlistWriter().WriteObject(stream, this.GetPlistObject(type, graph) as IDictionary);
        }

        /// <summary>
        /// Gets the plist value of the given object identified by the specified type.
        /// </summary>
        /// <param name="type">The of the object.</param>
        /// <param name="obj">The object to get the plist value of.</param>
        /// <returns>The plist value of the given object.</returns>
        private object GetPlistObject(Type type, object obj)
        {
            object result = null;

            if (obj != null)
            {
                if (typeof(IPlistSerializable).IsAssignableFrom(type))
                {
                    result = ((IPlistSerializable)obj).ToPlistDictionary();
                }
                else if (typeof(IDictionary).IsAssignableFrom(type))
                {
                    IDictionary dict = obj as IDictionary;
                    Dictionary<object, object> resultDict = new Dictionary<object, object>();

                    foreach (object key in dict)
                    {
                        object value = dict[key];
                        resultDict[this.GetPlistObject(key.GetType(), key)] = this.GetPlistObject(value.GetType(), value);
                    }

                    result = resultDict;
                }
                else if (type.IsCollection())
                {
                    IEnumerable coll = obj as IEnumerable;
                    List<object> resultColl = new List<object>();

                    foreach (object value in coll)
                    {
                        resultColl.Add(this.GetPlistObject(value.GetType(), value));
                    }

                    result = resultColl;
                }
                else if (type.IsPrimitiveOrEnum())
                {
                    result = obj;
                }
                else
                {
                    if (!this.typeCache.ContainsKey(type))
                    {
                        this.typeCache[type] = new TypeCacheItem(type);
                    }

                    TypeCacheItem cache = this.typeCache[type];
                    Dictionary<string, object> resultDict = new Dictionary<string, object>();

                    for (int i = 0; i < cache.Fields.Count; i++)
                    {
                        FieldInfo field = cache.Fields[i];
                        DataMemberAttribute member = cache.FieldMembers[i];
                        object fieldValue = field.GetValue(obj);

                        if (member.EmitDefaultValue || !field.FieldType.IsDefaultValue(fieldValue))
                        {
                            resultDict[member.Name] = this.GetPlistObject(field.FieldType, fieldValue);
                        }
                    }

                    for (int i = 0; i < cache.Properties.Count; i++)
                    {
                        PropertyInfo property = cache.Properties[i];
                        DataMemberAttribute member = cache.PropertyMembers[i];
                        object propertyValue = property.GetValue(obj, null);

                        if (member.EmitDefaultValue || !property.PropertyType.IsDefaultValue(propertyValue))
                        {
                            resultDict[member.Name] = this.GetPlistObject(property.PropertyType, propertyValue);
                        }
                    }

                    result = resultDict;
                }
            }

            return result;
        }
    }
}
