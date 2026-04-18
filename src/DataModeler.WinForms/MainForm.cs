using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DataModeler.Core.Graph;
using DataModeler.Core.Models;
using DataModeler.Core.Serialization;
using DataModeler.Core.Services;
using DataModeler.WinForms.Modeling;

namespace DataModeler.WinForms
{
    public sealed class MainForm : Form
    {
        private readonly SchemaGraphBuilder _graphBuilder = new SchemaGraphBuilder();
        private readonly SchemaJsonSerializer _serializer = new SchemaJsonSerializer();
        private readonly ITomSawyerRenderer _renderer = TomSawyerRendererFactory.Create();
        private readonly TreeView _entityTree = new TreeView { Dock = DockStyle.Fill, HideSelection = false };
        private readonly PropertyGrid _propertyGrid = new PropertyGrid { Dock = DockStyle.Fill, ToolbarVisible = false };
        private readonly ToolStripStatusLabel _statusLabel = new ToolStripStatusLabel { Text = "Ready" };
        private SchemaModel _schema;

        public MainForm()
        {
            Text = "Sample .NET Data Modeler";
            Width = 1400;
            Height = 900;
            StartPosition = FormStartPosition.CenterScreen;

            ToolStrip toolStrip = BuildToolbar();
            Control layout = BuildLayout();
            StatusStrip statusStrip = new StatusStrip();
            statusStrip.Items.Add(_statusLabel);

            Controls.Add(layout);
            Controls.Add(toolStrip);
            Controls.Add(statusStrip);

            Load += delegate { LoadSchema(SampleSchemaFactory.CreateRetailSchema()); };
            _entityTree.AfterSelect += OnTreeSelectionChanged;
        }

        private ToolStrip BuildToolbar()
        {
            ToolStrip toolStrip = new ToolStrip
            {
                GripStyle = ToolStripGripStyle.Hidden,
                Dock = DockStyle.Top
            };

            ToolStripButton reloadButton = new ToolStripButton("Load Sample Model");
            reloadButton.Click += delegate { LoadSchema(SampleSchemaFactory.CreateRetailSchema()); };

            ToolStripButton openButton = new ToolStripButton("Open Schema");
            openButton.Click += delegate { OpenSchemaFile(); };

            ToolStripButton applyLayoutButton = new ToolStripButton("Apply Layout");
            applyLayoutButton.Click += delegate { RenderCurrentSchema(); };

            toolStrip.Items.Add(reloadButton);
            toolStrip.Items.Add(openButton);
            toolStrip.Items.Add(applyLayoutButton);

            return toolStrip;
        }

        private Control BuildLayout()
        {
            SplitContainer split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 320
            };

            SplitContainer leftRightSplit = new SplitContainer
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
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                BackColor = Color.FromArgb(245, 247, 250)
            };

            Label title = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                Text = "Graph View",
                Font = new Font("Segoe UI Semibold", 11),
                ForeColor = Color.FromArgb(45, 55, 72)
            };

            Panel card = new Panel
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
            _statusLabel.Text = string.Format("Loaded {0}", schema.Name);
        }

        private void PopulateTree(SchemaModel schema)
        {
            _entityTree.BeginUpdate();
            _entityTree.Nodes.Clear();

            foreach (EntityDefinition entity in schema.Entities)
            {
                TreeNode entityNode = new TreeNode(entity.Name);
                entityNode.Tag = entity;

                foreach (PropertyDefinition property in entity.Properties)
                {
                    TreeNode propertyNode = new TreeNode(property.Name);
                    propertyNode.Tag = property;
                    entityNode.Nodes.Add(propertyNode);
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
            if (_schema == null)
            {
                return;
            }

            DiagramGraph graph = _graphBuilder.Build(_schema);
            _renderer.Render(graph);
        }

        private void OpenSchemaFile()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Schema JSON (*.json)|*.json|All files (*.*)|*.*";
                dialog.Title = "Open Schema";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    SchemaModel schema = _serializer.DeserializeFromFile(dialog.FileName);
                    LoadSchema(schema);
                    _statusLabel.Text = string.Format("Loaded {0}", Path.GetFileName(dialog.FileName));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        this,
                        ex.Message,
                        "Schema Load Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void OnTreeSelectionChanged(object sender, TreeViewEventArgs e)
        {
            object tag = e.Node.Tag;

            if (tag is EntityDefinition)
            {
                EntityDefinition entity = (EntityDefinition)tag;
                _propertyGrid.SelectedObject = new
                {
                    Name = entity.Name,
                    PropertyCount = entity.Properties.Count
                };
            }
            else if (tag is PropertyDefinition)
            {
                PropertyDefinition property = (PropertyDefinition)tag;
                _propertyGrid.SelectedObject = new
                {
                    Name = property.Name,
                    Type = property.Type,
                    IsPrimaryKey = property.IsPrimaryKey,
                    IsNullable = property.IsNullable
                };
            }
            else
            {
                _propertyGrid.SelectedObject = null;
            }
        }
    }
}