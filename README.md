# Mio I/O Library

[![Build result (master)](https://img.shields.io/appveyor/ci/takeshik/mio/master.svg?logo=appveyor)](https://ci.appveyor.com/project/takeshik/mio)
[![Test result (master)](https://img.shields.io/appveyor/tests/takeshik/mio/master.svg?logo=appveyor)](https://ci.appveyor.com/project/takeshik/mio/build/tests)
[![NuGet release](https://img.shields.io/nuget/v/Mio.svg)](https://www.nuget.org/packages/Mio/)
[![Nuget prerelease](https://img.shields.io/nuget/vpre/Mio.svg?label=nuget%20pre)](https://www.nuget.org/packages/Mio/)

**Mio** is a little library for .NET platforms. This is an alternative to standard `FileInfo` and `DirectoryInfo` classes.

## Features

* Wrapper of `File`, `Directory` and `Path` in `System.IO`.
* Unlike `FileInfo` and `DirectoryInfo`, they only retain `FullName` value. Metadata are not cached.
* Destructive operations, such as deletion are separated into special classes in `Mio.Destructive` namespace.
* Additional helper methods are available.

## Usage

```csharp
// dotnet add package Mio

using Mio;
```

`FilePath` and `DirectoryPath` are counterpart of `FileInfo` and `DirectoryInfo`. They are derived from `FileSystemPath`.

```csharp
// Equivalent to: new FileInfo(@"C:\foo\bar.txt")
var file = new FilePath(@"C:\foo\bar.txt");

// Equivalent to: new DirectoryInfo("/var/tmp")
var dir = new DirectoryPath("/var/tmp");
```

They have many methods to access the tree, get information, and read them.

```csharp
// dir2 == new DirectoryPath("C:/foo/bar/abc/def")
DirectoryPath dir2 = file.Parent.ChildDirectory("abc").ChildDirectory("def");

// file2 == new FilePath(@"C:\foo\bar/abc\def\xyz.txt"); path separators are normalized.
FilePath file2 = dir2.ChildFile("xyz.txt");

// Equalivalent to EnumerateFileSystemEntries with SearchOption.AllDirectories.
foreach (var entry in dir2.EnumerateAllEntries())
{
    if (entry is FilePath)
        Console.WriteLine($"File: {entry.Name}");
    else // entry is DirectoryPath
        Console.WriteLine($"Dir: {entry.Name}");
}

// Unlike FileInfo and DirectoryInfo, reading information like Exists, is not cached.
// So Exists() is method, not property.
if (file2.Exists()) Console.WriteLine(await file2.ReadAllTextAsync());
```

### Destructive Operations

Destructive operations, such as writing, deleting, etc. are separated into special types: `DestructiveFilePath` and `DestructiveDirectoryPath`, in namespace `Mio.Destructive`.

```csharp
// This indicates there are destructive operations.
using Mio.Destructive;

// AsDestructive is an extension method of DestructiveExtensions in Mio.Destructive namespace.
// And so, in any case, you need to import Destructive namespace for destructive operations.
DestructiveFilePath dFile = file.AsDestructive();
// or simply:
dFile = new DestructiveFilePath(@"C:\foo\bar\baz.txt");
```

`Destructive`-classes are derived from normal ones.

```csharp
// Exists() is declared in FileSystemPath.
if (!dFile.Exists()) await dFile.WriteAsync("Hello, world.");
```

To get full path of file or directory is only accessible by `FullName` property in `Destructive`- classes for prevention of unsafe evading from Destructive context.

Exceptionally, `DirectoryPath.EnsureCreated()`, equalivalent to `Directory.CreateDirectory(path)`, might be destructive in some cases, but it's regarded as non-destructive: declared in `DirectoryPath`. This is a compromise to frequent and relatively-safe operation.

### Static methods

Don't miss out static helper methods.

```csharp
// Get and set current directory. DirectoryPath.GetTempDirectory() is also useful.
DestructiveDirectoryPath.SetCurrentDirectory(DirectoryPath.GetCurrentDirectory().ChildDirectory("foo/bar"));
// Wrapper of Path.GetTempFileName().
var tempFile = DestructiveFilePath.CreateTempFile();
```

### Layered Configurations

If you need, default behavior of some methods, path comparer and text file encoding, are able to override.

```csharp
// In this scope, by default, compare paths case-sensitively.
using (FileSystemPath.Comparer.BeginWith(FileSystemPathComparer.CaseSensitive))
// And files with ".txt" extension will be read (in text) by UTF-16. Others are by ASCII.
using (FileSystemPath.Encoding.BeginWith(Encoding.ASCII))
using (FileSystemPath.Encoding.BeginWith(Encoding.Unicode, x => x.ExtensionEquals("txt")))
{
    var text1 = new FilePath("text1.doc").ReadAllText(); // read by ASCII
    var text2 = new FilePath("text2.txt").ReadAllText(); // read by UTF-16
    // Even if in setting scope, if argument is specified, its settings are used with highest priority.
    var text3 = new FilePath("text3.txt").ReadAllText(Encoding.UTF32); // read by UTF-32
}
// Default behaviors are back.
```

## Note about Performance

This library is mainly designed as simple path indicator, or helper of occasional filesystem operations. Due to internal mechanisms, metadata querying (check existence, get file size, etc.) with `FileInfo` and `DirectoryInfo` is faster than `File` and `Directory`.

Currently, using this library for heavy metadata access is hardly not recommended.

## Licensing

Copyright &copy; Takeshi KIRIYA (aka [takeshik](https://takeshik.org/)), All rights reserved.

Mio is [Free Software](https://www.gnu.org/philosophy/free-sw.html). Its source codes, binaries, and all other resources are licensed under the [MIT License](LICENSE.txt).
