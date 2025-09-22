namespace DHI.Services.Samples.Map.ViewModels
{
    using DHI.Services.Samples.Map.Composition;
    using DHI.Services.Samples.Map.DomainAdapters;
    using DHI.Services.Samples.Map.Helpers;
    using DHI.Spatial;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;
    using MessageBox = System.Windows.MessageBox;
    using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
    using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

    public class DfsuViewModel : INotifyPropertyChanged
    {
        private MapRuntime? _runtime;
        private MapPlaygroundAdapter? _adapter;

        private string? _selectedPath;
        private string? _selectedSourceId;
        private string? _selectedStyleId;
        private string _styleCode = "Default";
        private string _item = "1";
        private DateTime? _selectedTime;
        private string _crs = "EPSG:3857";

        private int _width = 900;
        private int _height = 600;

        private double _minX = -20037508;
        private double _minY = -20037508;
        private double _maxX = 20037508;
        private double _maxY = 20037508;

        private bool _includeVector;
        private string _vectorColor = "#000000";
        private string _isoline = "None";
        private string _coloringType = "DiscreteColoring";
        private string _shadingType = "ShadedContour";
        private int _vectorMaxLength = 10;

        private BitmapImage? _mapImage;
        private string _status = "Pick a DFSU file or a folder…";

        public ObservableCollection<string> SourceIds { get; } = new();
        public ObservableCollection<string> StyleIds { get; } = new();
        public ObservableCollection<DateTime> TimeSteps { get; } = new();

        public ICommand BrowseFileCommand { get; }
        public ICommand BrowseFolderCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand RenderCommand { get; }
        public ICommand SaveImageCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public DfsuViewModel()
        {
            BrowseFileCommand = new RelayCommand(_ => BrowseFile());
            BrowseFolderCommand = new RelayCommand(_ => BrowseFolder());
            ReloadCommand = new RelayCommand(_ => Reload(), _ => _adapter != null);
            RenderCommand = new RelayCommand(_ => Render(), _ => CanRender());
            SaveImageCommand = new RelayCommand(_ => SaveImage(), _ => MapImage != null);
        }

        #region Bindables (same as DFS2)

        public string? SelectedPath
        {
            get => _selectedPath;
            set { _selectedPath = value; OnPropertyChanged(); }
        }

        public string? SelectedSourceId
        {
            get => _selectedSourceId;
            set { _selectedSourceId = value; OnPropertyChanged(); LoadTimeAxisAndBounds(); }
        }

        public string? SelectedStyleId
        {
            get => _selectedStyleId;
            set { _selectedStyleId = value; OnPropertyChanged(); }
        }

        public string StyleCode { get => _styleCode; set { _styleCode = value; OnPropertyChanged(); } }
        public string Item { get => _item; set { _item = value; OnPropertyChanged(); } }
        public DateTime? SelectedTime { get => _selectedTime; set { _selectedTime = value; OnPropertyChanged(); } }
        public string CRS { get => _crs; set { _crs = value; OnPropertyChanged(); } }
        public int ImageWidth { get => _width; set { _width = Math.Max(100, value); OnPropertyChanged(); } }
        public int ImageHeight { get => _height; set { _height = Math.Max(100, value); OnPropertyChanged(); } }
        public double MinX { get => _minX; set { _minX = value; OnPropertyChanged(); } }
        public double MinY { get => _minY; set { _minY = value; OnPropertyChanged(); } }
        public double MaxX { get => _maxX; set { _maxX = value; OnPropertyChanged(); } }
        public double MaxY { get => _maxY; set { _maxY = value; OnPropertyChanged(); } }
        public bool IncludeVector { get => _includeVector; set { _includeVector = value; OnPropertyChanged(); } }
        public string VectorColor { get => _vectorColor; set { _vectorColor = value; OnPropertyChanged(); } }
        public string Isoline { get => _isoline; set { _isoline = value; OnPropertyChanged(); } }
        public string ColoringType { get => _coloringType; set { _coloringType = value; OnPropertyChanged(); } }
        public string ShadingType { get => _shadingType; set { _shadingType = value; OnPropertyChanged(); } }
        public int VectorMaxLength { get => _vectorMaxLength; set { _vectorMaxLength = Math.Max(0, value); OnPropertyChanged(); } }
        public BitmapImage? MapImage { get => _mapImage; set { _mapImage = value; OnPropertyChanged(); } }
        public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }

        #endregion

        #region Actions (different filters + DFSU wire)

        private void BrowseFile()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "MIKE DFSU (*.dfsu)|*.dfsu",
                Title = "Open DFSU File"
            };
            if (dlg.ShowDialog() == true)
            {
                SelectedPath = dlg.FileName;
                Wire(SelectedPath!);
                Reload();
            }
        }

        private void BrowseFolder()
        {
            using var dlg = new System.Windows.Forms.FolderBrowserDialog { Description = "Pick a folder with .dfsu files" };
            var result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                SelectedPath = dlg.SelectedPath;
                Wire(SelectedPath!);
                Reload();
            }
        }

        private void Wire(string path)
        {
            try
            {
                _runtime = CompositionRootDfsu.Wire(path);
                _adapter = new MapPlaygroundAdapter(_runtime);
                Status = _runtime.HasStyles
                    ? $"Map provider wired (styles: {_runtime.StylesPath})"
                    : "Map provider wired (no styles.json found, using inline style code)";
            }
            catch (Exception ex)
            {
                _adapter = null;
                Status = $"Error wiring: {ex.Message}";
                MessageBox.Show(ex.Message, "Wire error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Reload()
        {
            if (_adapter == null) return;

            SourceIds.Clear();
            foreach (var id in _adapter.ListSourceIds())
                SourceIds.Add(id);

            SelectedSourceId = SourceIds.FirstOrDefault();

            StyleIds.Clear();
            foreach (var sid in _adapter.ListStyleIds())
                StyleIds.Add(sid);

            SelectedStyleId = StyleIds.FirstOrDefault();
        }

        private void LoadTimeAxisAndBounds()
        {
            if (_adapter == null || string.IsNullOrWhiteSpace(SelectedSourceId)) return;

            try
            {
                TimeSteps.Clear();
                foreach (var t in _adapter.GetDateTimes(SelectedSourceId))
                    TimeSteps.Add(t);
                SelectedTime = TimeSteps.FirstOrDefault();

                var bb = _adapter.TryGetLayerBounds(SelectedSourceId);
                if (bb.HasValue)
                {
                    MinX = bb.Value.Xmin;
                    MinY = bb.Value.Ymin;
                    MaxX = bb.Value.Xmax;
                    MaxY = bb.Value.Ymax;
                }

                Status = $"Loaded '{SelectedSourceId}'. Time steps: {TimeSteps.Count}.";
            }
            catch (Exception ex)
            {
                Status = $"Error reading info: {ex.Message}";
            }
        }

        private bool CanRender()
            => _adapter != null && !string.IsNullOrWhiteSpace(SelectedSourceId) && ImageWidth > 0 && ImageHeight > 0;

        private void Render()
        {
            if (_adapter == null || string.IsNullOrWhiteSpace(SelectedSourceId)) return;

            try
            {
                var bbox = new BoundingBox(MinX, MinY, MaxX, MaxY);

                var p = new Parameters
                {
                    { "includevector", IncludeVector.ToString() },
                    { "vectorcolor",   VectorColor },
                    { "vectormaxlength", VectorMaxLength.ToString(CultureInfo.InvariantCulture) },
                    { "isoline",       Isoline },
                    { "coloringtype",  ColoringType },
                    { "shadingtype",   ShadingType },
                };

                var style = _runtime!.HasStyles && !string.IsNullOrWhiteSpace(SelectedStyleId)
                    ? SelectedStyleId!
                    : StyleCode;

                var bmp = _adapter.Render(style, CRS, bbox, ImageWidth, ImageHeight,
                                          SelectedSourceId!, SelectedTime, Item, p);

                MapImage = SkiaWpf.ToBitmapImage(bmp);
                Status = $"Rendered {ImageWidth}x{ImageHeight} at {SelectedTime?.ToString("u") ?? "(t0)"}";
            }
            catch (Exception ex)
            {
                Status = $"Render error: {ex.Message}";
                MessageBox.Show(ex.Message, "Render error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveImage()
        {
            if (MapImage == null) return;

            var sfd = new SaveFileDialog
            {
                Filter = "PNG Image (*.png)|*.png",
                Title = "Save Map Image",
                FileName = (SelectedSourceId ?? "map") + ".png"
            };
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using var fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(MapImage));
                    encoder.Save(fs);
                    Status = $"Saved: {sfd.FileName}";
                }
                catch (Exception ex)
                {
                    Status = $"Save error: {ex.Message}";
                }
            }
        }

        #endregion

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
