using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DataModeler.Core.Graph;

namespace DataModeler.WinForms.Modeling;

internal sealed class TomSawyerRenderer : ITomSawyerRenderer
{
    private readonly GraphCanvas _canvas = new GraphCanvas();

    public Control View => _canvas;

    public void Render(DiagramGraph graph)
    {
        _canvas.Render(graph);
    }

    private sealed class GraphCanvas : Panel
    {
        private const int HeaderHeight = 34;
        private const int OuterPadding = 24;
        private const int ColumnGap = 42;
        private const int RowGap = 36;
        private const int NodeWidth = 240;
        private const int NodeHeaderHeight = 30;
        private const int NodeDetailLineHeight = 18;
        private const int NodeBottomPadding = 12;
        private readonly Font _titleFont = new Font("Segoe UI", 10f, FontStyle.Bold);
        private readonly Font _nodeFont = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        private readonly Font _detailFont = new Font("Segoe UI", 8.5f, FontStyle.Regular);
        private readonly Pen _edgePen = new Pen(Color.FromArgb(90, 110, 140), 2f);
        private readonly Brush _nodeFill = new SolidBrush(Color.FromArgb(220, 235, 252));
        private readonly Brush _nodeHeaderFill = new SolidBrush(Color.FromArgb(67, 97, 142));
        private readonly Brush _nodeTextBrush = new SolidBrush(Color.FromArgb(255, 255, 255));
        private readonly Brush _detailTextBrush = new SolidBrush(Color.FromArgb(47, 56, 66));
        private readonly Brush _noticeBrush = new SolidBrush(Color.FromArgb(120, 92, 32));
        private readonly Pen _nodeBorderPen = new Pen(Color.FromArgb(42, 67, 101), 1.5f);
        private readonly Pen _noticeBorderPen = new Pen(Color.FromArgb(212, 179, 108), 1f);
        private readonly Brush _noticeFill = new SolidBrush(Color.FromArgb(255, 247, 222));
        private readonly StringFormat _centerFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        private DiagramGraph _graph;
        private Dictionary<string, RectangleF> _nodeBounds = new Dictionary<string, RectangleF>(StringComparer.Ordinal);

        public GraphCanvas()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.White;
            DoubleBuffered = true;
            Resize += delegate { RebuildLayout(); };
        }

        public void Render(DiagramGraph graph)
        {
            _graph = graph;
            RebuildLayout();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            DrawNotice(graphics);

            if (_graph == null || _graph.Nodes.Count == 0)
            {
                DrawEmptyState(graphics);
                return;
            }

            DrawEdges(graphics);
            DrawNodes(graphics);
        }

        private void RebuildLayout()
        {
            _nodeBounds = new Dictionary<string, RectangleF>(StringComparer.Ordinal);
            if (_graph == null || _graph.Nodes.Count == 0 || ClientSize.Width <= 0 || ClientSize.Height <= 0)
            {
                return;
            }

            int availableWidth = Math.Max(1, ClientSize.Width - (OuterPadding * 2));
            int columns = Math.Max(1, (availableWidth + ColumnGap) / (NodeWidth + ColumnGap));

            for (int i = 0; i < _graph.Nodes.Count; i++)
            {
                DiagramNode node = _graph.Nodes[i];
                int column = i % columns;
                int row = i / columns;
                int nodeHeight = CalculateNodeHeight(node);
                float left = OuterPadding + (column * (NodeWidth + ColumnGap));
                float top = HeaderHeight + OuterPadding + (row * (nodeHeight + RowGap));
                _nodeBounds[node.Id] = new RectangleF(left, top, NodeWidth, nodeHeight);
            }
        }

        private int CalculateNodeHeight(DiagramNode node)
        {
            int detailCount = node.Details == null ? 0 : node.Details.Count;
            return NodeHeaderHeight + NodeBottomPadding + Math.Max(1, detailCount) * NodeDetailLineHeight;
        }

        private void DrawNotice(Graphics graphics)
        {
            Rectangle noticeBounds = new Rectangle(OuterPadding, 10, Math.Max(200, ClientSize.Width - (OuterPadding * 2)), 40);
            graphics.FillRectangle(_noticeFill, noticeBounds);
            graphics.DrawRectangle(_noticeBorderPen, noticeBounds);
            graphics.DrawString(
                "Tom Sawyer host bridge is unavailable on this setup. Showing fallback graph renderer.",
                _detailFont,
                _noticeBrush,
                noticeBounds,
                _centerFormat);
        }

        private void DrawEmptyState(Graphics graphics)
        {
            Rectangle bounds = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);
            graphics.DrawString("No graph items to display.", _titleFont, Brushes.Gray, bounds, _centerFormat);
        }

        private void DrawEdges(Graphics graphics)
        {
            foreach (DiagramEdge edge in _graph.Edges)
            {
                RectangleF sourceBounds;
                RectangleF targetBounds;
                if (!_nodeBounds.TryGetValue(edge.SourceId, out sourceBounds) ||
                    !_nodeBounds.TryGetValue(edge.TargetId, out targetBounds))
                {
                    continue;
                }

                PointF start = new PointF(sourceBounds.Right, sourceBounds.Top + (sourceBounds.Height / 2f));
                PointF end = new PointF(targetBounds.Left, targetBounds.Top + (targetBounds.Height / 2f));

                if (targetBounds.Left < sourceBounds.Left)
                {
                    start = new PointF(sourceBounds.Left, sourceBounds.Top + (sourceBounds.Height / 2f));
                    end = new PointF(targetBounds.Right, targetBounds.Top + (targetBounds.Height / 2f));
                }

                graphics.DrawLine(_edgePen, start, end);
                DrawArrowHead(graphics, start, end);

                if (!string.IsNullOrWhiteSpace(edge.Label))
                {
                    float labelX = (start.X + end.X) / 2f;
                    float labelY = ((start.Y + end.Y) / 2f) - 14f;
                    SizeF labelSize = graphics.MeasureString(edge.Label, _detailFont);
                    RectangleF labelBounds = new RectangleF(
                        labelX - (labelSize.Width / 2f) - 6f,
                        labelY - 2f,
                        labelSize.Width + 12f,
                        labelSize.Height + 4f);

                    graphics.FillRectangle(Brushes.White, labelBounds);
                    graphics.DrawString(edge.Label, _detailFont, Brushes.DimGray, labelBounds, _centerFormat);
                }
            }
        }

        private void DrawArrowHead(Graphics graphics, PointF start, PointF end)
        {
            const float arrowLength = 10f;
            const float arrowWidth = 4f;

            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            float length = (float)Math.Sqrt((dx * dx) + (dy * dy));
            if (length <= 0.1f)
            {
                return;
            }

            float ux = dx / length;
            float uy = dy / length;
            PointF arrowBase = new PointF(end.X - (ux * arrowLength), end.Y - (uy * arrowLength));
            PointF left = new PointF(arrowBase.X - (uy * arrowWidth), arrowBase.Y + (ux * arrowWidth));
            PointF right = new PointF(arrowBase.X + (uy * arrowWidth), arrowBase.Y - (ux * arrowWidth));

            graphics.FillPolygon(_edgePen.Brush, new[] { end, left, right });
        }

        private void DrawNodes(Graphics graphics)
        {
            foreach (DiagramNode node in _graph.Nodes)
            {
                RectangleF bounds;
                if (!_nodeBounds.TryGetValue(node.Id, out bounds))
                {
                    continue;
                }

                GraphicsPath path = CreateRoundedRectangle(bounds, 10f);
                graphics.FillPath(_nodeFill, path);
                graphics.DrawPath(_nodeBorderPen, path);

                RectangleF headerBounds = new RectangleF(bounds.Left, bounds.Top, bounds.Width, NodeHeaderHeight);
                GraphicsPath headerPath = CreateRoundedTopRectangle(headerBounds, 10f);
                graphics.FillPath(_nodeHeaderFill, headerPath);

                RectangleF headerTextBounds = new RectangleF(bounds.Left + 10f, bounds.Top, bounds.Width - 20f, NodeHeaderHeight);
                graphics.DrawString(node.Label, _nodeFont, _nodeTextBrush, headerTextBounds, _centerFormat);

                float detailTop = bounds.Top + NodeHeaderHeight + 8f;
                IReadOnlyList<string> details = node.Details ?? Array.Empty<string>();
                if (details.Count == 0)
                {
                    graphics.DrawString("(no properties)", _detailFont, Brushes.Gray, bounds.Left + 12f, detailTop);
                    continue;
                }

                foreach (string detail in details)
                {
                    RectangleF detailBounds = new RectangleF(bounds.Left + 12f, detailTop, bounds.Width - 24f, NodeDetailLineHeight);
                    graphics.DrawString(detail, _detailFont, _detailTextBrush, detailBounds);
                    detailTop += NodeDetailLineHeight;
                }
            }
        }

        private static GraphicsPath CreateRoundedRectangle(RectangleF bounds, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float diameter = radius * 2f;
            RectangleF arc = new RectangleF(bounds.Location, new SizeF(diameter, diameter));

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static GraphicsPath CreateRoundedTopRectangle(RectangleF bounds, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float diameter = radius * 2f;
            RectangleF arc = new RectangleF(bounds.Location, new SizeF(diameter, diameter));

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            path.AddLine(bounds.Right, bounds.Bottom, bounds.Left, bounds.Bottom);
            path.CloseFigure();
            return path;
        }
    }
}
