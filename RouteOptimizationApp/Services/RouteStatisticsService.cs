using RouteOptimizationApp.Models;
using System.Linq;
using System.Collections.Generic;

namespace RouteOptimizationApp.Services
{
    public static class RouteStatisticsService
    {
        public static double CalculateTotalDistanceMeters(List<Node> path)
        {
            double total = 0;

            for (int i = 0; i < path.Count - 1; i++)
            {
                var edge = path[i].Edges.FirstOrDefault(e => e.Target == path[i + 1]);

                if (edge != null)
                    total += edge.DistanceMeters;
            }

            return total;
        }

        public static int CountNodes(List<Node> path)
        {
            return path.Count;
        }

        public static double CalculateAverageSpeedKmh(List<Node> path)
        {
            double distanceKm = CalculateTotalDistanceMeters(path) / 1000.0;

            double totalSeconds = 0;

            for (int i = 0; i < path.Count - 1; i++)
            {
                var edge = path[i].Edges.FirstOrDefault(e => e.Target == path[i + 1]);

                if (edge != null)
                    totalSeconds += edge.Weight;
            }

            if (totalSeconds == 0)
                return 0;

            return distanceKm / (totalSeconds / 3600.0);
        }
    }
}