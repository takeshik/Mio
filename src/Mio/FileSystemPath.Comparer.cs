/*
 * Mio I/O Library <https://github.com/takeshik/Mio>
 * Copyright © Takeshi KIRIYA (aka takeshik) <takeshik@tksk.io>
 * All rights reserved. Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;

namespace Mio
{
    partial class FileSystemPath
    {
        public abstract class Comparer :
            IEqualityComparer<FilePath>,
            IEqualityComparer<DirectoryPath>
        {
            public abstract int GetHashCode(FilePath obj);

            public abstract int GetHashCode(DirectoryPath obj);

            protected internal abstract bool Equals(string x, string y);

            public bool Equals(FilePath x, FilePath y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(null, x)) return false;
                if (ReferenceEquals(null, y)) return false;
                return this.Equals(x.FullName, y.FullName);
            }

            public bool Equals(DirectoryPath x, DirectoryPath y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(null, x)) return false;
                if (ReferenceEquals(null, y)) return false;
                return this.Equals(x.FullName, y.FullName);
            }
        }

        private sealed class CaseSensitiveComparerImpl : Comparer
        {
            public override int GetHashCode(FilePath obj)
                => obj.FullName.GetHashCode();

            public override int GetHashCode(DirectoryPath obj)
                => obj.FullName.GetHashCode();

            protected internal override bool Equals(string x, string y)
                => x == y;
        }

        private sealed class CaseInsensitiveComparerImpl : Comparer
        {
            public override int GetHashCode(FilePath obj)
                => StringComparer.OrdinalIgnoreCase.GetHashCode(obj.FullName);

            public override int GetHashCode(DirectoryPath obj)
                => StringComparer.OrdinalIgnoreCase.GetHashCode(obj.FullName);

            protected internal override bool Equals(string x, string y)
                => string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
        }

        /// <remarks>
        /// Currently, this is equals to <see cref="CaseInsensitiveComparer"/>.
        /// Some filesystems are case-sensitive, but others are not. Therefore the default is loosen the condition of equality.
        /// </remarks>
        public static Comparer DefaultComparer => CaseInsensitiveComparer;

        public static Comparer CaseSensitiveComparer { get; } = new CaseSensitiveComparerImpl();

        public static Comparer CaseInsensitiveComparer { get; } = new CaseInsensitiveComparerImpl();
    }
}
