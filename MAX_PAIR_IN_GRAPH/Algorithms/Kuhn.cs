using System;
using System.Collections.Generic;
using System.Text;
using MAX_PAIR_IN_GRAPH.Models;

namespace MAX_PAIR_IN_GRAPH.Algorithms
{
    public class Kuhn
    {
        private bool[] _used = null!;
        private int[] _matchRight = null!;

        public MatchingResult FindMaximumMatching(BipartiteGraph graph)
        {
            int left = graph.LeftCount;
            int right = graph.RightCount;

            _matchRight = Enumerable.Repeat(-1, right).ToArray();

            int matchCount = 0;

            for (int v = 0; v < left; v++)
            {
                _used = new bool[left];

                if (TryKuhn(v, graph))
                    matchCount++;
            }

            return BuildResult(matchCount);
        }

        private bool TryKuhn(int v, BipartiteGraph graph)
        {
            if (_used[v])
                return false;

            _used[v] = true;

            foreach (var to in graph.Adjacency[v])
            {
                if (_matchRight[to] == -1 ||
                    TryKuhn(_matchRight[to], graph))
                {
                    _matchRight[to] = v;
                    return true;
                }
            }

            return false;
        }

        private MatchingResult BuildResult(int size)
        {
            var result = new MatchingResult
            {
                MatchingSize = size
            };

            for (int v = 0; v < _matchRight.Length; v++)
            {
                if (_matchRight[v] != -1)
                    result.Matches[_matchRight[v]] = v;
            }

            return result;
        }
    }
}
