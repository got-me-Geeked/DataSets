using FluentAssertions;
using MAX_PAIR_IN_GRAPH;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Xunit.Assume;

namespace Test_MAX_PAIR_IN_GRAPH
{
    public class BipartiteGraphTests
    {

        [Fact]
        public void Initialize_WithValidParameters_SetsPropertiesCorrectly()
        {
            // Arrange
            var graph = new BipartiteGraph();
            int left = 3;
            int right = 4;

            // Act
            graph.Initialize(left, right);

            // Assert
            graph.LeftCount.Should().Be(left);
            graph.RightCount.Should().Be(right);
            graph.Adjacency.Should().NotBeNull();
            graph.Adjacency.Should().HaveCount(left);
            graph.Adjacency.Should().AllSatisfy(set => set.Should().BeEmpty());
            graph.EdgesCount.Should().Be(0);
        }

        [Theory]
        [InlineData(0, 5)]
        [InlineData(-1, 5)]
        [InlineData(5, 0)]
        [InlineData(5, -1)]
        [InlineData(0, 0)]
        [InlineData(-5, -5)]
        public void Initialize_WithNonPositiveParameters_ThrowsArgumentException(int left, int right)
        {
            // Arrange
            var graph = new BipartiteGraph();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => graph.Initialize(left, right));
            exception.Message.Should().Contain("Количество вершин должно быть > 0");
        }

        [Fact]
        public void Initialize_CanBeCalledMultipleTimes_ReinitializesGraph()
        {
            // Arrange
            var graph = new BipartiteGraph();
            graph.Initialize(3, 4);
            graph.AddEdge(0, 0);
            graph.AddEdge(1, 1);

            // Assume
            Assume.True(graph.EdgesCount > 0);

            // Act - повторная инициализация
            graph.Initialize(2, 2);

            // Assert
            graph.LeftCount.Should().Be(2);
            graph.RightCount.Should().Be(2);
            graph.Adjacency.Should().HaveCount(2);
            graph.EdgesCount.Should().Be(0);
        }


        [Fact]
        public void AddEdge_WithValidIndices_AddsEdge()
        {
            // Arrange
            var graph = new BipartiteGraph();
            graph.Initialize(3, 3);

            // Act
            graph.AddEdge(0, 1);
            graph.AddEdge(2, 2);

            // Assert
            graph.Adjacency[0].Should().Contain(1);
            graph.Adjacency[2].Should().Contain(2);
            graph.EdgesCount.Should().Be(2);
        }

        [Theory]
        [InlineData(-1, 0)] // u < 0
        [InlineData(5, 0)]  // u >= LeftCount
        [InlineData(0, -1)] // v < 0
        [InlineData(0, 5)]  // v >= RightCount
        [InlineData(-1, -1)] // оба невалидны
        public void AddEdge_WithInvalidIndices_ThrowsArgumentOutOfRangeException(int u, int v)
        {
            // Arrange
            var graph = new BipartiteGraph();
            graph.Initialize(3, 3);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => graph.AddEdge(u, v));
        }

        [Fact]
        public void AddEdge_DuplicateEdge_DoesNotIncreaseEdgeCount()
        {
            // Arrange
            var graph = new BipartiteGraph();
            graph.Initialize(3, 3);

            // Act
            graph.AddEdge(0, 0);
            graph.AddEdge(0, 0); // Добавляем то же ребро повторно

            // Assert
            graph.Adjacency[0].Should().ContainSingle();
            graph.EdgesCount.Should().Be(1);
        }

        [Fact]
        public void AddEdge_AfterInitialize_AddsToCorrectVertex()
        {
            // Arrange
            var graph = new BipartiteGraph();
            graph.Initialize(5, 5);

            // Act
            for (int i = 0; i < 5; i++)
            {
                graph.AddEdge(i, i);
                graph.AddEdge(i, (i + 1) % 5);
            }

            // Assert
            graph.EdgesCount.Should().Be(10);
            for (int i = 0; i < 5; i++)
            {
                graph.Adjacency[i].Should().Contain(i);
                graph.Adjacency[i].Should().Contain((i + 1) % 5);
            }
        }

        [Fact]
        public void EdgesCount_WithNoEdges_ReturnsZero()
        {
            // Arrange
            var graph = new BipartiteGraph();
            graph.Initialize(3, 3);

            // Act & Assert
            graph.EdgesCount.Should().Be(0);
        }

        [Fact]
        public void EdgesCount_AfterAddingEdges_ReturnsCorrectCount()
        {
            // Arrange
            var graph = new BipartiteGraph();
            graph.Initialize(5, 5);

            // Act
            for (int i = 0; i < 5; i++)
            {
                graph.AddEdge(i, i);
                graph.AddEdge(i, (i + 1) % 5);
                graph.AddEdge(i, (i + 2) % 5);
            }

            // Assert
            graph.EdgesCount.Should().Be(15);
        }

        [Fact]
        public void Adjacency_AfterInitialize_IsListOfHashSets()
        {
            // Arrange
            var graph = new BipartiteGraph();

            // Act
            graph.Initialize(3, 4);

            // Assert
            graph.Adjacency.Should().BeOfType<List<HashSet<int>>>();
            graph.Adjacency.Should().HaveCount(3);
            graph.Adjacency.Should().AllBeOfType<HashSet<int>>();
        }

        [Fact]
        public void Properties_AreVirtual_CanBeOverridden()
        {
            // Проверка, что свойства виртуальные (для моков)
            typeof(BipartiteGraph).GetProperty("LeftCount").GetGetMethod().IsVirtual.Should().BeTrue();
            typeof(BipartiteGraph).GetProperty("RightCount").GetGetMethod().IsVirtual.Should().BeTrue();
            typeof(BipartiteGraph).GetProperty("Adjacency").GetGetMethod().IsVirtual.Should().BeTrue();
        }



        [Fact]
        public void Initialize_WithMaximumValues_ShouldWork()
        {
            // Arrange
            var graph = new BipartiteGraph();
            int left = 1000;
            int right = 1000;

            // Act
            graph.Initialize(left, right);

            // Assert
            graph.LeftCount.Should().Be(left);
            graph.RightCount.Should().Be(right);
            graph.Adjacency.Should().HaveCount(left);
        }

        [Fact]
        public void AddEdge_ToMaximumCapacity_ShouldWork()
        {
            // Arrange
            var graph = new BipartiteGraph();
            int left = 100;
            int right = 100;
            graph.Initialize(left, right);

            // Act - добавляем максимально возможное количество рёбер
            for (int u = 0; u < left; u++)
            {
                for (int v = 0; v < right; v++)
                {
                    graph.AddEdge(u, v);
                }
            }

            // Assert
            graph.EdgesCount.Should().Be(left * right);
            foreach (var set in graph.Adjacency)
            {
                set.Should().HaveCount(right);
            }
        }

        [Fact]
        public void Adjacency_Modification_DoesNotAffectEdgeCountConsistency()
        {
            // Arrange
            var graph = new BipartiteGraph();
            graph.Initialize(3, 3);
            graph.AddEdge(0, 0);
            graph.AddEdge(0, 1);

            // Assume
            Assume.True(graph.EdgesCount == 2);

            // Act - прямое изменение Adjacency (нарушение инкапсуляции)
            graph.Adjacency[0].Add(2);
            graph.Adjacency[1].Add(1);

            // Assert - EdgesCount должен отражать изменения
            graph.EdgesCount.Should().Be(4);
        }


        [Fact]
        public void Constructor_DefaultValues_AreZeroOrEmpty()
        {
            // Arrange & Act
            var graph = new BipartiteGraph();

            // Assert
            graph.LeftCount.Should().Be(0);
            graph.RightCount.Should().Be(0);
            graph.Adjacency.Should().NotBeNull();
            graph.Adjacency.Should().BeEmpty();
            graph.EdgesCount.Should().Be(0);
        }



        [Fact]
        public void Graph_CanBeSerializedAndDeserialized()
        {
            // Этот тест требует JsonService, но мы можем его добавить
            // для проверки совместимости с JSON-сериализацией

            // Arrange
            var original = new BipartiteGraph();
            original.Initialize(2, 2);
            original.AddEdge(0, 0);
            original.AddEdge(0, 1);
            original.AddEdge(1, 1);

            // Act - используем System.Text.Json напрямую
            var json = System.Text.Json.JsonSerializer.Serialize(original);
            var deserialized = System.Text.Json.JsonSerializer.Deserialize<BipartiteGraph>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized.LeftCount.Should().Be(original.LeftCount);
            deserialized.RightCount.Should().Be(original.RightCount);

            // Сравниваем Adjacency (упрощённо)
            deserialized.Adjacency.Should().HaveCount(original.Adjacency.Count);
            for (int i = 0; i < original.Adjacency.Count; i++)
            {
                deserialized.Adjacency[i].Should().BeEquivalentTo(original.Adjacency[i]);
            }
        }

    }
}