using System;
using System.Collections.Generic;
using System.Text;
using MAX_PAIR_IN_GRAPH.Models;

namespace MAX_PAIR_IN_GRAPH.Services
{
    public static class RecommendationService
    {
        public static void PrintRecommendation(BipartiteGraph graph)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            else if (graph.LeftCount <= 0 || graph.LeftCount <= 0)
                throw new DivideByZeroException();

            int edges = graph.Adjacency?.Sum(a => a.Count) ?? 0;
            double density = (double)edges / (graph.LeftCount * graph.RightCount);

            Console.WriteLine("\nРекомендация:");

            if (density > 0.4)
                Console.WriteLine("Граф плотный — рекомендуется алгоритм Хопкрофта-Карпа.");
            else
                Console.WriteLine("Граф разреженный — алгоритм Куна допустим.");
        }
    }
}
