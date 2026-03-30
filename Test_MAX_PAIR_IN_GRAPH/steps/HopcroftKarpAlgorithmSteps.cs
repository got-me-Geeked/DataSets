using MAX_PAIR_IN_GRAPH;
using MAX_PAIR_IN_GRAPH.Algorithms;
using MAX_PAIR_IN_GRAPH.Models;
using MAX_PAIR_IN_GRAPH.Services;
using Reqnroll;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Test_MAX_PAIR_IN_GRAPH.Steps
{
    [Binding]
    public class HopcroftKarpAlgorithmSteps
    {
        private BipartiteGraph? _graph;
        private MatchingResult? _result;
        private Exception? _capturedException;
        private string? _hopcroftKarpTestDataPath;
        private string? _capturedConsoleOutput;

        // Для перехвата консольного вывода
        private StringWriter? _consoleOutput;
        private TextWriter? _originalOutput;

      

        private string GetFullPath(string filename)
        {
            if (_hopcroftKarpTestDataPath == null)
                throw new InvalidOperationException("Тестовая директория не задана. Используйте шаг 'Given тестовые данные для алгоритма Хопкрофта-Карпа находятся в директории'");
            return Path.Combine(_hopcroftKarpTestDataPath, filename);
        }

        private void StartCaptureConsoleOutput()
        {
            _originalOutput = Console.Out;
            _consoleOutput = new StringWriter();
            Console.SetOut(_consoleOutput);
        }

        private string StopCaptureConsoleOutput()
        {
            Console.SetOut(_originalOutput!);
            var output = _consoleOutput!.ToString();
            _consoleOutput.Dispose();
            return output ?? string.Empty;
        }

        private void CreateTestGraphFile(string fullPath, string filename)
        {
            switch (filename)
            {
                case "simple_graph.json":
                    CreateSimpleGraph(fullPath);
                    break;
                case "complete_graph.json":
                    CreateCompleteGraph(fullPath);
                    break;
                case "empty_graph.json":
                    CreateEmptyGraph(fullPath);
                    break;
                case "single_edge.json":
                    CreateSingleEdgeGraph(fullPath);
                    break;
                case "large_graph.json":
                    CreateLargeGraph(fullPath);
                    break;
                case "invalid_edge.json":
                    CreateInvalidEdgeGraph(fullPath);
                    break;
                default:
                    CreateDefaultGraph(fullPath);
                    break;
            }
        }

        private void CreateSimpleGraph(string fullPath)
        {
            // Простой граф: 2x2, рёбра: 0-0, 0-1, 1-0
            var content = @"{
                ""LeftCount"": 2,
                ""RightCount"": 2,
                ""Adjacency"": [[0,1], [0]]
            }";
            File.WriteAllText(fullPath, content);
        }

        private void CreateCompleteGraph(string fullPath)
        {
            // Полный двудольный граф: 3x3, все рёбра
            var adjacency = new System.Text.StringBuilder();
            adjacency.Append("[");
            for (int i = 0; i < 3; i++)
            {
                adjacency.Append("[0,1,2]");
                if (i < 2) adjacency.Append(",");
            }
            adjacency.Append("]");

            var content = $@"{{
                ""LeftCount"": 3,
                ""RightCount"": 3,
                ""Adjacency"": {adjacency}
            }}";
            File.WriteAllText(fullPath, content);
        }


        private void CreateEmptyGraph(string fullPath)
        {
            // Пустой граф: 3x3, без рёбер
            var content = @"{
                ""LeftCount"": 3,
                ""RightCount"": 3,
                ""Adjacency"": [[], [], []]
            }";
            File.WriteAllText(fullPath, content);
        }

        private void CreateSingleEdgeGraph(string fullPath)
        {
            // Граф с одним ребром: 2x2, только ребро 0-0
            var content = @"{
                ""LeftCount"": 2,
                ""RightCount"": 2,
                ""Adjacency"": [[0], []]
            }";
            File.WriteAllText(fullPath, content);
        }

        private void CreateLargeGraph(string fullPath)
        {
            // Большой граф: 10x10, все рёбра (плотный)
            var adjacency = new System.Text.StringBuilder();
            adjacency.Append("[");
            for (int i = 0; i < 10; i++)
            {
                adjacency.Append("[");
                for (int j = 0; j < 10; j++)
                {
                    adjacency.Append(j);
                    if (j < 9) adjacency.Append(",");
                }
                adjacency.Append("]");
                if (i < 9) adjacency.Append(",");
            }
            adjacency.Append("]");

            var content = $@"{{
                ""LeftCount"": 10,
                ""RightCount"": 10,
                ""Adjacency"": {adjacency}
            }}";
            File.WriteAllText(fullPath, content);
        }

        private void CreateInvalidEdgeGraph(string fullPath)
        {
            // Граф с некорректным ребром
            var content = @"{
                ""LeftCount"": 2,
                ""RightCount"": 2,
                ""Adjacency"": [[0,2], [0,1]]
            }";
            File.WriteAllText(fullPath, content);
        }

        private void CreateDefaultGraph(string fullPath)
        {
            // Граф по умолчанию
            var content = @"{
                ""LeftCount"": 2,
                ""RightCount"": 2,
                ""Adjacency"": [[0,1], [0,1]]
            }";
            File.WriteAllText(fullPath, content);
        }


        [Given(@"тестовые данные для алгоритма Хопкрофта-Карпа находятся в каталоге ""(.*)""")]
        public void GivenTestDataInDirectory(string directoryName)
        {
            _hopcroftKarpTestDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryName);

            if (!Directory.Exists(_hopcroftKarpTestDataPath))
            {
                var sourcePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", directoryName);
                if (Directory.Exists(sourcePath))
                {
                    _hopcroftKarpTestDataPath = sourcePath;
                }
                else
                {
                    throw new DirectoryNotFoundException($"Директория с тестовыми данными не найдена: {_hopcroftKarpTestDataPath}");
                }
            }
        }

        [Given(@"я загрузил граф для алгоритма Хопкрофта-Карпа из файла ""(.*)""")]
        public async Task GivenLoadedGraphFromJson(string filename)
        {
            var fullPath = GetFullPath(filename);

            if (!File.Exists(fullPath))
            {
                CreateTestGraphFile(fullPath, filename);
            }

            StartCaptureConsoleOutput();

            try
            {
                _graph = await JsonService.LoadGraphAsync(fullPath);
                _capturedException = null;
            }
            catch (Exception ex)
            {
                _capturedException = ex;
                _graph = null;
            }
            finally
            {
                _capturedConsoleOutput = StopCaptureConsoleOutput();
            }
        }

        [Given(@"граф для алгоритма Хопкрофта-Карпа не загружен")]
        public void GivenGraphNotLoaded()
        {
            _graph = null;
            _result = null;
            _capturedException = null;
            _capturedConsoleOutput = null;
        }

        [When(@"я запускаю алгоритм Хопкрофта-Карпа")]
        public void WhenRunHopcroftKarpAlgorithm()
        {
            StartCaptureConsoleOutput();

            try
            {
                if (_graph == null)
                {
                    Console.WriteLine("Граф не загружен.");
                    return;
                }

                if (!GraphInputService.ValidateStructure(_graph))
                {
                    Console.WriteLine("Граф некорректен.");
                    return;
                }
                var hopcroftKarp = new HopcroftKarp();
                _result = hopcroftKarp.FindMaximumMatching(_graph!);
                _capturedException = null;
            }
            catch (Exception ex)
            {
                _capturedException = ex;
                _result = null;
            }
            finally
            {
                _capturedConsoleOutput = StopCaptureConsoleOutput();
            }
        }

        [Then(@"размер паросочетания в ответе должен быть (\d+)")]
        public void ThenMatchingSizeShouldBe(int expectedSize)
        {
            Assert.NotNull(_result);
            Assert.Equal(expectedSize, _result.MatchingSize);
        }

        [Then(@"результат имеет паросочетание")]
        public void ThenResultContainsMatching()
        {
            Assert.NotNull(_result);
            Assert.NotNull(_result.Matches);
        }

        [Then(@"в паросочетании есть вершины (.*)")]
        public void ThenVerticesAreInMatching(string verticesString)
        {
            var expectedVertices = verticesString
                .Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToHashSet();

            Assert.NotNull(_result);
            Assert.NotNull(_result.Matches);

            var actualVertices = _result.Matches.Keys.ToHashSet();
            Assert.Equal(expectedVertices, actualVertices);
        }

        [Then(@"результат имеет пустое паросочетание")]
        public void ThenResultContainsEmptyMatching()
        {
            Assert.NotNull(_result);
            Assert.NotNull(_result.Matches);
            Assert.Empty(_result.Matches);
        }

        [Then(@"размер паросочетания равняется (\d+)")]
        public void ThenMatchingSizeEquals(int expectedSize)
        {
            Assert.NotNull(_result);
            Assert.Equal(expectedSize, _result.MatchingSize);
        }

        [Then(@"результат имеет ровно одно паросочетание")]
        public void ThenResultContainsExactlyOneMatching()
        {
            Assert.NotNull(_result);
            Assert.NotNull(_result.Matches);
            Assert.Single(_result.Matches);
        }

        [Then(@"ребро паросочетания соединяет вершину (\d+) с вершиной (\d+)")]
        public void ThenEdgeConnectsVertices(int leftVertex, int rightVertex)
        {
            Assert.NotNull(_result);
            Assert.NotNull(_result.Matches);
            Assert.Contains(leftVertex, _result.Matches.Keys);
            Assert.Equal(rightVertex, _result.Matches[leftVertex]);
        }

        [Then(@"время выполнения записано в ответе")]
        public void ThenExecutionTimeIsRecorded()
        {
            Assert.NotNull(_result);
            Assert.True(_result.ExecutionTimeMs >= 0, "Время выполнения должно быть записано (неотрицательное значение)");
        }


        [Then(@"время выполнения должно быть меньше (\d+) мс")]
        public void ThenExecutionTimeShouldBeLessThan(int maxMilliseconds)
        {
            Assert.NotNull(_result);
            Assert.True(_result.ExecutionTimeMs < maxMilliseconds,
                $"Время выполнения {_result.ExecutionTimeMs} мс превышает лимит {maxMilliseconds} мс");
        }

        [Then(@"возникает сообщение ""(.*)""")]
        public void ThenExceptionOccursWithMessage(string expectedMessage)
        {
            Assert.NotNull(_capturedConsoleOutput);
            Assert.Contains(expectedMessage, _capturedConsoleOutput);
        }
    }
}