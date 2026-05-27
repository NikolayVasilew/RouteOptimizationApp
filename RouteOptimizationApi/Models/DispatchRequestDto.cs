namespace RouteOptimizationApi.Models
{
    public class DispatchRequestDto
    {
        public int Id { get; set; }

        public int StartNodeId { get; set; }

        public int EndNodeId { get; set; }

        public string AlgorithmName { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public string? ResultPath { get; set; }

        public double? TotalDistanceMeters { get; set; }

        public double? ExecutionTimeMs { get; set; }

        public string CreatedAt { get; set; } = string.Empty;

        public string? CompletedAt { get; set; }
    }
}
