# TR-Code-Libraries

> **High-performance, zero-allocation C# utilities designed for games, apps, and demanding software.**

TR-Code-Libraries is a collection of utilities built to achieve the highest level of efficiency in both CPU cycles and RAM. By stripping away potential overhead, we aim to get as "close to the metal" as possible while maintaining a clean, user-friendly API. 

Currently, the library is focused on **C#**, but is open to expanding into other systems languages (like C++ or Rust) based on community request!

### 🎯 Our Philosophy
All code here was architected, written, and rigorously tested by Team Radiance (with the help of modern optimization tools) to ensure blazing-fast execution. We are constantly iterating to achieve maximum performance. If you spot an area where we can squeeze out even more efficiency, reaching out or submitting a PR is highly encouraged!

---

## 🛠️ Included Libraries

* **`MathTools`** A high-speed math library handling various calculations and statistical utilities. Includes SIMD-accelerated methods for mean, median, mode, variable casting, factorials, standard deviation, and more.

* **`PackedBuffer<T>`** A zero-allocation wrapper around an array (not a List) that keeps collections perfectly compact. It maintains a maximum capacity and ensures all null/default values are pushed to the end. It uses `ReadOnlySpan` to retrieve the active contents, preventing GC allocations and array resizing.
  > Imagine a shoe cubby with a capacity of 32. You have 15 shoes, but cubbies 2, 6, and 10 are empty. `PackedBuffer` instantly shifts the data so there are no gaps between the shoes.
  *Supports Read-Only, Preserve Order, and Unique Element flags.*

* **`Group<T>`** A specialized collection wrapper that handles complex list operations in a single class. Features include swapping elements (internally and with other groups), enforcing group limits, and high-speed Fisher-Yates shuffling. The "ultimate" collection for when you need granular control without writing tedious boilerplate.

* **`TRandom`** A dual-stream entropy engine. It separates RNG into a deterministic "Logic Stream" and a chaotic "Visual Stream". Features bias-specific mechanics too like Double Advantage/Disadvantage rolls.

* **`Collections Extensions`** High-performance extension methods for standard C# collections, including zero-allocation array compaction, seamless cross-list element swapping, and fast weighted-index selection.

* **`HashedString`** The ultimate Flyweight string. Equivalent to Unreal Engine's `FName` or Godot's `StringName`. It converts frequently used strings into a special 64-bit integer and keeps the ID saved in a global registry. This provides near-zero allocations and turns string comparisons into a single integer check, saving hundreds of CPU cycles in high-traffic environments.

* **`BitPacker`** A raw, bit-level utility used to quickly pack multiple integers of varying bit depths into a single number. Supports up to modern 128-bit integers (`Int128`).

---

## 🎮 Used In: Adventure Boy: The Centennial Tale
The architecture in this repository powers our upcoming personal project: **Adventure Boy: The Centennial Tale**, releasing soon on Steam. While the code there is more of a custom-tailored one using MemoryPack binary serialization and JSON parsing, the libraries used here can easily support those functions as well!

If you'd like to support the project or follow along with our development journey, check us out on Instagram: [@adventure.boy.official](https://instagram.com/adventure.boy.official).

---

## 🤝 Contributing & Feedback
We are dedicated to providing the community with high-power code. We welcome issues, feature requests, and pull requests! If you want to send out a donation or a nice message of support, use the contact info.

**Contact:** [samuelnchinda475@gmail.com](mailto:samuelnchinda475@gmail.com) | GitHub: [@Vensorax](https://github.com/Vensorax)
