using System;
using System.Collections.Generic;
using System.Text;
using MAX_PAIR_IN_GRAPH.Models;

namespace MAX_PAIR_IN_GRAPH.Algorithms
{
    public class HopcroftKarp
    {
        private int[] pairU = null!;
        private int[] pairV = null!;
        private int[] dist = null!;

        private const int NIL = -1;

        public MatchingResult FindMaximumMatching(BipartiteGraph graph)
        {
            int left = graph.LeftCount;
            int right = graph.RightCount;

            pairU = Enumerable.Repeat(NIL, left).ToArray();
            pairV = Enumerable.Repeat(NIL, right).ToArray();
            dist = new int[left];

            int result = 0;

            while (Bfs(graph))
            {
                for (int u = 0; u < left; u++)
                {
                    if (pairU[u] == NIL && Dfs(u, graph))
                        result++;
                }
            }

            return BuildResult(result);
        }

        private bool Bfs(BipartiteGraph graph)
        {
            Queue<int> queue = new();

            for (int u = 0; u < graph.LeftCount; u++)
            {
                if (pairU[u] == NIL)
                {
                    dist[u] = 0;
                    queue.Enqueue(u);
                }
                else
                {
                    dist[u] = int.MaxValue;
                }
            }

            bool foundAugmentingPath = false;

            while (queue.Count > 0)
            {
                int u = queue.Dequeue();

                foreach (var v in graph.Adjacency[u])
                {
                    int matchedU = pairV[v];

                    if (matchedU != NIL && dist[matchedU] == int.MaxValue)
                    {
                        dist[matchedU] = dist[u] + 1;
                        queue.Enqueue(matchedU);
                    }

                    if (matchedU == NIL)
                        foundAugmentingPath = true;
                }
            }

            return foundAugmentingPath;
        }

        private bool Dfs(int u, BipartiteGraph graph)
        {
            foreach (var v in graph.Adjacency[u])
            {
                int matchedU = pairV[v];

                if (matchedU == NIL ||
                    (dist[matchedU] == dist[u] + 1 && Dfs(matchedU, graph)))
                {
                    pairU[u] = v;
                    pairV[v] = u;
                    return true;
                }
            }

            dist[u] = int.MaxValue;
            return false;
        }

        private MatchingResult BuildResult(int size)
        {
            var result = new MatchingResult
            {
                MatchingSize = size
            };

            for (int u = 0; u < pairU.Length; u++)
            {
                if (pairU[u] != NIL)
                    result.Matches[u] = pairU[u];
            }

            return result;
        }
    }
}
