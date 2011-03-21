//-----------------------------------------------------------------------
// <copyright file="DataContractTests.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Runtime.Serialization.Plists.Test
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Data contract serialization tests.
    /// </summary>
    [TestClass]
    public sealed class DataContractTests
    {
        /// <summary>
        /// Write implicit contract tests.
        /// </summary>
        [TestMethod]
        public void DataContractWriteImplicitContract()
        {
            string outputPath = Guid.NewGuid().ToString() + ".plist";
            
            ImplicitContract obj = new ImplicitContract()
            {
                DateValue = new DateTime(1982, 5, 28),
                ListValue = new List<string>(new string[] { "One", "Two", "Three" }),
                InnerContractValue = new ImplicitContract()
                {
                    DateValue = new DateTime(2001, 1, 1),
                    ListValue = new List<string>(new string[] { "Four", "Five", "Six" })
                }
            };

            using (FileStream stream = File.Create(outputPath))
            {
                new DataContractBinaryPlistSerializer(typeof(ImplicitContract)).WriteObject(stream, obj);
            }

            IDictionary dict = new BinaryPlistReader().ReadObject(outputPath);
            Assert.AreEqual(3, dict.Count);
            Assert.AreEqual(new DateTime(1982, 5, 28), dict["DateValue"]);
            Assert.IsInstanceOfType(dict["ListValue"], typeof(object[]));
            Assert.AreEqual(3, ((object[])dict["ListValue"]).Length);
            Assert.AreEqual("One", ((object[])dict["ListValue"])[0]);
            Assert.AreEqual("Two", ((object[])dict["ListValue"])[1]);
            Assert.AreEqual("Three", ((object[])dict["ListValue"])[2]);
            Assert.IsInstanceOfType(dict["InnerContractValue"], typeof(IDictionary));
            Assert.AreEqual(3, ((IDictionary)dict["InnerContractValue"]).Count);
            Assert.AreEqual(new DateTime(2001, 1, 1), ((IDictionary)dict["InnerContractValue"])["DateValue"]);
            Assert.IsInstanceOfType(((IDictionary)dict["InnerContractValue"])["ListValue"], typeof(object[]));
            Assert.AreEqual(3, ((object[])((IDictionary)dict["InnerContractValue"])["ListValue"]).Length);
            Assert.AreEqual("Four", ((object[])((IDictionary)dict["InnerContractValue"])["ListValue"])[0]);
            Assert.AreEqual("Five", ((object[])((IDictionary)dict["InnerContractValue"])["ListValue"])[1]);
            Assert.AreEqual("Six", ((object[])((IDictionary)dict["InnerContractValue"])["ListValue"])[2]);
        }

        #region ImplicitContract Class

        /// <summary>
        /// Implicit data contract test class.
        /// </summary>
        private class ImplicitContract
        {
            /// <summary>
            /// Gets or sets the date value.
            /// </summary>
            public DateTime DateValue { get; set; }

            /// <summary>
            /// Gets or sets the list value.
            /// </summary>
            public IList<string> ListValue { get; set; }

            /// <summary>
            /// Gets or sets the inner contract value.
            /// </summary>
            public ImplicitContract InnerContractValue { get; set; }
        }

        #endregion
    }
}
