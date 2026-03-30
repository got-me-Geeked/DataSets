using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MAX_PAIR_IN_GRAPH
{
    public class BipartiteGraph
    {
        public virtual int LeftCount { get; set; }
        public virtual int RightCount { get; set; }
        public virtual List<HashSet<int>> Adjacency { get; set; } = new();

        [JsonIgnore]
        public int EdgesCount => Adjacency.Sum(x => x.Count);

        public void Initialize(int left, int right)
        {
            if (left <= 0 || right <= 0)
                throw new ArgumentException("Количество вершин должно быть > 0");

            LeftCount = left;
            RightCount = right;

            Adjacency = new List<HashSet<int>>();

            for (int i = 0; i < left; i++)
                Adjacency.Add(new HashSet<int>());
        }

        public void AddEdge(int u, int v)
        {
            if (u < 0 || u >= LeftCount)
                throw new ArgumentOutOfRangeException(nameof(u));

            if (v < 0 || v >= RightCount)
                throw new ArgumentOutOfRangeException(nameof(v));

            Adjacency[u].Add(v);
        }

        
    }
}
