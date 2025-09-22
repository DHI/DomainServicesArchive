using DHI.Services.Samples.TimeSeries.Composition;
using DHI.Services.Samples.TimeSeries.DomainAdapters;
using DHI.Services.Samples.TimeSeries.Helpers;
using DHI.Services.Samples.TimeSeries.ViewModels.Types;
using DHI.Services.TimeSeries;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Windows.Input;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace DHI.Services.Samples.TimeSeries.ViewModels
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private UpdatableTimeSeriesService<string, double>? _svc;
        private ClaimsPrincipal? _user;

        public MainViewModel()
        {
            BrowseFolderCommand = new RelayCommand(_ => BrowseFolder());
            BrowseFileCommand = new RelayCommand(_ => BrowseFile());
            ConnectCommand = new RelayCommand(_ => Connect(), _ => !string.IsNullOrWhiteSpace(ConnectionPath));
            ConnectToSelectedFileCommand = new RelayCommand(_ => ConnectToSelectedFile(), _ => SelectedFile != null);

            LoadIdsCommand = new RelayCommand(_ => LoadSeriesIds(), _ => IsConnected);
            LoadValuesCommand = new RelayCommand(_ => LoadValues(), _ => IsConnected && SelectedSeriesId != null);
            GetAtTimeCommand = new RelayCommand(_ => GetValueAtTime(), _ => IsConnected && SelectedSeriesId != null);
            GetInterpolatedAtTimeCommand = new RelayCommand(_ => GetInterpolatedAtTime(), _ => IsConnected && SelectedSeriesId != null);
            MinCommand = new RelayCommand(_ => Aggregate(AggregationType.Minimum), _ => IsConnected && SelectedSeriesId != null);
            MaxCommand = new RelayCommand(_ => Aggregate(AggregationType.Maximum), _ => IsConnected && SelectedSeriesId != null);
            AvgCommand = new RelayCommand(_ => Aggregate(AggregationType.Average), _ => IsConnected && SelectedSeriesId != null);
            SumCommand = new RelayCommand(_ => Aggregate(AggregationType.Sum), _ => IsConnected && SelectedSeriesId != null);

            ConnectionPath = @".\App_Data\dfs0";

            FromText = DateTime.UtcNow.AddHours(-24).ToString("yyyy-MM-dd HH:mm:ss");
            ToText = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            AtText = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        }

        // --------------- Bindables: connection ---------------
        public string ConnectionPath
        {
            get => _path;
            set { _path = value; OnPropertyChanged(); TryPopulateFolderFiles(); }
        }
        private string _path = "";

        public bool IsConnected { get => _isConn; set { _isConn = value; OnPropertyChanged(); } }
        private bool _isConn;
        public ObservableCollection<Dfs0FileItem> CandidateFiles { get; } = new();
        private Dfs0FileItem? _selectedFile;
        public Dfs0FileItem? SelectedFile { get => _selectedFile; set { _selectedFile = value; OnPropertyChanged(); } }

        // --------------- Series & time selection ---------------
        public ObservableCollection<SeriesIdItem> SeriesIds { get; } = new();
        private SeriesIdItem? _selectedSeriesId;
        public SeriesIdItem? SelectedSeriesId
        {
            get => _selectedSeriesId;
            set { _selectedSeriesId = value; OnPropertyChanged(); SuggestRangeFromSeries(); }
        }

        public string FromText { get => _from; set { _from = value; OnPropertyChanged(); } }
        public string ToText { get => _to; set { _to = value; OnPropertyChanged(); } }
        public string AtText { get => _at; set { _at = value; OnPropertyChanged(); } }
        private string _from = "", _to = "", _at = "";

        // --------------- Results ---------------
        public DataView? ValuesView { get => _valuesView; set { _valuesView = value; OnPropertyChanged(); } }
        private DataView? _valuesView;

        public string InfoText { get => _info; set { _info = value; OnPropertyChanged(); } }
        private string _info = "";

        // --------------- Errors ---------------
        public string ErrorText { get => _err; set { _err = value; HasError = !string.IsNullOrWhiteSpace(value); OnPropertyChanged(); } }
        private string _err = "";
        public bool HasError { get => _hasErr; set { _hasErr = value; OnPropertyChanged(); } }
        private bool _hasErr;

        // --------------- Commands ---------------
        public ICommand BrowseFolderCommand { get; }
        public ICommand BrowseFileCommand { get; }
        public ICommand ConnectCommand { get; }
        public ICommand ConnectToSelectedFileCommand { get; }

        public ICommand LoadIdsCommand { get; }
        public ICommand LoadValuesCommand { get; }
        public ICommand GetAtTimeCommand { get; }
        public ICommand GetInterpolatedAtTimeCommand { get; }
        public ICommand MinCommand { get; }
        public ICommand MaxCommand { get; }
        public ICommand AvgCommand { get; }
        public ICommand SumCommand { get; }

        // --------------- Actions ---------------
        private void BrowseFolder()
        {
            using var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Choose a folder containing .dfs0 files",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };
            var result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.SelectedPath))
            {
                ConnectionPath = dlg.SelectedPath;
            }
        }

        private void BrowseFile()
        {
            var dlg = new OpenFileDialog
            {
                Title = "Choose a .dfs0 file",
                Filter = "MIKE DFS0 (*.dfs0)|*.dfs0|All files (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            };
            if (dlg.ShowDialog() == true)
            {
                ConnectionPath = dlg.FileName;
            }
        }

        private void Connect()
        {
            try
            {
                ErrorText = "";
                ValuesView = null;
                SeriesIds.Clear();

                if (Directory.Exists(ConnectionPath))
                {
                    if (SelectedFile == null)
                    {
                        TryPopulateFolderFiles();
                        SelectedFile = CandidateFiles.FirstOrDefault();
                    }
                    if (SelectedFile == null)
                        throw new InvalidOperationException("No .dfs0 files found in the selected folder.");

                    _svc = CompositionRoot.CreateService(SelectedFile.FullPath);
                }
                else
                {
                    _svc = CompositionRoot.CreateService(ConnectionPath);
                    CandidateFiles.Clear();
                    SelectedFile = null;
                }

                IsConnected = true;
                LoadSeriesIds();
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
                IsConnected = false;
                _svc = null;
            }
        }

        private void ConnectToSelectedFile()
        {
            if (SelectedFile == null) return;
            try
            {
                ErrorText = "";
                ValuesView = null;
                SeriesIds.Clear();

                _svc = CompositionRoot.CreateService(SelectedFile.FullPath);
                IsConnected = true;
                LoadSeriesIds();
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
                IsConnected = false;
                _svc = null;
            }
        }

        private void TryPopulateFolderFiles()
        {
            try
            {
                CandidateFiles.Clear();
                if (Directory.Exists(ConnectionPath))
                {
                    var files = Directory.GetFiles(ConnectionPath, "*.dfs0", SearchOption.AllDirectories);
                    foreach (var f in files.OrderBy(f => f))
                        CandidateFiles.Add(new Dfs0FileItem(f));
                }
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
            }
        }

        private void LoadSeriesIds()
        {
            if (_svc == null) return;
            try
            {
                ErrorText = "";
                SeriesIds.Clear();
                foreach (var id in _svc.GetIds(_user))
                    SeriesIds.Add(new SeriesIdItem(id));

                SelectedSeriesId = SeriesIds.FirstOrDefault();
            }
            catch (Exception ex) { ErrorText = ex.Message; }
        }

        private void SuggestRangeFromSeries()
        {
            if (_svc == null || SelectedSeriesId == null) return;
            try
            {
                ErrorText = "";
                var first = _svc.GetFirstDateTime(SelectedSeriesId.Id, _user);
                var last = _svc.GetLastDateTime(SelectedSeriesId.Id, _user);

                if (last.HasValue)
                {
                    var from = first ?? last.Value.AddHours(-24);
                    FromText = from.ToString("yyyy-MM-dd HH:mm:ss");
                    ToText = last.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    AtText = last.Value.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            catch (Exception ex) { ErrorText = ex.Message; }
        }

        private bool TryParseRange(out DateTime from, out DateTime to)
        {
            if (!DateTime.TryParse(FromText, out from))
            { ErrorText = $"Could not parse From: '{FromText}' (use yyyy-MM-dd HH:mm:ss)"; to = default; return false; }
            if (!DateTime.TryParse(ToText, out to))
            { ErrorText = $"Could not parse To: '{ToText}' (use yyyy-MM-dd HH:mm:ss)"; return false; }
            if (from > to) { ErrorText = "From must be <= To."; return false; }
            return true;
        }

        private bool TryParseAt(out DateTime at)
        {
            if (!DateTime.TryParse(AtText, out at))
            { ErrorText = $"Could not parse time: '{AtText}' (use yyyy-MM-dd HH:mm:ss)"; return false; }
            return true;
        }

        private void LoadValues()
        {
            if (_svc == null || SelectedSeriesId == null) return;
            try
            {
                ErrorText = "";
                if (!TryParseRange(out var from, out var to)) return;

                var data = _svc.GetValues(SelectedSeriesId.Id, from, to, _user);
                ValuesView = TimeSeriesDataAdapter.ToDataTable(data).DefaultView;
                InfoText = $"Loaded {data.DateTimes.Count} points.";
            }
            catch (Exception ex) { ErrorText = ex.Message; }
        }

        private void GetValueAtTime()
        {
            if (_svc == null || SelectedSeriesId == null) return;
            try
            {
                ErrorText = "";
                if (!TryParseAt(out var t)) return;

                var dp = _svc.GetValue(SelectedSeriesId.Id, t, _user);
                InfoText = dp?.Value.HasValue == true
                    ? $"Exact value at {dp.DateTime:yyyy-MM-dd HH:mm:ss}: {dp.Value.Value}"
                    : $"Exact value at {t:yyyy-MM-dd HH:mm:ss}: null";
            }
            catch (Exception ex) { ErrorText = ex.Message; }
        }

        private void GetInterpolatedAtTime()
        {
            if (_svc == null || SelectedSeriesId == null) return;
            try
            {
                ErrorText = "";
                if (!TryParseAt(out var t)) return;

                var dp = _svc.GetInterpolatedValue(SelectedSeriesId.Id, t, _user);
                InfoText = $"Interpolated value at {dp.DateTime:yyyy-MM-dd HH:mm:ss}: {dp.Value?.ToString() ?? "null"}";
            }
            catch (Exception ex) { ErrorText = ex.Message; }
        }

        private void Aggregate(AggregationType agg)
        {
            if (_svc == null || SelectedSeriesId == null) return;
            try
            {
                ErrorText = "";
                if (!TryParseRange(out var from, out var to)) return;

                var val = _svc.GetAggregatedValue(SelectedSeriesId.Id, agg, from, to, _user);
                InfoText = $"{agg.DisplayName} [{from:yyyy-MM-dd HH:mm:ss} .. {to:yyyy-MM-dd HH:mm:ss}] = {(val.HasValue ? val.Value.ToString() : "null")}";
            }
            catch (Exception ex) { ErrorText = ex.Message; }
        }

        // -------------- INotifyPropertyChanged --------------
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
