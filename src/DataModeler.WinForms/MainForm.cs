using DataModeler.Core.Graph;
using DataModeler.Core.Models;
using DataModeler.Core.Serialization;
using DataModeler.Core.Services;
using DataModeler.WinForms.Modeling;

namespace DataModeler.WinForms;

public sealed class MainForm : Form
{
    private readonly SchemaGraphBuilder _graphBuilder = new();
    private readonly SchemaJsonSerializer _serializer = new();
    private readonly ITomSawyerRenderer _renderer = TomSawyerRendererFactory.Create();
    private readonly TreeView _entityTree = new() { Dock = DockStyle.Fill, HideSelection = false };
    private readonly PropertyGrid _propertyGrid = new() { Dock = DockStyle.Fill, ToolbarVisible = false };
    private readonly ToolStripStatusLabel _statusLabel = new() { Text = "Ready" };
    private SchemaModel? _schema;

    public MainForm()
    {
        Text = "Sample .NET Data Modeler";
        Width = 1400;
        Height = 900;
        StartPosition = FormStartPosition.CenterScreen;

        var toolStrip = BuildToolbar();
        var layout = BuildLayout();
        var statusStrip = new StatusStrip();
        statusStrip.Items.Add(_statusLabel);

        Controls.Add(layout);
        Controls.Add(toolStrip);
        Controls.Add(statusStrip);

        Load += (_, _) => LoadSchema(SampleSchemaFactory.CreateRetailSchema());
        _entityTree.AfterSelect += OnTreeSelectionChanged;
    }

    private ToolStrip BuildToolbar()
    {
        var toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
        var reloadButton = new ToolStripButton("Load Sample Model");
        reloadButton.Click += (_, _) => LoadSchema(SampleSchemaFactory.CreateRetailSchema());

        var openButton = new ToolStripButton("Open Schema");
        openButton.Click += (_, _) => OpenSchemaFile();

        var applyLayoutButton = new ToolStripButton("Apply Layout");
        applyLayoutButton.Click += (_, _) => RenderCurrentSchema();

        toolStrip.Items.Add(reloadButton);
        toolStrip.Items.Add(openButton);
        toolStrip.Items.Add(applyLayoutButton);
        return toolStrip;
    }

    private Control BuildLayout()
    {
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 320
        };

        var leftRightSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 500
        };

        leftRightSplit.Panel1.Controls.Add(_entityTree);
        leftRightSplit.Panel2.Controls.Add(_propertyGrid);

        split.Panel1.Controls.Add(leftRightSplit);
        split.Panel2.Controls.Add(WrapRenderer(_renderer.View));

        return split;
    }

    private static Control WrapRenderer(Control rendererView)
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = Color.FromArgb(245, 247, 250) };
        var title = new Label
        {
            Dock = DockStyle.Top,
            Height = 28,
            Text = "Graph View",
            Font = new Font("Segoe UI Semibold", 11),
            ForeColor = Color.FromArgb(45, 55, 72)
        };

        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(10)
        };

        rendererView.Dock = DockStyle.Fill;
        card.Controls.Add(rendererView);
        panel.Controls.Add(card);
        panel.Controls.Add(title);
        return panel;
    }

    private void LoadSchema(SchemaModel schema)
    {
        _schema = schema;
        PopulateTree(schema);
        RenderCurrentSchema();
        _statusLabel.Text = $"Loaded {schema.Name}";
    }

    private void PopulateTree(SchemaModel schema)
    {
        _entityTree.BeginUpdate();
        _entityTree.Nodes.Clear();

        foreach (var entity in schema.Entities)
        {
            var entityNode = new TreeNode(entity.Name) { Tag = entity };
            foreach (var property in entity.Properties)
            {
                entityNode.Nodes.Add(new TreeNode(property.Name) { Tag = property });
            }

            _entityTree.Nodes.Add(entityNode);
        }

        foreach (TreeNode node in _entityTree.Nodes)
        {
            node.Expand();
        }

        _entityTree.EndUpdate();
    }

    private void RenderCurrentSchema()
    {
        if (_schema is null)
        {
            return;
        }

        DiagramGraph graph = _graphBuilder.Build(_schema);
        _renderer.Render(graph);
    }

    private void OpenSchemaFile()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Schema JSON (*.json)|*.json|All files (*.*)|*.*",
            Title = "Open Schema"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var schema = _serializer.DeserializeFromFile(dialog.FileName);
            LoadSchema(schema);
            _statusLabel.Text = $"Loaded {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Schema Load Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnTreeSelectionChanged(object? sender, TreeViewEventArgs e)
    {
        _propertyGrid.SelectedObject = e.Node.Tag switch
        {
            EntityDefinition entity => new
            {
                entity.Name,
                PropertyCount = entity.Properties.Count
            },
            PropertyDefinition property => new
            {
                property.Name,
                property.Type,
                property.IsPrimaryKey,
                property.IsNullable
            },
            _ => null
        };
    }
}
