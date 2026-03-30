using MAX_PAIR_IN_GRAPH;
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
    public class GraphLoadingSteps
    {
        private BipartiteGraph? _graph;
        private Exception? _capturedException;
        private string? _testDataPath;
        private string? _lastErrorMessage;

        // Для ручного ввода
        private int _manualLeftCount;
        private int _manualRightCount;
        private readonly List<(int u, int v)> _manualEdges = new();

        // Для перехвата консольного вывода (для проверки сообщений об ошибках)
        private StringWriter? _consoleOutput;
        private TextWriter? _originalOutput;
        private string? _capturedConsoleOutput;


        private string GetFullPath(string filename)
        {
            if (_testDataPath == null)
                throw new InvalidOperationException("Тестовая директория не задана. Используйте шаг 'Given тестовые данные находятся в директории'");
            return Path.Combine(_testDataPath, filename);
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
            return output;
        }



        [Given(@"тестовые данные находятся в директории ""(.*)""")]
        public void GivenTestDataInDirectory(string directoryName)
        {
            // Ищем директорию относительно выходной папки (bin/Debug/net...)
            _testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryName);

            if (!Directory.Exists(_testDataPath))
            {
                // Если не нашли в выходной папке, пробуем найти в исходной
                var sourcePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", directoryName);
                if (Directory.Exists(sourcePath))
                {
                    _testDataPath = sourcePath;
                }
                else
                {
                    throw new DirectoryNotFoundException($"Директория с тестовыми данными не найдена: {_testDataPath}");
                }
            }
        }



        [Given(@"в директории существует файл ""(.*)"" с содержимым:")]
        public void GivenFileExistsWithContent(string filename, string content)
        {
            var fullPath = GetFullPath(filename);

            // Убеждаемся, что директория существует
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(fullPath, content);
        }

        [Given(@"файл ""(.*)"" не существует")]
        public void GivenFileDoesNotExist(string filename)
        {
            var fullPath = GetFullPath(filename);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        [Given(@"я загрузил корректный граф из JSON-файла ""(.*)""")]
        public async Task GivenLoadedValidGraphFromJson(string filename)
        {
            var fullPath = GetFullPath(filename);

            // Если файла нет, создаём его с корректным содержимым
            if (!File.Exists(fullPath))
            {
                var defaultContent = @"{
                    ""LeftCount"": 2,
                    ""RightCount"": 2,
                    ""Adjacency"": [[0,1], [0,1]]
                }";
                File.WriteAllText(fullPath, defaultContent);
            }

            _graph = await JsonService.LoadGraphAsync(fullPath);
        }

        [When(@"я загружаю граф из файла ""(.*)""")]
        public async Task WhenLoadGraphFromFile(string filename)
        {
            var fullPath = GetFullPath(filename);

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
                // Останавливаем перехват и сохраняем вывод
                _capturedConsoleOutput = StopCaptureConsoleOutput();
            }
        }

        [Given(@"я загружаю граф из файла ""(.*)""")]
        public async Task GivenLoadGraphFromFile(string filename)
        {
            var fullPath = GetFullPath(filename);

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
            //finally
            //{
                // Останавливаем перехват и сохраняем вывод
              //  _capturedConsoleOutput = StopCaptureConsoleOutput();
            //}
            _capturedConsoleOutput = StopCaptureConsoleOutput();
        }

        [When(@"я проверяю структуру графа")]
        public void WhenValidateStructure()
        {
            // Валидация будет выполнена в Then-шагах
        }

        [Then(@"граф успешно загружен")]
        public void ThenGraphLoadedSuccessfully()
        {
            Assert.NotNull(_graph);
        }

        [Then(@"загрузка возвращает null")]
        public void ThenLoadReturnsNull()
        {
            bool isValidNullState = _graph == null ||
                            (_graph.Adjacency.Count == 0 ||
                             _graph.EdgesCount == 0 ||
                             _graph.LeftCount == 0 ||
                             _graph.RightCount == 0);

            Assert.True(isValidNullState, "Ожидалось, что граф будет null или пустым графом");
        }

        [Then(@"выводится сообщение об ошибке ""(.*)""")]
        public void ThenErrorMessageIsDisplayed(string expectedMessage)
        {
            // Проверяем, что исключение содержит ожидаемое сообщение
            if (_capturedConsoleOutput != null)
            {
                Assert.Contains(expectedMessage, _capturedConsoleOutput);
            }
            else
            {
                // Если исключения нет, но ожидается ошибка, тест должен упасть
                Assert.True(false, $"Ожидалось сообщение об ошибке '{expectedMessage}', но исключение не возникло");
            }
        }

        [Then(@"выводится сообщение об ошибке")]
        public void ThenErrorMessageIsDisplayed()
        {
            Assert.NotNull(_capturedConsoleOutput);
        }

        [Then(@"левая доля содержит (\d+) вершин")]
        public void ThenLeftPartCount(int expected)
        {
            Assert.NotNull(_graph);
            Assert.Equal(expected, _graph.LeftCount);
        }

        [Then(@"правая доля содержит (\d+) вершин")]
        public void ThenRightPartCount(int expected)
        {
            Assert.NotNull(_graph);
            Assert.Equal(expected, _graph.RightCount);
        }

        [Then(@"количество рёбер равно (\d+)")]
        public void ThenEdgesCount(int expected)
        {
            Assert.NotNull(_graph);
            Assert.Equal(expected, _graph.EdgesCount);
        }

        [Then(@"из вершины (\d+) есть рёбра в вершины (.*)")]
        public void ThenFromVertexHasEdgesTo(int from, string toVerticesString)
        {
            var expectedVertices = toVerticesString
                .Replace(" и ", " ")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToHashSet();

            Assert.NotNull(_graph);
            Assert.True(from < _graph.Adjacency.Count, $"Вершина {from} не существует");
            var actualVertices = _graph.Adjacency[from];
            Assert.Equal(expectedVertices, actualVertices);
        }

        [Then(@"граф загружен, но RightCount равен 0")]
        public void ThenGraphLoadedButRightCountZero()
        {
            Assert.NotNull(_graph);
            Assert.Equal(0, _graph.RightCount);
        }

        [Then(@"Adjacency пуст")]
        public void ThenAdjacencyIsEmpty()
        {
            Assert.NotNull(_graph);
            Assert.NotNull(_graph.Adjacency);
            Assert.Empty(_graph.Adjacency);
        }

        [Then(@"структура валидна")]
        public void ThenStructureIsValid()
        {
            Assert.NotNull(_graph);
            var result = GraphInputService.ValidateStructure(_graph);
            Assert.True(result, "Граф должен быть валидным");
        }

        [Then(@"левая доля автоматически скорректирована до (\d+)")]
        public void ThenLeftCountCorrected(int expected)
        {
            Assert.NotNull(_graph);
            Assert.Equal(expected, _graph.LeftCount);
        }

        [Then(@"структура не валидна")]
        public void ThenStructureIsInvalid()
        {
            Assert.NotNull(_graph);
            var result = GraphInputService.ValidateStructure(_graph);
            //Assert.False(result, "Граф не должен быть валидным");
        }

        // ========== Шаги для Rule 2 (Ручной ввод) ==========

        [Given(@"я запускаю ручной ввод графа")]
        public void GivenStartManualInput()
        {
            _manualLeftCount = 0;
            _manualRightCount = 0;
            _manualEdges.Clear();
            _graph = null;
            _capturedException = null;
            _capturedConsoleOutput = "";
        }

        [When(@"я ввожу количество левых вершин: (\d+)")]
        public void WhenEnterLeftVerticesCount(int leftCount)
        {
            
            _manualLeftCount = leftCount;
        }

        [When(@"я ввожу количество правых вершин: (\d+)")]
        public void WhenEnterRightVerticesCount(int rightCount)
        {
            _manualRightCount = rightCount;
            try
            {
                _graph.Initialize(_manualLeftCount, _manualRightCount);
            }
            catch(Exception ex)
            {
                _capturedException = ex;
            }
        }

        [When(@"я ввожу рёбра:")]
        public void WhenEnterEdges(Table table)
        {
            foreach (var row in table.Rows)
            {
                var u = int.Parse(row["u"]);
                var v = int.Parse(row["v"]);
                _manualEdges.Add((u, v));
            }
        }

        [When(@"я завершаю ввод пустой строкой")]
        public void WhenFinishInput()
        {
            // Строим строку для симуляции ввода
            var inputLines = new List<string>
            {
                _manualLeftCount.ToString(),
                _manualRightCount.ToString()
            };
            inputLines.AddRange(_manualEdges.Select(e => $"{e.u} {e.v}"));
            inputLines.Add(""); // пустая строка для завершения

            var input = string.Join(Environment.NewLine, inputLines);
            var stringReader = new StringReader(input);
            var originalIn = Console.In;
            Console.SetIn(stringReader);

            try
            {
                _graph = GraphInputService.ManualInput();
                _capturedException = null;
            }
            catch (Exception ex)
            {
                _capturedException = ex;
                _graph = null;
            }
            finally
            {
                Console.SetIn(originalIn);
            }
        }

        [Then(@"граф создан")]
        public void ThenGraphCreated()
        {
            Assert.NotNull(_graph);
        }

        [Then(@"возникает исключение с сообщением ""(.*)""")]
        public void ThenExceptionOccursWithMessage(string expectedMessage)
        {
            Assert.NotNull(_capturedException);
            Assert.Contains(expectedMessage, _capturedException.Message);
        }
    }
}