/*
 * Mio I/O Library <https://github.com/takeshik/Mio>
 * Copyright © Takeshi KIRIYA (aka takeshik) <takeshik@tksk.io>
 * All rights reserved. Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Mio.Destructive;
using F = System.IO.File;

namespace Mio
{
    public class FilePath : FileSystemPath, IEquatable<FilePath>
    {
        public FilePath([NotNull] string path)
            : base(path, true)
        {
        }

        internal FilePath(string path, bool normalize)
            : base(path, normalize)
        {
        }

        [Pure]
        public static bool operator ==([CanBeNull] FilePath x, [CanBeNull] FilePath y)
            => Equals(x, y);

        [Pure]
        public static bool operator !=([CanBeNull] FilePath x, [CanBeNull] FilePath y)
            => !Equals(x, y);

        [Pure]
        public static bool Equals([CanBeNull] FilePath x, [CanBeNull] FilePath y, FileSystemPathComparer comparer = null)
            => (comparer ?? Comparer.GetValueFor((x, y))).Equals(x, y);

        [Pure]
        public bool Equals(FilePath other)
            => Equals(this, other);

        [Pure]
        public bool Equals([CanBeNull] FilePath other, [NotNull] FileSystemPathComparer comparer)
            => Equals(this, other, comparer);

        [Pure]
        public override bool Equals(object obj)
            => obj is FilePath file && this.Equals(file);

        [Pure]
        public override int GetHashCode()
            => StringComparer.Ordinal.GetHashCode(this.FullName);

        [Pure]
        public override string ToString()
            => "<File: " + this.FullName + ">";

        public override bool Exists()
            => F.Exists(this.FullName);

        public override DateTimeOffset GetCreationTime()
            => new DateTimeOffset(F.GetCreationTimeUtc(this.FullName), TimeSpan.Zero);

        public override DateTimeOffset GetLastAccessTime()
            => new DateTimeOffset(F.GetLastAccessTimeUtc(this.FullName), TimeSpan.Zero);

        public override DateTimeOffset GetLastWriteTime()
            => new DateTimeOffset(F.GetLastWriteTimeUtc(this.FullName), TimeSpan.Zero);

        [CanBeNull]
        public FilePath NullIfNotExists()
            => this.Exists() ? this : null;

        [Pure]
        [NotNull]
        public FilePath WithExtension([CanBeNull] string extension)
            => new FilePath(Path.ChangeExtension(this.FullName, extension), false);

        public long GetSize64()
            => new FileInfo(this.FullName).Length;

        public int GetSize()
            => checked((int)this.GetSize64());

        [NotNull]
        public byte[] ReadAllBytes()
            => F.ReadAllBytes(this.FullName);

        [NotNull]
        [ItemNotNull]
        public string[] ReadAllLines(Encoding encoding = null)
            => F.ReadAllLines(this.FullName, encoding ?? Encoding.GetValueFor(this));

        [NotNull]
        public string ReadAllText(Encoding encoding = null)
            => F.ReadAllText(this.FullName, encoding ?? Encoding.GetValueFor(this));

        [NotNull]
        [ItemNotNull]
        public IEnumerable<string> ReadLines(Encoding encoding = null)
            => F.ReadLines(this.FullName, encoding ?? Encoding.GetValueFor(this));

        [NotNull]
        [ItemNotNull]
        public Task<byte[]> ReadAllBytesAsync(CancellationToken cancellationToken = default)
        {
#if NETCOREAPP2_1
            return F.ReadAllBytesAsync(this.FullName, cancellationToken);
#else
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<byte[]>(cancellationToken);
            }

            async Task<byte[]> Core()
            {
                using (var fs = this.OpenRead())
                {
                    var buf = new byte[fs.Length];
                    await fs.ReadAsync(buf, 0, buf.Length, cancellationToken).ConfigureAwait(false);
                    return buf;
                }
            }

            return Core();
#endif
        }

        [NotNull]
        [ItemNotNull]
        public Task<string[]> ReadAllLinesAsync(Encoding encoding = null, CancellationToken cancellationToken = default)
        {
#if NETCOREAPP2_1
            return F.ReadAllLinesAsync(this.FullName, encoding ?? Encoding.GetValueFor(this), cancellationToken);
#else
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<string[]>(cancellationToken);
            }

            async Task<string[]> Core()
            {
                var lines = new List<string>();
                using (var sr = this.OpenReadText(encoding ?? Encoding.GetValueFor(this)))
                {
                    string str;
                    while ((str = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        lines.Add(str);
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }

                return lines.ToArray();
            }

            return Core();
#endif
        }

        [NotNull]
        [ItemNotNull]
        public Task<string> ReadAllTextAsync(Encoding encoding = null, CancellationToken cancellationToken = default)
        {
#if NETCOREAPP2_1
            return F.ReadAllTextAsync(this.FullName, encoding ?? Encoding.GetValueFor(this), cancellationToken);
#else
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<string>(cancellationToken);
            }

            using (var sr = this.OpenReadText(encoding ?? Encoding.GetValueFor(this)))
            using (cancellationToken.Register(x => ((StreamReader)x).Dispose(), sr, false))
            {
                return sr.ReadToEndAsync();
            }
#endif
        }

        [NotNull]
        public FileStream OpenRead(
            FileShare share = FileShare.Read,
            int bufferSize = DefaultFileStreamBufferSize,
            FileOptions options = FileOptions.Asynchronous)
            => new FileStream(this.FullName, FileMode.Open, FileAccess.Read, share, bufferSize, options);

        [NotNull]
        public StreamReader OpenReadText(
            Encoding encoding = null,
            FileShare share = FileShare.Read,
            int bufferSize = DefaultFileStreamBufferSize,
            FileOptions options = FileOptions.Asynchronous)
            => new StreamReader(new FileStream(this.FullName, FileMode.Open, FileAccess.Read, share, bufferSize, options), encoding ?? Encoding.GetValueFor(this));

        public void CopyTo([NotNull] DestructiveFilePath destination)
            => F.Copy(this.FullName, destination.FullName, true);

        public void Replace([NotNull] DestructiveFilePath destination, DestructiveFilePath destinationBackup)
            => F.Replace(this.FullName, destination.FullName, destinationBackup.FullName);

        [Pure]
        [NotNull]
        internal DestructiveFilePath CreateDestructive()
            => new DestructiveFilePath(this.FullName, false);
    }
}
