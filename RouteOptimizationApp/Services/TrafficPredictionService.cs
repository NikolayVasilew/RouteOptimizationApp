using RouteOptimizationApp.Models;

namespace RouteOptimizationApp.Services
{
    public static class TrafficPredictionService
    {
        public static double PredictTrafficMultiplier(Edge edge, int hour)
        {
            double baseMultiplier = 1.0;

            if (hour >= 7 && hour <= 9)
                baseMultiplier += 0.6;

            if (hour >= 17 && hour <= 19)
                baseMultiplier += 0.7;

            if (edge.RoadType == "motorway")
                baseMultiplier += 0.1;
            else if (edge.RoadType == "primary")
                baseMultiplier += 0.2;
            else if (edge.RoadType == "secondary")
                baseMultiplier += 0.35;
            else if (edge.RoadType == "mountain")
                baseMultiplier += 0.5;

            if (edge.DistanceMeters > 100000)
                baseMultiplier += 0.15;

            return baseMultiplier;
        }
    }
}
