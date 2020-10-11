/*
 * Mio I/O Library <https://github.com/takeshik/Mio>
 * Copyright © Takeshi KIRIYA (aka takeshik) <takeshik@tksk.io>
 * All rights reserved. Licensed under the MIT License.
 */

using System;
using System.IO;
using JetBrains.Annotations;
using D = System.IO.Directory;

namespace Mio.Destructive
{
    public sealed class DestructiveDirectoryPath : DirectoryPath
    {
        public new string FullName
            => base.FullName;

        public DestructiveDirectoryPath(string path)
            : base(path)
        {
        }

        internal DestructiveDirectoryPath(string path, bool normalize)
            : base(path, normalize)
        {
        }

        public static void SetCurrentDirectory(DirectoryPath directory)
            => D.SetCurrentDirectory(directory.FullName);

        [Pure]
        public override string ToString()
            => "<DestructiveDir: " + this.FullName + ">";

        [Pure]
        public Uri ToUri()
            => new Uri(this.FullName);

        public new DestructiveDirectoryPath EnsureCreated()
        {
            D.CreateDirectory(this.FullName);
            return this;
        }

        public new DestructiveDirectoryPath? NullIfNotExists()
            => this.Exists() ? this : null;

        public void SetAttributes(FileAttributes attributes)
            => File.SetAttributes(this.FullName, attributes);

        public void Encrypt()
            => File.Encrypt(this.FullName);

        public void Decrypt()
            => File.Decrypt(this.FullName);

        public void SetCreationTime(DateTimeOffset creationTime)
            => D.SetCreationTimeUtc(this.FullName, creationTime.UtcDateTime);

        public void SetLastAccessTime(DateTimeOffset lastAccessTime)
            => D.SetLastAccessTimeUtc(this.FullName, lastAccessTime.UtcDateTime);

        public void SetLastWriteTime(DateTimeOffset lastWriteTime)
            => D.SetLastWriteTimeUtc(this.FullName, lastWriteTime.UtcDateTime);

        public bool Delete()
        {
            if (!this.Exists()) return false;

            D.Delete(this.FullName);
            return true;
        }

        public bool DeleteEntries()
        {
            void DeleteCore(string path)
            {
                foreach (var file in D.EnumerateFiles(path))
                {
                    File.Delete(file);
                }

                foreach (var dir in D.EnumerateDirectories(path))
                {
                    DeleteCore(dir);
                    D.Delete(dir);
                }
            }

            if (!this.Exists()) return false;
            DeleteCore(this.FullName);
            return true;
        }

        public bool DeleteAll()
        {
            if (!this.Exists()) return false;

            D.Delete(this.FullName, true);
            return true;
        }
    }
}
