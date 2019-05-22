/*
 * Mio I/O Library <https://github.com/takeshik/Mio>
 * Copyright © Takeshi KIRIYA (aka takeshik) <takeshik@tksk.io>
 * All rights reserved. Licensed under the MIT License.
 */

using System;
using System.IO;
using Mio.Destructive;
using Xunit;

namespace Mio.Tests
{
    public class DirectoryPathTests
    {
        [Fact]
        public void RelativePathIsConvertedWithCurrentDirectory()
        {
            var cwd = Directory.GetCurrentDirectory();
            var dir = new DestructiveDirectoryPath(@"test");
            Assert.Equal(dir.FullName, $"{cwd}{Path.DirectorySeparatorChar}test");
        }

        [Fact]
        public void PathSeparatorsInPathAreNormalized()
        {
            var cwd = Directory.GetCurrentDirectory();
            var dir = new DestructiveDirectoryPath(@"foo/bar\baz");
            Assert.Equal(dir.FullName, $"{cwd}{Path.DirectorySeparatorChar}foo{Path.DirectorySeparatorChar}bar{Path.DirectorySeparatorChar}baz");
        }

        [Fact]
        public void TrailingPathSeparatorsInPathAreRemoved()
        {
            var cwd = Directory.GetCurrentDirectory();
            var dir = new DestructiveDirectoryPath(@"foo/bar\baz/\/");
            Assert.Equal(dir.FullName, $"{cwd}{Path.DirectorySeparatorChar}foo{Path.DirectorySeparatorChar}bar{Path.DirectorySeparatorChar}baz");
        }

        [Fact]
        public void NameReturnsDirectoryName()
        {
            var dir = new DirectoryPath(@"foo/bar.baz");
            Assert.Equal("bar.baz", dir.Name);
        }

        [Fact]
        public void NameReturnsDirectoryNameWithoutExtension()
        {
            var dir1 = new DirectoryPath(@"foo/bar.baz");
            Assert.Equal("bar", dir1.NameWithoutExtension);
            var dir2 = new DirectoryPath(@"foo/bar.baz.qux");
            Assert.Equal("bar.baz", dir2.NameWithoutExtension);
        }

        [Fact]
        public void ExtensionReturnsDirectoryExtension()
        {
            var dir1 = new DirectoryPath(@"foo/bar.baz");
            Assert.Equal(".baz", dir1.Extension);
            var dir2 = new DirectoryPath(@"foo/bar.baz.qux");
            Assert.Equal(".qux", dir2.Extension);
        }

        [Fact]
        public void ExtensionReturnsEmptyIfDirectoryDoesNotHaveExtension()
        {
            var dir = new DirectoryPath(@"foo/bar");
            Assert.Equal("", dir.Extension);
        }

        [Fact]
        public void ExtensionReturnsEmptyIfDirectoryEndsWithDot()
        {
            var dir1 = new DirectoryPath(@"foo/bar.");
            Assert.Equal("", dir1.Extension);
            var dir2 = new DirectoryPath(@"foo/bar..");
            Assert.Equal("", dir2.Extension);
        }

        [Fact]
        public void ExtensionEqualsIsTrueIfExtensionIsEqual()
        {
            var dir = new DirectoryPath(@"foo/bar.baz");
            Assert.True(dir.ExtensionEquals(".baz"));
        }

        [Fact]
        public void ExtensionEqualsIsTrueIfExtensionIsEqualWithoutLeadingDot()
        {
            var dir = new DirectoryPath(@"foo/bar.baz");
            Assert.True(dir.ExtensionEquals("baz"));
        }

        [Fact]
        public void ExtensionEqualsIgnoresCase()
        {
            var dir = new DirectoryPath("foo/bar.baz");
            Assert.True(dir.ExtensionEquals("Baz"));
        }

        [Fact]
        public void ExtensionEqualsWithComparerJudgesByTheirBehavior()
        {
            var dir = new DirectoryPath("foo/bar.baz");
            Assert.True(dir.ExtensionEquals("Baz", FileSystemPathComparer.CaseInsensitive));
            Assert.False(dir.ExtensionEquals("Baz", FileSystemPathComparer.CaseInsensitive));
        }

        [Fact]
        public void EqualsIsTrueIfFullNameIsEqual()
        {
            var dir1 = new DirectoryPath("foo/bar.baz");
            var dir2 = new DirectoryPath(@"foo\bar.baz");
            Assert.Equal(dir1, dir2);
        }

        [Fact]
        public void EqualsIgnoresCase()
        {
            var dir1 = new DirectoryPath("foobar");
            var dir2 = new DirectoryPath("FooBAR");
            Assert.Equal(dir1, dir2);
        }

        [Fact]
        public void EqualsOperatorIsTrueIfFullNameIsEqual()
        {
            var dir1 = new DirectoryPath("foo/bar.baz");
            var dir2 = new DirectoryPath(@"foo\bar.baz");
            Assert.True(dir1 == dir2);
            Assert.False(dir1 != dir2);
        }

        [Fact]
        public void EqualsOperatorIgnoresCase()
        {
            var dir1 = new DirectoryPath("foobar");
            var dir2 = new DirectoryPath("FooBAR");
            Assert.True(dir1 == dir2);
            Assert.False(dir1 != dir2);
        }

        [Fact]
        public void EqualsWithComparerJudgesByTheirBehavior()
        {
            var dir1 = new DirectoryPath("foobar");
            var dir2 = new DirectoryPath("FooBAR");
            Assert.True(dir1.Equals(dir2, FileSystemPathComparer.CaseInsensitive));
            Assert.False(dir1.Equals(dir2, FileSystemPathComparer.CaseSensitive));
        }

        [Fact]
        public void ExistIsTrueIfDirectoryExists()
        {
            try
            {
                Directory.CreateDirectory("dir");
                var dir = new DirectoryPath("dir");
                Assert.True(dir.Exists());
            }
            finally
            {
                Directory.Delete("dir");
            }
        }

        [Fact]
        public void ExistIsFalseIfDirectoryDoesNotExists()
        {
            var dir = new DirectoryPath("dir0");
            Assert.False(dir.Exists());
        }

        [Fact]
        public void ExistIsFalseIfFileExists()
        {
            try
            {
                File.WriteAllText("file1", "test");
                var dir = new DirectoryPath("file");
                Assert.False(dir.Exists());
            }
            finally
            {
                File.Delete("file1");
            }
        }

        [Fact]
        public void ExistIsNotCached()
        {
            Directory.CreateDirectory("dir1");
            var dir = new DirectoryPath("dir1");
            Assert.True(dir.Exists());
            Directory.Delete("dir1");
            Assert.False(dir.Exists());
        }

        [Fact]
        public void NullIfNotExistsReturnsItselfIfDirectoryExists()
        {
            try
            {
                Directory.CreateDirectory("dir1");
                var dir = new DirectoryPath("dir1");
                Assert.Same(dir.NullIfNotExists(), dir);
            }
            finally
            {
                Directory.Delete("dir1");
            }
        }

        [Fact]
        public void NullIfNotExistsReturnsNullIfDirectoryDoesNotExists()
        {
            var dir = new DirectoryPath("dir0");
            Assert.Null(dir.NullIfNotExists());
        }

        [Fact]
        public void WithExtensionReturnsDirectoryWithSpecifiedExtension()
        {
            var dir = new DirectoryPath("dir1.foo");
            Assert.Equal(dir.WithExtension("bar"), new DirectoryPath("dir1.bar"));
        }

        [Fact]
        public void IsDescendantIsTrueIfDirectoryIsDescendant()
        {
            var dir1 = new DirectoryPath("foo/bar/baz");
            var dir2 = new DirectoryPath("foo");
            Assert.True(dir1.IsDescendantOf(dir2));
        }

        [Fact]
        public void IsDescendantIgnoresCase()
        {
            var dir1 = new DirectoryPath("foo/bar/baz");
            var dir2 = new DirectoryPath("FOO");
            Assert.True(dir1.IsDescendantOf(dir2));
        }

        [Fact]
        public void IsDescendantWithComparerJudgesByTheirBehavior()
        {
            var dir1 = new DirectoryPath("foo/bar/baz");
            var dir2 = new DirectoryPath("FOO");
            Assert.True(dir1.IsDescendantOf(dir2, FileSystemPathComparer.CaseInsensitive));
            Assert.False(dir1.IsDescendantOf(dir2, FileSystemPathComparer.CaseSensitive));
        }

        [Fact]
        public void ParentReturnsParentDirectory()
        {
            var dir = new DirectoryPath("foo/bar/baz");
            Assert.Equal(dir.Parent, new DirectoryPath("foo/bar"));
        }

        [Fact]
        public void RootReturnsRootDirectory()
        {
            var dir = new DirectoryPath("foo/bar/baz");
            Assert.Equal(dir.Root, new DirectoryPath("/"));
        }

        [Fact]
        public void ParentThrowsIfDirectoryIsRootDirectory()
        {
            var dir = new DirectoryPath("/");
            Assert.Throws<InvalidOperationException>(() => dir.Parent);
        }

        [Fact]
        public void TryGetParentReturnsNullIfDirectoryIsRootDirectory()
        {
            var dir = new DirectoryPath("/");
            Assert.Null(dir.TryGetParent());
        }
    }
}
