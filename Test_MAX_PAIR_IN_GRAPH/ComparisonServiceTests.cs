using FluentAssertions;
using MAX_PAIR_IN_GRAPH;
using MAX_PAIR_IN_GRAPH.Models;
using MAX_PAIR_IN_GRAPH.Services;
using System;
using System.IO;
using Xunit;
using static Xunit.Assume;

namespace Test_MAX_PAIR_IN_GRAPH
{
    public class ComparisonServiceTests
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

        // Вспомогательный метод для создания тестового графа
        private BipartiteGraph CreateTestGraph(int left, int right, params (int u, int v)[] edges)
        {
            var graph = new BipartiteGraph();
            graph.Initialize(left, right);
            foreach (var (u, v) in edges)
            {
                graph.AddEdge(u, v);
            }
            return graph;
        }

        [Fact]
        public void Compare_WithValidGraph_ShouldOutputResults()
        {
            // Arrange
            var graph = CreateTestGraph(3, 3,
                (0, 0), (0, 1),
                (1, 1), (1, 2),
                (2, 2));

            // Assume: граф корректен
            Assume.NotNull(graph);
            Assume.True(graph.LeftCount > 0 && graph.RightCount > 0);

            // Act
            var output = CaptureConsoleOutput(() => ComparisonService.Compare(graph));

            // Assert
            output.Should().NotBeNullOrWhiteSpace();
            output.Should().Contain("Кун:");
            output.Should().Contain("Хопкрофт-Карп:");
            output.Should().Contain("ms");

            // Проверяем формат вывода с помощью регулярных выражений
            output.Should().MatchRegex(@"Кун: \d+, \d+ ms");
            output.Should().MatchRegex(@"Хопкрофт-Карп: \d+, \d+ ms");
        }

        [Fact]
        public void Compare_WithEmptyGraph_ShouldOutputZeroMatching()
        {
            // Arrange
            var graph = CreateTestGraph(3, 3); // без рёбер

            // Act
            var output = CaptureConsoleOutput(() => ComparisonService.Compare(graph));

            // Assert
            output.Should().Contain("Кун: 0,");
            output.Should().Contain("Хопкрофт-Карп: 0,");
        }

        [Fact]
        public void Compare_WithSingleEdgeGraph_ShouldOutputSizeOne()
        {
            // Arrange
            var graph = CreateTestGraph(2, 2, (0, 0));

            // Act
            var output = CaptureConsoleOutput(() => ComparisonService.Compare(graph));

            // Assert
            output.Should().Contain("Кун: 1,");
            output.Should().Contain("Хопкрофт-Карп: 1,");
        }

        [Fact]
        public void Compare_WithCompleteBipartiteGraph_ShouldOutputMaxMatching()
        {
            // Arrange - полный двудольный граф K3,3
            var graph = CreateTestGraph(3, 3,
                (0, 0), (0, 1), (0, 2),
                (1, 0), (1, 1), (1, 2),
                (2, 0), (2, 1), (2, 2));

            // Act
            var output = CaptureConsoleOutput(() => ComparisonService.Compare(graph));

            // Assert - максимальное паросочетание в K3,3 равно 3
            output.Should().Contain("Кун: 3,");
            output.Should().Contain("Хопкрофт-Карп: 3,");
        }

        [Fact]
        public void Compare_BothAlgorithms_ShouldReturnSameMatchingSize()
        {
            // Arrange - граф с несколькими возможными паросочетаниями
            var graph = CreateTestGraph(4, 4,
                (0, 0), (0, 1),
                (1, 1), (1, 2),
                (2, 2), (2, 3),
                (3, 3), (3, 0));

            // Act
            var output = CaptureConsoleOutput(() => ComparisonService.Compare(graph));

            // Assert - извлекаем размеры паросочетаний из вывода
            var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            lines.Should().HaveCount(2);

            var kuhnSize = ExtractMatchingSize(lines[0]);
            var hkSize = ExtractMatchingSize(lines[1]);

            kuhnSize.Should().Be(hkSize);
            kuhnSize.Should().BePositive();
        }

        private int ExtractMatchingSize(string line)
        {
            // Ожидаемый формат: "Кун: 3, 42 ms"
            var parts = line.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            return int.Parse(parts[1]);
        }

        [Fact]
        public void Compare_ExecutionTime_ShouldBeNonNegative()
        {
            // Arrange
            var graph = CreateTestGraph(5, 5,
                (0, 0), (1, 1), (2, 2), (3, 3), (4, 4));

            // Act
            var output = CaptureConsoleOutput(() => ComparisonService.Compare(graph));

            // Assert - извлекаем время выполнения
            var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            var kuhnTime = ExtractExecutionTime(lines[0]);
            var hkTime = ExtractExecutionTime(lines[1]);

            kuhnTime.Should().BeGreaterThanOrEqualTo(0);
            hkTime.Should().BeGreaterThanOrEqualTo(0);
        }

        private long ExtractExecutionTime(string line)
        {
            // Ожидаемый формат: "Кун: 3, 42 ms"
            var parts = line.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            return long.Parse(parts[2]);
        }

        [Fact]
        public void Compare_WithNullGraph_ShouldThrowNullReferenceException()
        {
            // Arrange
            BipartiteGraph nullGraph = null;

            // Assume
            Assume.Null(nullGraph);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ComparisonService.Compare(nullGraph));
        }

        [Theory]
        [InlineData(1, 1, 1)] // Минимальный граф с одним ребром
        [InlineData(2, 2, 2)] // Полный K2,2
        [InlineData(3, 2, 2)] // Несимметричный граф
        [InlineData(5, 5, 0)] // Пустой граф
        public void Compare_VariousGraphs_ShouldWork(int left, int right, int expectedMatching)
        {
            // Arrange
            var graph = new BipartiteGraph();
            graph.Initialize(left, right);

            // Добавляем рёбра для достижения ожидаемого паросочетания
            for (int i = 0; i < Math.Min(left, right) && i < expectedMatching; i++)
            {
                graph.AddEdge(i, i);
            }

            // Act
            var output = CaptureConsoleOutput(() => ComparisonService.Compare(graph));

            // Assert
            output.Should().Contain($"Кун: {expectedMatching},");
            output.Should().Contain($"Хопкрофт-Карп: {expectedMatching},");
        }

        [Fact]
        public void Compare_WithLargeGraph_ShouldCompleteWithoutTimeout()
        {
            // Arrange - создаём граф среднего размера
            var graph = new BipartiteGraph();
            graph.Initialize(100, 100);

            // Добавляем случайные рёбра
            var random = new Random(42); // фиксированный seed для воспроизводимости
            for (int u = 0; u < 100; u++)
            {
                for (int v = 0; v < 100; v++)
                {
                    if (random.NextDouble() > 0.7) // ~30% плотность
                        graph.AddEdge(u, v);
                }
            }

            // Act
            var exception = Record.Exception(() => ComparisonService.Compare(graph));

            // Assert
            exception.Should().BeNull();
        }

        [Fact]
        public void Compare_Output_ShouldBeFormattedCorrectly()
        {
            // Arrange
            var graph = CreateTestGraph(2, 2, (0, 0), (1, 1));

            // Act
            var output = CaptureConsoleOutput(() => ComparisonService.Compare(graph));
            var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // Assert - проверяем формат каждой строки
            lines.Should().HaveCount(2);

            // Проверяем, что строки соответствуют шаблону "Алгоритм: число, число ms"
            lines[0].Should().MatchRegex(@"^Кун: \d+, \d+ ms$");
            lines[1].Should().MatchRegex(@"^Хопкрофт-Карп: \d+, \d+ ms$");
        }

        [Fact]
        public void Compare_HopcroftKarp_ShouldBeFasterOrEqualForDenseGraphs()
        {
            // Arrange - плотный граф, где HK должен быть быстрее
            var graph = new BipartiteGraph();
            graph.Initialize(50, 50);

            // Добавляем много рёбер (почти полный граф)
            for (int u = 0; u < 50; u++)
            {
                for (int v = 0; v < 50; v++)
                {
                    if (u % 2 == 0 || v % 2 == 0) // ~75% рёбер
                        graph.AddEdge(u, v);
                }
            }

            // Act
            var output = CaptureConsoleOutput(() => ComparisonService.Compare(graph));
            var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            var kuhnTime = ExtractExecutionTime(lines[0]);
            var hkTime = ExtractExecutionTime(lines[1]);

            // Assert - Хопкрофт-Карп должен быть не медленнее Куна
            // (может быть не всегда из-за накладных расходов, но обычно так)
            hkTime.Should().BeLessThanOrEqualTo(kuhnTime);
        }
    }
}