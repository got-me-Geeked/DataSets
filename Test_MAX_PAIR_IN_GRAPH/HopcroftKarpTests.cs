using FluentAssertions;
using MAX_PAIR_IN_GRAPH;
using MAX_PAIR_IN_GRAPH.Algorithms;
using MAX_PAIR_IN_GRAPH.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Xunit.Assume;

namespace Test_MAX_PAIR_IN_GRAPH
{
    public class HopcroftKarpTests
    {
        private readonly HopcroftKarp _hopcroftKarp = new();
        private readonly Kuhn _kuhn = new(); // Для сравнения

        // Вспомогательный метод для создания графа
        private BipartiteGraph CreateGraph(int left, int right, params (int u, int v)[] edges)
        {
            var graph = new BipartiteGraph();
            graph.Initialize(left, right);
            foreach (var (u, v) in edges)
            {
                graph.AddEdge(u, v);
            }
            return graph;
        }

        // Проверка, что паросочетание корректно (нет общих вершин)
        private void AssertMatchingIsValid(MatchingResult result, BipartiteGraph graph)
        {
            // Проверяем размер
            result.MatchingSize.Should().Be(result.Matches.Count);

            // Проверяем, что нет повторяющихся левых вершин
            result.Matches.Keys.Should().OnlyHaveUniqueItems();

            // Проверяем, что нет повторяющихся правых вершин
            result.Matches.Values.Should().OnlyHaveUniqueItems();

            // Проверяем, что все рёбра существуют в графе
            foreach (var pair in result.Matches)
            {
                int u = pair.Key;
                int v = pair.Value;

                u.Should().BeInRange(0, graph.LeftCount - 1);
                v.Should().BeInRange(0, graph.RightCount - 1);
                graph.Adjacency[u].Should().Contain(v, $"ребро {u}->{v} должно существовать в графе");
            }
        }



        [Fact]
        public void FindMaximumMatching_EmptyGraph_ReturnsEmptyMatching()
        {
            // Arrange
            var graph = CreateGraph(3, 3);

            // Act
            var result = _hopcroftKarp.FindMaximumMatching(graph);

            // Assert
            result.Should().NotBeNull();
            result.MatchingSize.Should().Be(0);
            result.Matches.Should().BeEmpty();
        }

        [Fact]
        public void FindMaximumMatching_SingleEdge_ReturnsMatchingOfSize1()
        {
            // Arrange
            var graph = CreateGraph(3, 3, (0, 0));

            // Act
            var result = _hopcroftKarp.FindMaximumMatching(graph);

            // Assert
            result.MatchingSize.Should().Be(1);
            result.Matches.Should().HaveCount(1);
            result.Matches[0].Should().Be(0);

            AssertMatchingIsValid(result, graph);
        }

        [Fact]
        public void FindMaximumMatching_CompleteBipartiteK33_ReturnsMatchingOfSize3()
        {
            // Arrange - полный двудольный граф K3,3
            var graph = CreateGraph(3, 3,
                (0, 0), (0, 1), (0, 2),
                (1, 0), (1, 1), (1, 2),
                (2, 0), (2, 1), (2, 2));

            // Act
            var result = _hopcroftKarp.FindMaximumMatching(graph);

            // Assert
            result.MatchingSize.Should().Be(3);
            result.Matches.Should().HaveCount(3);

            AssertMatchingIsValid(result, graph);
        }

        [Fact]
        public void FindMaximumMatching_AsymmetricGraph_ReturnsMinLeftRightCount()
        {
            // Arrange - левых больше, чем правых
            var graph = CreateGraph(5, 3,
                (0, 0), (1, 0), (2, 1), (3, 1), (4, 2));

            // Act
            var result = _hopcroftKarp.FindMaximumMatching(graph);

            // Assert - максимальное паросочетание не может превышать меньшую долю
            result.MatchingSize.Should().BeLessThanOrEqualTo(3);
            result.MatchingSize.Should().Be(3); // В данном графе можно покрыть все правые вершины

            AssertMatchingIsValid(result, graph);
        }

        [Fact]
        public void FindMaximumMatching_DisconnectedGraph_ReturnsCorrectMatching()
        {
            // Arrange - граф с двумя компонентами связности
            var graph = CreateGraph(4, 4,
                (0, 0), (0, 1),  // компонента 1
                (1, 0), (1, 1),
                (2, 2), (2, 3),  // компонента 2
                (3, 2), (3, 3));

            // Act
            var result = _hopcroftKarp.FindMaximumMatching(graph);

            // Assert - можно получить паросочетание размера 4 (по 2 из каждой компоненты)
            result.MatchingSize.Should().Be(4);
            AssertMatchingIsValid(result, graph);
        }


        [Fact]
        public void CompareWithKuhn_OnRandomGraphs_ReturnsSameMatchingSize()
        {
            // Arrange - создаём несколько графов разных размеров
            var testGraphs = new List<BipartiteGraph>
            {
                CreateGraph(2, 2, (0, 0), (0, 1), (1, 0)), // K2,2 минус одно ребро
                CreateGraph(3, 4, (0, 0), (0, 1), (1, 1), (1, 2), (2, 2), (2, 3)), // Лесенка
                CreateGraph(4, 4, (0, 0), (0, 2), (1, 1), (1, 3), (2, 0), (2, 2), (3, 1), (3, 3)), // Два параллельных паросочетания
                CreateGraph(3, 3, (0, 0), (1, 1), (2, 2)), // Три отдельных ребра
                CreateGraph(5, 5, (0, 0), (0, 1), (1, 1), (1, 2), (2, 2), (2, 3), (3, 3), (3, 4), (4, 4), (4, 0)), // Цикл
            };

            foreach (var graph in testGraphs)
            {
                // Act
                var hkResult = _hopcroftKarp.FindMaximumMatching(graph);
                var kuhnResult = _kuhn.FindMaximumMatching(graph);

                // Assert
                hkResult.MatchingSize.Should().Be(
                    kuhnResult.MatchingSize,
                    $"для графа с {graph.LeftCount} левыми и {graph.RightCount} правыми вершинами");

                // Оба результата должны быть валидными паросочетаниями
                AssertMatchingIsValid(hkResult, graph);
                AssertMatchingIsValid(kuhnResult, graph);
            }
        }

        [Theory]
        [InlineData(3, 3, 9)]  // Полный K3,3
        [InlineData(5, 5, 15)] // Частичный граф
        [InlineData(10, 10, 50)] // Случайный граф
        [InlineData(20, 20, 200)] // Большой граф
        public void CompareWithKuhn_OnLargeGraphs_ReturnsSameSize(int left, int right, int edgeCount)
        {
            // Arrange - создаём граф со случайными рёбрами
            var graph = new BipartiteGraph();
            graph.Initialize(left, right);

            var random = new Random(42); // фиксированный seed для воспроизводимости
            int added = 0;
            while (added < edgeCount)
            {
                int u = random.Next(left);
                int v = random.Next(right);

                // Избегаем дубликатов
                if (!graph.Adjacency[u].Contains(v))
                {
                    graph.AddEdge(u, v);
                    added++;
                }
            }

            // Assume - проверяем, что граф создан корректно
            Assume.True(graph.EdgesCount > 0);

            // Act
            var hkResult = _hopcroftKarp.FindMaximumMatching(graph);
            var kuhnResult = _kuhn.FindMaximumMatching(graph);

            // Assert
            hkResult.MatchingSize.Should().Be(kuhnResult.MatchingSize);

            AssertMatchingIsValid(hkResult, graph);
            AssertMatchingIsValid(kuhnResult, graph);
        }

        [Fact]
        public void CompareWithKuhn_OnPathologicalCases_ReturnsSameResult()
        {
            // Arrange - граф, где порядок обхода может влиять на результат у Куна
            var graph = CreateGraph(4, 4,
                (0, 0), (0, 1),
                (1, 0), (1, 2),
                (2, 1), (2, 3),
                (3, 2), (3, 3));

            // Act
            var hkResult = _hopcroftKarp.FindMaximumMatching(graph);
            var kuhnResult = _kuhn.FindMaximumMatching(graph);

            // Assert - оба должны найти максимальное паросочетание (4)
            hkResult.MatchingSize.Should().Be(4);
            kuhnResult.MatchingSize.Should().Be(4);

            AssertMatchingIsValid(hkResult, graph);
        }

        [Fact]
        public void CompareWithKuhn_OnGraphWithMultipleOptimalMatchings_ReturnsSameSize()
        {
            // Arrange - граф, где есть несколько максимальных паросочетаний
            var graph = CreateGraph(3, 3,
                (0, 0), (0, 1),
                (1, 1), (1, 2),
                (2, 0), (2, 2));

            // Act
            var hkResult = _hopcroftKarp.FindMaximumMatching(graph);
            var kuhnResult = _kuhn.FindMaximumMatching(graph);

            // Assert
            hkResult.MatchingSize.Should().Be(3);
            kuhnResult.MatchingSize.Should().Be(3);

            // Конкретные пары могут отличаться, но размер должен быть одинаковым
            hkResult.MatchingSize.Should().Be(kuhnResult.MatchingSize);
        }


        [Fact]
        public void FindMaximumMatching_NullGraph_ThrowsNullReferenceException()
        {
            // Arrange
            BipartiteGraph nullGraph = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => _hopcroftKarp.FindMaximumMatching(nullGraph));
        }

        [Fact]
        public void FindMaximumMatching_GraphWithNoEdges_ReturnsZero()
        {
            // Arrange
            var graph = CreateGraph(5, 5);

            // Act
            var result = _hopcroftKarp.FindMaximumMatching(graph);

            // Assert
            result.MatchingSize.Should().Be(0);
            result.Matches.Should().BeEmpty();
        }

        [Fact]
        public void FindMaximumMatching_GraphWithIsolatedVertices_WorksCorrectly()
        {
            // Arrange - некоторые вершины изолированы
            var graph = CreateGraph(4, 4,
                (0, 0), (0, 1),
                (2, 2), (2, 3));
            // Вершины 1 и 3 левой доли изолированы

            // Act
            var result = _hopcroftKarp.FindMaximumMatching(graph);

            // Assert - максимум можно покрыть 2 ребра
            result.MatchingSize.Should().Be(2);
            AssertMatchingIsValid(result, graph);
        }

        [Fact]
        public void FindMaximumMatching_MultipleCalls_DoNotInterfere()
        {
            // Arrange
            var graph1 = CreateGraph(2, 2, (0, 0), (1, 1));
            var graph2 = CreateGraph(2, 2, (0, 1), (1, 0));

            // Act
            var result1 = _hopcroftKarp.FindMaximumMatching(graph1);
            var result2 = _hopcroftKarp.FindMaximumMatching(graph2);

            // Assert
            result1.MatchingSize.Should().Be(2);
            result2.MatchingSize.Should().Be(2);

            // Проверяем, что результаты не перепутались
            (result1.Matches.ContainsKey(0) && result1.Matches[0] == 0 ||
             result1.Matches.ContainsKey(0) && result1.Matches[0] == 1).Should().BeTrue();
        }



        [Fact]
        public void FindMaximumMatching_Result_MatchesPropertyIsCorrect()
        {
            // Arrange
            var graph = CreateGraph(3, 3,
                (0, 0), (0, 1),
                (1, 1), (1, 2),
                (2, 2), (2, 0));

            // Act
            var result = _hopcroftKarp.FindMaximumMatching(graph);

            // Assert
            result.MatchingSize.Should().Be(result.Matches.Count);

            // Проверяем, что ключи и значения в допустимых диапазонах
            result.Matches.Keys.Should().OnlyContain(u => u >= 0 && u < graph.LeftCount);
            result.Matches.Values.Should().OnlyContain(v => v >= 0 && v < graph.RightCount);
        }

        [Fact]
        public void FindMaximumMatching_Result_CanBeConvertedToGraph()
        {
            // Arrange
            var originalGraph = CreateGraph(4, 4,
                (0, 0), (0, 1),
                (1, 1), (1, 2),
                (2, 2), (2, 3),
                (3, 3), (3, 0));

            // Act
            var result = _hopcroftKarp.FindMaximumMatching(originalGraph);
            var matchingGraph = result.ToMatchingGraph(originalGraph.LeftCount, originalGraph.RightCount);

            // Assert
            matchingGraph.EdgesCount.Should().Be(result.MatchingSize);

            // Проверяем, что все рёбра matchingGraph есть в originalGraph
            for (int u = 0; u < matchingGraph.LeftCount; u++)
            {
                foreach (var v in matchingGraph.Adjacency[u])
                {
                    originalGraph.Adjacency[u].Should().Contain(v);
                }
            }
        }

        [Theory]
        [InlineData(2, 2, new int[] { 0, 0, 1, 1 }, 2)] // Два отдельных ребра
        [InlineData(2, 2, new int[] { 0, 0, 0, 1, 1, 0, 1, 1 }, 2)] // Полный K2,2
        [InlineData(3, 3, new int[] { 0, 0, 1, 1, 2, 2 }, 3)] // Три отдельных ребра
        [InlineData(3, 3, new int[] { 0, 0, 0, 1, 1, 0, 1, 2, 2, 1, 2, 2 }, 3)] // Почти полный
        [InlineData(2, 3, new int[] { 0, 0, 0, 1, 1, 1, 1, 2 }, 2)] // Левых меньше
        [InlineData(3, 2, new int[] { 0, 0, 1, 0, 1, 1, 2, 1 }, 2)] // Правых меньше
        [InlineData(1, 5, new int[] { 0, 0, 0, 1, 0, 2, 0, 3, 0, 4 }, 1)] // Одна левая вершина
        [InlineData(5, 1, new int[] { 0, 0, 1, 0, 2, 0, 3, 0, 4, 0 }, 1)] // Одна правая вершина
        public void FindMaximumMatching_VariousGraphs_ReturnsExpectedSize(
            int left, int right, int[] edges, int expectedSize)
        {
            // Arrange
            var graph = new BipartiteGraph();
            graph.Initialize(left, right);

            for (int i = 0; i < edges.Length; i += 2)
            {
                graph.AddEdge(edges[i], edges[i + 1]);
            }

            // Act
            var result = _hopcroftKarp.FindMaximumMatching(graph);

            // Assert
            result.MatchingSize.Should().Be(expectedSize);
            AssertMatchingIsValid(result, graph);
        }


        [Fact]
        public void HopcroftKarp_ShouldBeFasterOrEqual_ForDenseGraphs()
        {
            // Arrange - создаём достаточно большой плотный граф
            var graph = new BipartiteGraph();
            graph.Initialize(100, 100);

            // Добавляем много рёбер (почти полный граф)
            for (int u = 0; u < 100; u++)
            {
                for (int v = 0; v < 100; v++)
                {
                    if (u % 3 != 0 || v % 3 != 0) // ~89% рёбер
                        graph.AddEdge(u, v);
                }
            }

            // Act - измеряем время выполнения
            var sw = new System.Diagnostics.Stopwatch();

            sw.Start();
            var hkResult = _hopcroftKarp.FindMaximumMatching(graph);
            sw.Stop();
            long hkTime = sw.ElapsedMilliseconds;

            sw.Restart();
            var kuhnResult = _kuhn.FindMaximumMatching(graph);
            sw.Stop();
            long kuhnTime = sw.ElapsedMilliseconds;

            // Assert - Хопкрофт-Карп должен быть не медленнее Куна
            // (на больших плотных графах он обычно быстрее)
            hkTime.Should().BeLessThanOrEqualTo(kuhnTime * 2); // Допускаем некоторую погрешность

            // Размеры должны совпадать
            hkResult.MatchingSize.Should().Be(kuhnResult.MatchingSize);
        }

    }
}