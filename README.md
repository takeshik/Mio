# Mio I/O Library

[![Build result (master)](https://img.shields.io/appveyor/ci/takeshik/mio/master.svg?logo=appveyor)](https://ci.appveyor.com/project/takeshik/mio)
[![Test result (master)](https://img.shields.io/appveyor/tests/takeshik/mio/master.svg?logo=appveyor)](https://ci.appveyor.com/project/takeshik/mio/build/tests)
[![NuGet release](https://img.shields.io/nuget/v/Mio.svg)](https://www.nuget.org/packages/Mio/)
[![Nuget prerelease](https://img.shields.io/nuget/vpre/Mio.svg?label=nuget%20pre)](https://www.nuget.org/packages/Mio/)

**Mio** is a little library for .NET platforms. This is an alternative to standard `FileInfo` and `DirectoryInfo` classes.

## Features

* Wrapper of `File`, `Directory` and `Path` in `System.IO`.
* `FilePath` and `DirectoryPath` are counterpart of `FileInfo` and `DirectoryInfo`. They are derived from `FileSystemPath`.
* Unlike `FileInfo` and `DirectoryInfo`, they only retain `FullName` value. Metadata are not cached.
* Destructive operations, such as deletion are separated into special classes in `Mio.Destructive` namespace.
* Additional helper methods are available.

## Note about Performance

This library is mainly designed for simple path indicator, or helper of occasional filesystem operations. Due to internal mechanisms, metadata querying (check existence, get file size, etc.) with `FileInfo` and `DirectoryInfo` is faster than `File` and `Directory`.

Currently, using this library for heavy metadata access is hardly not recommended.

## Licensing

Copyright &copy; Takeshi KIRIYA (aka [takeshik](https://takeshik.org/)), All rights reserved.

Mio is [Free Software](https://www.gnu.org/philosophy/free-sw.html). Its source codes, binaries, and all other resources are licensed under the [MIT License](LICENSE.txt).
