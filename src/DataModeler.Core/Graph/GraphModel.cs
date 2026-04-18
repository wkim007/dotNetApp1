using System.Collections.Generic;

namespace DataModeler.Core.Graph
{
    public sealed class DiagramGraph
    {
        public DiagramGraph(string title, IReadOnlyList<DiagramNode> nodes, IReadOnlyList<DiagramEdge> edges)
        {
            Title = title;
            Nodes = nodes;
            Edges = edges;
        }

        public string Title { get; }

        public IReadOnlyList<DiagramNode> Nodes { get; }

        public IReadOnlyList<DiagramEdge> Edges { get; }
    }

    public sealed class DiagramNode
    {
        public DiagramNode(string id, string label, IReadOnlyList<string> details)
        {
            Id = id;
            Label = label;
            Details = details;
        }

        public string Id { get; }

        public string Label { get; }

        public IReadOnlyList<string> Details { get; }
    }

    public sealed class DiagramEdge
    {
        public DiagramEdge(string id, string sourceId, string targetId, string label)
        {
            Id = id;
            SourceId = sourceId;
            TargetId = targetId;
            Label = label;
        }

        public string Id { get; }

        public string SourceId { get; }

        public string TargetId { get; }

        public string Label { get; }
    }
}
