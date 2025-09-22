namespace DHI.Services.Samples.Radar.ViewModels
{
    using System;
    using System.Linq;
    using DHI.Services.Rasters;
    using DHI.Services.Rasters.Radar;
    using DHI.Services.Rasters.Radar.DELIMITEDASCII;
    using DHI.Services.Rasters.Zones;
    using DHI.Services.Samples.Radar.Composition;
    using DHI.Services.Samples.Radar.DomainAdapters;
    using DHI.Services.Samples.Radar.Helpers;
    using DHI.Services.Samples.Radar.ViewModels.Types;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Security.Claims;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;

    public sealed class MainViewModel : INotifyPropertyChanged
    {
        // services
        private RadarImageService<AsciiImage>? _radar;
        private ZoneService? _zones;
        private ClaimsPrincipal? _user;

        // current raster
        private AsciiImage? _current;

        public MainViewModel()
        {
            // Radar controls
            RefreshRadarTypesCommand = new RelayCommand(_ => RefreshRadarTypes());
            ConnectRadarCommand = new RelayCommand(_ => ConnectRadar(), _ => SelectedRadarRepositoryType != null && !string.IsNullOrWhiteSpace(RadarConnectionString));
            LoadTimesCommand = new RelayCommand(_ => LoadTimes(), _ => IsRadarConnected && FromDate <= ToDate);
            LoadImageCommand = new RelayCommand(_ => LoadImage(), _ => IsRadarConnected && SelectedDateTime != default);

            // Pixel
            ComputePixelIntensityCommand = new RelayCommand(_ => ComputePixelIntensity(), _ => _current != null && PixelCol > 0 && PixelRow > 0);
            BuildPixelTimeSeriesCommand = new RelayCommand(_ => BuildPixelSeries(), _ => IsRadarConnected && PixelCol > 0 && PixelRow > 0 && FromDate <= ToDate);
            ComputePixelDepthCommand = new RelayCommand(_ => ComputePixelDepth(), _ => PixelSeries.Any());

            // Zones controls
            RefreshZoneTypesCommand = new RelayCommand(_ => RefreshZoneTypes());
            ConnectZoneCommand = new RelayCommand(_ => ConnectZones(), _ => SelectedZoneRepositoryType != null && !string.IsNullOrWhiteSpace(ZoneConnectionString));
            ComputeZoneAverageCommand = new RelayCommand(_ => ComputeZoneAvg(), _ => IsRadarConnected && IsZonesConnected && SelectedZone != null && FromDate <= ToDate);
            ComputeZoneMaxCommand = new RelayCommand(_ => ComputeZoneMax(), _ => IsRadarConnected && IsZonesConnected && SelectedZone != null && FromDate <= ToDate);
            ComputeZoneMinCommand = new RelayCommand(_ => ComputeZoneMin(), _ => IsRadarConnected && IsZonesConnected && SelectedZone != null && FromDate <= ToDate);
            ComputeZoneDepthCommand = new RelayCommand(_ => ComputeZoneDepth(), _ => IsRadarConnected && IsZonesConnected && SelectedZone != null && FromDate <= ToDate);
            BuildZoneTimeSeriesCommand = new RelayCommand(_ => BuildZoneSeries(), _ => IsRadarConnected && IsZonesConnected && SelectedZone != null && FromDate <= ToDate);
            SavePickedZoneCommand = new RelayCommand(_ => SavePickedZone(), _ => IsZonesConnected && _current != null && PixelCol > 0 && PixelRow > 0 && !string.IsNullOrWhiteSpace(NewZoneName));

            // defaults
            RefreshRadarTypes();
            RefreshZoneTypes();

            RadarConnectionString = @".\App_Data\RadarImages;PM_{datetimeFormat}.txt;yyyyMMddHH_$$$";
            ZoneConnectionString = @".\App_Data\zones.json";

            FromDate = DateTime.Today.AddDays(-1);
            ToDate = DateTime.Today;
        }

        // ---------------------- Bindables: Radar connection ----------------------
        public ObservableCollection<RepositoryTypeItem> RadarRepositoryTypes { get; } = new();
        private RepositoryTypeItem? _selectedRadarRepoType;
        public RepositoryTypeItem? SelectedRadarRepositoryType { get => _selectedRadarRepoType; set { _selectedRadarRepoType = value; OnPropertyChanged(); } }
        public string RadarConnectionString { get => _radarConn; set { _radarConn = value; OnPropertyChanged(); } }
        private string _radarConn = "";
        public bool IsRadarConnected { get => _isRadarConnected; set { _isRadarConnected = value; OnPropertyChanged(); } }
        private bool _isRadarConnected;

        // ---------------------- Bindables: Zone connection ----------------------
        public ObservableCollection<RepositoryTypeItem> ZoneRepositoryTypes { get; } = new();
        private RepositoryTypeItem? _selectedZoneRepoType;
        public RepositoryTypeItem? SelectedZoneRepositoryType { get => _selectedZoneRepoType; set { _selectedZoneRepoType = value; OnPropertyChanged(); } }
        public string ZoneConnectionString { get => _zoneConn; set { _zoneConn = value; OnPropertyChanged(); } }
        private string _zoneConn = "";
        public bool IsZonesConnected { get => _isZonesConnected; set { _isZonesConnected = value; OnPropertyChanged(); } }
        private bool _isZonesConnected;

        // ---------------------- Time & image ----------------------
        public DateTime FromDate { get => _from; set { _from = value; OnPropertyChanged(); } }
        public DateTime ToDate { get => _to; set { _to = value; OnPropertyChanged(); } }
        private DateTime _from, _to;

        public ObservableCollection<DateTime> DateTimes { get; } = new();
        public DateTime SelectedDateTime { get => _selected; set { _selected = value; OnPropertyChanged(); } }
        private DateTime _selected;

        public BitmapSource? CurrentImage { get => _currentImage; set { _currentImage = value; OnPropertyChanged(); } }
        private BitmapSource? _currentImage;
        public string ImageInfo { get => _imageInfo; set { _imageInfo = value; OnPropertyChanged(); } }
        private string _imageInfo = "";

        // ---------------------- Pixel picking ----------------------
        public bool IsPicking { get => _isPicking; set { _isPicking = value; OnPropertyChanged(); } }
        private bool _isPicking;
        public int PixelCol { get => _col; set { _col = value; OnPropertyChanged(); } }
        public int PixelRow { get => _row; set { _row = value; OnPropertyChanged(); } }
        private int _col = 1, _row = 1;
        public string PixelIntensityText { get => _pixText; set { _pixText = value; OnPropertyChanged(); } }
        private string _pixText = "";
        public ObservableCollection<TimeSample> PixelSeries { get; } = new();
        public string PixelDepthText { get => _depthText; set { _depthText = value; OnPropertyChanged(); } }
        private string _depthText = "";

        // Crosshair overlay
        public bool ShowCrosshair { get => _showCross; set { _showCross = value; OnPropertyChanged(); } }
        private bool _showCross;
        public double CrossX1 { get => _cx1; set { _cx1 = value; OnPropertyChanged(); } }
        private double _cx1;
        public double CrossX2 { get => _cx2; set { _cx2 = value; OnPropertyChanged(); } }
        private double _cx2 = 99999;
        public double CrossY1 { get => _cy1; set { _cy1 = value; OnPropertyChanged(); } }
        private double _cy1;
        public double CrossY2 { get => _cy2; set { _cy2 = value; OnPropertyChanged(); } }
        private double _cy2 = 99999;
        public double CrossX { get => _cx; set { _cx = value; OnPropertyChanged(); } }
        private double _cx;
        public double CrossY { get => _cy; set { _cy = value; OnPropertyChanged(); } }
        private double _cy;

        // ---------------------- Zones ----------------------
        public ObservableCollection<ZoneItem> Zones { get; } = new();
        private ZoneItem? _selectedZone;
        public ZoneItem? SelectedZone
        {
            get => _selectedZone;
            set
            {
                _selectedZone = value;
                OnPropertyChanged();
                UpdateZoneOverlay();
            }
        }

        public bool ShowZoneOverlay { get => _showZoneOverlay; set { _showZoneOverlay = value; OnPropertyChanged(); UpdateZoneOverlay(); } }
        private bool _showZoneOverlay;

        public BitmapSource? ZoneOverlay { get => _zoneOverlay; set { _zoneOverlay = value; OnPropertyChanged(); } }
        private BitmapSource? _zoneOverlay;

        public string ZoneResultText { get => _zoneText; set { _zoneText = value; OnPropertyChanged(); } }
        private string _zoneText = "";
        public ObservableCollection<TimeSample> ZoneSeries { get; } = new();

        public string NewZoneName { get => _newZoneName; set { _newZoneName = value; OnPropertyChanged(); } }
        private string _newZoneName = "";

        // ---------------------- Errors ----------------------
        public string ErrorText { get => _err; set { _err = value; HasError = !string.IsNullOrWhiteSpace(value); OnPropertyChanged(); } }
        private string _err = "";
        public bool HasError { get => _hasErr; set { _hasErr = value; OnPropertyChanged(); } }
        private bool _hasErr;

        // ---------------------- Commands ----------------------
        public ICommand RefreshRadarTypesCommand { get; }
        public ICommand ConnectRadarCommand { get; }
        public ICommand LoadTimesCommand { get; }
        public ICommand LoadImageCommand { get; }
        public ICommand ComputePixelIntensityCommand { get; }
        public ICommand BuildPixelTimeSeriesCommand { get; }
        public ICommand ComputePixelDepthCommand { get; }

        public ICommand RefreshZoneTypesCommand { get; }
        public ICommand ConnectZoneCommand { get; }
        public ICommand ComputeZoneAverageCommand { get; }
        public ICommand ComputeZoneMaxCommand { get; }
        public ICommand ComputeZoneMinCommand { get; }
        public ICommand ComputeZoneDepthCommand { get; }
        public ICommand BuildZoneTimeSeriesCommand { get; }
        public ICommand SavePickedZoneCommand { get; }

        // ---------------------- Actions ----------------------
        private void RefreshRadarTypes()
        {
            RadarRepositoryTypes.Clear();
            foreach (var t in CompositionRoot.GetRadarRepositoryTypes())
                RadarRepositoryTypes.Add(new RepositoryTypeItem(t));
            SelectedRadarRepositoryType = RadarRepositoryTypes.FirstOrDefault();
        }

        private void ConnectRadar()
        {
            try
            {
                ErrorText = "";
                _radar = CompositionRoot.CreateRadarService(SelectedRadarRepositoryType!.Type.FullName!, RadarConnectionString);
                IsRadarConnected = true;

                var last = _radar.LastDateTime(_user);
                FromDate = last.AddHours(-1);
                ToDate = last;

                LoadTimes();
            }
            catch (Exception ex) { ErrorText = ex.Message; IsRadarConnected = false; }
        }

        private void LoadTimes()
        {
            if (_radar == null) return;
            try
            {
                ErrorText = "";
                DateTimes.Clear();
                foreach (var dt in _radar.GetDateTimes(FromDate, ToDate, _user))
                    DateTimes.Add(dt);
                SelectedDateTime = DateTimes.LastOrDefault();
            }
            catch (Exception ex) { ErrorText = ex.Message; }
        }

        private void LoadImage()
        {
            if (_radar == null || SelectedDateTime == default) return;
            try
            {
                ErrorText = "";
                _current = _radar.Get(SelectedDateTime, _user) as AsciiImage;
                if (_current == null) { ErrorText = "Image not found at selected time."; return; }

                using var bmp = _current.ToBitmap();
                CurrentImage = BitmapSourceAdapter.ToBitmapSource(bmp);

                ImageInfo =
                    $"Time: {_current.DateTime:yyyy-MM-dd HH:mm:ss} | Size: {_current.Size.Width}×{_current.Size.Height} | Pixels: {_current.Values.Count:N0} | PixelType: {_current.PixelValueType}";

                ShowCrosshair = false;
                PixelIntensityText = "";
                PixelSeries.Clear();
                PixelDepthText = "";

                UpdateZoneOverlay();
            }
            catch (Exception ex) { ErrorText = ex.Message; }
        }

        public void SetPickedPixel(int col, int row, double clickX, double clickY, double left, double top, double drawW, double drawH)
        {
            PixelCol = col;
            PixelRow = row;
            IsPicking = false;

            CrossX = clickX; CrossY = clickY;
            CrossX1 = left; CrossX2 = left + drawW;
            CrossY1 = top; CrossY2 = top + drawH;
            ShowCrosshair = true;
        }

        private void ComputePixelIntensity()
        {
            if (_current == null) return;
            try
            {
                ErrorText = "";
                var val = _current.GetIntensity(new Pixel(PixelCol, PixelRow));
                PixelIntensityText = $"I({PixelCol},{PixelRow}) = {val.ToString("0.###", CultureInfo.InvariantCulture)}";
            }
            catch (Exception ex) { ErrorText = ex.Message; }
        }

        private void BuildPixelSeries()
        {
            if (_radar == null) return;
            try
            {
                ErrorText = "";
                PixelSeries.Clear();

                var map = _radar.Get(FromDate, ToDate, _user);
                foreach (var kv in map)
                {
                    if (kv.Value is not IRadarImage img) continue;
                    var val = img.GetIntensity(new Pixel(PixelCol, PixelRow));
                    PixelSeries.Add(new TimeSample { Time = kv.Key, Value = val });
                }
            }
            catch (Exception ex) { ErrorText = ex.Message; }
        }

        private void ComputePixelDepth()
        {
            if (!PixelSeries.Any()) return;
            try
            {
                ErrorText = "";
                var ordered = PixelSeries.OrderBy(s => s.Time).ToList();
                double depth = 0;
                for (int i = 1; i < ordered.Count; i++)
                {
                    var dtHours = (ordered[i].Time - ordered[i - 1].Time).TotalHours;
                    var meanI = 0.5 * (ordered[i].Value + ordered[i - 1].Value);
                    depth += dtHours * meanI;
                }
                PixelDepthText = $"Depth for pixel ({PixelCol},{PixelRow}) across [{FromDate} .. {ToDate}] = {depth:0.###} (mm)";
            }
            catch (Exception ex) { ErrorText = ex.Message; }
        }

        // ---------------------- Zones: connect & list ----------------------
        private void RefreshZoneTypes()
        {
            ZoneRepositoryTypes.Clear();
            foreach (var t in CompositionRoot.GetZoneRepositoryTypes())
                ZoneRepositoryTypes.Add(new RepositoryTypeItem(t));
            SelectedZoneRepositoryType = ZoneRepositoryTypes.FirstOrDefault();
        }

        private void ConnectZones()
        {
            try
            {
                ErrorText = "";
                _zones = CompositionRoot.CreateZoneService(SelectedZoneRepositoryType!.Type.FullName!, ZoneConnectionString);
                IsZonesConnected = true;

                LoadZones();
            }
            catch (Exception ex) { ErrorText = ex.Message; IsZonesConnected = false; }
        }

        private void LoadZones()
        {
            if (_zones == null) return;
            try
            {
                Zones.Clear();
                foreach (var z in _zones.GetAll(_user))
                    Zones.Add(new ZoneItem(z));
                SelectedZone = Zones.FirstOrDefault();
            }
            catch (Exception ex) { ErrorText = ex.Message; }
        }

        private void UpdateZoneOverlay()
        {
            try
            {
                if (!ShowZoneOverlay || SelectedZone == null || _current == null)
                {
                    ZoneOverlay = null;
                    return;
                }

                var zone = SelectedZone.Zone;
                if (zone.ImageSize != _current.Size)
                    zone.ImageSize = _current.Size;

                using Bitmap bmp = zone.ToBitmap(Color.FromArgb(255, 30, 144, 255), Color.FromArgb(0, 0, 0, 0));
                ZoneOverlay = BitmapSourceAdapter.ToBitmapSource(bmp);
            }
            catch
            {
                ZoneOverlay = null;
            }
        }

        // ---------------------- Zones: analytics ----------------------
        private void ComputeZoneAvg()
        {
            if (_radar == null || SelectedZone == null) return;
            try
            {
                var avg = _radar.GetAverageIntensity(SelectedZone.Zone, FromDate, ToDate, _user);
                ZoneResultText = $"Zone average intensity: {avg:0.###}";
            }
            catch (Exception ex) { ZoneResultText = $"Average failed: {ex.Message}"; }
        }

        private void ComputeZoneMax()
        {
            if (_radar == null || SelectedZone == null) return;
            try
            {
                var v = _radar.GetMaxIntensity(SelectedZone.Zone, FromDate, ToDate, _user);
                ZoneResultText = $"Zone max intensity: {v:0.###}";
            }
            catch (Exception ex) { ZoneResultText = $"Max failed: {ex.Message}"; }
        }

        private void ComputeZoneMin()
        {
            if (_radar == null || SelectedZone == null) return;
            try
            {
                var v = _radar.GetMinIntensity(SelectedZone.Zone, FromDate, ToDate, _user);
                ZoneResultText = $"Zone min intensity: {v:0.###}";
            }
            catch (Exception ex) { ZoneResultText = $"Min failed: {ex.Message}"; }
        }

        private void ComputeZoneDepth()
        {
            if (_radar == null || SelectedZone == null) return;
            try
            {
                var v = _radar.GetDepth(SelectedZone.Zone, FromDate, ToDate, _user);
                ZoneResultText = $"Zone depth: {v:0.###} (mm)";
            }
            catch (Exception ex) { ZoneResultText = $"Depth failed: {ex.Message}"; }
        }

        private void BuildZoneSeries()
        {
            if (_radar == null || SelectedZone == null) return;
            try
            {
                ZoneSeries.Clear();
                var s = _radar.GetIntensities(SelectedZone.Zone, FromDate, ToDate, _user);
                foreach (var kv in s)
                    ZoneSeries.Add(new TimeSample { Time = kv.Key, Value = kv.Value });
            }
            catch (Exception ex) { ZoneResultText = $"Series failed: {ex.Message}"; }
        }

        // ---------------------- Zones: create & save from picked pixel ----------------------
        private void SavePickedZone()
        {
            if (_zones == null || _current == null) return;
            try
            {
                ErrorText = "";
                var z = new Zone(Guid.NewGuid().ToString("N"), NewZoneName, ZoneType.Point)
                {
                    ImageSize = _current.Size
                };
                z.PixelWeights.Add(new PixelWeight(new Pixel(PixelCol, PixelRow), new Weight(1.0)));

                _zones.Add(z, _user);

                LoadZones();
                SelectedZone = Zones.FirstOrDefault(zi => zi.Zone.Id == z.Id);
                ZoneResultText = $"Saved zone '{z.Name}' at ({PixelCol},{PixelRow}).";
                NewZoneName = "";
                UpdateZoneOverlay();
            }
            catch (Exception ex) { ErrorText = ex.Message; }
        }

        // ---------------------- INotifyPropertyChanged ----------------------
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
