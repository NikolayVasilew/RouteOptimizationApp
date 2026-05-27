namespace RouteOptimizationApi.Models
{
    public class NodeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string NodeType { get; set; } = string.Empty;
    }
}
