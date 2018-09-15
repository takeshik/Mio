/*
 * Mio I/O Library <https://github.com/takeshik/Mio>
 * Copyright © Takeshi KIRIYA (aka takeshik) <takeshik@tksk.io>
 * All rights reserved. Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace Mio
{
    public abstract partial class FileSystemPath
    {
        private protected const int DefaultFileStreamBufferSize = 4096;

        [NotNull]
        protected string FullName { get; }

        [NotNull]
        public string Name
            => Path.GetFileName(this.FullName);

        [NotNull]
        public string NameWithoutExtension
            => Path.GetFileNameWithoutExtension(Path.GetFileName(this.FullName));

        [NotNull]
        public string Extension
            => Path.GetExtension(this.FullName);

        public bool ExtensionEquals([CanBeNull] string extension)
            => this.ExtensionEquals(extension, DefaultComparer);

        public bool ExtensionEquals([CanBeNull] string extension, [NotNull] Comparer comparer)
            => comparer.Equals(this.Extension.TrimStart('.'), extension?.TrimStart('.'));

        public bool IsDescendantOf([NotNull] DirectoryPath directory)
            => this.IsDescendantOf(directory, DefaultComparer);

        public bool IsDescendantOf([NotNull] DirectoryPath directory, [NotNull] Comparer comparer)
            => this.FullName.Length >= directory.FullName.Length
                && comparer.Equals(directory.FullName, this.FullName.Substring(0, directory.FullName.Length));

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
                    // Before trimming, path must be like "C:/". "C:" not means "C:/" but "C:/.", so keep the last
                    // directory separator if original argument ends with directory separator.
                    normalizedPath = normalizedPath + Path.DirectorySeparatorChar;
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

        public FileAttributes GetAttributes()
            => File.GetAttributes(this.FullName);

        public abstract bool Exists();

        public abstract DateTimeOffset GetCreationTime();

        public abstract DateTimeOffset GetLastAccessTime();

        public abstract DateTimeOffset GetLastWriteTime();
    }
}
