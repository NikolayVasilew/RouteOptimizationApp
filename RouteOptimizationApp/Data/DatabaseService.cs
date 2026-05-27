using Microsoft.Data.Sqlite;
using RouteOptimizationApp.Models;

namespace RouteOptimizationApp.Data
{
    public class DatabaseService
    {
        private const string ConnectionString = "Data Source=routes.db";

        public void InitializeDatabase()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS Nodes (
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                Latitude REAL NOT NULL,
                Longitude REAL NOT NULL,
                X REAL NOT NULL,
                Y REAL NOT NULL,
                NodeType TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Edges (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FromNodeId INTEGER NOT NULL,
                ToNodeId INTEGER NOT NULL,
                StreetName TEXT NOT NULL,
                DistanceMeters REAL NOT NULL,
                SpeedLimitKmh INTEGER NOT NULL,
                TravelTimeSeconds REAL NOT NULL,
                RoadType TEXT NOT NULL,
                IsOneWay INTEGER NOT NULL DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS RouteExperiments (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                AlgorithmName TEXT NOT NULL,
                StartNodeId INTEGER NOT NULL,
                EndNodeId INTEGER NOT NULL,
                Path TEXT NOT NULL,
                TotalDistanceMeters REAL NOT NULL,
                ExecutionTimeMs REAL NOT NULL,
                VisitedNodes INTEGER NOT NULL,
                CreatedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS DispatchRequests (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                StartNodeId INTEGER NOT NULL,
                EndNodeId INTEGER NOT NULL,
                AlgorithmName TEXT NOT NULL,
                Status TEXT NOT NULL,
                ResultPath TEXT,
                TotalDistanceMeters REAL,
                ExecutionTimeMs REAL,
                CreatedAt TEXT NOT NULL,
                CompletedAt TEXT
            );
            """;

            command.ExecuteNonQuery();
        }

        public void SeedDatabase()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM Nodes;";

            var count = Convert.ToInt32(checkCommand.ExecuteScalar());

            if (count > 0)
                return;

            var command = connection.CreateCommand();

            command.CommandText =
            """
            INSERT INTO Nodes (Id, Name, Latitude, Longitude, X, Y, NodeType) VALUES
            (1, 'София', 42.6977, 23.3219, 160, 320, 'capital'),
            (2, 'Пловдив', 42.1354, 24.7453, 420, 430, 'city'),
            (3, 'Варна', 43.2141, 27.9147, 820, 170, 'city'),
            (4, 'Бургас', 42.5048, 27.4626, 760, 430, 'city'),
            (5, 'Русе', 43.8356, 25.9657, 560, 90, 'city'),
            (6, 'Стара Загора', 42.4258, 25.6345, 560, 430, 'city'),
            (7, 'Плевен', 43.4170, 24.6067, 360, 150, 'city'),
            (8, 'Велико Търново', 43.0757, 25.6172, 530, 230, 'city'),
            (9, 'Благоевград', 42.0209, 23.0943, 120, 520, 'city'),
            (10, 'Видин', 43.9962, 22.8679, 110, 80, 'city'),
            (11, 'Монтана', 43.4085, 23.2258, 150, 160, 'city'),
            (12, 'Враца', 43.2102, 23.5529, 200, 210, 'city'),
            (13, 'Габрово', 42.8742, 25.3187, 470, 280, 'city'),
            (14, 'Сливен', 42.6817, 26.3229, 660, 360, 'city'),
            (15, 'Шумен', 43.2712, 26.9361, 710, 160, 'city'),
            (16, 'Добрич', 43.5726, 27.8273, 830, 90, 'city'),
            (17, 'Хасково', 41.9344, 25.5556, 560, 540, 'city'),
            (18, 'Кърджали', 41.6338, 25.3777, 520, 610, 'city'),
            (19, 'Смолян', 41.5774, 24.7011, 390, 620, 'city'),
            (20, 'Перник', 42.6052, 23.0378, 120, 370, 'city');

            INSERT INTO Edges 
            (FromNodeId, ToNodeId, StreetName, DistanceMeters, SpeedLimitKmh, TravelTimeSeconds, RoadType, IsOneWay) VALUES
            (1, 20, 'I-1 / E79', 35000, 90, 1400, 'primary', 0),
            (20, 9, 'A3 Струма', 98000, 120, 2940, 'motorway', 0),
            (1, 2, 'A1 Тракия', 145000, 130, 4015, 'motorway', 0),
            (2, 6, 'A1 Тракия', 90000, 130, 2492, 'motorway', 0),
            (6, 4, 'A1 Тракия', 170000, 130, 4708, 'motorway', 0),
            (6, 14, 'A1 Тракия', 70000, 120, 2100, 'motorway', 0),
            (14, 4, 'A1 Тракия', 115000, 130, 3185, 'motorway', 0),
            (1, 7, 'A2 Хемус / I-3', 170000, 110, 5563, 'mixed', 0),
            (7, 8, 'I-3 / E83', 115000, 90, 4600, 'primary', 0),
            (8, 15, 'A2 Хемус', 135000, 120, 4050, 'motorway', 0),
            (15, 3, 'A2 Хемус', 90000, 120, 2700, 'motorway', 0),
            (3, 16, 'I-29', 52000, 90, 2080, 'primary', 0),
            (8, 5, 'I-5 / E85', 105000, 90, 4200, 'primary', 0),
            (5, 15, 'I-2', 110000, 90, 4400, 'primary', 0),
            (2, 17, 'A4 Марица', 95000, 120, 2850, 'motorway', 0),
            (17, 18, 'I-5', 50000, 80, 2250, 'primary', 0),
            (2, 19, 'II-86', 105000, 70, 5400, 'secondary', 0),
            (19, 18, 'II-86', 90000, 70, 4628, 'secondary', 0),
            (10, 11, 'I-1 / E79', 95000, 90, 3800, 'primary', 0),
            (11, 12, 'I-1 / E79', 43000, 90, 1720, 'primary', 0),
            (12, 1, 'I-1 / E79', 110000, 90, 4400, 'primary', 0),
            (12, 7, 'II-13 / II-35', 95000, 80, 4275, 'secondary', 0),
            (8, 13, 'I-5', 46000, 80, 2070, 'primary', 0),
            (13, 6, 'Проход Шипка / I-5', 72000, 60, 4320, 'mountain', 0),
            (6, 17, 'I-5', 65000, 90, 2600, 'primary', 0);
            """;

            command.ExecuteNonQuery();
        }

        public Graph LoadGraph()
        {
            var graph = new Graph();
            var nodes = new Dictionary<int, Node>();

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var nodeCommand = connection.CreateCommand();
            nodeCommand.CommandText =
            """
            SELECT Id, Name, Latitude, Longitude, X, Y, NodeType
            FROM Nodes;
            """;

            using (var reader = nodeCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    var node = new Node(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetDouble(2),
                        reader.GetDouble(3),
                        reader.GetDouble(4),
                        reader.GetDouble(5),
                        reader.GetString(6));

                    nodes[node.Id] = node;
                    graph.AddNode(node);
                }
            }

            var edgeCommand = connection.CreateCommand();
            edgeCommand.CommandText =
            """
            SELECT FromNodeId, ToNodeId, StreetName, DistanceMeters,
                   SpeedLimitKmh, TravelTimeSeconds, RoadType, IsOneWay
            FROM Edges;
            """;

            using (var reader = edgeCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    var from = nodes[reader.GetInt32(0)];
                    var to = nodes[reader.GetInt32(1)];

                    var streetName = reader.GetString(2);
                    var distanceMeters = reader.GetDouble(3);
                    var speedLimitKmh = reader.GetInt32(4);
                    var travelTimeSeconds = reader.GetDouble(5);
                    var roadType = reader.GetString(6);
                    var isOneWay = reader.GetInt32(7) == 1;

                    graph.AddDirectedEdge(
                        from,
                        to,
                        streetName,
                        distanceMeters,
                        speedLimitKmh,
                        travelTimeSeconds,
                        roadType,
                        isOneWay);

                    if (!isOneWay)
                    {
                        graph.AddDirectedEdge(
                            to,
                            from,
                            streetName,
                            distanceMeters,
                            speedLimitKmh,
                            travelTimeSeconds,
                            roadType,
                            false);
                    }
                }
            }

            return graph;
        }

        public void ClearDatabase()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            """
            DELETE FROM RouteExperiments;
            DELETE FROM Edges;
            DELETE FROM Nodes;
            """;

            command.ExecuteNonQuery();
        }
    }
}