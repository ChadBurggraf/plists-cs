//-----------------------------------------------------------------------
// <copyright file="BinaryPlistTests.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Runtime.Serialization.Plists.Test
{
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for binary plist serialization.
    /// </summary>
    [TestClass]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class BinaryPlistTests
    {
        private const string CalculatorPListPath = "Calculator.plist";
        private const string NestedPListpath = "Nested.plist";
        private const string TypesPListPath = "Types.plist";

        /// <summary>
        /// Read object from Calculator.plist tests.
        /// </summary>
        [TestMethod]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public void BinaryPlistReadObjectCalculator()
        {
            BinaryPlistReader reader = new BinaryPlistReader();
            IDictionary dictionary;

            using (Stream stream = File.OpenRead(CalculatorPListPath))
            {
                dictionary = reader.ReadObject(stream);
            }

            Assert.AreEqual(5, dictionary.Count);
            Assert.AreEqual("520 71 228 289 0 0 1440 878 ", dictionary["NSWindow Frame Calc_History_Window"]);
            Assert.AreEqual("368 425 423 283 0 0 1440 878 ", dictionary["NSWindow Frame Calc_Main_Window"]);
            Assert.AreEqual(true, dictionary["Programmer_BinaryIsHidden"]);
            Assert.AreEqual((short)10, dictionary["Programmer_InputMode"]);
            Assert.AreEqual("Scientific", dictionary["ViewDefaultsKey"]);
        }

        /// <summary>
        /// Read object from Nested.plist tests.
        /// </summary>
        [TestMethod]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public void BinaryPlistReadObjectNested()
        {
            BinaryPlistReader reader = new BinaryPlistReader();
            IDictionary dictionary;

            using (Stream stream = File.OpenRead(NestedPListpath))
            {
                dictionary = reader.ReadObject(stream);
            }

            Assert.AreEqual(2, dictionary.Count);
            Assert.IsInstanceOfType(dictionary["Array"], typeof(object[]));
            Assert.IsInstanceOfType(dictionary["Dictionary"], typeof(IDictionary));

            object[] arr = dictionary["Array"] as object[];
            IDictionary dict = dictionary["Dictionary"] as IDictionary;
            Assert.AreEqual(3, arr.Length);
            Assert.AreEqual(2, dictionary.Count);

            Assert.AreEqual("First", arr[0]);
            Assert.AreEqual("Second", arr[1]);
            Assert.AreEqual("Third", arr[2]);

            Assert.AreEqual(1.0000000000000011, dict["Double"]);
            Assert.AreEqual(4, ((object[])dict["Array"]).Length);

            arr = dict["Array"] as object[];
            Assert.AreEqual("1", arr[0]);
            Assert.AreEqual((short)2, arr[1]);
            Assert.AreEqual(true, arr[2]);
            Assert.AreEqual("4", arr[3]);
        }

        /// <summary>
        /// Read object from Types.plist tests.
        /// </summary>
        [TestMethod]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public void BinaryPlistReadObjectTypes()
        {
            BinaryPlistReader reader = new BinaryPlistReader();
            IDictionary dictionary;

            using (Stream stream = File.OpenRead(TypesPListPath))
            {
                dictionary = reader.ReadObject(stream);
            }

            Assert.AreEqual(7, dictionary.Count);
            Assert.AreEqual("¡™£¢∞§¶•ªº–≠πøˆ¨¥†®´∑œ", dictionary["Unicode"]);
            Assert.AreEqual(false, dictionary["False"]);
            Assert.IsInstanceOfType(dictionary["Data"], typeof(byte[]));
            Assert.AreEqual(new DateTime(1982, 05, 28, 7, 0, 0), dictionary["Date"]);
            Assert.AreEqual(true, dictionary["True"]);
            Assert.AreEqual(3.14159, dictionary["Pi"]);
            Assert.AreEqual("World", dictionary["Hello"]);
        }

        /// <summary>
        /// Write object from Calculator.plist tests.
        /// </summary>
        [TestMethod]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public void BinaryPlistWriteObjectCalculator()
        {
            string outputPath = Guid.NewGuid().ToString() + ".plist";
            BinaryPlistReader reader = new BinaryPlistReader();
            IDictionary dictionary;

            using (Stream stream = File.OpenRead(CalculatorPListPath))
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
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public void BinaryPlistWriteObjectNested()
        {
            string outputPath = Guid.NewGuid().ToString() + ".plist";
            BinaryPlistReader reader = new BinaryPlistReader();
            IDictionary dictionary;

            using (Stream stream = File.OpenRead(NestedPListpath))
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
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public void BinaryPlistWriteObjectTypes()
        {
            string outputPath = Guid.NewGuid().ToString() + ".plist";
            BinaryPlistReader reader = new BinaryPlistReader();
            IDictionary dictionary;

            using (Stream stream = File.OpenRead(TypesPListPath))
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
