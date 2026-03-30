using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using MAX_PAIR_IN_GRAPH.Models;
using MAX_PAIR_IN_GRAPH.Algorithms;

namespace MAX_PAIR_IN_GRAPH.Services
{
    public static class ComparisonService
    {
        public static void Compare(BipartiteGraph graph)
        {
            

            var kuhn = new Kuhn();
            var hk = new HopcroftKarp();

            var sw = Stopwatch.StartNew();
            var result1 = kuhn.FindMaximumMatching(graph);
            sw.Stop();
            result1.ExecutionTimeMs = sw.ElapsedMilliseconds;

            sw.Restart();
            var result2 = hk.FindMaximumMatching(graph);
            sw.Stop();
            result2.ExecutionTimeMs = sw.ElapsedMilliseconds;

            Console.WriteLine($"Кун: {result1.MatchingSize}, {result1.ExecutionTimeMs} ms");
            Console.WriteLine($"Хопкрофт-Карп: {result2.MatchingSize}, {result2.ExecutionTimeMs} ms");
        }
    }
}
