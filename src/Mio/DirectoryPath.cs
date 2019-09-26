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
#if NETCOREAPP2_1
using System.IO.Enumeration;
#endif

namespace Mio
{
    public class DirectoryPath : FileSystemPath,
        IEquatable<DirectoryPath>
    {
        private const string _defaultSearchPattern = "*";

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

        [NotNull]
        public static IReadOnlyList<DirectoryPath> GetRootDirectories()
            => Array.ConvertAll(D.GetLogicalDrives(), x => new DirectoryPath(x, false));

        [Pure]
        public static bool Equals([CanBeNull] DirectoryPath x, [CanBeNull] DirectoryPath y, FileSystemPathComparer comparer = null)
            => (comparer ?? Comparer.GetValueFor((x, y))).Equals(x, y);

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
        public FilePath RandomNamedChildFile(string prefix = "", string suffix = "")
            => this.ChildFile(prefix + Path.GetRandomFileName() + suffix);

        [NotNull]
        public DirectoryPath RandomNamedChildDirectory(string prefix = "", string suffix = "")
            => this.ChildDirectory(prefix + Path.GetRandomFileName() + suffix);

        [NotNull]
        [ItemNotNull]
        public IEnumerable<FilePath> EnumerateFiles(string searchPattern = _defaultSearchPattern)
            => D.EnumerateFiles(this.FullName, searchPattern).Select(x => new FilePath(x, false));

        [NotNull]
        [ItemNotNull]
        public IEnumerable<FilePath> EnumerateAllFiles(string searchPattern = _defaultSearchPattern)
            => D.EnumerateFiles(this.FullName, searchPattern, SearchOption.AllDirectories).Select(x => new FilePath(x, false));

        [NotNull]
        [ItemNotNull]
        public IEnumerable<DirectoryPath> EnumerateDirectories(string searchPattern = _defaultSearchPattern)
            => D.EnumerateDirectories(this.FullName, searchPattern).Select(x => new DirectoryPath(x, false));

        [NotNull]
        [ItemNotNull]
        public IEnumerable<DirectoryPath> EnumerateAllDirectories(string searchPattern = _defaultSearchPattern)
            => D.EnumerateDirectories(this.FullName, searchPattern, SearchOption.AllDirectories).Select(x => new DirectoryPath(x, false));

        [NotNull]
        [ItemNotNull]
        public IEnumerable<FileSystemPath> EnumerateEntries(string searchPattern = _defaultSearchPattern)
            => D.EnumerateDirectories(this.FullName, searchPattern).Select(x => (FileSystemPath)new DirectoryPath(x, false))
                .Concat(D.EnumerateFiles(this.FullName, searchPattern).Select(x => (FileSystemPath)new FilePath(x, false)));

        [NotNull]
        [ItemNotNull]
        public IEnumerable<FileSystemPath> EnumerateAllEntries(string searchPattern = _defaultSearchPattern)
            => D.EnumerateDirectories(this.FullName, searchPattern, SearchOption.AllDirectories).Select(x => (FileSystemPath)new DirectoryPath(x, false))
                .Concat(D.EnumerateFiles(this.FullName, searchPattern, SearchOption.AllDirectories).Select(x => (FileSystemPath)new FilePath(x, false)));

        [NotNull]
        [ItemNotNull]
        [MustUseReturnValue]
        public IEnumerable<FileSystemPath> SafeEnumerateEntries(
            FileAttributes attributesToSkip = default,
            Func<DirectoryPath, bool> recursionPredicate = null)
        {
#if NETCOREAPP2_1
            return new FileSystemEnumerable<FileSystemPath>(
                this.FullName,
                (ref FileSystemEntry x) => x.IsDirectory
                    ? (FileSystemPath) new DirectoryPath(x.ToFullPath())
                    : new FilePath(x.ToFullPath()),
                new EnumerationOptions()
                {
                    AttributesToSkip = attributesToSkip,
                    IgnoreInaccessible = true,
                    RecurseSubdirectories = true,
                    ReturnSpecialDirectories = false,
                    // MatchCasing and MatchType are not cared since searchPattern is not used
                }
            )
            {
                ShouldRecursePredicate = recursionPredicate == null
                    ? (FileSystemEnumerable<FileSystemPath>.FindPredicate) null
                    : (ref FileSystemEntry x) => recursionPredicate(new DirectoryPath(x.ToFullPath())),
            };
#else
            IEnumerable<FileSystemPath> Core(string origin)
            {
                var queue = new Queue<string>();
                var directories = Enumerable.Empty<string>();
                try
                {
                    directories = D.EnumerateDirectories(origin);
                }
                catch
                {
                    // skip
                }

                foreach (var dir in directories)
                {
                    // In .NET Standard 2.1, this GetAttributes should be replaced by EnumerationOptions.AttributesToSkip
                    if (attributesToSkip == default || (File.GetAttributes(dir) & attributesToSkip) == 0)
                    {
                        var path = new DirectoryPath(dir, false);
                        if (recursionPredicate == null || recursionPredicate(path))
                        {
                            queue.Enqueue(dir);
                        }
                        yield return path;
                    }
                }

                var files = Enumerable.Empty<string>();
                try
                {
                    files = D.EnumerateFiles(origin);
                }
                catch
                {
                    // skip
                }

                foreach (var file in files)
                {
                    if (attributesToSkip == default || (File.GetAttributes(file) & attributesToSkip) == 0)
                    {
                        yield return new FilePath(file, false);
                    }
                }

                while (queue.Count > 0)
                {
                    var dir = queue.Dequeue();
                    foreach (var x in Core(dir))
                    {
                        yield return x;
                    }
                }
            }

            return Core(this.FullName);
#endif
        }

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
