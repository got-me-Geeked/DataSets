using FluentAssertions;
using MAX_PAIR_IN_GRAPH;
using MAX_PAIR_IN_GRAPH.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;   
using Xunit;
using static Xunit.Assume;

namespace Test_MAX_PAIR_IN_GRAPH
{
    public class RecommendationServiceTests
    {
        // Вспомогательный метод для создания графа с заданными рёбрами
        private BipartiteGraph CreateGraph(int leftCount, int rightCount, params (int u, int v)[] edges)
        {
            var graph = new BipartiteGraph();
            graph.Initialize(leftCount, rightCount);
            foreach (var (u, v) in edges)
            {
                graph.AddEdge(u, v);
            }
            return graph;
        }

        // Перехват вывода консоли(Spy)
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

        // Данные для параметризованного теста
        public static IEnumerable<object[]> RecommendationTestData()
        {
            // Плотный граф: полный двудольный K2,2 (4 ребра)
            yield return new object[]
            {
                2, 2,
                new (int u, int v)[] { (0,0), (0,1), (1,0), (1,1) },
                "Граф плотный — рекомендуется алгоритм Хопкрофта-Карпа."
            };
            // Разреженный граф: пустой граф 5x5
            yield return new object[]
            {
                5, 5,
                Array.Empty<(int u, int v)>(),
                "Граф разреженный — алгоритм Куна допустим."
            };
        }

        //параметризованный тест
        [Theory]
        [MemberData(nameof(RecommendationTestData))]
        public void PrintRecommendation_ShouldOutputCorrectRecommendation(
                int leftCount,
                int rightCount,
                (int u, int v)[] edges,
                string expectedMessage)
        {
            // Arrange
            var graph = CreateGraph(leftCount, rightCount, edges);

            // Assume: проверяем, что граф корректен (иначе тест не имеет смысла)
            Assume.NotNull(graph);
            Assume.True(graph.LeftCount > 0 && graph.RightCount > 0);

            // Act
            var output = CaptureConsoleOutput(() => RecommendationService.PrintRecommendation(graph));

            // Assert
            Assert.NotNull(output);
            Assert.Contains("Рекомендация:", output);
            Assert.Contains(expectedMessage, output);

            // Assert (FluentAssertions matchers)
            output.Should().StartWith("\nРекомендация:");
            output.Should().Contain(expectedMessage);
        }

        [Fact]
        public void PrintRecommendation_WithMock_ShouldReadGraphProperties()
        {
            // Arrange: создаём mock графа (Mock)
            var mockGraph = new Mock<BipartiteGraph>();
            var adjacency = new List<HashSet<int>>
            {
                new HashSet<int> { 0, 1 },
                new HashSet<int> { 0, 1 }
            };
            mockGraph.Setup(g => g.Adjacency).Returns(adjacency);
            mockGraph.Setup(g => g.LeftCount).Returns(2);
            mockGraph.Setup(g => g.RightCount).Returns(2);

            Assume.NotNull(mockGraph.Object);

            // Act
            var output = CaptureConsoleOutput(() => RecommendationService.PrintRecommendation(mockGraph.Object));

            // Assert
            output.Should().Contain("Граф плотный");

            // Verify, что все свойства были прочитаны
            mockGraph.Verify(g => g.Adjacency, Times.AtLeastOnce);
            mockGraph.Verify(g => g.LeftCount, Times.AtLeastOnce);
            mockGraph.Verify(g => g.RightCount, Times.AtLeastOnce);
        }

        [Fact]
        public void PrintRecommendation_NullGraph_ShouldThrowArgumentNullException()
        {
            // Arrange
            BipartiteGraph nullGraph = null;

            // Assume: явно проверяем, что объект null
            Assume.Null(nullGraph);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => RecommendationService.PrintRecommendation(nullGraph));
        }

        [Fact]
        public void PrintRecommendation_ZeroDivisions_ShouldThrowDivideByZeroException()
        {
            // Arrange: создаём граф с нулевыми долями напрямую (без Initialize)
            var graph = new BipartiteGraph
            {
                LeftCount = 0,
                RightCount = 0,
                Adjacency = new List<HashSet<int>>()
            };

            // Assume: проверяем, что доли неположительные
            Assume.True(graph.LeftCount <= 0 || graph.RightCount <= 0);

            // Act & Assert
            Assert.Throws<DivideByZeroException>(() => RecommendationService.PrintRecommendation(graph));
        }
    }
}
