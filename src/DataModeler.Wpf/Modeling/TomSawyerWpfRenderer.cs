using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using com.tomsawyer.configuration;
using com.tomsawyer.integrator.excel;
using com.tomsawyer.model;
using com.tomsawyer.model.defaultmodel;
using com.tomsawyer.project;
using com.tomsawyer.project.xml;
using com.tomsawyer.util.shared;
using com.tomsawyer.view.drawing.wpf;
using DataModeler.Core.Graph;

namespace DataModeler.Wpf.Modeling
{
    internal sealed class TomSawyerWpfRenderer
    {
        private readonly Grid _container;
        private readonly Border _diagnosticBorder;
        private readonly TextBlock _diagnosticText;
        private readonly StringBuilder _diagnosticBuilder = new StringBuilder();
        private TSModel _model;
        private string _constructionDiagnostics;
        private TSWPFDrawingView _drawingView;
        private TSNProject _project;
        private DockPanel _hostPanel;
        private string _projectFilePath;
        private string _dataFilePath;
        private string _moduleName;
        private string _schemaName;
        private string _viewName;
        private string _integratorName;
        private bool _dataLoaded;

        public TomSawyerWpfRenderer()
        {
            _container = new Grid();

            _diagnosticText = new TextBlock
            {
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(61, 72, 82)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0)
            };

            ScrollViewer diagnosticScroller = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = _diagnosticText
            };

            _diagnosticBorder = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(12),
                Padding = new Thickness(10),
                MaxWidth = 560,
                MaxHeight = 320,
                Background = new SolidColorBrush(Color.FromArgb(235, 255, 249, 219)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(212, 179, 108)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Child = diagnosticScroller
            };

            Panel.SetZIndex(_diagnosticBorder, 10);
            _diagnosticBorder.Visibility = Visibility.Collapsed;

            AppendDiagnostic("Renderer constructed.");
            ResetModel();
            TryCreateBasicTutorialView();
            _constructionDiagnostics = _diagnosticBuilder.ToString();
            UpdateDiagnosticText();
        }

        public Grid View
        {
            get { return _container; }
        }

        public string DiagnosticsText
        {
            get
            {
                return _diagnosticBuilder.Length == 0
                    ? "Renderer diagnostics are empty."
                    : _diagnosticBuilder.ToString();
            }
        }

        public void SetProjectFile(string projectFilePath)
        {
            if (!string.IsNullOrWhiteSpace(projectFilePath))
            {
                _projectFilePath = projectFilePath;
            }
        }

        public void SetDataFile(string dataFilePath)
        {
            if (!string.IsNullOrWhiteSpace(dataFilePath))
            {
                _dataFilePath = dataFilePath;
            }
        }

        public void ApplyProjectAndDataFiles()
        {
            ReinitializeView();
        }

        public void Render(DiagramGraph graph)
        {
            _diagnosticBuilder.Clear();
            if (!string.IsNullOrEmpty(_constructionDiagnostics))
            {
                _diagnosticBuilder.AppendLine("Construction diagnostics:");
                _diagnosticBuilder.AppendLine(_constructionDiagnostics.TrimEnd());
                _diagnosticBuilder.AppendLine();
            }

            AppendDiagnostic("Render started.");
            AppendDiagnostic(string.Format("Input graph title: {0}", graph == null ? "(null)" : graph.Title));
            AppendDiagnostic(string.Format("Input nodes: {0}", graph == null ? 0 : graph.Nodes.Count));
            AppendDiagnostic(string.Format("Input edges: {0}", graph == null ? 0 : graph.Edges.Count));
            AppendDiagnostic("Manual TSEGraphManager path is disabled.");
            AppendDiagnostic(string.Format("Project file: {0}", SafeValue(_projectFilePath)));
            AppendDiagnostic(string.Format("Data file: {0}", SafeValue(_dataFilePath)));
            AppendDiagnostic(string.Format("Module/View: {0} / {1}", SafeValue(_moduleName), SafeValue(_viewName)));
            AppendDiagnostic(_drawingView == null ? "Drawing view is null." : "Drawing view is available.");
            AppendDiagnostic(_hostPanel == null ? "Host panel is null." : "Host panel is available.");

            if (_drawingView == null)
            {
                AppendDiagnostic("Render aborted because the project-backed drawing view was not created.");
                UpdateDiagnosticText();
                return;
            }

            try
            {
                _drawingView.setModel(_model);
                AppendDiagnostic("setModel(TSDefaultModel) completed.");

                if (!_dataLoaded)
                {
                    TryLoadTutorialData();
                }
                else
                {
                    AppendDiagnostic("Tutorial data already loaded.");
                }

                if (_hostPanel == null)
                {
                    AppendDiagnostic("Host panel is still null after view creation.");
                    UpdateDiagnosticText();
                    return;
                }

                _drawingView.updateView();
                AppendDiagnostic("updateView() completed.");

                _drawingView.fitInView(true);
                AppendDiagnostic("fitInView(true) completed.");

                _hostPanel.UpdateLayout();
                AppendDiagnostic(string.Format(
                    "Host panel actual size: {0:0} x {1:0}",
                    _hostPanel.ActualWidth,
                    _hostPanel.ActualHeight));
            }
            catch (Exception ex)
            {
                AppendDiagnostic("Render exception:");
                AppendDiagnostic(ex.GetType().FullName);
                AppendDiagnostic(ex.Message);
                if (!string.IsNullOrEmpty(ex.StackTrace))
                {
                    AppendDiagnostic(ex.StackTrace);
                }
            }

            UpdateDiagnosticText();
        }

        private void ReinitializeView()
        {
            if (_hostPanel != null)
            {
                _container.Children.Remove(_hostPanel);
            }

            _drawingView = null;
            _project = null;
            _hostPanel = null;
            _dataLoaded = false;

            _diagnosticBuilder.Clear();
            AppendDiagnostic("Renderer reinitialized.");
            ResetModel();
            TryCreateBasicTutorialView();
            _constructionDiagnostics = _diagnosticBuilder.ToString();
            UpdateDiagnosticText();
        }

        private void TryCreateBasicTutorialView()
        {
            try
            {
                _projectFilePath = ResolveProjectFilePath(_projectFilePath);
                _dataFilePath = ResolveDataFilePath(_dataFilePath);
                AppendDiagnostic(string.Format("Resolved project file: {0}", SafeValue(_projectFilePath)));
                AppendDiagnostic(string.Format("Resolved data file: {0}", SafeValue(_dataFilePath)));

                if (string.IsNullOrWhiteSpace(_projectFilePath) || !File.Exists(_projectFilePath))
                {
                    AppendDiagnostic("Known tutorial .tsp file was not found.");
                    return;
                }

                TSNApplicationContext context = new TSNApplicationContext();
                AppendDiagnostic("TSNApplicationContext created.");

                _project = (TSNProject)context.getBean(typeof(TSNProject));
                AppendDiagnostic(_project == null ? "TSNProject bean is null." : "TSNProject bean acquired.");
                if (_project == null)
                {
                    return;
                }

                TSNProjectXMLReader reader = new TSNProjectXMLReader(_projectFilePath, false);
                reader.setProject(_project);
                reader.read();
                AppendDiagnostic("TSNProjectXMLReader.read() completed.");

                _moduleName = ResolveModuleName(_project, _moduleName);
                _schemaName = _moduleName;
                _viewName = ResolveViewName(_project, _moduleName, _viewName);
                _integratorName = ResolveIntegratorName(_project, _moduleName, _integratorName);

                AppendDiagnostic(string.Format("Resolved module name: {0}", SafeValue(_moduleName)));
                AppendDiagnostic(string.Format("Resolved schema name: {0}", SafeValue(_schemaName)));
                AppendDiagnostic(string.Format("Resolved view name: {0}", SafeValue(_viewName)));
                AppendDiagnostic(string.Format("Resolved integrator name: {0}", SafeValue(_integratorName)));

                _project.getSchema(_schemaName).initModel(_model);
                AppendDiagnostic("Project schema initialized TSDefaultModel.");

                _drawingView = (TSWPFDrawingView)_project.newView(_moduleName, _viewName, TSUserAgent.WPF);
                AppendDiagnostic(_drawingView == null
                    ? "project.newView(...) returned null."
                    : "project.newView(...) returned a TSWPFDrawingView.");

                if (_drawingView == null)
                {
                    return;
                }

                _drawingView.setModel(_model);
                AppendDiagnostic("setModel(TSDefaultModel) completed during construction.");

                _hostPanel = _drawingView.getDockPanel() as DockPanel;
                AppendDiagnostic(_hostPanel == null
                    ? "getDockPanel() returned null."
                    : "getDockPanel() returned a DockPanel.");

                if (_hostPanel == null)
                {
                    _hostPanel = _drawingView.getComponent() as DockPanel;
                    AppendDiagnostic(_hostPanel == null
                        ? "getComponent() returned null."
                        : "getComponent() returned a DockPanel.");
                }

                if (_hostPanel != null && !_container.Children.Contains(_hostPanel))
                {
                    _container.Children.Insert(0, _hostPanel);
                    AttachHostPanelEvents(_hostPanel);
                    AppendDiagnostic("Host panel inserted into renderer container.");
                }
            }
            catch (Exception ex)
            {
                AppendDiagnostic("Project-backed view creation failed:");
                AppendDiagnostic(ex.GetType().FullName);
                AppendDiagnostic(ex.Message);
                if (!string.IsNullOrEmpty(ex.StackTrace))
                {
                    AppendDiagnostic(ex.StackTrace);
                }
            }
        }

        private void TryLoadTutorialData()
        {
            try
            {
                if (_project == null)
                {
                    AppendDiagnostic("Cannot load data because TSNProject is null.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(_dataFilePath) || !File.Exists(_dataFilePath))
                {
                    AppendDiagnostic("Known tutorial data file was not found.");
                    return;
                }

                TSExcelIntegrator integrator = (TSExcelIntegrator)_project.newIntegrator(_moduleName, _integratorName);
                AppendDiagnostic(integrator == null
                    ? "project.newIntegrator(...) returned null."
                    : "project.newIntegrator(...) returned a TSExcelIntegrator.");

                if (integrator == null)
                {
                    return;
                }

                integrator.setModel(_model);
                integrator.setFile(_dataFilePath);
                AppendDiagnostic("Integrator model and file assigned.");

                integrator.update();
                _dataLoaded = true;
                AppendDiagnostic("Integrator update() completed.");
            }
            catch (Exception ex)
            {
                AppendDiagnostic("Data load failed:");
                AppendDiagnostic(ex.GetType().FullName);
                AppendDiagnostic(ex.Message);
                if (!string.IsNullOrEmpty(ex.StackTrace))
                {
                    AppendDiagnostic(ex.StackTrace);
                }
            }
        }

        private void AttachHostPanelEvents(DockPanel hostPanel)
        {
            hostPanel.Loaded += delegate
            {
                AppendDiagnostic("Host panel loaded.");
                UpdateDiagnosticText();
            };

            hostPanel.SizeChanged += delegate(object sender, SizeChangedEventArgs e)
            {
                AppendDiagnostic(string.Format("Host panel size: {0:0} x {1:0}", e.NewSize.Width, e.NewSize.Height));
                UpdateDiagnosticText();
            };
        }

        private static string ResolveProjectFilePath(string currentProjectFilePath)
        {
            if (!string.IsNullOrWhiteSpace(currentProjectFilePath))
            {
                return currentProjectFilePath;
            }

            string configuredPath = Environment.GetEnvironmentVariable("TS_PROJECT_FILE");
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                return configuredPath;
            }

            //return FindWorkspaceFile(Path.Combine("examples", "tutorials", "basic", "project", "Basic.tsp"));
            return FindWorkspaceFile("Basic.tsp");
        }

        private static string ResolveDataFilePath(string currentDataFilePath)
        {
            if (!string.IsNullOrWhiteSpace(currentDataFilePath))
            {
                return currentDataFilePath;
            }

            string configuredPath = Environment.GetEnvironmentVariable("TS_DATA_FILE");
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                return configuredPath;
            }

            //return FindWorkspaceFile(Path.Combine("examples", "tutorials", "basic", "data", "Basic.xls"));
            return FindWorkspaceFile("Basic.xls");
        }

        private static string FindWorkspaceFile(string relativePath)
        {
            DirectoryInfo current = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            for (int i = 0; i < 8 && current != null; i++)
            {
                string candidate = Path.Combine(current.FullName, relativePath);
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                current = current.Parent;
            }

            return null;
        }

        private static string ResolveValue(string environmentKey, string fallback)
        {
            string value = Environment.GetEnvironmentVariable(environmentKey);
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static string SafeValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "(none)" : value;
        }

        private static string ResolveModuleName(TSProject project, string currentModuleName)
        {
            if (!string.IsNullOrWhiteSpace(currentModuleName) && project.getModule(currentModuleName) != null)
            {
                return currentModuleName;
            }

            string configuredModule = Environment.GetEnvironmentVariable("TS_PROJECT_MODULE");
            if (!string.IsNullOrWhiteSpace(configuredModule) && project.getModule(configuredModule) != null)
            {
                return configuredModule;
            }

            java.util.List moduleNames = project.getModuleNames();
            return GetFirstListValue(moduleNames);
        }

        private static string ResolveViewName(TSProject project, string moduleName, string currentViewName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
            {
                return null;
            }

            java.util.List viewNames = project.getViewNames(moduleName);
            if (ListContains(viewNames, currentViewName))
            {
                return currentViewName;
            }

            string configuredView = Environment.GetEnvironmentVariable("TS_PROJECT_VIEW");
            if (ListContains(viewNames, configuredView))
            {
                return configuredView;
            }

            string preferredDrawingView = FindPreferredValue(viewNames, "map", "drawing", "graph");
            return preferredDrawingView ?? GetFirstListValue(viewNames);
        }

        private static string ResolveIntegratorName(TSProject project, string moduleName, string currentIntegratorName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
            {
                return null;
            }

            TSModule module = project.getModule(moduleName);
            if (module == null)
            {
                return null;
            }

            java.util.List integratorNames = module.getIntegratorNames();
            if (ListContains(integratorNames, currentIntegratorName))
            {
                return currentIntegratorName;
            }

            string configuredIntegrator = Environment.GetEnvironmentVariable("TS_PROJECT_INTEGRATOR");
            if (ListContains(integratorNames, configuredIntegrator))
            {
                return configuredIntegrator;
            }

            string preferredIntegrator = FindPreferredValue(integratorNames, "excel", "xml", "json", "rest", "data");
            return preferredIntegrator ?? GetFirstListValue(integratorNames);
        }

        private static bool ListContains(java.util.List values, string target)
        {
            if (values == null || string.IsNullOrWhiteSpace(target))
            {
                return false;
            }

            for (int i = 0; i < values.size(); i++)
            {
                object current = values.get(i);
                if (string.Equals(Convert.ToString(current), target, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetFirstListValue(java.util.List values)
        {
            if (values == null || values.size() == 0)
            {
                return null;
            }

            return Convert.ToString(values.get(0));
        }

        private static string FindPreferredValue(java.util.List values, params string[] keywords)
        {
            if (values == null)
            {
                return null;
            }

            for (int i = 0; i < values.size(); i++)
            {
                string current = Convert.ToString(values.get(i));
                if (string.IsNullOrWhiteSpace(current))
                {
                    continue;
                }

                string normalized = current.ToLowerInvariant();
                for (int keywordIndex = 0; keywordIndex < keywords.Length; keywordIndex++)
                {
                    if (normalized.Contains(keywords[keywordIndex]))
                    {
                        return current;
                    }
                }
            }

            return null;
        }

        private void ResetModel()
        {
            _moduleName = null;
            _model = new TSDefaultModel();
            AppendDiagnostic("Fresh TSDefaultModel created.");
        }

        private void AppendDiagnostic(string line)
        {
            _diagnosticBuilder.AppendLine(line);
        }

        private void UpdateDiagnosticText()
        {
            _diagnosticText.Text = _diagnosticBuilder.Length == 0
                ? "Renderer diagnostics will appear here."
                : _diagnosticBuilder.ToString();
        }
    }
}
