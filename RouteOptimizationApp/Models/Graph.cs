namespace RouteOptimizationApp.Models
{
    public class Graph
    {
        public List<Node> Nodes { get; } = new();

        public void AddNode(Node node)
        {
            Nodes.Add(node);
        }

        public void AddDirectedEdge(
            Node from,
            Node to,
            string streetName,
            double distanceMeters,
            int speedLimitKmh,
            double travelTimeSeconds,
            string roadType,
            bool isOneWay)
        {
            from.Edges.Add(new Edge(
                to,
                streetName,
                distanceMeters,
                speedLimitKmh,
                travelTimeSeconds,
                roadType,
                isOneWay));
        }

        public void AddUndirectedEdge(
            Node a,
            Node b,
            string streetName,
            double distanceMeters,
            int speedLimitKmh,
            double travelTimeSeconds,
            string roadType)
        {
            AddDirectedEdge(a, b, streetName, distanceMeters, speedLimitKmh, travelTimeSeconds, roadType, false);
            AddDirectedEdge(b, a, streetName, distanceMeters, speedLimitKmh, travelTimeSeconds, roadType, false);
        }
    }
}
