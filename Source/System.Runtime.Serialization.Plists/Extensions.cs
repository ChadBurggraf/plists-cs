//-----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Runtime.Serialization.Plists
{
    /// <summary>
    /// Extensions and helpers for plist serialization.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Gets a value indicating whether the given string is all ASCII.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns>True if the string contains only ASCII characters, false otherwise.</returns>
        public static bool IsAscii(this string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                foreach (char c in value)
                {
                    if (c > 127)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Converts the given value into its binary representation as a string.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The value's binary representation as a string.</returns>
        public static string ToBinaryString(this byte value)
        {
            return Convert.ToString(value, 2);
        }

        /// <summary>
        /// Converts the given value into its binary representation as a string.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The value's binary representation as a string.</returns>
        public static string ToBinaryString(this int value)
        {
            return Convert.ToString(value, 2);
        }
    }
}
