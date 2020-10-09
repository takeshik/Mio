/*
 * Mio I/O Library <https://github.com/takeshik/Mio>
 * Copyright © Takeshi KIRIYA (aka takeshik) <takeshik@tksk.io>
 * All rights reserved. Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using JetBrains.Annotations;
using Mio.Utils;

namespace Mio
{
    [DataContract]
    [KnownType(typeof(FilePath))]
    [KnownType(typeof(DirectoryPath))]
    [KnownType(typeof(Destructive.DestructiveFilePath))]
    [KnownType(typeof(Destructive.DestructiveDirectoryPath))]
    public abstract partial class FileSystemPath
    {
        private protected const int DefaultFileStreamBufferSize = 4096;

        public static LayeredState<FileSystemPathComparer, (FileSystemPath, FileSystemPath)> Comparer { get; }
            // Some filesystems are case-sensitive, but others are not. Therefore the default is loosen the condition of equality.
            = new LayeredState<FileSystemPathComparer, (FileSystemPath, FileSystemPath)>(FileSystemPathComparer.CaseInsensitive);

        public static LayeredState<Encoding, FileSystemPath> Encoding { get; }
            = new LayeredState<Encoding, FileSystemPath>(new UTF8Encoding(false, true));

        [NotNull]
        [DataMember]
        protected internal string FullName { get; }

        [NotNull]
        public string Name
            => Path.GetFileName(this.FullName);

        [NotNull]
        public string NameWithoutExtension
            => Path.GetFileNameWithoutExtension(Path.GetFileName(this.FullName));

        [NotNull]
        public string Extension
            => Path.GetExtension(this.FullName);

        [MustUseReturnValue]
        public bool ExtensionEquals([CanBeNull] string extension, FileSystemPathComparer comparer = null)
            => (comparer ?? Comparer.GetValueFor((this, this))).Equals(this.Extension.TrimStart('.'), extension?.TrimStart('.'));

        [MustUseReturnValue]
        public bool IsDescendantOf([NotNull] DirectoryPath directory, FileSystemPathComparer comparer = null)
            => this.Ancestors.Any(x => x.Equals(directory, comparer));

        [NotNull]
        [ItemNotNull]
        public IEnumerable<DirectoryPath> Ancestors
        {
            get
            {
                for (var parent = this.TryGetParent(); parent != null; parent = parent.TryGetParent())
                {
                    yield return parent;
                }
            }
        }

        [NotNull]
        public DirectoryPath Parent
            => this.TryGetParent()
                ?? throw new InvalidOperationException("Root directory does not have parent directory.");

        [NotNull]
        public DirectoryPath Root
            => new DirectoryPath(Path.GetPathRoot(this.FullName));

        private protected FileSystemPath(string path, bool normalize)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Invalid path.", nameof(path));

            if (normalize)
            {
                // Get out of weird behavior of standard classes: if the FullPath
                // ends with the directory separator, its Name is empty string.
                var normalizedPath = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (normalizedPath == "")
                {
                    // Before trimming, path must be like "///". Keep the last directory separator.
                    normalizedPath = Path.DirectorySeparatorChar.ToString();
                }
                else if (normalizedPath != path && normalizedPath[normalizedPath.Length - 1] == Path.VolumeSeparatorChar)
                {
                    // If Path.VolumeSeparatorChar == Path.DirectorySeparatorChar, this clause must be meaningless.
                    // Before trimming, path must be like "C:/". "C:" does not mean "C:/" but "C:/.", so keep the last
                    // directory separator if original argument ends with directory separator.
                    normalizedPath += Path.DirectorySeparatorChar;
                }
                normalizedPath = Path.GetFullPath(normalizedPath);
                this.FullName = normalizedPath;
            }
            else
            {
                this.FullName = path;
            }
        }

        [CanBeNull]
        public DirectoryPath TryGetParent()
        {
            var path = Path.GetDirectoryName(this.FullName);
            return path == null
                ? null
                : new DirectoryPath(path, false);
        }

        [MustUseReturnValue]
        public FileAttributes GetAttributes()
            => File.GetAttributes(this.FullName);

        [MustUseReturnValue]
        public abstract bool Exists();

        [MustUseReturnValue]
        public abstract DateTimeOffset GetCreationTime();

        [MustUseReturnValue]
        public abstract DateTimeOffset GetLastAccessTime();

        [MustUseReturnValue]
        public abstract DateTimeOffset GetLastWriteTime();
    }
}
