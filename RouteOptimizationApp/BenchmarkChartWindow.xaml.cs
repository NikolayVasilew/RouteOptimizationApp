using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RouteOptimizationApp
{
    public partial class BenchmarkChartWindow : Window
    {
        public BenchmarkChartWindow(List<AlgorithmResult> results)
        {
            InitializeComponent();

            DrawChart(results);
        }

        private void DrawChart(List<AlgorithmResult> results)
        {
            if (results.Count == 0)
                return;

            AddBarChart(
                "Време за изчисление (ms)",
                results.Select(r =>
                    (r.AlgorithmName, r.CalculationTimeMs)).ToList());

            AddBarChart(
                "Време по маршрута (сек.)",
                results.Select(r =>
                    (r.AlgorithmName, r.TotalTravelTime)).ToList());

            AddBarChart(
                "Разстояние (km)",
                results.Select(r =>
                    (r.AlgorithmName, r.TotalDistanceKm)).ToList());

            AddBarChart(
                "Разход на гориво (лв.)",
                results.Select(r =>
                    (r.AlgorithmName, r.FuelCost)).ToList());
        }

        private void AddBarChart(
            string title,
            List<(string Name, double Value)> data)
        {
            var container = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 0, 0, 18),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1)
            };

            var stack = new StackPanel();

            stack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 12)
            });

            double max = data.Max(d => d.Value);

            foreach (var item in data)
            {
                var row = new Grid
                {
                    Margin = new Thickness(0, 6, 0, 6)
                };

                row.ColumnDefinitions.Add(
                    new ColumnDefinition
                    {
                        Width = new GridLength(140)
                    });

                row.ColumnDefinitions.Add(
                    new ColumnDefinition
                    {
                        Width = new GridLength(1, GridUnitType.Star)
                    });

                row.ColumnDefinitions.Add(
                    new ColumnDefinition
                    {
                        Width = new GridLength(80)
                    });

                var name = new TextBlock
                {
                    Text = item.Name,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var bar = new Rectangle
                {
                    Height = 22,
                    Fill = Brushes.SteelBlue,
                    Width = max == 0
                        ? 0
                        : Math.Max(8, item.Value / max * 420)
                };

                var value = new TextBlock
                {
                    Text = item.Value.ToString("0.00"),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                Grid.SetColumn(name, 0);
                Grid.SetColumn(bar, 1);
                Grid.SetColumn(value, 2);

                row.Children.Add(name);
                row.Children.Add(bar);
                row.Children.Add(value);

                stack.Children.Add(row);
            }

            container.Child = stack;

            ChartPanel.Children.Add(container);
        }
    }
}
