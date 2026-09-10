namespace GraphProcessorAPI.Models
{
    public enum AlgorithmType { Dfs, Bfs, Dijkstra }
    public enum GraphType { Oriented, NonOriented }

    public partial class Graph
    {
        public int GraphId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime Creationat { get; set; }

        public string Structure { get; set; } = null!;

        public GraphType Type { get; set; }

        public int UserId { get; set; }

        public virtual ICollection<Edge> Edges { get; set; } = new List<Edge>();

        public virtual ICollection<Node> Nodes { get; set; } = new List<Node>();

        public virtual ICollection<ProcessingResult> ProcessingResults { get; set; } = new List<ProcessingResult>();

        public virtual User User { get; set; } = null!;
    }

    public partial class ProcessingResult
    {
        public int ProcessingResultId { get; set; }

        public int GraphId { get; set; }

        public double TimeInNs { get; set; }

        public string StartVertex { get; set; } = null!;

        public string? TargetVertex { get; set; }

        public List<string> ShortestPath { get; set; } = null!;

        public int? TotalDistance { get; set; }

        public AlgorithmType Algorithm { get; set; }

        public virtual Graph Graph { get; set; } = null!;
    }

    public partial class Node
    {
        public int NodeId { get; set; }

        public string Value { get; set; } = null!;

        public string? Image { get; set; }

        public string? Color { get; set; }

        public string? Description { get; set; }

        public int GraphId { get; set; }

        public virtual Graph Graph { get; set; } = null!;
    }

    public partial class Edge
    {
        public int EdgeId { get; set; }

        public string Value { get; set; } = null!;

        public string? Description { get; set; }

        public int Weight { get; set; }

        public int GraphId { get; set; }

        public int FromNode { get; set; }

        public int ToNode { get; set; }

        public virtual Graph Graph { get; set; } = null!;
    }

}
