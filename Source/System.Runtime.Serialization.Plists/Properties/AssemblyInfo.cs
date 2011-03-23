//-----------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("System.Runtime.Serialization.Plists")]
[assembly: AssemblyDescription("Serialization classes for working with Apple's plist file format.")]
[assembly: Guid("d81371d5-62ec-4554-b231-6829e41b99f2")]

#if DEBUG
[assembly: InternalsVisibleTo("System.Runtime.Serialization.Plists.Test")]
#endif

[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "Namespace", Target = "System.Runtime.Serialization.Plists", Justification = "There are no namespaces available to merge with.")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "Assembly", Target = "System.Runtime.Serialization.Plists.dll", Justification = "The spelling is correct.")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "Namespace", Target = "System.Runtime.Serialization.Plists", Justification = "The spelling is correct.")]