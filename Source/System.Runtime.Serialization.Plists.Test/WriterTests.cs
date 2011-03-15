//-----------------------------------------------------------------------
// <copyright file="WriterTests.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Runtime.Serialization.Plists.Test
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for binary plist writing.
    /// </summary>
    [TestClass]
    public class WriterTests
    {
        /// <summary>
        /// Write empty string tests.
        /// </summary>
        [TestMethod]
        public void WriterWriteEmptyString()
        {
            string outputPath = Guid.NewGuid().ToString() + ".plist";
            
            IDictionary dict = new Dictionary<string, object>();
            dict["Empty"] = String.Empty;

            BinaryPlistWriter writer = new BinaryPlistWriter();
            writer.WriteObject(outputPath, dict);

            BinaryPlistReader reader = new BinaryPlistReader();
            dict = reader.ReadObject(outputPath);

            Assert.IsTrue(dict.Contains("Empty"));
            Assert.AreEqual(String.Empty, dict["Empty"]);
        }

        /// <summary>
        /// Write long collections tests.
        /// </summary>
        [TestMethod]
        public void WriterWriteLongCollection()
        {
            string outputPath = Guid.NewGuid().ToString() + ".plist";

            IDictionary dict = new Dictionary<string, object>();
            IDictionary longDict = new Dictionary<string, object>();
            List<int> longArray = new List<int>();

            for (int i = 0; i < 72; i++)
            {
                longDict.Add(i.ToString(), i);
            }

            for (int i = 0; i < 756; i++)
            {
                longArray.Add(i);
            }

            dict["Dictionary"] = longDict;
            dict["Array"] = longArray;

            BinaryPlistWriter writer = new BinaryPlistWriter();
            writer.WriteObject(outputPath, dict);

            BinaryPlistReader reader = new BinaryPlistReader();
            dict = reader.ReadObject(outputPath);

            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual(72, ((IDictionary)dict["Dictionary"]).Count);
            Assert.AreEqual(756, ((object[])dict["Array"]).Length);
        }

        /// <summary>
        /// Write long ASCII string tests.
        /// </summary>
        [TestMethod]
        public void WriterWriteLongAsciiString()
        {
            string outputPath = Guid.NewGuid().ToString() + ".plist";

            IDictionary dict = new Dictionary<string, string>();
            dict["LongAscii"] = "Lorem ipsum dolor sit amet, consectetur adipiscing elit imperdiet ornare.";
            new BinaryPlistWriter().WriteObject(outputPath, dict);

            dict = new BinaryPlistReader().ReadObject(outputPath);
            Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit imperdiet ornare.", dict["LongAscii"]);
        }

        /// <summary>
        /// Write long Unicode string tests.
        /// </summary>
        [TestMethod]
        public void WriterWriteLongUnicodeString()
        {
            string outputPath = Guid.NewGuid().ToString() + ".plist";

            IDictionary dict = new Dictionary<string, string>();
            dict["LongUnicode"] = "ºª•¶§∞¢£™¬˚∆¨˙©ƒ´ßƒçºª•¶§∞¢£™¬˚∆¨˙©ƒ´ßƒçºª•¶§∞¢£™¬˚∆¨˙©ƒ´ßƒçºª•¶§∞¢£™¬˚∆¨˙©ƒ´ßƒçºª•¶§∞¢£™¬˚∆¨˙©ƒ´ßƒçºª•¶§∞¢£™¬˚∆¨˙©ƒ´ßƒç";
            new BinaryPlistWriter().WriteObject(outputPath, dict);

            dict = new BinaryPlistReader().ReadObject(outputPath);
            Assert.AreEqual("ºª•¶§∞¢£™¬˚∆¨˙©ƒ´ßƒçºª•¶§∞¢£™¬˚∆¨˙©ƒ´ßƒçºª•¶§∞¢£™¬˚∆¨˙©ƒ´ßƒçºª•¶§∞¢£™¬˚∆¨˙©ƒ´ßƒçºª•¶§∞¢£™¬˚∆¨˙©ƒ´ßƒçºª•¶§∞¢£™¬˚∆¨˙©ƒ´ßƒç", dict["LongUnicode"]);
        }

        /// <summary>
        /// Write object from Calculator.plist tests.
        /// </summary>
        [TestMethod]
        public void WriterWriteObjectCalculator()
        {
            string outputPath = Guid.NewGuid().ToString() + ".plist";
            BinaryPlistReader reader = new BinaryPlistReader();
            IDictionary dictionary;

            using (Stream stream = File.OpenRead(Paths.CalculatorPlistPath))
            {
                dictionary = reader.ReadObject(stream);
            }

            dictionary["NSWindow Frame Calc_History_Window"] = "620 71 228 289 0 0 1440 870 ";
            dictionary["NSWindow Frame Calc_Main_Window"] = "668 425 423 283 0 0 1440 870 ";
            dictionary["Programmer_BinaryIsHidden"] = false;
            dictionary["Programmer_InputMode"] = 11;
            dictionary["ViewDefaultsKey"] = "Religious";

            BinaryPlistWriter writer = new BinaryPlistWriter();

            using (Stream stream = File.Create(outputPath))
            {
                writer.WriteObject(stream, dictionary);
            }

            using (Stream stream = File.OpenRead(outputPath))
            {
                dictionary = reader.ReadObject(stream);
            }

            Assert.AreEqual(5, dictionary.Count);
            Assert.AreEqual("620 71 228 289 0 0 1440 870 ", dictionary["NSWindow Frame Calc_History_Window"]);
            Assert.AreEqual("668 425 423 283 0 0 1440 870 ", dictionary["NSWindow Frame Calc_Main_Window"]);
            Assert.AreEqual(false, dictionary["Programmer_BinaryIsHidden"]);
            Assert.AreEqual((short)11, dictionary["Programmer_InputMode"]);
            Assert.AreEqual("Religious", dictionary["ViewDefaultsKey"]);
        }

        /// <summary>
        /// Write object from Nested.plist tests.
        /// </summary>
        [TestMethod]
        public void WriterWriteObjectNested()
        {
            string outputPath = Guid.NewGuid().ToString() + ".plist";
            BinaryPlistReader reader = new BinaryPlistReader();
            IDictionary dictionary;

            using (Stream stream = File.OpenRead(Paths.NestedPlistPath))
            {
                dictionary = reader.ReadObject(stream);
            }

            object[] arr = dictionary["Array"] as object[];
            IDictionary dict = dictionary["Dictionary"] as IDictionary;

            arr[0] = "1st";
            arr[1] = "2nd";
            arr[2] = "3rd";

            dict["Double"] = 2.0000000000000011;

            arr = dict["Array"] as object[];
            arr[0] = "One";
            arr[1] = 3;
            arr[2] = false;
            arr[3] = "Four";

            BinaryPlistWriter writer = new BinaryPlistWriter();

            using (Stream stream = File.Create(outputPath))
            {
                writer.WriteObject(stream, dictionary);
            }

            using (Stream stream = File.OpenRead(outputPath))
            {
                dictionary = reader.ReadObject(stream);
            }

            Assert.AreEqual(2, dictionary.Count);
            Assert.IsInstanceOfType(dictionary["Array"], typeof(object[]));
            Assert.IsInstanceOfType(dictionary["Dictionary"], typeof(IDictionary));

            arr = dictionary["Array"] as object[];
            dict = dictionary["Dictionary"] as IDictionary;
            Assert.AreEqual(3, arr.Length);
            Assert.AreEqual(2, dictionary.Count);

            Assert.AreEqual("1st", arr[0]);
            Assert.AreEqual("2nd", arr[1]);
            Assert.AreEqual("3rd", arr[2]);

            Assert.AreEqual(2.0000000000000011, dict["Double"]);
            Assert.AreEqual(4, ((object[])dict["Array"]).Length);

            arr = dict["Array"] as object[];
            Assert.AreEqual("One", arr[0]);
            Assert.AreEqual((short)3, arr[1]);
            Assert.AreEqual(false, arr[2]);
            Assert.AreEqual("Four", arr[3]);
        }

        /// <summary>
        /// Write object from Types.plist tests.
        /// </summary>
        [TestMethod]
        public void WriterWriteObjectTypes()
        {
            string outputPath = Guid.NewGuid().ToString() + ".plist";
            BinaryPlistReader reader = new BinaryPlistReader();
            IDictionary dictionary;

            using (Stream stream = File.OpenRead(Paths.TypesPlistPath))
            {
                dictionary = reader.ReadObject(stream);
            }

            dictionary["Unicode"] = "ºª•¶§∞¢£™¬˚∆¨˙©ƒ´ßƒç";
            dictionary["False"] = true;
            dictionary["Data"] = new byte[3];
            dictionary["Date"] = new DateTime(2011, 3, 13);
            dictionary["True"] = false;
            dictionary["Pi"] = 2.71828;
            dictionary["Hello"] = "Japan";

            BinaryPlistWriter writer = new BinaryPlistWriter();

            using (Stream stream = File.Create(outputPath))
            {
                writer.WriteObject(stream, dictionary);
            }

            using (Stream stream = File.OpenRead(outputPath))
            {
                dictionary = reader.ReadObject(stream);
            }

            Assert.AreEqual(7, dictionary.Count);
            Assert.AreEqual("ºª•¶§∞¢£™¬˚∆¨˙©ƒ´ßƒç", dictionary["Unicode"]);
            Assert.AreEqual(true, dictionary["False"]);
            Assert.IsInstanceOfType(dictionary["Data"], typeof(byte[]));
            Assert.AreEqual(new DateTime(2011, 3, 13), dictionary["Date"]);
            Assert.AreEqual(false, dictionary["True"]);
            Assert.AreEqual(2.71828, dictionary["Pi"]);
            Assert.AreEqual("Japan", dictionary["Hello"]);
        }
    }
}
