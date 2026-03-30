using FluentAssertions;
using MAX_PAIR_IN_GRAPH;
using MAX_PAIR_IN_GRAPH.Models;
using MAX_PAIR_IN_GRAPH.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Sdk;
using static Xunit.Assume;

namespace Test_MAX_PAIR_IN_GRAPH
{
    public class GraphInputServiceTests
    {
        // Вспомогательный метод для эмуляции ввода с консоли
        private void SimulateConsoleInput(string input)
        {
            var stringReader = new StringReader(input);
            Console.SetIn(stringReader);
        }

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

        // Сброс консоли после каждого теста (чтобы не влиять на другие тесты)
        public GraphInputServiceTests()
        {
            // Возвращаем стандартный ввод/вывод после каждого теста
            var standardIn = new StreamReader(Console.OpenStandardInput());
            var standardOut = new StreamWriter(Console.OpenStandardOutput());
            Console.SetIn(standardIn);
            Console.SetOut(standardOut);
        }

    

        [Fact]
        public void ManualInput_ValidInput_ReturnsGraph()
        {
            // Arrange - симулируем ввод пользователя: L=2, R=2, рёбра: 0 0, 0 1, 1 0, 1 1, пустая строка
            var input = "2\n2\n0 0\n0 1\n1 0\n1 1\n\n";
            SimulateConsoleInput(input);

            // Act
            var graph = GraphInputService.ManualInput();

            // Assert
            Assert.NotNull(graph);
            Assert.Equal(2, graph.LeftCount);
            Assert.Equal(2, graph.RightCount);
            Assert.Equal(4, graph.EdgesCount);

            // Assert (FluentAssertions)
            graph.Should().NotBeNull();
            graph.LeftCount.Should().Be(2);
            graph.RightCount.Should().Be(2);
            graph.EdgesCount.Should().Be(4);
            graph.Adjacency[0].Should().BeEquivalentTo(new[] { 0, 1 });
            graph.Adjacency[1].Should().BeEquivalentTo(new[] { 0, 1 });
        }

        [Theory]
        [InlineData("0\n2\n0 0\n\n", typeof(ArgumentException), "L", "Количество вершин должно быть > 0")]      // L <= 0
        [InlineData("-1\n2\n0 0\n\n", typeof(ArgumentException), "L", "Количество вершин должно быть > 0")]     // L < 0
        [InlineData("3000000000\n2\n0 0\n\n", typeof(OverflowException), "L", null)] // L > MAX_INT
        [InlineData("2\n0\n0 0\n\n", typeof(ArgumentException), "R", "Количество вершин должно быть > 0")]      // R <= 0
        [InlineData("2\n-1\n0 0\n\n", typeof(ArgumentException), "R", "Количество вершин должно быть > 0")]     // R < 0
        [InlineData("2\n3000000000\n0 0\n\n", typeof(OverflowException), "R", null)] // R > MAX_INT
        public void ManualInput_InvalidLR_ThrowsException(
     string consoleInput,
     Type expectedExceptionType,
     string expectedParameter,
     string expectedMessage)
        {
            // Arrange
            SimulateConsoleInput(consoleInput);

            // Act & Assert
            var exception = Assert.Throws(expectedExceptionType, () => GraphInputService.ManualInput());

            // Дополнительные проверки для разных типов исключений
            if (expectedExceptionType == typeof(ArgumentException))
            {
                var argEx = exception as ArgumentException;
                argEx.Should().NotBeNull();

                // Проверяем сообщение
                if (expectedMessage != null)
                    argEx.Message.Should().Contain(expectedMessage);

    
                if (expectedParameter == "L")
                    Console.WriteLine("Ошибка в параметре left"); // для отладки
                else if (expectedParameter == "R")
                    Console.WriteLine("Ошибка в параметре right");
            }
            else if (expectedExceptionType == typeof(OverflowException))
            {
                exception.Should().BeOfType<OverflowException>();
            }
        }


        [Theory]
        [InlineData("2\n2\n-1 0\n\n")] // IND L < 0 (TC_IN_10)
        [InlineData("2\n2\n2 0\n\n")] // IND L >= L (TC_IN_11)
        [InlineData("2\n2\n0 -1\n\n")] // IND R < 0 (TC_IN_14)
        [InlineData("2\n2\n0 2\n\n")] // IND R >= R (TC_IN_15)
        public void ManualInput_InvalidIndices_ThrowsException(string consoleInput)
        {
            // Arrange
            SimulateConsoleInput(consoleInput);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => GraphInputService.ManualInput());
            exception.Message.Should().Contain("Ошибка валидации");
            exception.Message.Should().Contain("индексы");
        }

       

        [Theory]
        [InlineData("a\n2\n0 0\n\n")] // L не число (TC_IN_4)
        [InlineData("2\nb\n0 0\n\n")] // R не число (TC_IN_8)
        public void ManualInput_NonNumericLR_ThrowsFormatException(string consoleInput)
        {
            // Arrange
            SimulateConsoleInput(consoleInput);

            // Act & Assert
            Assert.Throws<FormatException>(() => GraphInputService.ManualInput());
        }

        [Fact]
        public void ManualInput_EmptyEdges_ReturnsGraphWithNoEdges()
        {
            // Arrange
            var input = "3\n3\n\n"; // Только L и R, пустая строка сразу
            SimulateConsoleInput(input);

            // Act
            var graph = GraphInputService.ManualInput();

            // Assert
            graph.Should().NotBeNull();
            graph.LeftCount.Should().Be(3);
            graph.RightCount.Should().Be(3);
            graph.EdgesCount.Should().Be(0);
            graph.Adjacency.Should().AllSatisfy(set => set.Should().BeEmpty());
        }


        [Fact]
        public void ValidateStructure_NullGraph_ReturnsFalse()
        {
            // Arrange
            BipartiteGraph graph = null;

            // Act
            var result = GraphInputService.ValidateStructure(graph);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateStructure_NullAdjacency_ReturnsFalse()
        {
            // Arrange
            var graph = new BipartiteGraph
            {
                LeftCount = 2,
                RightCount = 2,
                Adjacency = null
            };

            // Act
            var result = GraphInputService.ValidateStructure(graph);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateStructure_NullRowInAdjacency_ReturnsFalse()
        {
            // Arrange
            var graph = new BipartiteGraph();
            graph.Initialize(2, 2);
            graph.Adjacency[0] = null; // Искусственно делаем null

            // Act
            var result = GraphInputService.ValidateStructure(graph);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateStructure_InvalidEdgeRightIndex_ReturnsFalse()
        {
            // Arrange
            var graph = new BipartiteGraph();
            graph.Initialize(2, 2);

            // Добавляем некорректное ребро напрямую, минуя AddEdge
            graph.Adjacency[0].Add(2); // v = 2, RightCount = 2 -> невалидно

            // Act
            var result = GraphInputService.ValidateStructure(graph);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateStructure_ValidGraph_ReturnsTrueAndCorrectsLeftCount()
        {
            // Arrange
            var graph = new BipartiteGraph();
            graph.Initialize(5, 3); // LeftCount = 5
            graph.AddEdge(0, 0);
            graph.AddEdge(1, 1);

            // Искусственно меняем LeftCount на неверное значение
            graph.LeftCount = 10;

            // Act
            var result = GraphInputService.ValidateStructure(graph);

            // Assert
            result.Should().BeTrue();
            graph.LeftCount.Should().Be(5); 
        }

        [Fact]
        public void ValidateStructure_ValidGraphWithAllEdges_ReturnsTrue()
        {
            // Arrange
            var graph = new BipartiteGraph();
            graph.Initialize(2, 2);
            graph.AddEdge(0, 0);
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 0);
            graph.AddEdge(1, 1);

            // Act
            var result = GraphInputService.ValidateStructure(graph);

            // Assert
            result.Should().BeTrue();
            graph.LeftCount.Should().Be(2);
        }


        [Fact]
        public void ValidateBipartiteViaColoring_ValidGraph_ReturnsTrue()
        {
            // Arrange
            var graph = new BipartiteGraph();
            graph.Initialize(2, 2);
            graph.AddEdge(0, 0);
            graph.AddEdge(1, 1);

            // Act
            var result = GraphInputService.ValidateBipartiteViaColoring(graph);

            // Assert
            result.Should().BeTrue();
        }

        

        [Fact]
        public void ManualInput_WithMockConsole_ShouldCallReadLine()
        {
            // Arrange - создаём мок для TextReader
            var mockReader = new Mock<TextReader>();
            var sequence = new MockSequence();

            // Настраиваем последовательность вызовов ReadLine
            mockReader.InSequence(sequence).Setup(r => r.ReadLine()).Returns("2");
            mockReader.InSequence(sequence).Setup(r => r.ReadLine()).Returns("2");
            mockReader.InSequence(sequence).Setup(r => r.ReadLine()).Returns("0 0");
            mockReader.InSequence(sequence).Setup(r => r.ReadLine()).Returns("");

            var originalIn = Console.In;
            Console.SetIn(mockReader.Object);

            try
            {
                // Act
                var graph = GraphInputService.ManualInput();

                // Assert
                graph.Should().NotBeNull();

                // Verify, что ReadLine был вызван нужное количество раз
                mockReader.Verify(r => r.ReadLine(), Times.AtLeast(4));
            }
            finally
            {
                Console.SetIn(originalIn);
            }
        }

        [Fact]
        public void ValidateStructure_WithMockGraph_ShouldCheckProperties()
        {
            // Arrange
            var mockGraph = new Mock<BipartiteGraph>();
            var adjacency = new List<HashSet<int>>
    {
        new HashSet<int> { 0, 1 },
        new HashSet<int> { 0, 1 }
    };

            mockGraph.Setup(g => g.Adjacency).Returns(adjacency);
            mockGraph.Setup(g => g.RightCount).Returns(2);
            mockGraph.Setup(g => g.LeftCount).Returns(2);

            // Настраиваем и геттер, и сеттер для LeftCount
            mockGraph.SetupProperty(g => g.LeftCount, 2); // Начальное значение 2

            // Act
            var result = GraphInputService.ValidateStructure(mockGraph.Object);

            // Assert
            result.Should().BeTrue();

            // Verify, что свойства были прочитаны
            mockGraph.Verify(g => g.Adjacency, Times.AtLeastOnce);
            mockGraph.Verify(g => g.RightCount, Times.AtLeastOnce);
        }

        [Theory]
        [InlineData(2, 2, 4, true)]  // Плотный граф
        [InlineData(5, 5, 0, true)]  // Пустой граф
        [InlineData(0, 2, 0, false)] // Невалидный L
        [InlineData(2, 0, 0, false)] // Невалидный R
        public void ValidateStructure_ParameterizedTest(int left, int right, int edgesCount, bool expectedValid)
        {
            // Arrange
            var graph = new BipartiteGraph();

            // Assume: если left или right <= 0, Initialize бросит исключение
            if (left <= 0 || right <= 0)
            {
                Assert.Throws<ArgumentException>(() => graph.Initialize(left, right));
                return;
            }

            graph.Initialize(left, right);

            // Добавляем рёбра в зависимости от edgesCount (упрощённо)
            if (edgesCount == 4 && left >= 2 && right >= 2)
            {
                graph.AddEdge(0, 0);
                graph.AddEdge(0, 1);
                graph.AddEdge(1, 0);
                graph.AddEdge(1, 1);
            }

            // Act
            var result = GraphInputService.ValidateStructure(graph);

            // Assert
            result.Should().Be(expectedValid);
        }

    }
}