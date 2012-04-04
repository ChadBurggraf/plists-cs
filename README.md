# Serialization.Plists
#### Plist serialization and de-serialization for C# and .NET

Serialization.Plists is a binary plist reader/writer implementation for .NET. It supports the complete plist specification that Apple has published, with two caveats:

 - Serialization of opaque data or non plist-compatible objects will break plist editing in Property List Editor
 - Sets are treated as arrays

## Prerequisites to Building

 - .NET Framework v3.5
 - MSBuild v3.5
 - StyleCop v4.7
 - FxCop (either standalone, or auto-installed by Visual Studio 2010)

The build scripts expect StyleCop and FxCop to be installed in their default locations. You can build without FxCop, but only by using the Visual Studio solution (StyleCop is required either way).

If you'd like to create a signed build using your own signing key (I distribute signed assemblies; I recommend you just use those), run the following command from inside of the `Source\` directory:

    sn -k System.Runtime.Serialization.Plists.snk
    
The build script will pick up this key automatically and sign the output assembly with it.

## Building

You can build with MSBuild v3.5 from the command line:

    msbuild build.proj

or using the batch file:

    build.bat
    
You can also build using the solution in Visual Studio 2008.

## Basic Usage

You can use [WCF Data Contracts](http://msdn.microsoft.com/en-us/library/ms733127.aspx) to mark up model or business objects for plist serialization. See below for details and caveats. Continue reading this section for manual creation of plist-serializable dictionaries.

There are two primary classes exposed by the assembly: `BinaryPlistReader` and `BinaryPlistWriter`. As their names imply, they read and write `IDictionary` objects to and from binary plists. The plist format specifies that the following types are supported:

 - `null`
 - `bool`
 - `int` (`byte`, `sbyte`, `ushort`, `short`, `uint`, `int` and `long`)
 - `double` (`float`, `double` and `decimal`)
 - `DateTime`
 - `string` (ASCII and Unicode)
 - `object[]` (arrays with members of any of the above types plus `object[]` and `IDictionary`)
 - `IDictionary` (dictionaries with members of any of the above types plus `IDictionary`)
 
If an object not in the above list is somewhere in the object graph, it will be treated as binary data. Such objects must be marked `Serializable` or implement `ISerializable` or `IPlistSerializable`. Byte arrays (`byte[]`) will be written un-modified.

### Writing

To write an `IDictionary` instance to a stream or file, do something similar to the following:

    Dictionary<string, object> myObjectGraph = new Dictionary<string, object>();
    
    ...
    // Fill your dictionary with objects.
    ...
    
    BinaryPlistWriter writer = new BinaryPlistWriter();
    writer.WriteObject(@"C:\Some\Path\MyPlist.plist", myObjectGraph);
    
You can also write to a `Stream` directly using the appropriate overload.

### Reading

To read a plist stream or file into an `IDictionary` instance, do something similar to the following:

    BinaryPlistReader reader = new BinaryPlistReader();
    IDictionary myObjectGraph = reader.ReadObject(@"C:\Some\Path\MyPlist.plist");
    
Like above, you can also read from a `Stream` directly using the appropriate overload.

## IPlistSerializable

You can embed the transformation of your object graph to and from `IDictionary` instances by implementing `IPlistSerializable`.

As an example, a simple object might be implemented as follows:

    class PropertyList : IPlistSerializable
    {
		public int Id { get; set; }
        public string Name { get; set; }
        public OtherIPlistSerializableType Stuff { get; set; }
        
        public void FromPlistDictionary(IDictionary plist)
        {
            this.Id = (int)plist["Id"];
            this.Name = (string)plist["Name"];
            this.Stuff = new OtherIPlistSerializableType();
            this.Stuff.FromPlistDictionary((IDictionary)plist["Stuff"]);
        }
        
        public IDictionary ToPlistDictionary()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["Id"] = this.Id;
            dict["Name"] = this.Name;
            dict["Stuff"] = this.Stuff;
            return dict;
        }
    }

With the above implementation in place, you can read and write instances of your objects directly using the appropriate reader/writer overloads.

## Data Contracts

A first-draft version of [WCF Data Contract](http://msdn.microsoft.com/en-us/library/ms733127.aspx) support is currently included. The behavior is very similar to that of the [DataContractSerializer](http://msdn.microsoft.com/en-us/library/system.runtime.serialization.datacontractserializer.aspx):

 - If no `DataContract` is defined for an object, a default contract is inferred from all public read+write fields and properties.
 - If a `DataContract` is defined, all read+write fields and properties marked as `DataMember`s are used, regardless of visibility.
 - The root object **must** be a complex type or an `IDictionary` instance (i.e., an array or primitive type cannot be the root of the graph).
 - `DataContractSerializer` does not require collections to implement `IList` formally; instead an informal protocol requiring an `Add` method is used. Right now, `DataContractBinaryPlistSerializer` requires an actual `IList` implementation or an array for collection objects. This may change in the future if there is any demand.
 - This functionality has not been thoroughly tested. Please report bugs!
 
The basic usage of `DataContractBinaryPlistSerializer` is almost identical to `DataContractSerializer`. One big difference is that it does not use `XmlReader` and `XmlWriter` under the covers. Therefore, there aren't any overloads for either the constructor or the `ReadObject` and `WriteObject` methods.

Binary plists are a very limited serialization format, so a number of `DataContract` features aren't supported and will be ignored (e.g., custom names and namespaces, member orders, etc.).
    
## License

Licensed under the [MIT](http://www.opensource.org/licenses/mit-license.html) license. See LICENSE.txt.

Copyright (c) 2011 Chad Burggraf.