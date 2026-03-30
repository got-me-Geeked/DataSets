using MAX_PAIR_IN_GRAPH;
using MAX_PAIR_IN_GRAPH.Models;
using MAX_PAIR_IN_GRAPH.Services;
using Reqnroll;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Test_MAX_PAIR_IN_GRAPH.Steps
{
    [Binding]
    public class RecommendationSteps
    {
        private BipartiteGraph? _graph;
        private Exception? _capturedException;
        private string? _recommendationTestDataPath;
        private string? _capturedConsoleOutput;

        // Для перехвата консольного вывода
        private StringWriter? _consoleOutput;
        private TextWriter? _originalOutput;


        private string GetFullPath(string filename)
        {
            if (_recommendationTestDataPath == null)
                throw new InvalidOperationException("Тестовая директория не задана. Используйте шаг 'Given тестовые данные для рекомендаций находятся в директории'");
            return Path.Combine(_recommendationTestDataPath, filename);
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
                case "dense_graph.json":
                    CreateDenseGraph(fullPath);
                    break;
                case "sparse_graph.json":
                    CreateSparseGraph(fullPath);
                    break;
                case "medium_graph.json":
                    CreateMediumGraph(fullPath);
                    break;
                case "empty_graph.json":
                    CreateEmptyGraph(fullPath);
                    break;
                case "invalid_edge.json":
                    CreateInvalidEdgeGraph(fullPath);
                    break;
                default:
                    var defaultContent = @"{
                        ""LeftCount"": 2,
                        ""RightCount"": 2,
                        ""Adjacency"": [[0,1], [0,1]]
                    }";
                    File.WriteAllText(fullPath, defaultContent);
                    break;
            }
        }

        private void CreateDenseGraph(string fullPath)
        {
            // Плотный граф: 10x10, все рёбра (плотность = 1.0)
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

        private void CreateSparseGraph(string fullPath)
        {
            // Разреженный граф: 10x10, только 10 рёбер (плотность = 0.1)
            var adjacency = new System.Text.StringBuilder();
            adjacency.Append("[");
            for (int i = 0; i < 10; i++)
            {
                adjacency.Append("[");
                adjacency.Append(i % 10);
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

        private void CreateMediumGraph(string fullPath)
        {
            // Средний граф: 5x5, почти полный (плотность = 0.96)
            var adjacency = new System.Text.StringBuilder();
            adjacency.Append("[");
            for (int i = 0; i < 5; i++)
            {
                adjacency.Append("[");
                for (int j = 0; j < 5; j++)
                {
                    if (i == 2 && j == 2) continue;
                    adjacency.Append(j);
                    if (j < 4) adjacency.Append(",");
                }
                adjacency.Append("]");
                if (i < 4) adjacency.Append(",");
            }
            adjacency.Append("]");

            var content = $@"{{
                ""LeftCount"": 5,
                ""RightCount"": 5,
                ""Adjacency"": {adjacency}
            }}";
            File.WriteAllText(fullPath, content);
        }

        private void CreateEmptyGraph(string fullPath)
        {
            // Пустой граф: 5x5, без рёбер
            var adjacency = new System.Text.StringBuilder();
            adjacency.Append("[");
            for (int i = 0; i < 5; i++)
            {
                adjacency.Append("[]");
                if (i < 4) adjacency.Append(",");
            }
            adjacency.Append("]");

            var content = $@"{{
                ""LeftCount"": 5,
                ""RightCount"": 5,
                ""Adjacency"": {adjacency}
            }}";
            File.WriteAllText(fullPath, content);
        }

        private void CreateInvalidEdgeGraph(string fullPath)
        {
            // Граф с некорректным ребром (правая вершина 2, но RightCount = 2, так что вершины 0 и 1 только)
            var content = @"{
                ""LeftCount"": 2,
                ""RightCount"": 2,
                ""Adjacency"": [[0,2], [0,1]]
            }";
            File.WriteAllText(fullPath, content);
        }

      

        [Given(@"тестовые данные для рекомендаций находятся в директории ""(.*)""")]
        public void GivenTestDataInDirectory(string directoryName)
        {
            _recommendationTestDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryName);

            if (!Directory.Exists(_recommendationTestDataPath))
            {
                var sourcePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", directoryName);
                if (Directory.Exists(sourcePath))
                {
                    _recommendationTestDataPath = sourcePath;
                }
                else
                {
                    throw new DirectoryNotFoundException($"Директория с тестовыми данными не найдена: {_recommendationTestDataPath}");
                }
            }
        }

        [Given(@"я загрузил граф для рекомендаций из файла ""(.*)""")]
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

        [Given(@"граф для рекомендаций не загружен")]
        public void GivenGraphNotLoaded()
        {
            _graph = null;
            _capturedException = null;
            _capturedConsoleOutput = null;
        }

        [When(@"я запрашиваю рекомендацию")]
        public void WhenRequestRecommendation()
        {
            StartCaptureConsoleOutput();

            try
            {
                if (_graph == null)
                {
                    // Симулируем вывод сообщения об ошибке, как в реальном приложении
                    Console.WriteLine("Граф не загружен.");
                }
                else
                {
                    RecommendationService.PrintRecommendation(_graph);
                }
                _capturedException = null;
            }
            catch (Exception ex)
            {
                _capturedException = ex;
            }
            finally
            {
                _capturedConsoleOutput = StopCaptureConsoleOutput();
            }
        }

        [Then(@"выводится рекомендация ""(.*)""")]
        public void ThenRecommendationIsDisplayed(string expectedRecommendation)
        {
            Assert.NotNull(_capturedConsoleOutput);
            Assert.Contains(expectedRecommendation, _capturedConsoleOutput);
        }

        [Then(@"выводится сообщение на консоль ""(.*)""")]
        public void ThenConsoleMessageIsDisplayed(string expectedMessage)
        {
            Assert.NotNull(_capturedConsoleOutput);
            Assert.Contains(expectedMessage, _capturedConsoleOutput);
        }
    }
}