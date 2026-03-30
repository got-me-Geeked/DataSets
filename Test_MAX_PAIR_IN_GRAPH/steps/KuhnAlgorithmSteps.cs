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
    public class KuhnAlgorithmSteps
    {
        private BipartiteGraph? _graph;
        private MatchingResult? _result;
        private Exception? _capturedException;
        private string? _kuhnTestDataPath;
        private string? _capturedConsoleOutput;

        // Для перехвата консольного вывода
        private StringWriter? _consoleOutput;
        private TextWriter? _originalOutput;

     

        private string GetFullPath(string filename)
        {
            if (_kuhnTestDataPath == null)
                throw new InvalidOperationException("Тестовая директория не задана. Используйте шаг 'Given тестовые данные для алгоритма Куна находятся в директории'");
            return Path.Combine(_kuhnTestDataPath, filename);
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
                case "sparse_graph2.json":
                    CreateSparseGraph(fullPath);
                    break;
                case "empty_graph.json":
                    CreateEmptyGraph(fullPath);
                    break;
                case "single_edge.json":
                    CreateSingleEdgeGraph(fullPath);
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

        private void CreateSparseGraph(string fullPath)
        {
            // Разреженный граф: 5x5, 5 рёбер
            var content = @"{
                ""LeftCount"": 5,
                ""RightCount"": 5,
                ""Adjacency"": [[0], [1], [2], [3], [4]]
            }";
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


        [Given(@"тестовые данные для алгоритма Куна находятся в директории ""(.*)""")]
        public void GivenTestDataInDirectory(string directoryName)
        {
            _kuhnTestDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryName);

            if (!Directory.Exists(_kuhnTestDataPath))
            {
                var sourcePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", directoryName);
                if (Directory.Exists(sourcePath))
                {
                    _kuhnTestDataPath = sourcePath;
                }
                else
                {
                    throw new DirectoryNotFoundException($"Директория с тестовыми данными не найдена: {_kuhnTestDataPath}");
                }
            }
        }

        [Given(@"я загрузил граф для алгоритма Куна из файла ""(.*)""")]
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

        [Given(@"граф для алгоритма Куна не загружен")]
        public void GivenGraphNotLoaded()
        {
            _graph = null;
            _result = null;
            _capturedException = null;
            _capturedConsoleOutput = null;
        }

        [When(@"я запускаю алгоритм Куна")]
        public void WhenRunKuhnAlgorithm()
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
                var kuhn = new Kuhn();
                _result = kuhn.FindMaximumMatching(_graph!);
                
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

        [Then(@"размер паросочетания должен быть (\d+)")]
        public void ThenMatchingSizeShouldBe(int expectedSize)
        {
            Assert.NotNull(_result);
            Assert.Equal(expectedSize, _result.MatchingSize);
        }

        [Then(@"результат содержит паросочетание")]
        public void ThenResultContainsMatching()
        {
            Assert.NotNull(_result);
            Assert.NotNull(_result.Matches);
        }

        [Then(@"в паросочетании участвуют вершины (.*)")]
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

        [Then(@"результат содержит пустое паросочетание")]
        public void ThenResultContainsEmptyMatching()
        {
            Assert.NotNull(_result);
            Assert.NotNull(_result.Matches);
            Assert.Empty(_result.Matches);
        }

        [Then(@"размер паросочетания равен (\d+)")]
        public void ThenMatchingSizeEquals(int expectedSize)
        {
            Assert.NotNull(_result);
            Assert.Equal(expectedSize, _result.MatchingSize);
        }

        [Then(@"время выполнения записано")]
        public void ThenExecutionTimeIsRecorded()
        {
            Assert.NotNull(_result);
            Assert.True(_result.ExecutionTimeMs >= 0, "Время выполнения должно быть записано (неотрицательное значение)");
        }

        [Then(@"выводится сообщение ""(.*)""")]
        public void ThenExceptionOccursWithMessage(string expectedMessage)
        {
            Assert.NotNull(_capturedConsoleOutput);
            Assert.Contains(expectedMessage, _capturedConsoleOutput);
        }
    }
}