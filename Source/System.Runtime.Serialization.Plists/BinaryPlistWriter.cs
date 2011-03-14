//-----------------------------------------------------------------------
// <copyright file="BinaryPlistWriter.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Runtime.Serialization.Plists
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;

    /// <summary>
    /// Performs serialization of objects into binary plist format.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public sealed class BinaryPlistWriter
    {
        #region Internal Fields

        /// <summary>
        /// Gets the magic number value used in a binary plist header.
        /// </summary>
        internal const uint HeaderMagicNumber = 0x62706c69;

        /// <summary>
        /// Gets the version number value used in a binary plist header.
        /// </summary>
        internal const uint HeaderVersionNumber = 0x73743030;

        /// <summary>
        /// Gets Apple's reference date value.
        /// </summary>
        internal static readonly DateTime ReferenceDate = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        #endregion

        #region Private Fields

        private List<object> objectTable;
        private List<long> offsetTable;
        private int topLevelObjectOffset, offsetIntSize, objectRefSize, maxCollectionSize;

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Writes the specified <see cref="IPlistSerializable"/> object to the given file path as a binary plist.
        /// </summary>
        /// <param name="path">The file path to write to.</param>
        /// <param name="obj">The <see cref="IPlistSerializable"/> object to write.</param>
        public void WriteObject(string path, IPlistSerializable obj)
        {
            using (FileStream stream = File.Create(path))
            {
                this.WriteObject(stream, obj);
            }
        }

        /// <summary>
        /// Writes the specified <see cref="IPlistSerializable"/> object to the given stream as a binary plist.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="obj">The <see cref="IPlistSerializable"/> object to write.</param>
        public void WriteObject(Stream stream, IPlistSerializable obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj", "obj cannot be null.");
            }

            this.WriteObject(stream, obj.ToPlistDictionary());
        }

        /// <summary>
        /// Writes the specified <see cref="IDictionary"/> object to the given file path as a binary plist.
        /// </summary>
        /// <param name="path">The file path to write to.</param>
        /// <param name="dictionary">The <see cref="IDictionary"/> object to write.</param>
        public void WriteObject(string path, IDictionary dictionary)
        {
            using (FileStream stream = File.Create(path))
            {
                this.WriteObject(stream, dictionary);
            }
        }

        /// <summary>
        /// Writes the specified <see cref="IDictionary"/> object to the given stream as a binary plist.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="dictionary">The <see cref="IDictionary"/> object to write.</param>
        public void WriteObject(Stream stream, IDictionary dictionary)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream", "stream cannot be null.");
            }

            if (!stream.CanWrite)
            {
                throw new ArgumentException("The stream must be writable.", "stream");
            }

            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary", "dictionary cannot be null.");
            }

            // Reset the state and then build the object table.
            this.Reset();
            this.AddDictionary(dictionary);

            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                long baseOffset = stream.Position;

                // Write the header.
                writer.Write(HeaderMagicNumber.ToBigEndianConditional());
                writer.Write(HeaderVersionNumber.ToBigEndianConditional());

                // Write the object table.
                this.topLevelObjectOffset = 8;
                this.offsetIntSize = ByteSizeForRefCount(this.offsetTable.Count);
                this.objectRefSize = ByteSizeForRefCount(this.objectTable.Count);
                this.WriteObjectTable(writer);

                // Write the offset table.
                long offsetTableOffset = stream.Position - baseOffset;

                foreach (int offset in this.offsetTable)
                {
                    WriteReferenceInteger(writer, offset, this.offsetIntSize);
                }

                // Write the trailer.
                stream.Position += 6;
                writer.Write((byte)this.offsetIntSize);
                writer.Write((byte)this.objectRefSize);
                writer.Write(((long)this.objectTable.Count).ToBigEndianConditional());
                writer.Write((long)0);
                writer.Write(offsetTableOffset.ToBigEndianConditional());
            }
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Gets the number of bytes to use for referene values given the provided number of values
        /// being addressed.
        /// </summary>
        /// <param name="count">The number of values being addressed.</param>
        /// <returns>The number of bytes required.</returns>
        private static int ByteSizeForRefCount(int count)
        {
            int size = 1;

            if (count > 255)
            {
                size = 1 << size;
            }

            if (count > 65535)
            {
                size = 1 << size;
            }

            return size;
        }

        /// <summary>
        /// Gets a big-endian byte array that corresponds to the given integer value.
        /// </summary>
        /// <param name="value">The integer value to get bytes for.</param>
        /// <returns>A big-endian byte array.</returns>
        private static byte[] GetIntegerBytes(long value)
        {
            if (value >= 0 && value < 128)
            {
                return new byte[] { (byte)value };
            }
            else if (value >= Int16.MinValue && value <= Int16.MaxValue)
            {
                return BitConverter.GetBytes(((short)value).ToBigEndianConditional());
            }
            else if (value >= Int32.MinValue && value <= Int32.MaxValue)
            {
                return BitConverter.GetBytes(((int)value).ToBigEndianConditional());
            }
            else
            {
                return BitConverter.GetBytes(value.ToBigEndianConditional());
            }
        }

        /// <summary>
        /// Writes an object as raw data to the given <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        /// <param name="value">The object to write.</param>
        /// <returns>The number of bytes written.</returns>
        private static int WriteData(BinaryWriter writer, object value)
        {
            int size = 1, index = 0, count;
            byte[] buffer = value as byte[];

            if (buffer == null)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, value);

                    stream.Position = 0;
                    buffer = new byte[stream.Length];

                    while (0 < (count = stream.Read(buffer, 0, buffer.Length - index)))
                    {
                        index += count;
                    }
                }
            }

            if (buffer.Length < 15)
            {
                writer.Write((byte)((byte)0x40 | (byte)buffer.Length));
            }
            else
            {
                writer.Write((byte)0x4F);
                size += WriteIntegerWithoutMarker(writer, buffer.Length);
            }

            writer.Write(buffer, 0, buffer.Length);

            return buffer.Length + size;
        }

        /// <summary>
        /// Writes a date to the given <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        /// <param name="value">The date to write.</param>
        /// <returns>The number of bytes written.</returns>
        private static int WriteDate(BinaryWriter writer, DateTime value)
        {
            byte[] buffer = BitConverter.GetBytes(value.ToUniversalTime().Subtract(ReferenceDate).TotalSeconds);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }

            writer.Write((byte)0x33);
            writer.Write(buffer, 0, buffer.Length);

            return buffer.Length + 1;
        }

        /// <summary>
        /// Writes an integer to the given <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        /// <param name="value">The integer to write.</param>
        /// <returns>The number of bytes written.</returns>
        private static int WriteInteger(BinaryWriter writer, long value)
        {
            byte[] buffer = GetIntegerBytes(value);

            writer.Write((byte)((byte)0x10 | (byte)Math.Log(buffer.Length, 2)));
            writer.Write(buffer, 0, buffer.Length);

            return buffer.Length + 1;
        }

        /// <summary>
        /// Writes an integer value without an object-table marker to the given <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        /// <param name="value">The integer to write.</param>
        /// <returns>The number of bytes written.</returns>
        private static int WriteIntegerWithoutMarker(BinaryWriter writer, long value)
        {
            byte[] buffer = GetIntegerBytes(value);

            writer.Write((byte)Math.Log(buffer.Length, 2));
            writer.Write(buffer, 0, buffer.Length);

            return buffer.Length + 1;
        }

        /// <summary>
        /// Writes a primitive value to the given <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        /// <param name="value">The primitive to write.</param>
        /// <returns>The number of bytes written.</returns>
        private static int WritePrimitive(BinaryWriter writer, bool? value)
        {
            byte val = 0;

            if (value.HasValue)
            {
                val = value.Value ? (byte)0x9 : (byte)0x8;
            }

            writer.Write(val);
            return 1;
        }

        /// <summary>
        /// Writes a floating-point value to the given <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        /// <param name="value">The floating-point value to write.</param>
        /// <returns>The number of bytes written.</returns>
        private static int WriteReal(BinaryWriter writer, double value)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }

            writer.Write((byte)((byte)0x20 | (byte)Math.Log(buffer.Length, 2)));
            writer.Write(buffer, 0, buffer.Length);

            return buffer.Length + 1;
        }

        /// <summary>
        /// Writes the given value using the number of bytes indicated by the specified size
        /// to the given <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="size">The size of the integer to write.</param>
        /// <returns>The number of bytes written.</returns>
        private static int WriteReferenceInteger(BinaryWriter writer, long value, int size)
        {
            byte[] buffer;

            switch (size)
            {
                case 1:
                    buffer = new byte[] { (byte)value };
                    break;
                case 2:
                    buffer = BitConverter.GetBytes(((short)value).ToBigEndianConditional());
                    break;
                case 4:
                    buffer = BitConverter.GetBytes(((int)value).ToBigEndianConditional());
                    break;
                case 8:
                    buffer = BitConverter.GetBytes(value.ToBigEndianConditional());
                    break;
                default:
                    throw new ArgumentException("The reference size must be one of 1, 2, 4 or 8. The specified reference size was: " + size, "size");
            }

            writer.Write(buffer, 0, buffer.Length);
            return buffer.Length;
        }

        /// <summary>
        /// Writes a string to the given <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        /// <param name="value">The string to write.</param>
        /// <returns>The number of bytes written.</returns>
        private static int WriteString(BinaryWriter writer, string value)
        {
            bool ascii = value.IsAscii();
            int size = 1;
            byte[] buffer;

            if (value.Length < 15)
            {
                writer.Write((byte)((byte)(ascii ? 0x50 : 0x60) | (byte)value.Length));
            }
            else
            {
                writer.Write((byte)(ascii ? 0x5F : 0x6F));
                size += WriteIntegerWithoutMarker(writer, value.Length);
            }

            if (ascii)
            {
                buffer = Encoding.ASCII.GetBytes(value);
            }
            else
            {
                buffer = Encoding.Unicode.GetBytes(value);

                if (BitConverter.IsLittleEndian)
                {
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        byte l = buffer[i];
                        buffer[i] = buffer[++i];
                        buffer[i] = l;
                    }
                }
            }

            writer.Write(buffer, 0, buffer.Length);
            return buffer.Length + size;
        }

        #endregion

        #region Private Instance Methods

        /// <summary>
        /// Adds an array to the internal object table.
        /// </summary>
        /// <param name="array">The array to add.</param>
        /// <returns>The index of the added array.</returns>
        private int AddArray(IEnumerable array)
        {
            int index = this.objectTable.Count, itemCount = 0;

            BinaryPlistArray arr = new BinaryPlistArray(this.objectTable);
            this.objectTable.Add(arr);

            foreach (object value in array)
            {
                arr.ObjectReference.Add(this.AddObject(value));
                itemCount++;
            }

            if (itemCount > this.maxCollectionSize)
            {
                this.maxCollectionSize = itemCount;
            }

            return index;
        }

        /// <summary>
        /// Adds a dictionary to the internal object table.
        /// </summary>
        /// <param name="dictionary">The dictionary to add.</param>
        /// <returns>The index of the added dictionary.</returns>
        private int AddDictionary(IDictionary dictionary)
        {
            int index = this.objectTable.Count;

            BinaryPlistDictionary dict = new BinaryPlistDictionary(this.objectTable, dictionary.Count);
            this.objectTable.Add(dict);

            foreach (object key in dictionary.Keys)
            {
                dict.KeyReference.Add(this.AddObject(key));
                dict.ObjectReference.Add(this.AddObject(dictionary[key]));
            }

            if (dictionary.Count > this.maxCollectionSize)
            {
                this.maxCollectionSize = dictionary.Count;
            }

            return index;
        }

        /// <summary>
        /// Adds an object to the internal object table.
        /// </summary>
        /// <param name="value">The object value to add.</param>
        /// <returns>The index of the added object.</returns>
        private int AddObject(object value)
        {
            int index = this.objectTable.Count;

            if (value != null)
            {
                Type type = value.GetType();

                if (typeof(IPlistSerializable).IsAssignableFrom(type))
                {
                    index = this.AddDictionary(((IPlistSerializable)value).ToPlistDictionary());
                }
                else if (typeof(IDictionary).IsAssignableFrom(type))
                {
                    index = this.AddDictionary(value as IDictionary);
                }
                else if ((typeof(Array).IsAssignableFrom(type) || typeof(ICollection).IsAssignableFrom(type)) && !typeof(byte[]).IsAssignableFrom(type))
                {
                    index = this.AddArray(value as IEnumerable);
                }
                else
                {
                    this.objectTable.Add(value);
                }
            }
            else
            {
                this.objectTable.Add(null);
            }

            return index;
        }

        /// <summary>
        /// Resets this instance's state.
        /// </summary>
        private void Reset()
        {
            this.topLevelObjectOffset =
            this.offsetIntSize =
            this.objectRefSize =
            this.maxCollectionSize = 0;

            this.objectTable = new List<object>();
            this.offsetTable = new List<long>();
        }

        /// <summary>
        /// Writes an array to the given <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        /// <param name="value">The array to write.</param>
        /// <returns>The number of bytes written.</returns>
        private int WriteArray(BinaryWriter writer, BinaryPlistArray value)
        {
            int size = 1;

            if (value.ObjectReference.Count < 15)
            {
                writer.Write((byte)((byte)0xA0 | (byte)value.ObjectReference.Count));
            }
            else
            {
                writer.Write((byte)0xAF);
                size += WriteIntegerWithoutMarker(writer, value.ObjectReference.Count);
            }

            foreach (int objectRef in value.ObjectReference)
            {
                WriteReferenceInteger(writer, objectRef, this.objectRefSize);
            }

            return size + (value.ObjectReference.Count * this.objectRefSize);
        }

        /// <summary>
        /// Writes a dictionary to the given <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        /// <param name="value">The dictionary to write.</param>
        /// <returns>The number of bytes written.</returns>
        private int WriteDictionary(BinaryWriter writer, BinaryPlistDictionary value)
        {
            int size = 1, skip = value.KeyReference.Count * this.objectRefSize;

            if (value.KeyReference.Count < 15)
            {
                writer.Write((byte)((byte)0xD0 | (byte)value.KeyReference.Count));
            }
            else
            {
                writer.Write((byte)0xDF);
                size += WriteIntegerWithoutMarker(writer, value.KeyReference.Count);
            }

            long startPosition = writer.BaseStream.Position;

            for (int i = 0; i < value.KeyReference.Count; i++)
            {
                writer.BaseStream.Position = startPosition + (i * this.objectRefSize);
                WriteReferenceInteger(writer, value.KeyReference[i], this.objectRefSize);
                writer.BaseStream.Position = startPosition + skip + (i * this.objectRefSize);
                WriteReferenceInteger(writer, value.ObjectReference[i], this.objectRefSize);
            }

            return size + (value.KeyReference.Count * this.objectRefSize * 2);
        }

        /// <summary>
        /// Writes the object table to the given <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the object table to.</param>
        private void WriteObjectTable(BinaryWriter writer)
        {
            int currentOffset = this.topLevelObjectOffset;
            object obj;
            Type type;

            for (int i = 0; i < this.objectTable.Count; i++)
            {
                obj = this.objectTable[i];
                this.offsetTable.Add(currentOffset);

                if (obj != null)
                {
                    type = obj.GetType();

                    if (typeof(BinaryPlistDictionary).IsAssignableFrom(type))
                    {
                        currentOffset += this.WriteDictionary(writer, (BinaryPlistDictionary)obj);
                    }
                    else if (typeof(BinaryPlistArray).IsAssignableFrom(type))
                    {
                        currentOffset += this.WriteArray(writer, (BinaryPlistArray)obj);
                    }
                    else if (typeof(bool).IsAssignableFrom(type))
                    {
                        currentOffset += WritePrimitive(writer, (bool)obj);
                    }
                    else if (typeof(long).IsAssignableFrom(type))
                    {
                        currentOffset += WriteInteger(writer, (long)obj);
                    }
                    else if (typeof(int).IsAssignableFrom(type))
                    {
                        currentOffset += WriteInteger(writer, (long)(int)obj);
                    }
                    else if (typeof(uint).IsAssignableFrom(type))
                    {
                        currentOffset += WriteInteger(writer, (long)(uint)obj);
                    }
                    else if (typeof(short).IsAssignableFrom(type))
                    {
                        currentOffset += WriteInteger(writer, (long)(short)obj);
                    }
                    else if (typeof(ushort).IsAssignableFrom(type))
                    {
                        currentOffset += WriteInteger(writer, (long)(ushort)obj);
                    }
                    else if (typeof(byte).IsAssignableFrom(type))
                    {
                        currentOffset += WriteInteger(writer, (long)(byte)obj);
                    }
                    else if (typeof(double).IsAssignableFrom(type))
                    {
                        currentOffset += WriteReal(writer, (double)obj);
                    }
                    else if (typeof(float).IsAssignableFrom(type))
                    {
                        currentOffset += WriteReal(writer, (double)(float)obj);
                    }
                    else if (typeof(decimal).IsAssignableFrom(type))
                    {
                        currentOffset += WriteReal(writer, (double)(decimal)obj);
                    }
                    else if (typeof(DateTime).IsAssignableFrom(type))
                    {
                        currentOffset += WriteDate(writer, (DateTime)obj);
                    }
                    else if (typeof(string).IsAssignableFrom(type))
                    {
                        currentOffset += WriteString(writer, (string)obj);
                    }
                    else if (typeof(byte[]).IsAssignableFrom(type) || typeof(ISerializable).IsAssignableFrom(type) || type.IsSerializable)
                    {
                        currentOffset += WriteData(writer, obj);
                    }
                    else
                    {
                        throw new InvalidOperationException("A type was found in the object table that is not serializable. Types that are natively serializable to a binary plist include: null, booleans, integers, floats, dates, strings, arrays and dictionaries. Any other types must be marked with a SerializableAttribute or implement ISerializable.");
                    }
                }
                else
                {
                    currentOffset += WritePrimitive(writer, null);
                }
            }
        }

        #endregion
    }
}
