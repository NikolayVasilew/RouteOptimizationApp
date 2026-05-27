using RouteOptimizationApp.Models;

namespace RouteOptimizationApp.Algorithms
{
    public static class RouteAlgorithms
    {
        public static List<Node> Dijkstra(
            Graph graph,
            Node start,
            Node goal,
            OptimizationMode mode = OptimizationMode.Fastest)
        {
            var distances = graph.Nodes.ToDictionary(n => n, _ => double.MaxValue);
            var previous = graph.Nodes.ToDictionary<Node, Node, Node?>(n => n, _ => null);
            var queue = new List<Node>(graph.Nodes);

            distances[start] = 0;

            while (queue.Count > 0)
            {
                var current = queue.OrderBy(n => distances[n]).First();
                queue.Remove(current);

                if (current == goal)
                    break;

                foreach (var edge in current.Edges)
                {
                    var cost = edge.GetCost(mode);

                    if (cost >= double.MaxValue / 8)
                        continue;

                    var newDistance = distances[current] + cost;

                    if (newDistance < distances[edge.Target])
                    {
                        distances[edge.Target] = newDistance;
                        previous[edge.Target] = current;
                    }
                }
            }

            return ReconstructPath(previous, goal);
        }

        public static List<Node> AStar(
            Graph graph,
            Node start,
            Node goal,
            Func<Node, Node, double> heuristic,
            OptimizationMode mode = OptimizationMode.Fastest)
        {
            var openSet = new List<Node> { start };
            var cameFrom = graph.Nodes.ToDictionary<Node, Node, Node?>(n => n, _ => null);

            var gScore = graph.Nodes.ToDictionary(n => n, _ => double.MaxValue);
            var fScore = graph.Nodes.ToDictionary(n => n, _ => double.MaxValue);

            gScore[start] = 0;
            fScore[start] = heuristic(start, goal);

            while (openSet.Count > 0)
            {
                var current = openSet.OrderBy(n => fScore[n]).First();

                if (current == goal)
                    return ReconstructPath(cameFrom, goal);

                openSet.Remove(current);

                foreach (var edge in current.Edges)
                {
                    var cost = edge.GetCost(mode);

                    if (cost >= double.MaxValue / 8)
                        continue;

                    var tentativeGScore = gScore[current] + cost;

                    if (tentativeGScore < gScore[edge.Target])
                    {
                        cameFrom[edge.Target] = current;
                        gScore[edge.Target] = tentativeGScore;
                        fScore[edge.Target] = tentativeGScore + heuristic(edge.Target, goal);

                        if (!openSet.Contains(edge.Target))
                            openSet.Add(edge.Target);
                    }
                }
            }

            return new List<Node>();
        }

        public static List<Node> GeneticAlgorithm(
            Graph graph,
            Node start,
            Node goal,
            OptimizationMode mode = OptimizationMode.Fastest)
        {
            var random = new Random();
            const int populationSize = 40;
            const int generations = 120;
            const double mutationRate = 0.25;

            var population = new List<List<Node>>();

            for (int i = 0; i < populationSize; i++)
            {
                var path = GenerateRandomValidPath(start, goal, graph.Nodes.Count * 2, random);

                if (path.Count > 0)
                    population.Add(path);
            }

            if (population.Count == 0)
                return Dijkstra(graph, start, goal, mode);

            for (int generation = 0; generation < generations; generation++)
            {
                population = population
                    .OrderBy(path => CalculatePathCost(path, mode))
                    .Take(populationSize / 2)
                    .ToList();

                while (population.Count < populationSize)
                {
                    var child = GenerateRandomValidPath(start, goal, graph.Nodes.Count * 2, random);

                    if (child.Count > 0 && random.NextDouble() < mutationRate)
                        population.Add(child);
                    else if (child.Count > 0)
                        population.Add(child);
                }
            }

            return population.OrderBy(path => CalculatePathCost(path, mode)).First();
        }

        public static double EuclideanHeuristic(Node a, Node b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;

            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static double MLInspiredHeuristic(Node a, Node b)
        {
            var baseDistance = EuclideanHeuristic(a, b);

            const double alpha = 0.85;
            const double beta = 5.0;

            return alpha * baseDistance + beta;
        }

        public static double CalculatePathDistance(List<Node> path)
        {
            double total = 0;

            for (int i = 0; i < path.Count - 1; i++)
            {
                var edge = path[i].Edges.FirstOrDefault(e => e.Target == path[i + 1]);

                if (edge != null)
                    total += edge.Weight;
            }

            return total;
        }

        public static double CalculatePathCost(List<Node> path, OptimizationMode mode)
        {
            double total = 0;

            for (int i = 0; i < path.Count - 1; i++)
            {
                var edge = path[i].Edges.FirstOrDefault(e => e.Target == path[i + 1]);

                if (edge != null)
                    total += edge.GetCost(mode);
            }

            return total;
        }

        private static List<Node> GenerateRandomValidPath(
            Node start,
            Node goal,
            int maxSteps,
            Random random)
        {
            var path = new List<Node> { start };
            var current = start;
            var visited = new HashSet<Node> { start };

            for (int i = 0; i < maxSteps; i++)
            {
                if (current == goal)
                    return path;

                var possibleEdges = current.Edges
                    .Where(e => !visited.Contains(e.Target) || e.Target == goal)
                    .ToList();

                if (possibleEdges.Count == 0)
                    break;

                var nextEdge = possibleEdges[random.Next(possibleEdges.Count)];

                current = nextEdge.Target;
                path.Add(current);
                visited.Add(current);
            }

            return current == goal ? path : new List<Node>();
        }

        private static List<Node> ReconstructPath(Dictionary<Node, Node?> previous, Node goal)
        {
            var path = new List<Node>();
            var current = goal;

            while (current != null)
            {
                path.Insert(0, current);

                if (!previous.ContainsKey(current) || previous[current] == null)
                    break;

                current = previous[current]!;
            }

            return path;
        }
    }
}