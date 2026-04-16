namespace DataModeler.Core.Graph;

public sealed record DiagramGraph(
    string Title,
    IReadOnlyList<DiagramNode> Nodes,
    IReadOnlyList<DiagramEdge> Edges);

public sealed record DiagramNode(
    string Id,
    string Label,
    IReadOnlyList<string> Details);

public sealed record DiagramEdge(
    string Id,
    string SourceId,
    string TargetId,
    string Label);
