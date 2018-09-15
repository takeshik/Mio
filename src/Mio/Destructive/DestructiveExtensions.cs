/*
 * Mio I/O Library <https://github.com/takeshik/Mio>
 * Copyright © Takeshi KIRIYA (aka takeshik) <takeshik@tksk.io>
 * All rights reserved. Licensed under the MIT License.
 */

using JetBrains.Annotations;

namespace Mio.Destructive
{
    public static class DestructiveExtensions
    {
        [Pure]
        [NotNull]
        public static DestructiveFilePath AsDestructive([NotNull] this FilePath file)
            => file.CreateDestructive();

        [Pure]
        [NotNull]
        public static DestructiveDirectoryPath AsDestructive([NotNull] this DirectoryPath directory)
            => directory.CreateDestructive();
    }
}
