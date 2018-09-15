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
    public class FilePathTests
    {
        [Fact]
        public void RelativePathIsConvertedWithCurrentDirectory()
        {
            var cwd = Directory.GetCurrentDirectory();
            var file = new DestructiveFilePath(@"test");
            Assert.Equal(file.FullName, $"{cwd}{Path.DirectorySeparatorChar}test");
        }

        [Fact]
        public void PathSeparatorsInPathAreNormalized()
        {
            var cwd = Directory.GetCurrentDirectory();
            var file = new DestructiveFilePath(@"foo/bar\baz");
            Assert.Equal(file.FullName, $"{cwd}{Path.DirectorySeparatorChar}foo{Path.DirectorySeparatorChar}bar{Path.DirectorySeparatorChar}baz");
        }

        [Fact]
        public void TrailingPathSeparatorsInPathAreRemoved()
        {
            var cwd = Directory.GetCurrentDirectory();
            var file = new DestructiveFilePath(@"foo/bar\baz/\/");
            Assert.Equal(file.FullName, $"{cwd}{Path.DirectorySeparatorChar}foo{Path.DirectorySeparatorChar}bar{Path.DirectorySeparatorChar}baz");
        }

        [Fact]
        public void NameReturnsFileName()
        {
            var file = new FilePath(@"foo/bar.baz");
            Assert.Equal("bar.baz", file.Name);
        }

        [Fact]
        public void NameReturnsFileNameWithoutExtension()
        {
            var file1 = new FilePath(@"foo/bar.baz");
            Assert.Equal("bar", file1.NameWithoutExtension);
            var file2 = new FilePath(@"foo/bar.baz.qux");
            Assert.Equal("bar.baz", file2.NameWithoutExtension);
        }

        [Fact]
        public void ExtensionReturnsFileExtension()
        {
            var file1 = new FilePath(@"foo/bar.baz");
            Assert.Equal(".baz", file1.Extension);
            var file2 = new FilePath(@"foo/bar.baz.qux");
            Assert.Equal(".qux", file2.Extension);
        }

        [Fact]
        public void ExtensionReturnsEmptyIfFileDoesNotHaveExtension()
        {
            var file = new FilePath(@"foo/bar");
            Assert.Equal("", file.Extension);
        }

        [Fact]
        public void ExtensionReturnsEmptyIfFileEndsWithDot()
        {
            var file1 = new FilePath(@"foo/bar.");
            Assert.Equal("", file1.Extension);
            var file2 = new FilePath(@"foo/bar..");
            Assert.Equal("", file2.Extension);
        }

        [Fact]
        public void ExtensionEqualsIsTrueIfExtensionIsEqual()
        {
            var file = new FilePath(@"foo/bar.baz");
            Assert.True(file.ExtensionEquals(".baz"));
        }

        [Fact]
        public void ExtensionEqualsIsTrueIfExtensionIsEqualWithoutLeadingDot()
        {
            var file = new FilePath(@"foo/bar.baz");
            Assert.True(file.ExtensionEquals("baz"));
        }

        [Fact]
        public void ExtensionEqualsIgnoresCase()
        {
            var file = new FilePath("foo/bar.baz");
            Assert.True(file.ExtensionEquals("Baz"));
        }

        [Fact]
        public void ExtensionEqualsWithComparerJudgesByTheirBehavior()
        {
            var file = new FilePath("foo/bar.baz");
            Assert.True(file.ExtensionEquals("Baz", FileSystemPath.CaseInsensitiveComparer));
            Assert.False(file.ExtensionEquals("Baz", FileSystemPath.CaseSensitiveComparer));
        }

        [Fact]
        public void EqualsIsTrueIfFullNameIsEqual()
        {
            var file1 = new FilePath("foo/bar.baz");
            var file2 = new FilePath(@"foo\bar.baz");
            Assert.Equal(file1, file2);
        }

        [Fact]
        public void EqualsIgnoresCase()
        {
            var file1 = new FilePath("foobar");
            var file2 = new FilePath("FooBAR");
            Assert.Equal(file1, file2);
        }

        [Fact]
        public void EqualsWithComparerJudgesByTheirBehavior()
        {
            var file1 = new FilePath("foobar");
            var file2 = new FilePath("FooBAR");
            Assert.True(file1.Equals(file2, FileSystemPath.CaseInsensitiveComparer));
            Assert.False(file1.Equals(file2, FileSystemPath.CaseSensitiveComparer));
        }

        [Fact]
        public void EqualsOperatorIsTrueIfFullNameIsEqual()
        {
            var file1 = new FilePath("foo/bar.baz");
            var file2 = new FilePath(@"foo\bar.baz");
            Assert.True(file1 == file2);
            Assert.False(file1 != file2);
        }

        [Fact]
        public void EqualsOperatorIgnoresCase()
        {
            var file1 = new FilePath("foobar");
            var file2 = new FilePath("FooBAR");
            Assert.True(file1 == file2);
            Assert.False(file1 != file2);
        }

        [Fact]
        public void ExistIsTrueIfFileExists()
        {
            try
            {
                File.WriteAllText("file1", "test");
                var file = new FilePath("file1");
                Assert.True(file.Exists());
            }
            finally
            {
                File.Delete("file1");
            }
        }

        [Fact]
        public void ExistIsFalseIfFileDoesNotExists()
        {
            var file = new FilePath("file0");
            Assert.False(file.Exists());
        }

        [Fact]
        public void ExistIsFalseIfDirectoryExists()
        {
            try
            {
                Directory.CreateDirectory("dir1");
                var file = new FilePath("dir");
                Assert.False(file.Exists());
            }
            finally
            {
                Directory.Delete("dir1");
            }
        }

        [Fact]
        public void ExistIsNotCached()
        {
            File.WriteAllText("file1", "test");
            var file = new FilePath("file1");
            Assert.True(file.Exists());
            File.Delete("file1");
            Assert.False(file.Exists());
        }

        [Fact]
        public void NullIfNotExistsReturnsItselfIfFileExists()
        {
            try
            {
                File.WriteAllText("file1", "test");
                var file = new FilePath("file1");
                Assert.Same(file.NullIfNotExists(), file);
            }
            finally
            {
                File.Delete("file1");
            }
        }

        [Fact]
        public void NullIfNotExistsReturnsNullIfFileDoesNotExists()
        {
            var file = new FilePath("file0");
            Assert.Null(file.NullIfNotExists());
        }

        [Fact]
        public void WithExtensionReturnsFileWithSpecifiedExtension()
        {
            var file = new FilePath("file1.foo");
            Assert.Equal(file.WithExtension("bar"), new FilePath("file1.bar"));
        }

        [Fact]
        public void IsDescendantIsTrueIfDirectoryIsDescendant()
        {
            var file = new FilePath("foo/bar/baz");
            var dir = new DirectoryPath("foo");
            Assert.True(file.IsDescendantOf(dir));
        }

        [Fact]
        public void IsDescendantIgnoresCase()
        {
            var file = new FilePath("foo/bar/baz");
            var dir = new DirectoryPath("FOO");
            Assert.True(file.IsDescendantOf(dir));
        }

        [Fact]
        public void IsDescendantWithComparerJudgesByTheirBehavior()
        {
            var file = new FilePath("foo/bar/baz");
            var dir = new DirectoryPath("FOO");
            Assert.True(file.IsDescendantOf(dir, FileSystemPath.CaseInsensitiveComparer));
            Assert.False(file.IsDescendantOf(dir, FileSystemPath.CaseSensitiveComparer));
        }

        [Fact]
        public void ParentReturnsParentDirectory()
        {
            var file = new FilePath("foo/bar/baz");
            Assert.Equal(file.Parent, new DirectoryPath("foo/bar"));
        }

        [Fact]
        public void RootReturnsRootDirectory()
        {
            var file = new FilePath("foo/bar/baz");
            Assert.Equal(file.Root, new DirectoryPath("/"));
        }

        [Fact]
        public void ParentThrowsIfFileIsRootDirectory()
        {
            var file = new FilePath("/");
            Assert.Throws<InvalidOperationException>(() => file.Parent);
        }

        [Fact]
        public void TryGetParentReturnsNullIfFileIsRootDirectory()
        {
            var file = new FilePath("/");
            Assert.Null(file.TryGetParent());
        }
    }
}
