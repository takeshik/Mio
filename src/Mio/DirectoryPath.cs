/*
 * Mio I/O Library <https://github.com/takeshik/Mio>
 * Copyright © Takeshi KIRIYA (aka takeshik) <takeshik@tksk.io>
 * All rights reserved. Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Mio.Destructive;
using D = System.IO.Directory;

namespace Mio
{
    public class DirectoryPath : FileSystemPath,
        IEquatable<DirectoryPath>
    {
        public DirectoryPath([NotNull] string path)
            : base(path, true)
        {
        }

        internal DirectoryPath(string path, bool normalize)
            : base(path, normalize)
        {
        }

        [Pure]
        public static bool operator ==([CanBeNull] DirectoryPath x, [CanBeNull] DirectoryPath y)
            => Equals(x, y);

        [Pure]
        public static bool operator !=([CanBeNull] DirectoryPath x, [CanBeNull] DirectoryPath y)
            => !Equals(x, y);

        [NotNull]
        public static DirectoryPath GetTempDirectory()
            => new DirectoryPath(Path.GetTempPath());

        [NotNull]
        public static DirectoryPath GetCurrentDirectory()
            => new DirectoryPath(D.GetCurrentDirectory());

        [Pure]
        public static bool Equals([CanBeNull] DirectoryPath x, [CanBeNull] DirectoryPath y)
            => Equals(x, y, FileSystemPathComparer.Default);

        [Pure]
        public static bool Equals([CanBeNull] DirectoryPath x, [CanBeNull] DirectoryPath y, [NotNull] FileSystemPathComparer comparer)
            => comparer.Equals(x, y);

        [Pure]
        public bool Equals(DirectoryPath other)
            => Equals(this, other);

        [Pure]
        public override bool Equals(object obj)
            => obj is DirectoryPath file && this.Equals(file);

        [Pure]
        public bool Equals([CanBeNull] DirectoryPath other, [NotNull] FileSystemPathComparer comparer)
            => Equals(this, other, comparer);

        [Pure]
        public override int GetHashCode()
            => StringComparer.OrdinalIgnoreCase.GetHashCode(this.FullName);

        [Pure]
        public override string ToString()
            => "<Dir: " + this.FullName + ">";

        [Pure]
        public override bool Exists()
            => D.Exists(this.FullName);

        public override DateTimeOffset GetCreationTime()
            => new DateTimeOffset(D.GetCreationTimeUtc(this.FullName), TimeSpan.Zero);

        public override DateTimeOffset GetLastAccessTime()
            => new DateTimeOffset(D.GetLastAccessTimeUtc(this.FullName), TimeSpan.Zero);

        public override DateTimeOffset GetLastWriteTime()
            => new DateTimeOffset(D.GetLastWriteTimeUtc(this.FullName), TimeSpan.Zero);

        [CanBeNull]
        public DirectoryPath NullIfNotExists()
            => this.Exists() ? this : null;

        [Pure]
        [NotNull]
        public DirectoryPath WithExtension([CanBeNull] string extension)
            => new DirectoryPath(Path.ChangeExtension(this.FullName, extension), false);

        [Pure]
        [NotNull]
        public FilePath ChildFile([NotNull] string path)
            => new FilePath(Path.Combine(this.FullName, path));

        [Pure]
        [NotNull]
        public DirectoryPath ChildDirectory([NotNull] string path)
            => new DirectoryPath(Path.Combine(this.FullName, path));

        [NotNull]
        [ItemNotNull]
        public IEnumerable<FilePath> EnumerateFiles(string searchPattern = "*")
            => D.EnumerateFiles(this.FullName, searchPattern).Select(x => new FilePath(x, false));

        [NotNull]
        [ItemNotNull]
        public IEnumerable<FilePath> EnumerateAllFiles(string searchPattern = "*")
            => D.EnumerateFiles(this.FullName, searchPattern, SearchOption.AllDirectories).Select(x => new FilePath(x, false));

        [NotNull]
        [ItemNotNull]
        public IEnumerable<DirectoryPath> EnumerateDirectories(string searchPattern = "*")
            => D.EnumerateDirectories(this.FullName, searchPattern).Select(x => new DirectoryPath(x, false));

        [NotNull]
        [ItemNotNull]
        public IEnumerable<DirectoryPath> EnumerateAllDirectories(string searchPattern = "*")
            => D.EnumerateDirectories(this.FullName, searchPattern, SearchOption.AllDirectories).Select(x => new DirectoryPath(x, false));

        [NotNull]
        [ItemNotNull]
        public IEnumerable<FileSystemPath> EnumerateEntries(string searchPattern = "*")
            => D.EnumerateDirectories(this.FullName, searchPattern).Select(x => (FileSystemPath)new DirectoryPath(x, false))
                .Concat(D.EnumerateFiles(this.FullName, searchPattern).Select(x => (FileSystemPath)new FilePath(x, false)));

        [NotNull]
        [ItemNotNull]
        public IEnumerable<FileSystemPath> EnumerateAllEntries(string searchPattern = "*")
            => D.EnumerateDirectories(this.FullName, searchPattern, SearchOption.AllDirectories).Select(x => (FileSystemPath)new DirectoryPath(x, false))
                .Concat(D.EnumerateFiles(this.FullName, searchPattern, SearchOption.AllDirectories).Select(x => (FileSystemPath)new FilePath(x, false)));

        [NotNull]
        public DirectoryPath EnsureCreated()
        {
            D.CreateDirectory(this.FullName);
            return this;
        }

        public void CopyTo([NotNull] DestructiveDirectoryPath destination)
        {
            foreach (var file in this.EnumerateFiles())
            {
                file.CopyTo(destination.ChildFile(file.Name).CreateDestructive());
            }

            foreach (var dir in this.EnumerateDirectories())
            {
                dir.CopyTo(destination.ChildDirectory(dir.Name).EnsureCreated().CreateDestructive());
            }
        }

        [Pure]
        [NotNull]
        internal DestructiveDirectoryPath CreateDestructive()
            => new DestructiveDirectoryPath(this.FullName, false);
    }
}
