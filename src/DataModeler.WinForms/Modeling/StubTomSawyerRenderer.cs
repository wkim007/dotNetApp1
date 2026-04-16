using DataModeler.Core.Graph;

namespace DataModeler.WinForms.Modeling;

internal sealed class StubTomSawyerRenderer : ITomSawyerRenderer
{
    private readonly RichTextBox _textBox = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        Font = new Font("Cascadia Code", 10),
        BackColor = Color.White,
        BorderStyle = BorderStyle.None
    };

    public Control View => _textBox;

    public void Render(DiagramGraph graph)
    {
        var lines = new List<string>
        {
            "Tom Sawyer renderer placeholder",
            string.Empty,
            $"Diagram: {graph.Title}",
            $"Nodes: {graph.Nodes.Count}",
            $"Edges: {graph.Edges.Count}",
            string.Empty,
            "Entities"
        };

        lines.AddRange(graph.Nodes.Select(node => $"- {node.Label}"));
        lines.Add(string.Empty);
        lines.Add("Relationships");
        lines.AddRange(graph.Edges.Select(edge => $"- {edge.SourceId} -> {edge.TargetId} [{edge.Label}]"));

        _textBox.Lines = lines.ToArray();
    }
}
