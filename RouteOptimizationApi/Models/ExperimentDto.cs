namespace RouteOptimizationApi.Models
{
    public class ExperimentDto
    {
        public int Id { get; set; }
        public string AlgorithmName { get; set; } = string.Empty;
        public int StartNodeId { get; set; }
        public int EndNodeId { get; set; }
        public string Path { get; set; } = string.Empty;
        public double TotalDistanceMeters { get; set; }
        public double ExecutionTimeMs { get; set; }
        public int VisitedNodes { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }
}
