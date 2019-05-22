/*
 * Mio I/O Library <https://github.com/takeshik/Mio>
 * Copyright © Takeshi KIRIYA (aka takeshik) <takeshik@tksk.io>
 * All rights reserved. Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using F = System.IO.File;

namespace Mio.Destructive
{
    public class DestructiveFilePath : FilePath
    {
        [NotNull]
        public new string FullName
            => base.FullName;

        public DestructiveFilePath([NotNull] string path)
            : base(path)
        {
        }

        internal DestructiveFilePath(string path, bool normalize)
            : base(path, normalize)
        {
        }

        [Pure]
        public override string ToString()
            => "<DestructiveFile: " + this.FullName + ">";

        [NotNull]
        public static DestructiveFilePath CreateTempFile()
            => new DestructiveFilePath(Path.GetTempFileName());

        [NotNull]
        public Uri ToUri()
            => new Uri(this.FullName);

        [CanBeNull]
        public new DestructiveFilePath NullIfNotExists()
            => this.Exists() ? this : null;

        public void SetAttributes(FileAttributes attributes)
            => F.SetAttributes(this.FullName, attributes);

        public void Encrypt()
            => F.Encrypt(this.FullName);

        public void Decrypt()
            => F.Decrypt(this.FullName);

        public void SetCreationTime(DateTimeOffset creationTime)
            => F.SetCreationTimeUtc(this.FullName, creationTime.UtcDateTime);

        public void SetLastAccessTime(DateTimeOffset lastAccessTime)
            => F.SetLastAccessTimeUtc(this.FullName, lastAccessTime.UtcDateTime);

        public void SetLastWriteTime(DateTimeOffset lastWriteTime)
            => F.SetLastWriteTimeUtc(this.FullName, lastWriteTime.UtcDateTime);

        public bool Delete()
        {
            if (!F.Exists(this.FullName)) return false;
            try
            {
                F.Delete(this.FullName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void MoveTo([NotNull] DestructiveFilePath destination)
            => F.Move(this.FullName, destination.FullName);

        public void Append([NotNull] byte[] bytes)
        {
            using (var fs = this.Open(FileMode.Append, FileAccess.Write))
            {
                fs.Write(bytes, 0, bytes.Length);
                fs.Flush();
            }
        }

        public void Append([NotNull] IEnumerable<string> contents, Encoding encoding = null)
            => F.AppendAllLines(this.FullName, contents, encoding ?? Encoding.GetValueFor(this));

        public void Append([NotNull] string contents, Encoding encoding = null)
            => F.AppendAllText(this.FullName, contents, encoding ?? Encoding.GetValueFor(this));

        public void Write([NotNull] byte[] bytes)
            => F.WriteAllBytes(this.FullName, bytes);

        public void Write([NotNull] IEnumerable<string> contents, Encoding encoding = null)
            => F.WriteAllLines(this.FullName, contents, encoding ?? Encoding.GetValueFor(this));

        public void Write([NotNull] string[] contents, Encoding encoding = null)
            => F.WriteAllLines(this.FullName, contents, encoding ?? Encoding.GetValueFor(this));

        public void Write([NotNull] string contents, Encoding encoding = null)
            => F.WriteAllText(this.FullName, contents, encoding ?? Encoding.GetValueFor(this));

        [NotNull]
        public FileStream Create(
            FileShare share = FileShare.Read,
            int bufferSize = DefaultFileStreamBufferSize,
            FileOptions options = FileOptions.Asynchronous)
            => new FileStream(this.FullName, FileMode.Create, FileAccess.ReadWrite, share, bufferSize, options);

        [NotNull]
        public StreamWriter CreateText(
            Encoding encoding = null,
            FileShare share = FileShare.Read,
            int bufferSize = DefaultFileStreamBufferSize,
            FileOptions options = FileOptions.Asynchronous)
            => new StreamWriter(new FileStream(this.FullName, FileMode.Create, FileAccess.ReadWrite, share, bufferSize, options), encoding ?? Encoding.GetValueFor(this));

        [NotNull]
        public FileStream Open(
            FileMode mode = FileMode.OpenOrCreate,
            FileAccess access = FileAccess.ReadWrite,
            FileShare share = FileShare.Read,
            int bufferSize = DefaultFileStreamBufferSize,
            FileOptions options = FileOptions.Asynchronous)
            => new FileStream(this.FullName, mode, access, share, bufferSize, options);

        [NotNull]
        public FileStream OpenWrite(
            FileShare share = FileShare.Read,
            int bufferSize = DefaultFileStreamBufferSize,
            FileOptions options = FileOptions.Asynchronous)
            => new FileStream(this.FullName, FileMode.OpenOrCreate, FileAccess.Write, share, bufferSize, options);

        [NotNull]
        public StreamWriter OpenWriteText(
            Encoding encoding = null,
            FileShare share = FileShare.Read,
            int bufferSize = DefaultFileStreamBufferSize,
            FileOptions options = FileOptions.Asynchronous)
            => new StreamWriter(new FileStream(this.FullName, FileMode.OpenOrCreate, FileAccess.Write, share, bufferSize, options), encoding ?? Encoding.GetValueFor(this));
    }
}
