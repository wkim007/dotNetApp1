using System.Windows.Forms;
using System.Windows.Forms.Integration;
using com.tomsawyer.graph;
using com.tomsawyer.graphicaldrawing;
using com.tomsawyer.util.shared;
using com.tomsawyer.view.drawing;
using com.tomsawyer.view.drawing.wpf;
using DataModeler.Core.Graph;

namespace DataModeler.WinForms.Modeling;

internal sealed class TomSawyerRenderer : ITomSawyerRenderer
{
    private readonly ElementHost _host;
    private readonly TSWPFDrawingView _drawingView;

    public TomSawyerRenderer()
    {
        var viewDefinition = new TSModelDrawingViewDefinition();
        _drawingView = new TSWPFDrawingView(viewDefinition, TSUserAgent.WPF);

        _host = new ElementHost
        {
            Dock = DockStyle.Fill,
            Child = (System.Windows.Controls.DockPanel)_drawingView.getDockPanel()
        };
    }

    public Control View => _host;

    public void Render(DiagramGraph graph)
    {
        var graphManager = new TSEGraphManager();
        var drawingGraph = (TSEGraph)graphManager.addGraph();
        var nodeMap = new Dictionary<string, TSENode>(StringComparer.Ordinal);

        foreach (var node in graph.Nodes)
        {
            var drawingNode = (TSENode)drawingGraph.addNode();
            drawingNode.setName(node.Label);
            nodeMap[node.Id] = drawingNode;
        }

        foreach (var edge in graph.Edges)
        {
            if (!nodeMap.TryGetValue(edge.SourceId, out var sourceNode) ||
                !nodeMap.TryGetValue(edge.TargetId, out var targetNode))
            {
                continue;
            }

            var drawingEdge = drawingGraph.addEdge(sourceNode, targetNode);
            drawingEdge.setName(edge.Label);
        }

        graphManager.setMainDisplayGraph(drawingGraph);
        _drawingView.getCanvas().setGraphManager(graphManager);
        _drawingView.updateView();
        _drawingView.getCanvas().fitInCanvas(true);
    }
}