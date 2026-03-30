using System;
using System.Collections.Generic;
using System.Text;

namespace MAX_PAIR_IN_GRAPH.Services
{
    public static class HelpService
    {
        public static void Show()
        {
            Console.WriteLine(@"
Программа вычисляет максимальное паросочетание в двудольном графе.

Алгоритмы:
1. Куна — простой DFS-подход.
2. Хопкрофта-Карпа — оптимизированный (BFS+DFS).

Граф должен быть двудольным.
Левая доля: вершины 0..N-1
Правая доля: вершины 0..M-1

Формат JSON:
{
  ""LeftCount"": 3,
  ""RightCount"": 3,
  ""Adjacency"": [[0,1],[1],[2]]
}
");
        }
    }
}
