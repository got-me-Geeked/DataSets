
using MAX_PAIR_IN_GRAPH.Algorithms;
using MAX_PAIR_IN_GRAPH.Models;
using MAX_PAIR_IN_GRAPH.Services;
using System.Diagnostics;

namespace MAX_PAIR_IN_GRAPH
{
    class Program
    {
        static BipartiteGraph? _graph;
        static MatchingResult? _lastResult;
        static void Main(string[] args)
        {
            while (true)
            {
                ShowMenu();
                Console.Write("Выбор: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ManualInput();
                        break;
                    case "2":
                        //GenerateLargeTest();
                        LoadFromJson();
                        break;
                    case "3":
   
                        SolveWithKuhn();
                        break;
                    case "4":

                        SolveWithHopcroftKarp();
                        break;
                    case "5":
                        if (_graph == null)
                        {
                            Console.WriteLine("Граф не загружен.");
                            continue;
                        }

                        if (!GraphInputService.ValidateStructure(_graph))
                        {
                            Console.WriteLine("Граф некорректен.");
                            continue;
                        }
                        ComparisonService.Compare(_graph);
                        RecommendationService.PrintRecommendation(_graph);
                        break;
                    case "6":
                        SaveResult();
                        break;
                    case "7":
                        HelpService.Show();
                        break;
                    case "8":
                        if (_graph == null)
                        {
                            Console.WriteLine("Граф не загружен.");
                            continue;
                        }

                        if (!GraphInputService.ValidateStructure(_graph))
                        {
                            Console.WriteLine("Граф некорректен.");
                            continue;
                        }
                        RecommendationService.PrintRecommendation(_graph);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Неверный выбор.");
                        break;
                }

            }
        }

        static void ShowMenu()
        {
            Console.WriteLine("\n=== Максимальное паросочетание ===");
            Console.WriteLine("1. Ввести двудольный граф вручную");
            Console.WriteLine("2. Загрузить граф из JSON");
            Console.WriteLine("3. Решить алгоритмом Куна");
            Console.WriteLine("4. Решить алгоритмом Хопкрофта-Карпа");
            Console.WriteLine("5. Сравнить алгоритмы");
            Console.WriteLine("6. Сохранить результат в JSON");
            Console.WriteLine("7. Справка");
            Console.WriteLine("8. Получить рекомендацию");
            Console.WriteLine("0. Выход");
            
        }

        static void ManualInput()
        {
            try
            {
                _graph = GraphInputService.ManualInput();
                if (!GraphInputService.ValidateStructure(_graph))
                    Console.WriteLine("ВНИМАНИЕ: граф не является корректным двудольным. \nВходные данные сброшены");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
        }

        static async void LoadFromJson()
        {
            Console.Write("Путь к файлу: ");
            var path = Console.ReadLine();
            _graph = await JsonService.LoadGraphAsync(path!);

            if (_graph == null || !GraphInputService.ValidateStructure(_graph))
            {
                _graph = null;
                Console.WriteLine("Ошибка: граф некорректный или не двудольный.");
            }
        }

        static void SolveWithKuhn()
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

            var algo = new Kuhn();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            _lastResult = algo.FindMaximumMatching(_graph);

            sw.Stop();
            _lastResult.ExecutionTimeMs = sw.ElapsedMilliseconds;

            _lastResult.Print();
        }

        static void SolveWithHopcroftKarp()
        {
            if (_graph == null)
            {
                Console.WriteLine("Сначала загрузите или создайте граф.");
                return;
            }
            var algo = new HopcroftKarp();
            var sw = Stopwatch.StartNew();
            _lastResult = algo.FindMaximumMatching(_graph!);
            sw.Stop();
            _lastResult.ExecutionTimeMs = sw.ElapsedMilliseconds;

            _lastResult.Print();
        }

        static async void SaveResult()
        {
            if (_lastResult == null)
            {
                Console.WriteLine("Нет результата для сохранения.");
                return;
            }

            Console.Write("Путь для сохранения: ");
            var path = Console.ReadLine();
            await JsonService.SaveAsync(path!, _lastResult);
        }

    }
}
