namespace RouteOptimizationApi.Models
{
    public class EdgeDto
    {
        public int Id { get; set; }
        public int FromNodeId { get; set; }
        public int ToNodeId { get; set; }
        public string StreetName { get; set; } = string.Empty;
        public double DistanceMeters { get; set; }
        public int SpeedLimitKmh { get; set; }
        public double TravelTimeSeconds { get; set; }
        public string RoadType { get; set; } = string.Empty;
        public bool IsOneWay { get; set; }
    }
}
