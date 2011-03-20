//-----------------------------------------------------------------------
// <copyright file="ExtensionTests.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Runtime.Serialization.Plists.Test
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Extensions tests.
    /// </summary>
    [TestClass]
    public sealed class ExtensionTests
    {
        /// <summary>
        /// Type is collection tests.
        /// </summary>
        [TestMethod]
        public void ExtensionTypeIsCollection()
        {
            Assert.IsTrue(typeof(object[]).IsCollection());
            Assert.IsTrue(typeof(List<string>).IsCollection());
            Assert.IsTrue(typeof(StringCollection).IsCollection());
            Assert.IsTrue(typeof(ICollection<object>).IsCollection());
            Assert.IsFalse(typeof(byte[]).IsCollection());
            Assert.IsFalse(typeof(string).IsCollection());
        }

        /// <summary>
        /// Type is primitive or enum tests.
        /// </summary>
        [TestMethod]
        public void ExtensionTypeIsPrimitiveOrEnum()
        {
            Assert.IsTrue(typeof(int).IsPrimitiveOrEnum());
            Assert.IsTrue(typeof(Guid).IsPrimitiveOrEnum());
            Assert.IsTrue(typeof(string).IsPrimitiveOrEnum());
            Assert.IsTrue(typeof(AppDomainManagerInitializationOptions).IsPrimitiveOrEnum());
            Assert.IsFalse(typeof(BinaryPlistWriter).IsPrimitiveOrEnum());
        }
    }
}
