using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using DataModeler.Core.Graph;
using DataModeler.Core.Models;
using DataModeler.Core.Serialization;
using DataModeler.Core.Services;
using DataModeler.Wpf.Modeling;
using Microsoft.Win32;

namespace DataModeler.Wpf
{
    public partial class MainWindow : Window
    {
        private readonly SchemaGraphBuilder _graphBuilder = new SchemaGraphBuilder();
        private readonly SchemaJsonSerializer _serializer = new SchemaJsonSerializer();
        private readonly TomSawyerWpfRenderer _renderer = new TomSawyerWpfRenderer();
        private SchemaModel _schema;
        private string _pendingTomSawyerProjectFile;
        private string _pendingTomSawyerDataFile;

        public MainWindow()
        {
            InitializeComponent();
            GraphHost.Content = _renderer.View;
            Loaded += delegate { LoadSchema(SampleSchemaFactory.CreateRetailSchema()); };
        }

        private void LoadSampleModelClick(object sender, RoutedEventArgs e)
        {
            LoadSchema(SampleSchemaFactory.CreateRetailSchema());
        }

        private void OpenSchemaClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Schema JSON (*.json)|*.json|All files (*.*)|*.*",
                Title = "Open Schema"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            try
            {
                SchemaModel schema = _serializer.DeserializeFromFile(dialog.FileName);
                LoadSchema(schema);
                StatusTextBlock.Text = string.Format("Loaded {0}", Path.GetFileName(dialog.FileName));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Schema Load Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyLayoutClick(object sender, RoutedEventArgs e)
        {
            RenderCurrentSchema();
        }

        private void OpenTomSawyerProjectClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Tom Sawyer Project (*.tsp)|*.tsp|All files (*.*)|*.*",
                Title = "Open Tom Sawyer Project"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            try
            {
                _pendingTomSawyerProjectFile = dialog.FileName;
                StatusTextBlock.Text = string.Format(
                    "Pending TSP file: {0}. Click Apply to use it.",
                    Path.GetFileName(dialog.FileName));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Project Open Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenTomSawyerDataClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Excel Data (*.xls)|*.xls|All files (*.*)|*.*",
                Title = "Open Tom Sawyer Data File"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            try
            {
                _pendingTomSawyerDataFile = dialog.FileName;
                StatusTextBlock.Text = string.Format(
                    "Pending data file: {0}. Click Apply to use it.",
                    Path.GetFileName(dialog.FileName));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Data File Open Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyTomSawyerFilesClick(object sender, RoutedEventArgs e)
        {
            try
            {
                bool hasPendingChanges = false;

                if (!string.IsNullOrWhiteSpace(_pendingTomSawyerProjectFile))
                {
                    _renderer.SetProjectFile(_pendingTomSawyerProjectFile);
                    hasPendingChanges = true;
                }

                if (!string.IsNullOrWhiteSpace(_pendingTomSawyerDataFile))
                {
                    _renderer.SetDataFile(_pendingTomSawyerDataFile);
                    hasPendingChanges = true;
                }

                if (!hasPendingChanges)
                {
                    StatusTextBlock.Text = "No pending TSP or data file changes to apply.";
                    return;
                }

                _renderer.ApplyProjectAndDataFiles();
                RenderCurrentSchema();

                string projectName = string.IsNullOrWhiteSpace(_pendingTomSawyerProjectFile)
                    ? "(unchanged)"
                    : Path.GetFileName(_pendingTomSawyerProjectFile);
                string dataName = string.IsNullOrWhiteSpace(_pendingTomSawyerDataFile)
                    ? "(unchanged)"
                    : Path.GetFileName(_pendingTomSawyerDataFile);

                _pendingTomSawyerProjectFile = null;
                _pendingTomSawyerDataFile = null;

                StatusTextBlock.Text = string.Format("Applied TSP: {0}, Data: {1}", projectName, dataName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Apply Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowDiagnosticsClick(object sender, RoutedEventArgs e)
        {
            Window diagnosticsWindow = new Window
            {
                Title = "Tom Sawyer Diagnostics",
                Owner = this,
                Width = 900,
                Height = 640,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new TextBox
                {
                    Text = _renderer.DiagnosticsText,
                    IsReadOnly = true,
                    TextWrapping = TextWrapping.NoWrap,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    AcceptsReturn = true,
                    AcceptsTab = true,
                    FontFamily = new System.Windows.Media.FontFamily("Consolas")
                }
            };

            diagnosticsWindow.ShowDialog();
        }

        private void LoadSchema(SchemaModel schema)
        {
            _schema = schema;
            PopulateTree(schema);
            RenderCurrentSchema();
            StatusTextBlock.Text = string.Format("Loaded {0}", schema.Name);
        }

        private void PopulateTree(SchemaModel schema)
        {
            EntityTreeView.Items.Clear();

            foreach (EntityDefinition entity in schema.Entities)
            {
                TreeViewItem entityItem = new TreeViewItem
                {
                    Header = entity.Name,
                    Tag = entity,
                    IsExpanded = true
                };

                foreach (PropertyDefinition property in entity.Properties)
                {
                    entityItem.Items.Add(new TreeViewItem
                    {
                        Header = property.Name,
                        Tag = property
                    });
                }

                EntityTreeView.Items.Add(entityItem);
            }
        }

        private void EntityTreeSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem selectedItem = EntityTreeView.SelectedItem as TreeViewItem;
            object selectedTag = selectedItem == null ? null : selectedItem.Tag;

            if (selectedTag is EntityDefinition)
            {
                EntityDefinition entity = (EntityDefinition)selectedTag;
                PropertiesGrid.ItemsSource = new[]
                {
                    new PropertyRow("Name", entity.Name),
                    new PropertyRow("PropertyCount", entity.Properties.Count.ToString())
                };
                return;
            }

            if (selectedTag is PropertyDefinition)
            {
                PropertyDefinition property = (PropertyDefinition)selectedTag;
                PropertiesGrid.ItemsSource = new[]
                {
                    new PropertyRow("Name", property.Name),
                    new PropertyRow("Type", property.Type),
                    new PropertyRow("IsPrimaryKey", property.IsPrimaryKey.ToString()),
                    new PropertyRow("IsNullable", property.IsNullable.ToString())
                };
                return;
            }

            PropertiesGrid.ItemsSource = Array.Empty<PropertyRow>();
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

        private sealed class PropertyRow
        {
            public PropertyRow(string name, string value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; private set; }

            public string Value { get; private set; }
        }
    }
}
