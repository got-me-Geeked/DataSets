using System;
using System.Collections.Generic;
using System.Text;
using MAX_PAIR_IN_GRAPH.Models;

namespace MAX_PAIR_IN_GRAPH.Services
{
    public static class GraphInputService
    {
        public static BipartiteGraph ManualInput()
        {
            Console.Write("Введите количество вершин левой доли: ");
            int left = int.Parse(Console.ReadLine()!);

            Console.Write("Введите количество вершин правой доли: ");
            int right = int.Parse(Console.ReadLine()!);

            var graph = new BipartiteGraph();
            graph.Initialize(left, right);

            Console.WriteLine("Введите рёбра в формате: u v (через пробел). Пустая строка — завершить.");

            while (true)
            {
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    break;

                var parts = line.Split(' ');
                try
                {
                    int u = int.Parse(parts[0]);
                    int v = int.Parse(parts[1]);

                    if (u < 0 || u >= left || v < 0 || v >= right)
                    {
                        throw new Exception();
                    }

                    graph.AddEdge(u, v);
                }
                catch (Exception ex) { throw new Exception("Ошибка валидации: некоррестные индексы рёбер!"); }

                
            }

            return graph;
        }

        public static bool ValidateStructure(BipartiteGraph graph)
        {
            if (graph == null)
            {
                Console.WriteLine("Граф равен null");
                return false;
            }

            if (graph.Adjacency == null || graph.Adjacency.Count == 0)
            {
                Console.WriteLine("Adjacency равен null");
                return false;
            }

            // Автоматически корректируем LeftCount
            graph.LeftCount = graph.Adjacency.Count;

            for (int u = 0; u < graph.Adjacency.Count; u++)
            {
                if (graph.Adjacency[u] == null)
                {
                    Console.WriteLine($"Строка {u} равна null");
                    return false;
                }
                foreach (var v in graph.Adjacency[u])
                {
                    if (v < 0 || v >= graph.RightCount)
                    {
                        Console.WriteLine($"Некорректное ребро: {u} -> {v}");
                        return false;
                    }
                }
            }

            return true;
        }
        

        public static bool ValidateBipartiteViaColoring(BipartiteGraph graph)
        {
            return ValidateStructure(graph);
        }
    }
}
