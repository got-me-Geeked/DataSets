using FluentAssertions;
using MAX_PAIR_IN_GRAPH.Models;
using System;
using System.IO;
using System.Linq;
using Xunit;
using static Xunit.Assume;

namespace Test_MAX_PAIR_IN_GRAPH
{
    public class MatchingResultTests
    {
        // Вспомогательный метод для захвата вывода консоли
        private string CaptureConsoleOutput(Action action)
        {
            using (var sw = new StringWriter())
            {
                var originalOut = Console.Out;
                Console.SetOut(sw);
                try
                {
                    action();
                    return sw.ToString();
                }
                finally
                {
                    Console.SetOut(originalOut);
                }
            }
        }

 

        [Fact]
        public void Constructor_DefaultValues_AreSetCorrectly()
        {
            // Arrange & Act
            var result = new MatchingResult();

            // Assert
            result.MatchingSize.Should().Be(0);
            result.Matches.Should().NotBeNull();
            result.Matches.Should().BeEmpty();
            result.ExecutionTimeMs.Should().Be(0);
        }

        [Fact]
        public void Properties_CanBeSetAndGet()
        {
            // Arrange
            var result = new MatchingResult();
            var matches = new Dictionary<int, int> { { 0, 1 }, { 2, 3 } };

            // Act
            result.MatchingSize = 2;
            result.Matches = matches;
            result.ExecutionTimeMs = 123;

            // Assert
            result.MatchingSize.Should().Be(2);
            result.Matches.Should().BeSameAs(matches);
            result.Matches.Should().HaveCount(2);
            result.ExecutionTimeMs.Should().Be(123);
        }


        [Fact]
        public void FromRightMatches_WithValidArray_CreatesCorrectMatches()
        {
            // Arrange
            int[] matchRight = new int[] { 0, -1, 1, -1, 2 };
            int size = 3;

            // Act
            var result = MatchingResult.FromRightMatches(matchRight, size);

            // Assert
            result.MatchingSize.Should().Be(size);
            result.Matches.Should().HaveCount(3);

            // matchRight[v] = u означает, что левая вершина u соединена с правой v
            // В словаре Matches ключ = левая вершина, значение = правая вершина
            result.Matches[0].Should().Be(0); // matchRight[0] = 0
            result.Matches[1].Should().Be(2); // matchRight[2] = 1
            result.Matches[2].Should().Be(4); // matchRight[4] = 2
        }

        [Fact]
        public void FromRightMatches_WithEmptyArray_CreatesEmptyMatches()
        {
            // Arrange
            int[] matchRight = Array.Empty<int>();
            int size = 0;

            // Act
            var result = MatchingResult.FromRightMatches(matchRight, size);

            // Assert
            result.MatchingSize.Should().Be(0);
            result.Matches.Should().BeEmpty();
        }

        [Fact]
        public void FromRightMatches_WithAllMinusOne_CreatesEmptyMatches()
        {
            // Arrange
            int[] matchRight = new int[] { -1, -1, -1, -1 };
            int size = 0;

            // Act
            var result = MatchingResult.FromRightMatches(matchRight, size);

            // Assert
            result.MatchingSize.Should().Be(0);
            result.Matches.Should().BeEmpty();
        }

        [Fact]
        public void FromRightMatches_WithNullArray_ThrowsNullReferenceException()
        {
            // Arrange
            int[] matchRight = null;
            int size = 0;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => MatchingResult.FromRightMatches(matchRight, size));
        }

        [Fact]
        public void FromPairs_WithValidArray_CreatesCorrectMatches()
        {
            // Arrange
            int[] pairU = new int[] { 1, -1, 3, -1, 0 };
            int size = 3;

            // Act
            var result = MatchingResult.FromPairs(pairU, size);

            // Assert
            result.MatchingSize.Should().Be(size);
            result.Matches.Should().HaveCount(3);

            result.Matches[0].Should().Be(1); // pairU[0] = 1
            result.Matches[2].Should().Be(3); // pairU[2] = 3
            result.Matches[4].Should().Be(0); // pairU[4] = 0
        }

        [Fact]
        public void FromPairs_WithEmptyArray_CreatesEmptyMatches()
        {
            // Arrange
            int[] pairU = Array.Empty<int>();
            int size = 0;

            // Act
            var result = MatchingResult.FromPairs(pairU, size);

            // Assert
            result.MatchingSize.Should().Be(0);
            result.Matches.Should().BeEmpty();
        }

        [Fact]
        public void FromPairs_WithAllMinusOne_CreatesEmptyMatches()
        {
            // Arrange
            int[] pairU = new int[] { -1, -1, -1, -1 };
            int size = 0;

            // Act
            var result = MatchingResult.FromPairs(pairU, size);

            // Assert
            result.MatchingSize.Should().Be(0);
            result.Matches.Should().BeEmpty();
        }

        [Fact]
        public void FromPairs_WithNullArray_ThrowsNullReferenceException()
        {
            // Arrange
            int[] pairU = null;
            int size = 0;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => MatchingResult.FromPairs(pairU, size));
        }



        [Fact]
        public void Print_WithEmptyResult_OutputsCorrectly()
        {
            // Arrange
            var result = new MatchingResult();
            result.MatchingSize = 0;
            result.ExecutionTimeMs = 123;
            // Matches пустой по умолчанию

            // Act
            var output = CaptureConsoleOutput(() => result.Print());

            // Assert
            output.Should().Contain("=== Результат ===");
            output.Should().Contain("Размер паросочетания: 0");
            output.Should().Contain("Время выполнения: 123 ms");
            output.Should().Contain("=================");

            // Не должно быть строк с парами
            var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            lines.Should().HaveCount(4); // Заголовок, размер, время, разделитель
        }

        [Fact]
        public void Print_WithMatches_OutputsAllPairs()
        {
            // Arrange
            var result = new MatchingResult();
            result.MatchingSize = 3;
            result.ExecutionTimeMs = 456;
            result.Matches[0] = 1;
            result.Matches[2] = 3;
            result.Matches[4] = 5;

            // Act
            var output = CaptureConsoleOutput(() => result.Print());

            // Assert
            output.Should().Contain("Размер паросочетания: 3");
            output.Should().Contain("Время выполнения: 456 ms");
            output.Should().Contain("0 -> 1");
            output.Should().Contain("2 -> 3");
            output.Should().Contain("4 -> 5");

            // Проверяем сортировку по ключу
            var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var pairLines = lines.Where(l => l.Contains("->")).ToList();
            pairLines.Should().HaveCount(3);
            pairLines[0].Should().Be("0 -> 1");
            pairLines[1].Should().Be("2 -> 3");
            pairLines[2].Should().Be("4 -> 5");
        }

        [Fact]
        public void Print_WithUnsortedMatches_OutputsSortedByKey()
        {
            // Arrange
            var result = new MatchingResult();
            result.MatchingSize = 3;
            result.ExecutionTimeMs = 0;
            result.Matches[4] = 5;
            result.Matches[0] = 1;
            result.Matches[2] = 3;

            // Act
            var output = CaptureConsoleOutput(() => result.Print());

            // Assert
            var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var pairLines = lines.Where(l => l.Contains("->")).ToList();

            // Должны быть отсортированы по ключу (0, 2, 4)
            pairLines[0].Should().Be("0 -> 1");
            pairLines[1].Should().Be("2 -> 3");
            pairLines[2].Should().Be("4 -> 5");
        }


        [Fact]
        public void ToMatchingGraph_WithValidMatches_CreatesCorrectGraph()
        {
            // Arrange
            var result = new MatchingResult();
            result.MatchingSize = 3;
            result.Matches[0] = 1;
            result.Matches[2] = 3;
            result.Matches[4] = 5;
            int left = 6;
            int right = 6;

            // Act
            var graph = result.ToMatchingGraph(left, right);

            // Assert
            graph.Should().NotBeNull();
            graph.LeftCount.Should().Be(left);
            graph.RightCount.Should().Be(right);
            graph.EdgesCount.Should().Be(3);

            graph.Adjacency[0].Should().ContainSingle().And.Contain(1);
            graph.Adjacency[2].Should().ContainSingle().And.Contain(3);
            graph.Adjacency[4].Should().ContainSingle().And.Contain(5);

            // Другие вершины не должны иметь рёбер
            graph.Adjacency[1].Should().BeEmpty();
            graph.Adjacency[3].Should().BeEmpty();
            graph.Adjacency[5].Should().BeEmpty();
        }

        [Fact]
        public void ToMatchingGraph_WithEmptyMatches_CreatesGraphWithNoEdges()
        {
            // Arrange
            var result = new MatchingResult();
            result.MatchingSize = 0;
            // Matches пустой
            int left = 3;
            int right = 3;

            // Act
            var graph = result.ToMatchingGraph(left, right);

            // Assert
            graph.Should().NotBeNull();
            graph.LeftCount.Should().Be(left);
            graph.RightCount.Should().Be(right);
            graph.EdgesCount.Should().Be(0);
            graph.Adjacency.Should().AllSatisfy(set => set.Should().BeEmpty());
        }

        [Fact]
        public void ToMatchingGraph_WithMatchesExceedingBounds_ThrowsException()
        {
            // Arrange
            var result = new MatchingResult();
            result.MatchingSize = 1;
            result.Matches[0] = 5; // правая вершина 5 при RightCount=3
            int left = 3;
            int right = 3;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => result.ToMatchingGraph(left, right));
        }



        [Fact]
        public void FromRightMatches_And_ToMatchingGraph_AreConsistent()
        {
            // Arrange
            int[] matchRight = new int[] { 0, -1, 1, -1, 2 };
            int size = 3;
            int left = 3;
            int right = 5;

            // Act
            var result = MatchingResult.FromRightMatches(matchRight, size);
            var graph = result.ToMatchingGraph(left, right);

            // Assert
            graph.EdgesCount.Should().Be(size);

            // Проверяем, что рёбра соответствуют исходным данным
            // matchRight[v] = u означает ребро u-v
            // В графе должны быть рёбра: 0-0, 1-2, 2-4
            graph.Adjacency[0].Should().Contain(0);
            graph.Adjacency[1].Should().Contain(2);
            graph.Adjacency[2].Should().Contain(4);
        }

        [Fact]
        public void FromPairs_And_ToMatchingGraph_AreConsistent()
        {
            // Arrange
            int[] pairU = new int[] { 1, -1, 3, -1, 0 };
            int size = 3;
            int left = 5;
            int right = 4;

            // Act
            var result = MatchingResult.FromPairs(pairU, size);
            var graph = result.ToMatchingGraph(left, right);

            // Assert
            graph.EdgesCount.Should().Be(size);

            // Проверяем, что рёбра соответствуют исходным данным
            // pairU[u] = v означает ребро u-v
            // В графе должны быть рёбра: 0-1, 2-3, 4-0
            graph.Adjacency[0].Should().Contain(1);
            graph.Adjacency[2].Should().Contain(3);
            graph.Adjacency[4].Should().Contain(0);
        }



        [Theory]
        [InlineData(new int[] { 0, 1, 2 }, 3, new int[] { 0, 1, 2 })] // matchRight
        [InlineData(new int[] { -1, -1, -1 }, 0, new int[0])] // все -1
        [InlineData(new int[] { }, 0, new int[0])] // пустой
        public void FromRightMatches_Parameterized(int[] matchRight, int expectedSize, int[] expectedKeys)
        {
            // Act
            var result = MatchingResult.FromRightMatches(matchRight, expectedSize);

            // Assert
            result.MatchingSize.Should().Be(expectedSize);
            result.Matches.Keys.Should().BeEquivalentTo(expectedKeys);
        }

        [Theory]
        [InlineData(new int[] { 0, 1, 2 }, 3, new int[] { 0, 1, 2 })] // pairU
        [InlineData(new int[] { -1, -1, -1 }, 0, new int[0])] // все -1
        [InlineData(new int[] { }, 0, new int[0])] // пустой
        public void FromPairs_Parameterized(int[] pairU, int expectedSize, int[] expectedKeys)
        {
            // Act
            var result = MatchingResult.FromPairs(pairU, expectedSize);

            // Assert
            result.MatchingSize.Should().Be(expectedSize);
            result.Matches.Keys.Should().BeEquivalentTo(expectedKeys);
        }

    }
}