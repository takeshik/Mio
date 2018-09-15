# Mio I/O Library

**Mio** is a little library for .NET platforms. This is an alternative to standard `FileInfo` and `DirectoryInfo` classes.

## Features

* Wrapper of `File`, `Directory` and `Path` in `System.IO`.
* `FilePath` and `DirectoryPath` are counterpart of `FileInfo` and `DirectoryInfo`. They are derived from `FileSystemPath`.
* Unlike `FileInfo` and `DirectoryInfo`, they only retain `FullName` value. Metadata are not cached.
* Destructive operations, such as deletion are separated into special classes in `Mio.Destructive` namespace.
* Additional helper methods are available.

## Licensing

Copyright &copy; Takeshi KIRIYA (aka [takeshik](https://takeshik.org/)), All rights reserved.

Mio is [Free Software](https://www.gnu.org/philosophy/free-sw.html). Its source codes, binaries, and all other resources are licensed under the [MIT License](LICENSE.txt).
