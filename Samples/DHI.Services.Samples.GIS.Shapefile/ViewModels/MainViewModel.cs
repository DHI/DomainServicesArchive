using DHI.Services.GIS;
using DHI.Services.Samples.GIS.Shapefile.Composition;
using DHI.Services.Samples.GIS.Shapefile.DomainAdapters;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace DHI.Services.Samples.GIS.Shapefile.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private GisRuntime? _runtime;
        private GisPlaygroundAdapter? _adapter;

        private string? _selectedPath;
        private string? _selectedCollectionId;
        private DataView? _attributesView;
        private DataView? _featuresView;
        private string? _filterAttributeName;
        private string? _filterEqualsValue;
        private string _status = "Pick a shapefile or a folder…";

        public ObservableCollection<string> CollectionIds { get; } = new();
        public ObservableCollection<string> AttributeNames { get; } = new();
        public ObservableCollection<OperatorOption> AvailableOperators { get; } = new();
        private OperatorOption? _selectedOperator;
        public OperatorOption? SelectedOperator
        {
            get => _selectedOperator;
            set { _selectedOperator = value; OnPropertyChanged(); }
        }

        public string? SelectedPath
        {
            get => _selectedPath;
            set { _selectedPath = value; OnPropertyChanged(); }
        }

        public string? SelectedCollectionId
        {
            get => _selectedCollectionId;
            set { _selectedCollectionId = value; OnPropertyChanged(); LoadSelectedCollection(); }
        }

        public DataView? AttributesView
        {
            get => _attributesView;
            set { _attributesView = value; OnPropertyChanged(); }
        }

        public DataView? FeaturesView
        {
            get => _featuresView;
            set { _featuresView = value; OnPropertyChanged(); }
        }

        public string? FilterAttributeName
        {
            get => _filterAttributeName;
            set
            {
                _filterAttributeName = value;
                OnPropertyChanged();
                UpdateOperatorsForAttribute();
            }
        }

        public string? FilterEqualsValue
        {
            get => _filterEqualsValue;
            set { _filterEqualsValue = value; OnPropertyChanged(); }
        }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public ICommand BrowseFileCommand { get; }
        public ICommand BrowseFolderCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand ApplyFilterCommand { get; }
        public ICommand SaveAsCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            BrowseFileCommand = new RelayCommand(_ => BrowseFile());
            BrowseFolderCommand = new RelayCommand(_ => BrowseFolder());
            ReloadCommand = new RelayCommand(_ => Reload(), _ => _adapter != null);
            ApplyFilterCommand = new RelayCommand(
                  _ => ApplyFilter(),
                  _ => _adapter != null && !string.IsNullOrWhiteSpace(SelectedCollectionId) && !string.IsNullOrWhiteSpace(FilterAttributeName) && SelectedOperator != null);
            SaveAsCommand = new RelayCommand(_ => SaveAs(), _ => _adapter != null && !string.IsNullOrWhiteSpace(SelectedCollectionId));
        }

        private void BrowseFile()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "ESRI Shapefile (*.shp)|*.shp",
                Title = "Open Shapefile"
            };
            if (dlg.ShowDialog() == true)
            {
                SelectedPath = dlg.FileName;
                Wire(SelectedPath);
                Reload();
            }
        }

        private void BrowseFolder()
        {
            using var dlg = new System.Windows.Forms.FolderBrowserDialog { Description = "Pick a folder with .shp files" };
            var result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                SelectedPath = dlg.SelectedPath;
                Wire(SelectedPath);
                Reload();
            }
        }

        private void Wire(string path)
        {
            try
            {
                _runtime = CompositionRoot.Wire(path);
                _adapter = new GisPlaygroundAdapter(_runtime);
                Status = $"Provider ready @ {path}";
            }
            catch (Exception ex)
            {
                Status = $"Error: {ex.Message}";
                MessageBox.Show(ex.Message, "Error wiring provider", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Reload()
        {
            if (_adapter == null) return;

            CollectionIds.Clear();
            foreach (var id in _adapter.ListCollectionIds())
            {
                CollectionIds.Add(id);
            }

            if (CollectionIds.Count > 0)
            {
                SelectedCollectionId = CollectionIds[0];
            }
            else
            {
                SelectedCollectionId = null;
                AttributesView = null;
                FeaturesView = null;
                Status = "No shapefiles found.";
            }
        }

        private void LoadSelectedCollection()
        {
            if (_adapter == null || string.IsNullOrWhiteSpace(SelectedCollectionId))
                return;

            try
            {
                var coll = _adapter.GetCollection(SelectedCollectionId, associations: false);

                AttributesView = _adapter.ToAttributesTable(coll).DefaultView;
                FeaturesView = _adapter.ToFeaturesTable(coll).DefaultView;

                AttributeNames.Clear();
                foreach (var a in coll.Attributes) AttributeNames.Add(a.Name);

                FilterAttributeName ??= AttributeNames.FirstOrDefault();
                UpdateOperatorsForAttribute();

                Status = $"Loaded '{SelectedCollectionId}': {coll.Features.Count} feature(s), {coll.Attributes.Count} attribute(s).";
            }
            catch (Exception ex)
            {
                Status = $"Error loading '{SelectedCollectionId}': {ex.Message}";
                MessageBox.Show(ex.Message, "Load error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilter()
        {
            if (_adapter == null || string.IsNullOrWhiteSpace(SelectedCollectionId) ||
                string.IsNullOrWhiteSpace(FilterAttributeName) || SelectedOperator == null)
                return;

            try
            {
                FeatureCollection<string> coll;

                if (string.IsNullOrWhiteSpace(FilterEqualsValue))
                {
                    coll = _adapter.GetCollection(SelectedCollectionId, associations: false);
                    Status = "Filter cleared.";
                }
                else
                {
                    var info = _adapter.GetCollectionInfo(SelectedCollectionId);
                    var attr = info.Attributes.FirstOrDefault(a => a.Name == FilterAttributeName);
                    var op = SelectedOperator.Value;

                    bool stringy = op == QueryOperator.Like || op == QueryOperator.NotLike || op == QueryOperator.Contains;
                    var typedValue = stringy ? (object)FilterEqualsValue! : CoerceValue(attr?.DataType, FilterEqualsValue!);

                    coll = _adapter.GetCollectionFiltered(SelectedCollectionId, FilterAttributeName, op, typedValue);
                    Status = $"Filter: {FilterAttributeName} {SelectedOperator.Label} '{FilterEqualsValue}'. Result: {coll.Features.Count} feature(s).";
                }

                FeaturesView = _adapter.ToFeaturesTable(coll).DefaultView;
            }
            catch (Exception ex)
            {
                Status = $"Filter error: {ex.Message}";
                MessageBox.Show(ex.Message, "Filter error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SaveAs()
        {
            if (_adapter == null || string.IsNullOrWhiteSpace(SelectedCollectionId))
                return;

            var sfd = new SaveFileDialog
            {
                Filter = "ESRI Shapefile (*.shp)|*.shp",
                Title = "Save Shapefile As…",
                FileName = Path.GetFileNameWithoutExtension(SelectedCollectionId) + "_copy.shp"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    _adapter.SaveAs(SelectedCollectionId!, sfd.FileName);
                    Status = $"Saved: {sfd.FileName}";
                }
                catch (Exception ex)
                {
                    Status = $"Save error: {ex.Message}";
                    MessageBox.Show(ex.Message, "Save error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private static object CoerceValue(Type? t, string text)
        {
            if (t == null || t == typeof(string)) return text;
            if (t == typeof(int) || t == typeof(long)) return int.Parse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
            if (t == typeof(double) || t == typeof(float) || t == typeof(decimal)) return double.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
            if (t == typeof(bool)) return bool.Parse(text);
            if (t == typeof(DateTime)) return DateTime.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
            return text;
        }

        private void UpdateOperatorsForAttribute()
        {
            AvailableOperators.Clear();
            if (_adapter == null || string.IsNullOrWhiteSpace(SelectedCollectionId) || string.IsNullOrWhiteSpace(FilterAttributeName))
                return;

            var info = _adapter.GetCollectionInfo(SelectedCollectionId);
            var t = info.Attributes.FirstOrDefault(a => a.Name == FilterAttributeName)?.DataType;

            IEnumerable<QueryOperator> ops;

            if (t == typeof(string) || t == null)
                ops = new[] { QueryOperator.Equal, QueryOperator.NotEqual, QueryOperator.Like, QueryOperator.NotLike, QueryOperator.Contains };
            else if (t == typeof(bool))
                ops = new[] { QueryOperator.Equal, QueryOperator.NotEqual };
            else
                ops = new[] { QueryOperator.Equal, QueryOperator.NotEqual, QueryOperator.GreaterThan, QueryOperator.GreaterThanOrEqual, QueryOperator.LessThan, QueryOperator.LessThanOrEqual };

            foreach (var op in ops) AvailableOperators.Add(new OperatorOption(op));
            SelectedOperator = AvailableOperators.FirstOrDefault();
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> _exec;
        private readonly Predicate<object?>? _can;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _exec = execute ?? throw new ArgumentNullException(nameof(execute));
            _can = canExecute;
        }

        public bool CanExecute(object? parameter) => _can?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _exec(parameter);

        public event EventHandler? CanExecuteChanged
        { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }
    }

    public sealed class OperatorOption
    {
        public OperatorOption(QueryOperator value)
        {
            Value = value;
            Label = GetDescription(value);
        }
        public QueryOperator Value { get; }
        public string Label { get; }

        private static string GetDescription(Enum e)
        {
            var fi = e.GetType().GetField(e.ToString());
            var attr = (System.ComponentModel.DescriptionAttribute?)
                       Attribute.GetCustomAttribute(fi!, typeof(System.ComponentModel.DescriptionAttribute));
            return attr?.Description ?? e.ToString();
        }
    }
}
