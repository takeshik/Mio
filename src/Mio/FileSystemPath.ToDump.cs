/*
 * Mio I/O Library <https://github.com/takeshik/Mio>
 * Copyright © Takeshi KIRIYA (aka takeshik) <takeshik@tksk.io>
 * All rights reserved. Licensed under the MIT License.
 */

using JetBrains.Annotations;

namespace Mio
{
    partial class FileSystemPath
    {
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

            public string Name
                => this._path.Name;

            public object Parent
                => (object)this._path.TryGetParent() ?? "";
        }

        [UsedImplicitly]
        [NotNull]
        private protected object ToDump()
            => new DumpProxy(this);
    }
}
