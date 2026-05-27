using RouteOptimizationApp.Models;

namespace RouteOptimizationApp.Algorithms
{
    public class FloydWarshallService
    {
        private double[,]? distances;
        private int[,]? next;
        private List<Node> nodes = new();

        public void Compute(Graph graph, OptimizationMode mode)
        {
            nodes = graph.Nodes;
            int n = nodes.Count;

            distances = new double[n, n];
            next = new int[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    distances[i, j] = i == j ? 0 : double.MaxValue / 4;
                    next[i, j] = -1;
                }
            }

            for (int i = 0; i < n; i++)
            {
                foreach (var edge in nodes[i].Edges)
                {
                    int j = nodes.IndexOf(edge.Target);

                    distances[i, j] = edge.GetCost(mode);
                    next[i, j] = j;
                }
            }

            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (distances[i, k] + distances[k, j] < distances[i, j])
                        {
                            distances[i, j] = distances[i, k] + distances[k, j];
                            next[i, j] = next[i, k];
                        }
                    }
                }
            }
        }

        public List<Node> GetPath(Node start, Node goal)
        {
            if (distances == null || next == null)
                return new List<Node>();

            int startIndex = nodes.IndexOf(start);
            int goalIndex = nodes.IndexOf(goal);

            if (startIndex == -1 || goalIndex == -1)
                return new List<Node>();

            if (next[startIndex, goalIndex] == -1)
                return new List<Node>();

            var path = new List<Node> { start };
            int current = startIndex;

            while (current != goalIndex)
            {
                current = next[current, goalIndex];

                if (current == -1)
                    return new List<Node>();

                path.Add(nodes[current]);
            }

            return path;
        }
    }
}
