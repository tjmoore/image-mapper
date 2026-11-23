using ImageMapper.Services.Utils;
using Microsoft.Extensions.Configuration;

namespace ImageMapper.Tests
{
    public class FolderPatternTest
    {
        private string _root = null!;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(TestContext.CurrentContext.TestDirectory, $"pattern-test-{Guid.NewGuid()}");
            Directory.CreateDirectory(Path.Combine(_root, "a", "2024"));
            Directory.CreateDirectory(Path.Combine(_root, "b", "2024"));
            Directory.CreateDirectory(Path.Combine(_root, "b", "2023"));
            Directory.CreateDirectory(Path.Combine(_root, "c", "x", "2024"));
        }

        [TearDown]
        public void TearDown() => Directory.Delete(_root, recursive: true);

        [Test]
        public void StarMatchesSingleLevel()
        {
            var result = ImageFetcherHelpers.ExpandFolderPattern(Path.Combine(_root, "*", "2024")).ToList();

            Assert.That(result, Is.EquivalentTo(new[]
            {
                Path.Combine(_root, "a", "2024"),
                Path.Combine(_root, "b", "2024")
            }));
        }

        [Test]
        public void DoubleStarMatchesAnyDepth()
        {
            var result = ImageFetcherHelpers.ExpandFolderPattern(Path.Combine(_root, "**", "2024")).ToList();

            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result, Does.Contain(Path.Combine(_root, "c", "x", "2024")));
        }

        [Test]
        public void NonMatchingPatternReturnsNothing()
        {
            Assert.That(ImageFetcherHelpers.ExpandFolderPattern(Path.Combine(_root, "*", "1999")), Is.Empty);
        }

        [Test]
        public void LiteralPathIsUnchangedAndMissingIsDropped()
        {
            Assert.That(ImageFetcherHelpers.ExpandFolderPattern(_root), Is.EqualTo(new[] { _root }));
            Assert.That(ImageFetcherHelpers.ExpandFolderPattern(Path.Combine(_root, "missing")), Is.Empty);
        }

        [Test]
        public void UncPathPatternIsExpanded()
        {
            Assume.That(OperatingSystem.IsWindows());
            var drive = Path.GetPathRoot(_root)!.TrimEnd('\\').Replace(":", "$");
            var unc = @"\\localhost\" + drive + _root[2..];
            Assume.That(Directory.Exists(unc), "Administrative share unavailable");

            var result = ImageFetcherHelpers.ExpandFolderPattern(Path.Combine(unc, "*", "2024")).ToList();

            Assert.That(result, Has.Count.EqualTo(2));
        }

        [Test]
        public void ResolveImageFoldersExpandsAndDeduplicates()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection([
                    new("ImageFolders:0", Path.Combine(_root, "*", "2024")),
                    new("ImageFolders:1", Path.Combine(_root, "a", "2024"))
                ])
                .Build();

            Assert.That(ImageFetcherHelpers.ResolveImageFolders(config), Has.Length.EqualTo(2));
        }
    }
}
