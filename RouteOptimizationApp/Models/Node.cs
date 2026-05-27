namespace RouteOptimizationApp.Models
{
    public class Node
    {
        public int Id { get; }
        public string Name { get; }
        public double Latitude { get; }
        public double Longitude { get; }
        public double X { get; }
        public double Y { get; }
        public string NodeType { get; }

        public List<Edge> Edges { get; } = new();

        public Node(
            int id,
            string name,
            double latitude,
            double longitude,
            double x,
            double y,
            string nodeType)
        {
            Id = id;
            Name = name;
            Latitude = latitude;
            Longitude = longitude;
            X = x;
            Y = y;
            NodeType = nodeType;
        }
    }
}
