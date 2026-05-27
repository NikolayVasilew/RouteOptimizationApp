using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;

using RouteOptimizationApp.Algorithms;
using RouteOptimizationApp.Data;
using RouteOptimizationApp.Models;
using RouteOptimizationApp.Services;

using ShapePath = System.Windows.Shapes.Path;

using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace RouteOptimizationApp
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseService databaseService = new();
        private readonly FloydWarshallService floydWarshallService = new();

        private Graph graph = new();

        private Node? startNode;
        private Node? endNode;

        private List<Node> currentPath = new();
        private List<AlgorithmResult> lastBenchmarkResults = new();

        private string currentTrafficLevel = "Нисък трафик";

        private readonly DispatcherTimer rerouteTimer = new();
        private readonly DispatcherTimer animationTimer = new();

        private int animationIndex = 0;
        private List<Node> animatedPath = new();

        public MainWindow()
        {
            InitializeComponent();

            databaseService.InitializeDatabase();
            databaseService.SeedDatabase();

            graph = databaseService.LoadGraph();

            InitializeMap();
            ApplyTrafficSimulation();
            DrawMap();

            InitializeReroutingTimer();
            InitializeAnimationTimer();
        }

        private void InitializeMap()
        {
            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            Map.MapProvider = GMapProviders.GoogleMap;
            Map.Position = new PointLatLng(42.7339, 25.4858);

            Map.MinZoom = 6;
            Map.MaxZoom = 18;
            Map.Zoom = 7;

            Map.ShowCenter = false;
            Map.CanDragMap = true;
        }

        private void InitializeReroutingTimer()
        {
            rerouteTimer.Interval = TimeSpan.FromSeconds(5);

            rerouteTimer.Tick += (s, e) =>
            {
                if (AutoRerouteCheckBox?.IsChecked == true &&
                    startNode != null &&
                    endNode != null)
                {
                    ApplyTrafficSimulation();
                    RecalculateCurrentRoute();
                }
            };

            rerouteTimer.Start();
        }

        private void InitializeAnimationTimer()
        {
            animationTimer.Interval = TimeSpan.FromMilliseconds(500);

            animationTimer.Tick += (s, e) =>
            {
                if (animationIndex > animatedPath.Count)
                {
                    animationTimer.Stop();
                    return;
                }

                currentPath = animatedPath.Take(animationIndex).ToList();
                animationIndex++;

                DrawMap();
            };
        }

        private OptimizationMode GetSelectedOptimizationMode()
        {
            var selected = (OptimizationModeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            return selected switch
            {
                "Най-бърз маршрут" => OptimizationMode.Fastest,
                "Най-кратък маршрут" => OptimizationMode.Shortest,
                "Минимален трафик" => OptimizationMode.MinimumTraffic,
                "Минимален разход" => OptimizationMode.MinimumFuel,
                _ => OptimizationMode.Fastest
            };
        }

        private void ApplyMlTrafficPrediction()
        {
            if (MlPredictionCheckBox == null || MlPredictionCheckBox.IsChecked != true)
                return;

            int currentHour = DateTime.Now.Hour;

            foreach (var node in graph.Nodes)
            {
                foreach (var edge in node.Edges)
                {
                    edge.PredictedTrafficMultiplier =
                        TrafficPredictionService.PredictTrafficMultiplier(edge, currentHour);
                }
            }
        }

        private void ApplyTrafficSimulation()
        {
            double multiplier = currentTrafficLevel switch
            {
                "Нисък трафик" => 1.0,
                "Среден трафик" => 1.5,
                "Висок трафик" => 2.5,
                "Инцидент / затворен път" => 6.0,
                _ => 1.0
            };

            foreach (var node in graph.Nodes)
            {
                foreach (var edge in node.Edges)
                {
                    edge.TrafficMultiplier = multiplier;
                    edge.PredictedTrafficMultiplier = 1.0;
                    edge.IsClosed = false;
                }
            }

            ApplyMlTrafficPrediction();
        }

        private void DrawMap()
        {
            Map.Markers.Clear();

            DrawRoads();
            DrawCurrentPath();
            DrawCities();
        }

        private void DrawRoads()
        {
            var drawn = new HashSet<string>();

            foreach (var node in graph.Nodes)
            {
                foreach (var edge in node.Edges)
                {
                    var key = $"{Math.Min(node.Id, edge.Target.Id)}-{Math.Max(node.Id, edge.Target.Id)}";

                    if (drawn.Contains(key))
                        continue;

                    drawn.Add(key);

                    Brush roadColor;

                    double load = edge.TrafficMultiplier * edge.PredictedTrafficMultiplier;

                    if (edge.IsClosed)
                        roadColor = Brushes.Black;
                    else if (load <= 1.2)
                        roadColor = Brushes.Green;
                    else if (load <= 2.0)
                        roadColor = Brushes.Goldenrod;
                    else if (load <= 3.0)
                        roadColor = Brushes.OrangeRed;
                    else
                        roadColor = Brushes.DarkRed;

                    var points = new List<PointLatLng>
                    {
                        new PointLatLng(node.Latitude, node.Longitude),
                        new PointLatLng(edge.Target.Latitude, edge.Target.Longitude)
                    };

                    var route = new GMapRoute(points)
                    {
                        Shape = new ShapePath
                        {
                            Stroke = roadColor,
                            StrokeThickness = 2,
                            Opacity = 0.55
                        }
                    };

                    Map.Markers.Add(route);
                }
            }
        }

        private void DrawCities()
        {
            foreach (var node in graph.Nodes)
            {
                Brush color = Brushes.SteelBlue;

                if (node == startNode)
                    color = Brushes.Green;
                else if (node == endNode)
                    color = Brushes.Red;

                var marker = new GMapMarker(new PointLatLng(node.Latitude, node.Longitude));

                var stack = new StackPanel
                {
                    Orientation = Orientation.Vertical
                };

                var ellipse = new Ellipse
                {
                    Width = 18,
                    Height = 18,
                    Fill = color,
                    Stroke = Brushes.White,
                    StrokeThickness = 2,
                    Tag = node
                };

                ellipse.MouseLeftButtonDown += City_Click;

                var label = new TextBlock
                {
                    Text = node.Name,
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black,
                    Background = Brushes.White,
                    Padding = new Thickness(2),
                    IsHitTestVisible = false
                };

                stack.Children.Add(ellipse);
                stack.Children.Add(label);

                marker.Shape = stack;
                marker.Offset = new Point(-9, -9);

                Map.Markers.Add(marker);
            }
        }

        private void DrawCurrentPath()
        {
            if (currentPath.Count < 2)
                return;

            var points = currentPath
                .Select(n => new PointLatLng(n.Latitude, n.Longitude))
                .ToList();

            var route = new GMapRoute(points)
            {
                Shape = new ShapePath
                {
                    Stroke = Brushes.DeepSkyBlue,
                    StrokeThickness = 6,
                    Opacity = 0.95
                }
            };

            Map.Markers.Add(route);
        }

        private void City_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is not Ellipse ellipse || ellipse.Tag is not Node clickedNode)
                return;

            if (startNode == null)
            {
                startNode = clickedNode;
            }
            else if (endNode == null && clickedNode != startNode)
            {
                endNode = clickedNode;
            }
            else
            {
                startNode = clickedNode;
                endNode = null;
                currentPath.Clear();
            }

            DrawMap();
        }

        private void TrafficComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            if (TrafficComboBox?.SelectedItem is not ComboBoxItem item)
                return;

            currentTrafficLevel = item.Content.ToString() ?? "Нисък трафик";

            if (graph.Nodes.Count > 0)
            {
                ApplyTrafficSimulation();
                currentPath.Clear();
                DrawMap();
            }

            ResultTextBlock.Text =
                $"Активирано ниво на трафик:\n{currentTrafficLevel}\n\n" +
                $"Теглата на пътните връзки са актуализирани динамично.";
        }

        private void FindRoute_Click(object sender, RoutedEventArgs e)
        {
            if (startNode == null || endNode == null)
            {
                ResultTextBlock.Text = "Моля, изберете начален и краен град.";
                return;
            }

            ApplyTrafficSimulation();

            var optimizationMode = GetSelectedOptimizationMode();

            var algorithm = ((ComboBoxItem)AlgorithmComboBox.SelectedItem).Content.ToString();

            var stopwatch = Stopwatch.StartNew();

            var calculatedPath = algorithm switch
            {
                "Dijkstra" =>
                    RouteAlgorithms.Dijkstra(graph, startNode, endNode, optimizationMode),

                "A*" =>
                    RouteAlgorithms.AStar(
                        graph,
                        startNode,
                        endNode,
                        RouteAlgorithms.EuclideanHeuristic,
                        optimizationMode),

                "A* ML Heuristic" =>
                    RouteAlgorithms.AStar(
                        graph,
                        startNode,
                        endNode,
                        RouteAlgorithms.MLInspiredHeuristic,
                        optimizationMode),

                "Genetic Algorithm" =>
                    RouteAlgorithms.GeneticAlgorithm(
                        graph,
                        startNode,
                        endNode,
                        optimizationMode),

                "Floyd-Warshall" =>
                    GetFloydWarshallPath(optimizationMode),

                _ => new List<Node>()
            };

            stopwatch.Stop();

            currentPath = calculatedPath;
            DrawMap();

            var totalTravelTime = RouteAlgorithms.CalculatePathDistance(calculatedPath);

            var totalDistanceMeters =
                RouteStatisticsService.CalculateTotalDistanceMeters(calculatedPath);

            var averageSpeed =
                RouteStatisticsService.CalculateAverageSpeedKmh(calculatedPath);

            var nodeCount =
                RouteStatisticsService.CountNodes(calculatedPath);

            var totalFuelLiters =
                CalculateFuelLitersForPath(calculatedPath);

            var totalFuelCost =
                totalFuelLiters * FuelCalculator.FuelPricePerLiter;

            var totalCo2Kg =
                FuelCalculator.CalculateCO2Kg(totalFuelLiters);

            var routeText = calculatedPath.Count > 0
                ? string.Join(" → ", calculatedPath.Select(n => n.Name))
                : "Няма намерен маршрут";

            ResultTextBlock.Text =
                $"ИЗБРАН АЛГОРИТЪМ:\n{algorithm}\n\n" +
                $"КРИТЕРИЙ ЗА ОПТИМИЗАЦИЯ:\n{((ComboBoxItem)OptimizationModeComboBox.SelectedItem).Content}\n\n" +
                $"НИВО НА ТРАФИК:\n{currentTrafficLevel}\n\n" +
                $"НАЧАЛНА ТОЧКА:\n{startNode.Name}\n\n" +
                $"КРАЙНА ТОЧКА:\n{endNode.Name}\n\n" +
                $"НАМЕРЕН МАРШРУТ:\n{routeText}\n\n" +
                $"ОБЩО ВРЕМЕ ПО МАРШРУТА:\n{totalTravelTime:0.00} секунди\n\n" +
                $"ОБЩО РАЗСТОЯНИЕ:\n{totalDistanceMeters / 1000.0:0.00} km\n\n" +
                $"СРЕДНА СКОРОСТ:\n{averageSpeed:0.00} km/h\n\n" +
                $"БРОЙ ВЪЗЛИ:\n{nodeCount}\n\n" +
                $"РАЗХОД НА ГОРИВО:\n{totalFuelLiters:0.00} литра\n\n" +
                $"ЦЕНА НА ГОРИВО:\n{totalFuelCost:0.00} лв.\n\n" +
                $"CO₂ ЕМИСИИ:\n{totalCo2Kg:0.00} kg\n\n" +
                $"ВРЕМЕ ЗА ИЗЧИСЛЕНИЕ:\n{stopwatch.Elapsed.TotalMilliseconds:0.0000} ms";

            if (AnimationCheckBox?.IsChecked == true && calculatedPath.Count > 1)
            {
                animatedPath = new List<Node>(calculatedPath);
                currentPath.Clear();
                animationIndex = 1;
                animationTimer.Start();
            }
        }

        private void CompareAlgorithms_Click(object sender, RoutedEventArgs e)
        {
            if (startNode == null || endNode == null)
            {
                ResultTextBlock.Text = "Моля, изберете начален и краен град.";
                return;
            }

            ApplyTrafficSimulation();

            var optimizationMode = GetSelectedOptimizationMode();

            var results = new List<AlgorithmResult>();

            results.Add(RunBenchmark(
                "Dijkstra",
                () => RouteAlgorithms.Dijkstra(graph, startNode, endNode, optimizationMode)));

            results.Add(RunBenchmark(
                "A*",
                () => RouteAlgorithms.AStar(
                    graph,
                    startNode,
                    endNode,
                    RouteAlgorithms.EuclideanHeuristic,
                    optimizationMode)));

            results.Add(RunBenchmark(
                "A* ML Heuristic",
                () => RouteAlgorithms.AStar(
                    graph,
                    startNode,
                    endNode,
                    RouteAlgorithms.MLInspiredHeuristic,
                    optimizationMode)));

            results.Add(RunBenchmark(
                "Genetic Algorithm",
                () => RouteAlgorithms.GeneticAlgorithm(
                    graph,
                    startNode,
                    endNode,
                    optimizationMode)));

            results.Add(RunBenchmark(
                "Floyd-Warshall",
                () => GetFloydWarshallPath(optimizationMode)));

            lastBenchmarkResults = results;

            var bestByCalculationTime = results
                .OrderBy(r => r.CalculationTimeMs)
                .First();

            var bestByRouteTime = results
                .OrderBy(r => r.TotalTravelTime)
                .First();

            ResultTextBlock.Text =
                $"BENCHMARK COMPARISON\n\n" +
                $"Критерий: {((ComboBoxItem)OptimizationModeComboBox.SelectedItem).Content}\n" +
                $"Трафик: {currentTrafficLevel}\n\n" +

                string.Join("\n\n", results.Select(r =>
                    $"{r.AlgorithmName}\n" +
                    $"Маршрут: {r.Route}\n" +
                    $"Време по маршрута: {r.TotalTravelTime:0.00} сек.\n" +
                    $"Време за изчисление: {r.CalculationTimeMs:0.0000} ms\n" +
                    $"Разстояние: {r.TotalDistanceKm:0.00} km\n" +
                    $"Гориво: {r.FuelLiters:0.00} л\n" +
                    $"Разход: {r.FuelCost:0.00} лв."
                )) +

                $"\n\nНАЙ-БЪРЗ АЛГОРИТЪМ:\n" +
                $"{bestByCalculationTime.AlgorithmName}\n\n" +

                $"НАЙ-ДОБЪР МАРШРУТ:\n" +
                $"{bestByRouteTime.AlgorithmName}";

            currentPath = bestByRouteTime.Path;
            DrawMap();
        }

        private AlgorithmResult RunBenchmark(string algorithmName, Func<List<Node>> algorithm)
        {
            var stopwatch = Stopwatch.StartNew();

            var path = algorithm();

            stopwatch.Stop();

            var totalTravelTime =
                RouteAlgorithms.CalculatePathDistance(path);

            var totalDistanceMeters =
                RouteStatisticsService.CalculateTotalDistanceMeters(path);

            var totalFuelLiters =
                CalculateFuelLitersForPath(path);

            var totalFuelCost =
                totalFuelLiters * FuelCalculator.FuelPricePerLiter;

            return new AlgorithmResult
            {
                AlgorithmName = algorithmName,

                Path = path,

                Route = path.Count > 0
                    ? string.Join(" → ", path.Select(n => n.Name))
                    : "Няма намерен маршрут",

                TotalTravelTime = totalTravelTime,

                CalculationTimeMs =
                    stopwatch.Elapsed.TotalMilliseconds,

                TrafficLevel = currentTrafficLevel,

                TotalDistanceKm =
                    totalDistanceMeters / 1000.0,

                FuelLiters = totalFuelLiters,

                FuelCost = totalFuelCost
            };
        }

        private List<Node> GetFloydWarshallPath(OptimizationMode optimizationMode)
        {
            if (startNode == null || endNode == null)
                return new List<Node>();

            floydWarshallService.Compute(graph, optimizationMode);

            return floydWarshallService.GetPath(startNode, endNode);
        }

        private double CalculateFuelLitersForPath(List<Node> path)
        {
            double total = 0;

            for (int i = 0; i < path.Count - 1; i++)
            {
                var edge = path[i].Edges.FirstOrDefault(e => e.Target == path[i + 1]);

                if (edge != null)
                {
                    total += FuelCalculator.CalculateFuelLiters(
                        edge.DistanceMeters,
                        edge.RoadType,
                        edge.TrafficMultiplier);
                }
            }

            return total;
        }

        private void RecalculateCurrentRoute()
        {
            if (startNode == null || endNode == null)
                return;

            var optimizationMode = GetSelectedOptimizationMode();

            var algorithm = ((ComboBoxItem)AlgorithmComboBox.SelectedItem).Content.ToString();

            currentPath = algorithm switch
            {
                "Dijkstra" =>
                    RouteAlgorithms.Dijkstra(graph, startNode, endNode, optimizationMode),

                "A*" =>
                    RouteAlgorithms.AStar(
                        graph,
                        startNode,
                        endNode,
                        RouteAlgorithms.EuclideanHeuristic,
                        optimizationMode),

                "A* ML Heuristic" =>
                    RouteAlgorithms.AStar(
                        graph,
                        startNode,
                        endNode,
                        RouteAlgorithms.MLInspiredHeuristic,
                        optimizationMode),

                "Genetic Algorithm" =>
                    RouteAlgorithms.GeneticAlgorithm(
                        graph,
                        startNode,
                        endNode,
                        optimizationMode),

                "Floyd-Warshall" =>
                    GetFloydWarshallPath(optimizationMode),

                _ => new List<Node>()
            };

            DrawMap();

            ResultTextBlock.Text =
                $"Маршрутът е автоматично преизчислен.\n\n" +
                $"Активен трафик: {currentTrafficLevel}\n\n" +
                $"Маршрут: {string.Join(" → ", currentPath.Select(n => n.Name))}";
        }

        private void ShowCharts_Click(object sender, RoutedEventArgs e)
        {
            if (lastBenchmarkResults.Count == 0)
            {
                ResultTextBlock.Text = "Първо изпълнете benchmark сравнение.";
                return;
            }

            var chartWindow = new BenchmarkChartWindow(lastBenchmarkResults);
            chartWindow.Show();
        }

        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            if (lastBenchmarkResults.Count == 0)
            {
                ResultTextBlock.Text =
                    "Първо изпълнете 'Сравни всички алгоритми', след което експортирайте резултатите.";
                return;
            }

            var fileName = $"benchmark_results_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            var filePath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                fileName);

            var sb = new StringBuilder();

            sb.AppendLine("Algorithm;TrafficLevel;Route;TotalTravelTimeSeconds;CalculationTimeMs;DistanceKm;FuelLiters;FuelCost");

            foreach (var result in lastBenchmarkResults)
            {
                sb.AppendLine(
                    $"{result.AlgorithmName};" +
                    $"{result.TrafficLevel};" +
                    $"{result.Route};" +
                    $"{result.TotalTravelTime.ToString("0.00", CultureInfo.InvariantCulture)};" +
                    $"{result.CalculationTimeMs.ToString("0.0000", CultureInfo.InvariantCulture)};" +
                    $"{result.TotalDistanceKm.ToString("0.00", CultureInfo.InvariantCulture)};" +
                    $"{result.FuelLiters.ToString("0.00", CultureInfo.InvariantCulture)};" +
                    $"{result.FuelCost.ToString("0.00", CultureInfo.InvariantCulture)}");
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

            ResultTextBlock.Text =
                $"CSV файлът е създаден успешно:\n{filePath}";
        }

        private void ClearSelection_Click(object sender, RoutedEventArgs e)
        {
            startNode = null;
            endNode = null;
            currentPath.Clear();

            ResultTextBlock.Text =
                "Изберете начален и краен град, след което стартирайте алгоритъм.";

            DrawMap();
        }
    }

    public class AlgorithmResult
    {
        public string AlgorithmName { get; set; } = string.Empty;

        public List<Node> Path { get; set; } = new();

        public string Route { get; set; } = string.Empty;

        public double TotalTravelTime { get; set; }

        public double CalculationTimeMs { get; set; }

        public string TrafficLevel { get; set; } = string.Empty;

        public double TotalDistanceKm { get; set; }

        public double FuelLiters { get; set; }

        public double FuelCost { get; set; }
    }
}