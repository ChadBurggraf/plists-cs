# Serialization.Plists
#### Plist serialization and de-serialization for C# and .NET

Serialization.Plists is a binary plist reader/writer implementation for .NET. It supports the complete plist specification that Apple has published, with two caveats:

 - Serialization of opaque data or non plist-compatible objects will break plist editing in Property List Editor
 - Sets are treated as arrays
 
## Building

You can build with MSBuild v3.5 from the command line:

    msbuild build.proj

or using the batch file:

    build.bat
    
You can also build using the solution in Visual Studio 2008.

## Basic Usage

There are two primary classes exposed by the assembly: `BinaryPlistReader` and `BinaryPlistWriter`. As their names imply, they read and write `IDictionary` objects to and from binary plists. The plist format specifies that the following types are supported:

 - `null`
 - `bool`
 - `int` (`byte`, `ushort`, `short`, `uint`, `int` and `long`)
 - `double` (`float`, `double` and `decimal`)
 - `DateTime`
 - `string` (ASCII and Unicode)
 - `object[]` (arrays with members of any of the above types plus `object[]` and `IDictionary`)
 - `IDictionary` (with members of any of the above types plus `IDictionary`)
 
If an object not in the above list is somewhere in the object graph, it will be treated as binary data. Such objects must be marked `Serializable` or implement `ISerializable`.

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
    
## License

Licensed under the [MIT](http://www.opensource.org/licenses/mit-license.html) license. See LICENSE.txt.

Copyright (c) 2011 Chad Burggraf.