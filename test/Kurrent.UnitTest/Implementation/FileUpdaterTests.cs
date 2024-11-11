using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Kurrent.Implementation.Git;
using Kurrent.Models.Data;
using Kurrent.Utils;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Kurrent.UnitTest.Implementation.Git
{
    public class FileUpdaterTests
    {
        private readonly Mock<ILogger<FileUpdater>> _loggerMock;
        private readonly FileUpdater _sut;

        public FileUpdaterTests()
        {
            _loggerMock = new Mock<ILogger<FileUpdater>>();
            _sut = new FileUpdater(_loggerMock.Object);
        }

        [Fact]
        public async Task UpdateFilesInRepository_WithMatchingTag_UpdatesLine()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                Repository.Init(tempDir);
                using var repo = new Repository(tempDir);

                string host = "myregistry.azurecr.io";
                string repoName = "hello-world";
                string tag(int version) => $"development-test-update{version}";
                string entry(int version) => $"acrImage: {host}/{repoName}:{tag(version)} # kurrent update; regex: .*dev*.;";

                var contentWithTag = $@"foo: hello there
{entry(8)}
noDomainImage: hello-world:development # kurrent update; regex: .*dev*.;
pollingImage: owdev.azurecr.io/epc-api/api:1.0.29802-development-613f742 # kurrent update; regex: .*dev*.;";

                var file1 = Path.Combine(tempDir, "file1.txt");
                await File.WriteAllTextAsync(file1, contentWithTag);

                var repoConfig = new RepositoryConfig
                {
                    FileExtensions =new List<string> { ".txt" },
                    Name = "TestRepo"
                };

                var image = new Image
                {
                    Tag = tag(9),
                    Host = host,
                    Repository = repoName
                };

                // Act
                await _sut.UpdateFilesInRepository(repo, repoConfig, image);

                // Assert
                var updatedContent = await File.ReadAllTextAsync(file1);

                using var _ = new AssertionScope();
                updatedContent.Should().Contain(entry(9));
                updatedContent.Should().NotContain(entry(8));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public async Task UpdateFilesInRepository_WithMatchingTag_UpdatesMultipleLines()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                Repository.Init(tempDir);
                using var repo = new Repository(tempDir);

                string host = "myregistry.azurecr.io";
                string repoName = "hello-world";
                string tag(int version) => $"development-test-update{version}";
                string entry(int version) => $"acrImage: {host}/{repoName}:{tag(version)} # kurrent update; regex: .*dev*.;";

                var contentWithTag = $@"foo: hello there
{entry(8)}
{entry(8)}
noDomainImage: hello-world:development # kurrent update; regex: .*dev*.;
pollingImage: owdev.azurecr.io/epc-api/api:1.0.29802-development-613f742 # kurrent update; regex: .*dev*.;";

                var file1 = Path.Combine(tempDir, "file1.txt");
                await File.WriteAllTextAsync(file1, contentWithTag);

                var repoConfig = new RepositoryConfig
                {
                    FileExtensions = new List<string> { ".txt" },
                    Name = "TestRepo"
                };

                var image = new Image
                {
                    Tag = tag(9),
                    Host = host,
                    Repository = repoName
                };

                // Act
                await _sut.UpdateFilesInRepository(repo, repoConfig, image);

                // Assert
                var updatedContent = await File.ReadAllTextAsync(file1);
                var lines = updatedContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                using var _ = new AssertionScope();
                lines.Count(line => line.Contains(entry(9))).Should().Be(2); // updated entries
                lines.Count(line => line.Contains(entry(8))).Should().Be(0); // original entries
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public async Task UpdateFilesInRepository_WithoutMatchingTag_DoesNotAlterLine()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                Repository.Init(tempDir);
                using var repo = new Repository(tempDir);

                var contentWithTag = @"foo: hello there
acrImage: myregistry.azurecr.io/hello-world:development-test-update8 # kurrent update; regex: .*dev*.;
noDomainImage: hello-world:development # kurrent update; regex: .*dev*.;
pollingImage: owdev.azurecr.io/epc-api/api:1.0.29802-development-613f742 # kurrent update; regex: .*dev*.;";

                var file1 = Path.Combine(tempDir, "file1.txt");
                await File.WriteAllTextAsync(file1, contentWithTag);

                var repoConfig = new RepositoryConfig
                {
                    FileExtensions = new List<string> { ".txt" },
                    Name = "TestRepo"
                };

                var image = new Image
                {
                    Tag = "production-update1",
                    Host = "myregistry.azurecr.io",
                    Repository = "hello-world"
                };

                // Act
                await _sut.UpdateFilesInRepository(repo, repoConfig, image);

                // Assert
                var updatedContent = await File.ReadAllTextAsync(file1);
                updatedContent.Should().Be(contentWithTag);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
