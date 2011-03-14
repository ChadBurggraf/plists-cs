//-----------------------------------------------------------------------
// <copyright file="BinaryPlistDictionary.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
//     Inspired by BinaryPListParser.java, copyright (c) 2005 Werner Randelshofer
//          http://www.java2s.com/Open-Source/Java-Document/Swing-Library/jide-common/com/jidesoft/plaf/aqua/BinaryPListParser.java.htm
// </copyright>
//-----------------------------------------------------------------------

namespace System.Runtime.Serialization.Plists
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    /// <summary>
    /// Represents a dictionary in a binary plist.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    internal class BinaryPlistDictionary
    {
        /// <summary>
        /// Initializes a new instance of the BinaryPlistDictionary class.
        /// </summary>
        /// <param name="objectTable">A reference to the binary plist's object table.</param>
        public BinaryPlistDictionary(IList<object> objectTable)
            : this(objectTable, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BinaryPlistDictionary class.
        /// </summary>
        /// <param name="objectTable">A reference to the binary plist's object table.</param>
        /// <param name="size">The size of the dictionay.</param>
        public BinaryPlistDictionary(IList<object> objectTable, int size)
        {
            this.KeyReference = new List<int>(size);
            this.ObjectReference = new List<int>(size);
            this.ObjectTable = objectTable;
        }

        /// <summary>
        /// Gets the dictionary's key reference collection.
        /// </summary>
        public IList<int> KeyReference { get; private set; }

        /// <summary>
        /// Gets the dictionary's object reference collection.
        /// </summary>
        public IList<int> ObjectReference { get; private set; }

        /// <summary>
        /// Gets a reference to the binary plist's object table.
        /// </summary>
        public IList<object> ObjectTable { get; private set; }

        /// <summary>
        /// Gets the key at the specified index as a string.
        /// </summary>
        /// <param name="index">The index in the key reference collection to get the key at.</param>
        /// <returns>The specified key as a string.</returns>
        public string GetKey(int index)
        {
            return this.ObjectTable[this.KeyReference[index]].ToString();
        }

        /// <summary>
        /// Gets the object value at the specified index.
        /// </summary>
        /// <param name="index">The index in the object reference collection to get the object value at.</param>
        /// <returns>The specified object value.</returns>
        public object GetValue(int index)
        {
            return this.ObjectTable[this.ObjectReference[index]];
        }

        /// <summary>
        /// Converts this instance into a <see cref="Dictionary{Object, Object}"/>.
        /// </summary>
        /// <returns>A <see cref="Dictionary{Object, Object}"/> representation this instance.</returns>
        public Dictionary<object, object> ToDictionary()
        {
            Dictionary<object, object> dictionary = new Dictionary<object, object>();
            int keyRef, objectRef;
            object objectValue;
            BinaryPlistArray innerArray;
            BinaryPlistDictionary innerDict;

            for (int i = 0; i < this.KeyReference.Count; i++)
            {
                keyRef = this.KeyReference[i];
                objectRef = this.ObjectReference[i];

                if (keyRef >= 0 && keyRef < this.ObjectTable.Count && this.ObjectTable[keyRef] != this &&
                    objectRef >= 0 && objectRef < this.ObjectTable.Count && this.ObjectTable[objectRef] != this)
                {
                    objectValue = this.ObjectTable[objectRef];
                    innerDict = objectValue as BinaryPlistDictionary;

                    if (innerDict != null)
                    {
                        objectValue = innerDict.ToDictionary();
                    }
                    else
                    {
                        innerArray = objectValue as BinaryPlistArray;

                        if (innerArray != null)
                        {
                            objectValue = innerArray.ToArray();
                        }
                    }

                    dictionary[this.ObjectTable[keyRef]] = objectValue;
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Returns the string representation of this instance.
        /// </summary>
        /// <returns>This instance's string representation.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("{");
            int keyRef, objectRef;

            for (int i = 0; i < this.KeyReference.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(",");
                }

                keyRef = this.KeyReference[i];
                objectRef = this.ObjectReference[i];

                if (keyRef < 0 || keyRef >= this.ObjectTable.Count)
                {
                    sb.Append("#" + keyRef);
                }
                else if (this.ObjectTable[keyRef] == this)
                {
                    sb.Append("*" + keyRef);
                }
                else
                {
                    sb.Append(this.ObjectTable[keyRef]);
                }

                sb.Append(":");

                if (objectRef < 0 || objectRef >= this.ObjectTable.Count)
                {
                    sb.Append("#" + objectRef);
                }
                else if (this.ObjectTable[objectRef] == this)
                {
                    sb.Append("*" + objectRef);
                }
                else
                {
                    sb.Append(this.ObjectTable[objectRef]);
                }
            }

            return sb.ToString() + "}";
        }
    }
}
