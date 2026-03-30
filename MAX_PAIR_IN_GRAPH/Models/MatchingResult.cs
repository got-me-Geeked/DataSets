using System;
using System.Collections.Generic;
using System.Text;

namespace MAX_PAIR_IN_GRAPH.Models
{
    

    public class MatchingResult
    {
        public int MatchingSize { get; set; }
        public Dictionary<int, int> Matches { get; set; } = new();
        public long ExecutionTimeMs { get; set; }

        public static MatchingResult FromRightMatches(int[] matchRight, int size)
        {
            var result = new MatchingResult();
            result.MatchingSize = size;

            for (int v = 0; v < matchRight.Length; v++)
            {
                if (matchRight[v] != -1)
                    result.Matches[matchRight[v]] = v;
            }

            return result;
        }

        public static MatchingResult FromPairs(int[] pairU, int size)
        {
            var result = new MatchingResult();
            result.MatchingSize = size;

            for (int u = 0; u < pairU.Length; u++)
            {
                if (pairU[u] != -1)
                    result.Matches[u] = pairU[u];
            }

            return result;
        }

        public void Print()
        {
            Console.WriteLine("\n=== Результат ===");
            Console.WriteLine($"Размер паросочетания: {MatchingSize}");
            Console.WriteLine($"Время выполнения: {ExecutionTimeMs} ms");

            foreach (var pair in Matches.OrderBy(p => p.Key))
                Console.WriteLine($"{pair.Key} -> {pair.Value}");

            Console.WriteLine("=================");
        }

        public BipartiteGraph ToMatchingGraph(int left, int right)
        {
            var g = new BipartiteGraph();
            g.Initialize(left, right);

            foreach (var pair in Matches)
                g.AddEdge(pair.Key, pair.Value);

            return g;
        }
    }
}
