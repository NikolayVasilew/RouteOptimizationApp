using Microsoft.Data.Sqlite;
using RouteOptimizationApi.Models;
using System.IO;

namespace RouteOptimizationApi.Data
{
    public class ApiDatabaseService
    {
        private readonly string ConnectionString =
    $"Data Source={Path.Combine(AppContext.BaseDirectory, "Data", "routes.db")}";

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
        public void CreateDispatchRequest(DispatchRequestDto request)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            """
            INSERT INTO DispatchRequests
            (StartNodeId, EndNodeId, AlgorithmName, Status, CreatedAt)
            VALUES
            ($start, $end, $algorithm, $status, $createdAt);
            """;

            command.Parameters.AddWithValue("$start", request.StartNodeId);
            command.Parameters.AddWithValue("$end", request.EndNodeId);
            command.Parameters.AddWithValue("$algorithm", request.AlgorithmName);
            command.Parameters.AddWithValue("$status", "Pending");
            command.Parameters.AddWithValue("$createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            command.ExecuteNonQuery();
        }

        public List<DispatchRequestDto> GetDispatchRequests()
        {
            var result = new List<DispatchRequestDto>();

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            """
            SELECT Id, StartNodeId, EndNodeId, AlgorithmName, Status,
            ResultPath, TotalDistanceMeters, ExecutionTimeMs, CreatedAt, CompletedAt
            FROM DispatchRequests
            ORDER BY Id DESC;
            """;

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                result.Add(new DispatchRequestDto
                {
                    Id = reader.GetInt32(0),
                    StartNodeId = reader.GetInt32(1),
                    EndNodeId = reader.GetInt32(2),
                    AlgorithmName = reader.GetString(3),
                    Status = reader.GetString(4),
                    ResultPath = reader.IsDBNull(5) ? null : reader.GetString(5),
                    TotalDistanceMeters = reader.IsDBNull(6) ? null : reader.GetDouble(6),
                    ExecutionTimeMs = reader.IsDBNull(7) ? null : reader.GetDouble(7),
                    CreatedAt = reader.GetString(8),
                    CompletedAt = reader.IsDBNull(9) ? null : reader.GetString(9)
                });
            }

            return result;
        }

        public void UpdateDispatchStatus(int id, string status)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            """
            UPDATE DispatchRequests
            SET Status = $status
            WHERE Id = $id;
            """;

            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$status", status);

            command.ExecuteNonQuery();
        }
        public List<NodeDto> GetNodes()
        {
            var result = new List<NodeDto>();

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Latitude, Longitude, X, Y, NodeType FROM Nodes;";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                result.Add(new NodeDto
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Latitude = reader.GetDouble(2),
                    Longitude = reader.GetDouble(3),
                    X = reader.GetDouble(4),
                    Y = reader.GetDouble(5),
                    NodeType = reader.GetString(6)
                });
            }

            return result;
        }

        public List<EdgeDto> GetEdges()
        {
            var result = new List<EdgeDto>();

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            """
            SELECT Id, FromNodeId, ToNodeId, StreetName, DistanceMeters,
                   SpeedLimitKmh, TravelTimeSeconds, RoadType, IsOneWay
            FROM Edges;
            """;

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                result.Add(new EdgeDto
                {
                    Id = reader.GetInt32(0),
                    FromNodeId = reader.GetInt32(1),
                    ToNodeId = reader.GetInt32(2),
                    StreetName = reader.GetString(3),
                    DistanceMeters = reader.GetDouble(4),
                    SpeedLimitKmh = reader.GetInt32(5),
                    TravelTimeSeconds = reader.GetDouble(6),
                    RoadType = reader.GetString(7),
                    IsOneWay = reader.GetInt32(8) == 1
                });
            }

            return result;
        }

        public void SaveExperiment(ExperimentDto experiment)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            """
            INSERT INTO RouteExperiments
            (AlgorithmName, StartNodeId, EndNodeId, Path, TotalDistanceMeters, ExecutionTimeMs, VisitedNodes, CreatedAt)
            VALUES
            ($algorithm, $start, $end, $path, $distance, $time, $visited, $createdAt);
            """;

            command.Parameters.AddWithValue("$algorithm", experiment.AlgorithmName);
            command.Parameters.AddWithValue("$start", experiment.StartNodeId);
            command.Parameters.AddWithValue("$end", experiment.EndNodeId);
            command.Parameters.AddWithValue("$path", experiment.Path);
            command.Parameters.AddWithValue("$distance", experiment.TotalDistanceMeters);
            command.Parameters.AddWithValue("$time", experiment.ExecutionTimeMs);
            command.Parameters.AddWithValue("$visited", experiment.VisitedNodes);
            command.Parameters.AddWithValue("$createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            command.ExecuteNonQuery();
        }

        public string GetDatabasePath()
        {
            return Path.Combine(AppContext.BaseDirectory, "routes.db");
        }

        public List<ExperimentDto> GetExperiments()
        {
            var result = new List<ExperimentDto>();

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            """
            SELECT Id, AlgorithmName, StartNodeId, EndNodeId, Path,
                   TotalDistanceMeters, ExecutionTimeMs, VisitedNodes, CreatedAt
            FROM RouteExperiments
            ORDER BY Id DESC;
            """;

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                result.Add(new ExperimentDto
                {
                    Id = reader.GetInt32(0),
                    AlgorithmName = reader.GetString(1),
                    StartNodeId = reader.GetInt32(2),
                    EndNodeId = reader.GetInt32(3),
                    Path = reader.GetString(4),
                    TotalDistanceMeters = reader.GetDouble(5),
                    ExecutionTimeMs = reader.GetDouble(6),
                    VisitedNodes = reader.GetInt32(7),
                    CreatedAt = reader.GetString(8)
                });
            }

            return result;
        }
    }
}
