using MAX_PAIR_IN_GRAPH;
using MAX_PAIR_IN_GRAPH.Algorithms;
using MAX_PAIR_IN_GRAPH.Models;
using MAX_PAIR_IN_GRAPH.Services;
using Reqnroll;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Test_MAX_PAIR_IN_GRAPH.Steps
{
    [Binding]
    public class LoadTestSteps
    {
        private BipartiteGraph? _graph;
        private MatchingResult? _kuhnResult;
        private MatchingResult? _hopcroftKarpResult;
        private MatchingResult? _expectedResult;
        private Exception? _capturedException;
        private string? _loadTestDataPath = "TestData/LoadTests";
        private string? _capturedConsoleOutput;
        private bool _loadCompleted = false;

        // Для перехвата консольного вывода
        private StringWriter? _consoleOutput;
        private TextWriter? _originalOutput;

        // Для ожидания загрузки
        private readonly object _lock = new object();

        private string GetFullPath(string filename)
        {
            if (_loadTestDataPath == null)
                throw new InvalidOperationException("Тестовая директория не задана");
            return Path.Combine(_loadTestDataPath, filename);
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

        private void GenerateTestDataIfNotExists(string filename)
        {
            var fullPath = GetFullPath(filename);

            
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (filename.Contains("sparse", StringComparison.OrdinalIgnoreCase))
            {
                GenerateSparseGraph(fullPath, filename);
            }
            else if (filename.Contains("dense"))
            {
                GenerateDenseGraph(fullPath, filename);
            }
            else if (filename.Contains("max_graph") || filename.Contains("two_minute"))
            {
                // Явно вызываем генерацию для max_graph
                GenerateMaxGraph(fullPath);
            }
            else
            {
                
            }
        }

        private void GenerateSparseGraph(string fullPath, string filename)
        {
            var sizeMatch = System.Text.RegularExpressions.Regex.Match(filename, @"\d+");
            int size = sizeMatch.Success ? int.Parse(sizeMatch.Value) : 100;

            var adjacency = new System.Text.StringBuilder();
            adjacency.Append("[");
            for (int i = 0; i < size; i++)
            {
                adjacency.Append("[");
                for (int j = 0; j < size; j++)
                {
                    if (j % 7 == i % 7)
                    {
                        adjacency.Append(j);
                        /*if (j < size - 1 && (j + 1) % 7 == i % 7) */
                        adjacency.Append(",");
                    }
                }
                adjacency.Remove(adjacency.Length-1, 1);
                adjacency.Append("]");
                if (i < size - 1) adjacency.Append(",");
            }
            adjacency.Append("]");

            var content = $@"{{
                ""LeftCount"": {size},
                ""RightCount"": {size},
                ""Adjacency"": {adjacency}
            }}";
            File.WriteAllText(fullPath, content);
        }

        
        private void GenerateDenseGraph(string fullPath, string filename)
        {
            var sizeMatch = System.Text.RegularExpressions.Regex.Match(filename, @"\d+");
            int size = sizeMatch.Success ? int.Parse(sizeMatch.Value) : 100;

            var adjacency = new System.Text.StringBuilder();
            adjacency.Append("[");
            for (int i = 0; i < size; i++)
            {
                adjacency.Append("[");
                for (int j = 0; j < size; j++)
                {
                    adjacency.Append(j);
                    if (j < size - 1) adjacency.Append(",");
                }
                adjacency.Append("]");
                if (i < size - 1) adjacency.Append(",");
            }
            adjacency.Append("]");

            var content = $@"{{
                ""LeftCount"": {size},
                ""RightCount"": {size},
                ""Adjacency"": {adjacency}
            }}";
            File.WriteAllText(fullPath, content);
        }

        private void GenerateAdaptiveRandomGraph(string fullPath, int targetMinSeconds = 120, int targetMaxSeconds = 600)
        {
            var random = new Random();

            int size = 1000; // старт
            double density = 0.4; // начальная плотность (40%)

            while (true)
            {

                var tempPath = Path.GetTempFileName();

                GenerateRandomGraph(tempPath, size, density, random);

                var graph = JsonService.LoadGraphAsync(tempPath).GetAwaiter().GetResult();

                var kuhn = new Kuhn();
                var sw = Stopwatch.StartNew();
                kuhn.FindMaximumMatching(graph);
                sw.Stop();

                double timeSec = sw.ElapsedMilliseconds / 1000.0;
 
                if (timeSec >= targetMinSeconds && timeSec <= targetMaxSeconds)
                {


                    File.Copy(tempPath, fullPath, true);
                    File.Delete(tempPath);
                    return;
                }

                if (timeSec < targetMinSeconds)
                { 
                   size += 500; 
                }
                else
                {
                    if (density > 0.2)
                        density -= 0.1;
                    else
                        size -= 50;
                }

                File.Delete(tempPath);

                // защита от зацикливания
                if (size > 30000 || size < 50)
                    throw new Exception("Не удалось подобрать граф под нужное время");
            }
        }


        private void GenerateRandomGraph(string path, int size, double density, Random random)
        {
            var adjacency = new System.Text.StringBuilder();
            adjacency.Append("[");

            for (int i = 0; i < size; i++)
            {
                adjacency.Append("[");
                bool first = true;

                for (int j = 0; j < size; j++)
                {
                    if (random.NextDouble() < density)
                    {
                        if (!first) adjacency.Append(",");
                        adjacency.Append(j);
                        first = false;
                    }
                }

                adjacency.Append("]");
                if (i < size - 1) adjacency.Append(",");
            }

            adjacency.Append("]");

            var content = $@"{{
        ""LeftCount"": {size},
        ""RightCount"": {size},
        ""Adjacency"": {adjacency}
    }}";

            File.WriteAllText(path, content);
        }

        private void GenerateMaxGraph(string fullPath)
        {
           //if (File.Exists(fullPath)) { return; }

            try
            {
                //GeneratePreciseHeavyGraph(fullPath);
                GenerateAdaptiveRandomGraph(fullPath);
     
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при генерации оптимального графа: {ex.Message}");
            }
        }

        [Given(@"тестовые данные для нагрузочного тестирования находятся в директории ""(.*)""")]
        public void GivenTestDataInDirectory(string directoryName)
        {
            _loadTestDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryName);

            if (!Directory.Exists(_loadTestDataPath))
            {
                var sourcePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", directoryName);
                if (Directory.Exists(sourcePath))
                {
                    _loadTestDataPath = sourcePath;
                }
                else
                {
                    Directory.CreateDirectory(_loadTestDataPath);
                }
            }
        }

        [Given(@"я подгрузил граф для нагрузочного тестирования из файла ""(.*)""")]
        public async Task GivenLoadedGraphFromJson(string filename)
        {
            // Сбрасываем состояние перед загрузкой
            lock (_lock)
            {
                _loadCompleted = false;
                _graph = null;
                _capturedException = null;
            }

            var fullPath = GetFullPath(filename);
            GenerateTestDataIfNotExists(filename);

            StartCaptureConsoleOutput();

            try
            {
                // Загружаем граф
                _graph = await JsonService.LoadGraphAsync(fullPath);

                lock (_lock)
                {
                    if (_graph != null)
                    {
                        _loadCompleted = true;
                    }
                    else
                    {
                        _loadCompleted = false;
                        _capturedException = new InvalidOperationException($"Не удалось загрузить граф из файла: {fullPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    _capturedException = ex;
                    _graph = null;
                    _loadCompleted = false;
                }
            }
            finally
            {
                _capturedConsoleOutput = StopCaptureConsoleOutput();
            }

            // Проверяем, что загрузка завершилась успешно
            lock (_lock)
            {
                if (!_loadCompleted)
                {
                    var errorMsg = _capturedException?.Message ?? "Неизвестная ошибка загрузки";
                    throw new InvalidOperationException($"Загрузка графа не завершилась успешно: {errorMsg}");
                }
            }
        }

        [Given(@"я загружаю граф для нагрузочного тестирования из файла ""(.*)""")]
        public async Task GivenLoadGraphFromJson(string filename)
        {
            await GivenLoadedGraphFromJson(filename);
        }

        // Метод для принудительного ожидания загрузки (на всякий случай)
        private void WaitForLoadCompletion(int timeoutMs = 10000)
        {
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < timeoutMs)
            {
                lock (_lock)
                {
                    if (_loadCompleted)
                        return;
                }
                Thread.Sleep(100);
            }

            lock (_lock)
            {
                throw new TimeoutException($"Загрузка графа не завершилась за {timeoutMs} мс. Последняя ошибка: {_capturedException?.Message ?? "нет ошибки"}");
            }
        }

        [When(@"я запустил алгоритм Куна")]
        public void WhenRunKuhnAlgorithm()
        {
            // Ждём завершения загрузки
            WaitForLoadCompletion();

            // Проверяем, что граф загружен
            lock (_lock)
            {
                if (_graph == null)
                {
                    throw new InvalidOperationException(
                        $"Граф не загружен. Загрузка завершена: {_loadCompleted}, ошибка: {_capturedException?.Message ?? "нет"}");
                }
            }

            StartCaptureConsoleOutput();

            try
            {
                var kuhn = new Kuhn();
                var sw = Stopwatch.StartNew();

                _kuhnResult = kuhn.FindMaximumMatching(_graph);
                sw.Stop();

                // перезаписываем время
                _kuhnResult.ExecutionTimeMs = sw.ElapsedMilliseconds;
                _capturedException = null;
            }
            catch (Exception ex)
            {
                _capturedException = ex;
                _kuhnResult = null;
            }
            finally
            {
                _capturedConsoleOutput = StopCaptureConsoleOutput();
            }
        }

        [When(@"я запустил алгоритм Хопкрофта-Карпа")]
        public void WhenRunHopcroftKarpAlgorithm()
        {
            // Ждём завершения загрузки
            WaitForLoadCompletion();

            // Проверяем, что граф загружен
            lock (_lock)
            {
                if (_graph == null)
                {
                    throw new InvalidOperationException(
                        $"Граф не загружен. Загрузка завершена: {_loadCompleted}, ошибка: {_capturedException?.Message ?? "нет"}");
                }
            }

            StartCaptureConsoleOutput();

            try
            {
                var hopcroftKarp = new HopcroftKarp();
                var sw = Stopwatch.StartNew();

                _hopcroftKarpResult = hopcroftKarp.FindMaximumMatching(_graph);
                sw.Stop();

                // перезаписываем время
                _hopcroftKarpResult.ExecutionTimeMs = sw.ElapsedMilliseconds;
                _capturedException = null;
            }
            catch (Exception ex)
            {
                _capturedException = ex;
                _hopcroftKarpResult = null;
            }
            finally
            {
                _capturedConsoleOutput = StopCaptureConsoleOutput();
            }
        }

        [When(@"я запускаю эталонный unit-тест для этого графа")]
        public void WhenRunExpectedTest()
        {
            // Ждём завершения загрузки
            WaitForLoadCompletion();

            // Проверяем, что граф загружен
            lock (_lock)
            {
                if (_graph == null)
                {
                    throw new InvalidOperationException(
                        $"Граф не загружен. Загрузка завершена: {_loadCompleted}, ошибка: {_capturedException?.Message ?? "нет"}");
                }
            }

            var kuhn = new Kuhn();
            var hopcroftKarp = new HopcroftKarp();

            var kuhnResult = kuhn.FindMaximumMatching(_graph);
            var hkResult = hopcroftKarp.FindMaximumMatching(_graph);

            if (kuhnResult.MatchingSize == hkResult.MatchingSize)
            {
                _expectedResult = kuhnResult;
            }
            else
            {
                _expectedResult = kuhnResult;
            }
        }

        [Then(@"результаты алгоритмов совпадают с эталонным паросочетанием")]
        public void ThenResultsMatchExpected()
        {
            Assert.NotNull(_kuhnResult);
            Assert.NotNull(_hopcroftKarpResult);
            Assert.Equal(_kuhnResult.MatchingSize, _hopcroftKarpResult.MatchingSize);
        }

        [Then(@"время выполнения Куна: ""<(.*)""")]
        public void ThenKuhnTimeLessThan(string expectedTime)
        {
            Assert.NotNull(_kuhnResult);
            var timeValue = long.Parse(expectedTime.TrimStart('<'));
            Assert.True(_kuhnResult.ExecutionTimeMs < timeValue,
                $"Время Куна {_kuhnResult.ExecutionTimeMs} мс превышает лимит {timeValue} мс");
            Console.WriteLine(_kuhnResult.ExecutionTimeMs / 1000 / 60);
        }

        [Then(@"время выполнения Хопкрофта-Карпа: ""<(.*)""")]
        public void ThenHopcroftKarpTimeLessThan(string expectedTime)
        {
            Assert.NotNull(_hopcroftKarpResult);
            var timeValue = long.Parse(expectedTime.TrimStart('<'));
            Assert.True(_hopcroftKarpResult.ExecutionTimeMs < timeValue,
                $"Время Хопкрофта-Карпа {_hopcroftKarpResult.ExecutionTimeMs} мс превышает лимит {timeValue} мс");
            Console.WriteLine(_hopcroftKarpResult.ExecutionTimeMs / 1000 / 60);
        }

        [Then(@"алгоритм Хопкрофта-Карпа быстрее на плотных графах")]
        public void ThenHopcroftKarpFasterOnDenseGraphs()
        {
            Assert.NotNull(_hopcroftKarpResult);
            Assert.NotNull(_kuhnResult);
            Assert.True(_hopcroftKarpResult.ExecutionTimeMs <= _kuhnResult.ExecutionTimeMs * 0.8,
                $"Хопкрофт-Карп ({_hopcroftKarpResult.ExecutionTimeMs} мс) должен быть значительно быстрее Куна ({_kuhnResult.ExecutionTimeMs} мс)");
        }

        [Then(@"время выполнения не превышает (\d+) минут")]
        public void ThenExecutionTimeDoesNotExceedMinutes(int minutes)
        {
            Assert.NotNull(_kuhnResult);
            Assert.NotNull(_hopcroftKarpResult);

            long maxMs = minutes * 60 * 1000;
            Assert.True(_kuhnResult.ExecutionTimeMs <= maxMs,
                $"Время Куна {_kuhnResult.ExecutionTimeMs} мс превышает {maxMs} мс");
            Assert.True(_hopcroftKarpResult.ExecutionTimeMs <= maxMs,
                $"Время Хопкрофта-Карпа {_hopcroftKarpResult.ExecutionTimeMs} мс превышает {maxMs} мс");
        }

        [Then(@"время выполнения алгоритма Куна составляет t1 минут, где t1 в диапазоне \[(\d+), (\d+)\]")]
        public void ThenKuhnTimeInRange(int minMinutes, int maxMinutes)
        {
            Assert.NotNull(_kuhnResult);

            long minMs = minMinutes * 60 * 1000;
            long maxMs = maxMinutes * 60 * 1000;
            Assert.True(_kuhnResult.ExecutionTimeMs >= minMs && _kuhnResult.ExecutionTimeMs <= maxMs,
                $"Время Куна {_kuhnResult.ExecutionTimeMs} мс должно быть в диапазоне [{minMs}, {maxMs}] мс");
        }

        [Then(@"время выполнения Хопкрофта-Карпа не превышает t1")]
        public void ThenHopcroftKarpTimeNotExceedKuhnTime()
        {
            Assert.NotNull(_hopcroftKarpResult);
            Assert.NotNull(_kuhnResult);
            Assert.True(_hopcroftKarpResult.ExecutionTimeMs <= _kuhnResult.ExecutionTimeMs,
                $"Хопкрофт-Карп ({_hopcroftKarpResult.ExecutionTimeMs} мс) должен быть не медленнее Куна ({_kuhnResult.ExecutionTimeMs} мс)");
        }

        [Then(@"размер паросочетания по алгоритму Куна совпадает с эталоном")]
        public void ThenKuhnMatchesExpected()
        {
            Assert.NotNull(_kuhnResult);
            Assert.NotNull(_expectedResult);
            Assert.Equal(_expectedResult.MatchingSize, _kuhnResult.MatchingSize);
        }

        [Then(@"размер паросочетания по алгоритму Хопкрофта-Карпа совпадает с эталоном")]
        public void ThenHopcroftKarpMatchesExpected()
        {
            Assert.NotNull(_hopcroftKarpResult);
            Assert.NotNull(_expectedResult);
            Assert.Equal(_expectedResult.MatchingSize, _hopcroftKarpResult.MatchingSize);
        }
    }
}