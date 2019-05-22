/*
 * Mio I/O Library <https://github.com/takeshik/Mio>
 * Copyright © Takeshi KIRIYA (aka takeshik) <takeshik@tksk.io>
 * All rights reserved. Licensed under the MIT License.
 */

using System;
using JetBrains.Annotations;

namespace Mio
{
    partial class FileSystemPath
    {
        private static readonly Type _hyperlinqType = Type.GetType("LINQPad.Hyperlinq, LINQPad");

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private sealed class DumpProxy
        {
            private readonly FileSystemPath _path;

            public DumpProxy(FileSystemPath path)
            {
                this._path = path;
            }

            public override string ToString()
                => this._path.FullName;
        }

        [UsedImplicitly]
        [NotNull]
        private protected object ToDump()
            => _hyperlinqType == null
                ? new DumpProxy(this)
                : Activator.CreateInstance(_hyperlinqType, this.FullName, this.ToString());
    }
}
