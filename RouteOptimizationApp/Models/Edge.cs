namespace RouteOptimizationApp.Models
{
    public class Edge
    {
        public Node Target { get; }
        public string StreetName { get; }
        public double DistanceMeters { get; }
        public int SpeedLimitKmh { get; }
        public double TravelTimeSeconds { get; }
        public string RoadType { get; }
        public bool IsOneWay { get; }

        public double TrafficMultiplier { get; set; } = 1.0;
        public double PredictedTrafficMultiplier { get; set; } = 1.0;
        public bool IsClosed { get; set; } = false;

        public double Weight
        {
            get
            {
                if (IsClosed)
                    return double.MaxValue / 4;

                return TravelTimeSeconds * TrafficMultiplier * PredictedTrafficMultiplier;
            }
        }

        public Edge(
            Node target,
            string streetName,
            double distanceMeters,
            int speedLimitKmh,
            double travelTimeSeconds,
            string roadType,
            bool isOneWay)
        {
            Target = target;
            StreetName = streetName;
            DistanceMeters = distanceMeters;
            SpeedLimitKmh = speedLimitKmh;
            TravelTimeSeconds = travelTimeSeconds;
            RoadType = roadType;
            IsOneWay = isOneWay;
        }

        public double GetCost(OptimizationMode mode)
        {
            if (IsClosed)
                return double.MaxValue / 4;

            return mode switch
            {
                OptimizationMode.Fastest =>
                    TravelTimeSeconds * TrafficMultiplier * PredictedTrafficMultiplier,

                OptimizationMode.Shortest =>
                    DistanceMeters,

                OptimizationMode.MinimumTraffic =>
                    TravelTimeSeconds * TrafficMultiplier * PredictedTrafficMultiplier * TrafficMultiplier,

                OptimizationMode.MinimumFuel =>
                    FuelCalculator.CalculateFuelCost(DistanceMeters, RoadType, TrafficMultiplier),

                _ => Weight
            };
        }
    }
}