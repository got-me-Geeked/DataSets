using FluentAssertions;
using MAX_PAIR_IN_GRAPH;
using MAX_PAIR_IN_GRAPH.Models;
using MAX_PAIR_IN_GRAPH.Services;
using Moq;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using static Xunit.Assume;

namespace Test_MAX_PAIR_IN_GRAPH
{
    public class JsonServiceTests
    {
        // Вспомогательный метод для создания временного файла с содержимым
        private string CreateTempJsonFile(string content)
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, content);
            return path;
        }

        // Вспомогательный метод для создания временной директории
        private string GetTempFilePath(string fileName)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            return Path.Combine(tempDir, fileName);
        }

        [Fact]
        public async Task LoadGraphAsync_ValidJson_ReturnsGraph()
        {
            // Arrange
            var json = @"{
                ""LeftCount"": 2,
                ""RightCount"": 2,
                ""Adjacency"": [[0,1], [0,1]]
            }";
            var path = CreateTempJsonFile(json);

            // Assume: файл существует
            Assume.True(File.Exists(path));

            // Act
            var graph = await JsonService.LoadGraphAsync(path);

            // Assert
            Assert.NotNull(graph);
            Assert.Equal(2, graph.LeftCount);
            Assert.Equal(2, graph.RightCount);
            Assert.Equal(2, graph.Adjacency.Count);
            Assert.Equal(4, graph.EdgesCount);
            Assert.Contains(0, graph.Adjacency[0]);
            Assert.Contains(1, graph.Adjacency[0]);
            Assert.Contains(0, graph.Adjacency[1]);
            Assert.Contains(1, graph.Adjacency[1]);

            // Assert (matchers)
            graph.Should().NotBeNull();
            graph.LeftCount.Should().Be(2);
            graph.RightCount.Should().Be(2);
            graph.Adjacency.Should().HaveCount(2);
            graph.EdgesCount.Should().Be(4);

            // Cleanup
            File.Delete(path);
        }

        [Fact]
        public async Task LoadGraphAsync_FileNotFound_ReturnsNull()
        {
            // Arrange
            var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");

            // Assume: файл не существует
            Assume.False(File.Exists(nonExistentPath));

            // Act
            var result = await JsonService.LoadGraphAsync(nonExistentPath);

            // Assert
            Assert.Null(result);
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("invalid json")]
        [InlineData("")]
        public async Task LoadGraphAsync_InvalidJson_ReturnsNull(string invalidJson)
        {
            // Arrange
            var path = CreateTempJsonFile(invalidJson);

            // Assume: файл существует
            Assume.True(File.Exists(path));

            // Act
            var result = await JsonService.LoadGraphAsync(path);

            // Assert
            Assert.Null(result);
            result.Should().BeNull();

            // Cleanup
            File.Delete(path);
        }

        [Theory]
        [InlineData("{}")]
        [InlineData("{ \"LeftCount\": 2 }")]
        public async Task LoadGraphAsync_PartialJson_ReturnsGraphWithDefaults(string partialJson)
        {
            // Arrange
            var path = CreateTempJsonFile(partialJson);

            // Assume
            Assume.True(File.Exists(path));

            // Act
            var result = await JsonService.LoadGraphAsync(path);

            // Assert
            Assert.NotNull(result); 
            result.Should().NotBeNull();

            // Проверяем, что объект создан, но данные неполные
            if (partialJson.Contains("LeftCount"))
                result.LeftCount.Should().Be(2);
            else
                result.LeftCount.Should().Be(0);

            result.RightCount.Should().Be(0);
            result.Adjacency.Should().BeEmpty(); // или пустой список

            // Cleanup
            File.Delete(path);
        }

        [Fact]
        public async Task LoadGraphAsync_WithException_ShouldHandleGracefully()
        {
            // Arrange - создаём файл, но потом блокируем его (для имитации исключения)
            var path = CreateTempJsonFile("{}");

            // Блокируем файл, чтобы вызвать исключение при чтении
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                // Act - пытаемся прочитать заблокированный файл
                var result = await JsonService.LoadGraphAsync(path);

                // Assert - метод должен перехватить исключение и вернуть null
                Assert.Null(result);
                result.Should().BeNull();
            }

            // Cleanup
            File.Delete(path);
        }

        [Fact]
        public async Task SaveAsync_ValidGraph_SavesFile()
        {
            // Arrange
            var graph = new BipartiteGraph();
            graph.Initialize(2, 2);
            graph.AddEdge(0, 0);
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 0);
            graph.AddEdge(1, 1);

            var path = GetTempFilePath("graph.json");

            // Assume: директория не существует (будет создана)
            var dir = Path.GetDirectoryName(path);
            Assume.False(Directory.Exists(dir));

            // Act
            await JsonService.SaveAsync(path, graph);

            // Assert
            File.Exists(path).Should().BeTrue();
            Directory.Exists(dir).Should().BeTrue();

            // Проверяем содержимое
            var savedContent = await File.ReadAllTextAsync(path);
            var cleaned = savedContent.Replace("\n", "")
                                        .Replace("\r", "")
                                        .Replace("\t", "")
                                        .Replace(" ", "");
            savedContent.Should().NotBeNullOrEmpty();
            savedContent.Should().Contain("\"LeftCount\": 2");
            savedContent.Should().Contain("\"RightCount\": 2");
            savedContent.Should().Contain("\"Adjacency\": [");
            cleaned.Should().Contain("[0,1]");

            // Cleanup
            File.Delete(path);
            Directory.Delete(dir, true);
        }

        [Fact]
        public async Task SaveAsync_AnonymousObject_SavesFile()
        {
            // Arrange - тестируем сохранение произвольного объекта
            var testData = new
            {
                Name = "Test",
                Value = 42,
                Items = new[] { 1, 2, 3 }
            };

            var path = GetTempFilePath("anonymous.json");

            // Act
            await JsonService.SaveAsync(path, testData);

            // Assert
            File.Exists(path).Should().BeTrue();

            var savedContent = await File.ReadAllTextAsync(path);
            var cleaned = savedContent.Replace("\n", "")
                                        .Replace("\r", "")
                                        .Replace("\t", "")
                                        .Replace(" ", "");
            savedContent.Should().Contain("\"Name\": \"Test\"");
            savedContent.Should().Contain("\"Value\": 42");
            cleaned.Should().Contain("\"Items\":[1,2,3]");

            // Cleanup
            File.Delete(path);
            Directory.Delete(Path.GetDirectoryName(path), true);
        }

        [Fact]
        public async Task SaveAsync_ShouldCreateDeepDirectoryIfNotExists()
        {
            // Arrange
            var graph = new BipartiteGraph();
            graph.Initialize(1, 1);

            var deepPath = GetTempFilePath(Path.Combine("subdir1", "subdir2", "subdir3", "graph.json"));

            // Assume: глубокая директория не существует
            var deepDir = Path.GetDirectoryName(deepPath);
            Assume.False(Directory.Exists(deepDir));

            // Act
            await JsonService.SaveAsync(deepPath, graph);

            // Assert
            Directory.Exists(deepDir).Should().BeTrue();
            File.Exists(deepPath).Should().BeTrue();

            // Cleanup
            File.Delete(deepPath);
            Directory.Delete(deepDir, true);
        }

        [Fact]
        public async Task SaveAsync_WhenIOExceptionOccurs_ShouldHandleGracefully()
        {
            // Arrange
            var graph = new BipartiteGraph();
            graph.Initialize(1, 1);

            // Создаём путь, который невозможно создать (например, с некорректными символами)
            // или путь к защищённой директории
            var invalidPath = Path.Combine(Path.GetTempPath(), ":\0invalid:"); // содержит недопустимые символы

            // Act - метод не должен бросать исключение, а должен перехватить и вывести сообщение
            var exception = await Record.ExceptionAsync(() => JsonService.SaveAsync(invalidPath, graph));

            // Assert
            Assert.Null(exception); // Метод должен перехватить исключение и не пробрасывать его
        }

        [Fact]
        public async Task SaveAsync_WithNullData_ShouldSaveNull()
        {
            // Arrange
            var path = GetTempFilePath("null.json");

            // Act
            await JsonService.SaveAsync<object>(path, null!);

            // Assert
            File.Exists(path).Should().BeTrue();
            var content = await File.ReadAllTextAsync(path);
            content.Should().Be("null"); // JSON-представление null

            // Cleanup
            File.Delete(path);
            Directory.Delete(Path.GetDirectoryName(path), true);
        }
    }
}