namespace RouteOptimizationApp.Models
{
    public static class FuelCalculator
    {
        public const double FuelPricePerLiter = 2.80;

        public static double CalculateFuelLiters(
            double distanceMeters,
            string roadType,
            double trafficMultiplier)
        {
            double distanceKm = distanceMeters / 1000.0;

            double consumptionPer100Km = roadType switch
            {
                "motorway" => 6.5,
                "primary" => 7.2,
                "secondary" => 8.0,
                "mountain" => 9.5,
                _ => 7.5
            };

            consumptionPer100Km *= trafficMultiplier;

            return distanceKm / 100.0 * consumptionPer100Km;
        }

        public static double CalculateFuelCost(
            double distanceMeters,
            string roadType,
            double trafficMultiplier)
        {
            return CalculateFuelLiters(distanceMeters, roadType, trafficMultiplier)
                   * FuelPricePerLiter;
        }

        public static double CalculateCO2Kg(double fuelLiters)
        {
            return fuelLiters * 2.31;
        }
    }
}
