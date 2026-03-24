# TR-Code-Libraries

High quality code libraries and utilities for software development of any kind from apps, games, and more. Currently only in C# but can add more languages (e.g. C++) under request. This code aims for the highest level of efficiency in both CPU cycles and RAM, by stripping any potential overhead to be "close to the metal" as possible while still being user friendly in terms of the API. These libraries are still being perfected and more additions will be made in terms of files and expanding on each file, but there is a chance that some stuff isn't as close to the metal or user friendly as advertised. In cases like this, reaaching out would be helpful to make sure the community overall has high power code to use.

Some of the libraries included:

- MathTools: A math library that handles various calculation and statistics utilities, including mean, median, mode, variable casting, factorials, standard deviation and more.

- PackedBuffer: A class that keeps collections compact by holding a maximum capacity for a collection, making all null/default values at the end of the collection and using a ReadOnlySpan to retrieve the actual contents of the filled up collection to prevent resizing arrays.

  (e.g. A shoe cubby has a capacity of 32, and there are 15 shoes but there is no shoes in cubby 2, 6, or 10. The PackedBuffer will make sure there is no gaps in between the shoes.)

  Supports read-only, preserve order and unique element flag types as well to ensure every use case is accounted for. It is a wrapper around an array, not a List to make sure the capacity of the array is actually what you need.
  
- HashedString: A special type of string that's used for strings that appear often by converting it to a special 64-bit integer and keeping it's ID saved in a registry. Equivalent to Unreal Engine FName or Godot StringName, it provides little to no allocations compared to creating multiple strings, and makes comparing two strings a single integer comparison instead of comparing individual characters, saving potentially hundreds of CPU cycles in high traffic string allocated/compared environments.

- Group: A special type of collection that is a wrapper around a List and handles every practical use of collections in a single class. Swapping elements within itself and with other groups, group limits, shuffling items and so on. This is meant to be the "ultimate" collection for cases where you need a lot of control over the elements but don't want to build the tedious methods yourself.

- BitPacker: A class used to quickly pack multiple integers of different bit types into a single number. Up to 128 bit numbers supported.
