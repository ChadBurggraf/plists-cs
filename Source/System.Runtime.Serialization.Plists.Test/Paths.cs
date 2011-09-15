//-----------------------------------------------------------------------
// <copyright file="Paths.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Runtime.Serialization.Plists.Test
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Provides static access to common test paths.
    /// </summary>
    public static class Paths
    {
        /// <summary>
        /// Gets the path of the calculator plist file.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public const string CalculatorPlistPath = "Calculator.plist";

        /// <summary>
        /// Gets the path of the nested plist file.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public const string NestedPlistPath = "Nested.plist";

        /// <summary>
        /// Gets the path of the profile plist file.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public const string ProfilePlistPath = "Profile.plist";

        /// <summary>
        /// Gets the path of the profile 2 plist file.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public const string Profile2PlistPath = "Profile2.plist";

        /// <summary>
        /// Gets the path of the types plist file.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public const string TypesPlistPath = "Types.plist";
    }
}
