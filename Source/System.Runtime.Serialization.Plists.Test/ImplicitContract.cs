//-----------------------------------------------------------------------
// <copyright file="ImplicitContract.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Runtime.Serialization.Plists.Test
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Implicit data contract test class.
    /// </summary>
    internal class ImplicitContract
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
}
