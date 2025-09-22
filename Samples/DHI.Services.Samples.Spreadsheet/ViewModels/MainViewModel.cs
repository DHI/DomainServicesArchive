namespace DHI.Services.Samples.Spreadsheet.ViewModels
{
    using DHI.Services.Samples.Spreadsheet.Composition;
    using DHI.Services.Samples.Spreadsheet.DomainAdapters;
    using DHI.Services.Samples.Spreadsheet.Helpers;
    using DHI.Services.Samples.Spreadsheet.ViewModels.Types;
    using DHI.Services.Spreadsheets;
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Security.Claims;
    using System.Windows.Input;
    using Range = Spreadsheets.Range;

    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private SpreadsheetService<string>? _svc;
        private ClaimsPrincipal? _user;
        private string? _singleFileId;
        public ICommand BrowseFolderCommand { get; }
        public ICommand BrowseFileCommand { get; }

        public MainViewModel()
        {
            RefreshTypesCommand = new RelayCommand(_ => RefreshTypes());
            ConnectCommand = new RelayCommand(_ => Connect(), _ => SelectedRepositoryType != null);
            LoadUsedRangeCommand = new RelayCommand(_ => LoadUsedRange(), _ => IsConnected && SelectedSpreadsheet != null && !string.IsNullOrWhiteSpace(SelectedSheetName));
            LoadNamedRangeCommand = new RelayCommand(_ => LoadNamedRange(), _ => IsConnected && SelectedSpreadsheet != null && !string.IsNullOrWhiteSpace(SelectedSheetName) && !string.IsNullOrWhiteSpace(NamedRange));
            GetCellValueCommand = new RelayCommand(_ => GetCellValue(), _ => IsConnected && SelectedSpreadsheet != null && !string.IsNullOrWhiteSpace(SelectedSheetName) && CellRow >= 1 && CellCol >= 1);
            GetRangeCommand = new RelayCommand(_ => GetRange(), _ => IsConnected && SelectedSpreadsheet != null && !string.IsNullOrWhiteSpace(SelectedSheetName) && ULRow >= 1 && ULCol >= 1 && LRRow >= ULRow && LRCol >= ULCol);
            BrowseFolderCommand = new RelayCommand(_ => BrowseFolder());
            BrowseFileCommand = new RelayCommand(_ => BrowseFile());

            RefreshTypes();

            ConnectionString = @".\App_Data";
        }

        // ---------------- Connection ----------------
        public ObservableCollection<RepositoryTypeItem> RepositoryTypes { get; } = new();
        private RepositoryTypeItem? _selectedRepoType;
        public RepositoryTypeItem? SelectedRepositoryType { get => _selectedRepoType; set { _selectedRepoType = value; OnPropertyChanged(); } }
        public string ConnectionString { get => _conn; set { _conn = value; OnPropertyChanged(); } }
        private string _conn = "";
        public bool IsConnected { get => _isConn; set { _isConn = value; OnPropertyChanged(); } }
        private bool _isConn;

        // ---------------- Data / selection ----------------
        public ObservableCollection<SpreadsheetItem> Spreadsheets { get; } = new();
        private SpreadsheetItem? _selectedSpreadsheet;
        public SpreadsheetItem? SelectedSpreadsheet
        {
            get => _selectedSpreadsheet;
            set
            {
                _selectedSpreadsheet = value;
                OnPropertyChanged();
                PopulateSheetAndRanges();
            }
        }

        public ObservableCollection<string> SheetNames { get; } = new();
        private string _selectedSheetName = "";
        public string SelectedSheetName { get => _selectedSheetName; set { _selectedSheetName = value; OnPropertyChanged(); } }

        public ObservableCollection<string> DefinedNames { get; } = new();

        // Inputs
        public string NamedRange { get => _namedRange; set { _namedRange = value; OnPropertyChanged(); } }
        private string _namedRange = "";

        public int CellRow { get => _cellRow; set { _cellRow = value; OnPropertyChanged(); } }
        public int CellCol { get => _cellCol; set { _cellCol = value; OnPropertyChanged(); } }
        private int _cellRow = 1, _cellCol = 1;

        public int ULRow { get => _ulRow; set { _ulRow = value; OnPropertyChanged(); } }
        public int ULCol { get => _ulCol; set { _ulCol = value; OnPropertyChanged(); } }
        public int LRRow { get => _lrRow; set { _lrRow = value; OnPropertyChanged(); } }
        public int LRCol { get => _lrCol; set { _lrCol = value; OnPropertyChanged(); } }
        private int _ulRow = 1, _ulCol = 1, _lrRow = 1, _lrCol = 1;

        // Results
        public DataView? ValuesView { get => _valuesView; set { _valuesView = value; OnPropertyChanged(); } }
        private DataView? _valuesView;

        public DataView? FormatsView { get => _formatsView; set { _formatsView = value; OnPropertyChanged(); } }
        private DataView? _formatsView;

        public string CellValueText { get => _cellVal; set { _cellVal = value; OnPropertyChanged(); } }
        private string _cellVal = "";

        // Errors
        public string ErrorText { get => _err; set { _err = value; HasError = !string.IsNullOrWhiteSpace(value); OnPropertyChanged(); } }
        private string _err = "";
        public bool HasError { get => _hasErr; set { _hasErr = value; OnPropertyChanged(); } }
        private bool _hasErr;

        // ---------------- Commands ----------------
        public ICommand RefreshTypesCommand { get; }
        public ICommand ConnectCommand { get; }
        public ICommand LoadUsedRangeCommand { get; }
        public ICommand LoadNamedRangeCommand { get; }
        public ICommand GetCellValueCommand { get; }
        public ICommand GetRangeCommand { get; }

        // ---------------- Actions ----------------
        private void RefreshTypes()
        {
            RepositoryTypes.Clear();
            foreach (var t in CompositionRoot.GetSpreadsheetRepositoryTypes())
                RepositoryTypes.Add(new RepositoryTypeItem(t));
            SelectedRepositoryType = RepositoryTypes.FirstOrDefault();
        }

        private void Connect()
        {
            try
            {
                ErrorText = "";
                _svc = CompositionRoot.CreateSpreadsheetService(
                           SelectedRepositoryType!.Type.FullName!,
                           ConnectionString,
                           out _singleFileId);
                IsConnected = true;
                LoadSpreadsheets();
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
                IsConnected = false;
            }
        }

        private void LoadSpreadsheets()
        {
            if (_svc == null) return;
            try
            {
                ErrorText = "";
                Spreadsheets.Clear();
                foreach (var s in _svc.GetAll(_user))
                    Spreadsheets.Add(new SpreadsheetItem(s));

                if (!string.IsNullOrWhiteSpace(_singleFileId))
                {
                    var only = Spreadsheets.FirstOrDefault(
                        x => string.Equals(x.Id, _singleFileId, StringComparison.OrdinalIgnoreCase));

                    if (only != null)
                    {
                        Spreadsheets.Clear();
                        Spreadsheets.Add(only);
                    }
                    else
                    {
                        ErrorText = $"Selected file '{_singleFileId}' was not found inside the chosen root.";
                    }
                }

                SelectedSpreadsheet = Spreadsheets.FirstOrDefault();
            }
            catch (Exception ex) { ErrorText = ex.Message; }
        }

        private void PopulateSheetAndRanges()
        {
            SheetNames.Clear();
            DefinedNames.Clear();
            ValuesView = null;
            FormatsView = null;
            CellValueText = "";

            var s = SelectedSpreadsheet?.Spreadsheet;
            if (s == null) return;

            if (s.Metadata.TryGetValue("SheetNames", out var sheetsObj) && sheetsObj is IEnumerable list1)
            {
                foreach (var o in list1) if (o is string n) SheetNames.Add(n);
            }
            if (s.Metadata.TryGetValue("DefinedNames", out var dnObj) && dnObj is IEnumerable list2)
            {
                foreach (var o in list2) if (o is string n) DefinedNames.Add(n);
            }

            SelectedSheetName = SheetNames.FirstOrDefault() ?? "";
        }

        private void LoadUsedRange()
        {
            if (_svc == null || SelectedSpreadsheet == null || string.IsNullOrWhiteSpace(SelectedSheetName)) return;
            try
            {
                ErrorText = "";
                var data = _svc.GetUsedRange(SelectedSpreadsheet.Id, SelectedSheetName, _user);
                var formats = _svc.GetUsedRangeFormats(SelectedSpreadsheet.Id, SelectedSheetName, _user);

                ValuesView = ObjectArrayAdapter.ToDataTable(data).DefaultView;
                FormatsView = ObjectArrayAdapter.ToDataTable<DHI.Services.Spreadsheets.CellFormat>(formats).DefaultView;
            }
            catch (Exception ex) { ErrorText = ex.Message; }
        }


        private void LoadNamedRange()
        {
            if (_svc == null || SelectedSpreadsheet == null || string.IsNullOrWhiteSpace(SelectedSheetName) || string.IsNullOrWhiteSpace(NamedRange)) return;
            try
            {
                ErrorText = "";
                var data = _svc.GetNamedRange(SelectedSpreadsheet.Id, SelectedSheetName, NamedRange, _user);
                ValuesView = ObjectArrayAdapter.ToDataTable(data).DefaultView;
                FormatsView = null; // (Formats for named range not fetched here)
            }
            catch (Exception ex) { ErrorText = ex.Message; }
        }

        private void GetCellValue()
        {
            if (_svc == null || SelectedSpreadsheet == null || string.IsNullOrWhiteSpace(SelectedSheetName)) return;
            try
            {
                ErrorText = "";
                // The service uses 1-based row/col in our UI. Its provider’s Cell usually expects 1-based too.
                var val = _svc.GetCellValue(SelectedSpreadsheet.Id, SelectedSheetName, new Cell(CellRow, CellCol), _user);
                CellValueText = $"R{CellRow}C{CellCol} = {val ?? "null"}";
            }
            catch (Exception ex) { ErrorText = ex.Message; }
        }

        private void GetRange()
        {
            if (_svc == null || SelectedSpreadsheet == null || string.IsNullOrWhiteSpace(SelectedSheetName)) return;
            try
            {
                ErrorText = "";
                var data = _svc.GetRange(SelectedSpreadsheet.Id, SelectedSheetName,
                                         new Range(new Cell(ULRow, ULCol), new Cell(LRRow, LRCol)), _user);
                ValuesView = ObjectArrayAdapter.ToDataTable(data).DefaultView;
                FormatsView = null;
            }
            catch (Exception ex) { ErrorText = ex.Message; }
        }

        private void BrowseFolder()
        {
            using var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Choose a folder with .xlsx files",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };
            var result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.SelectedPath))
            {
                ConnectionString = dlg.SelectedPath;
            }
        }

        private void BrowseFile()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Choose an .xlsx file",
                Filter = "Excel Workbook (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            };
            if (dlg.ShowDialog() == true)
            {
                ConnectionString = dlg.FileName;
            }
        }


        // -------------- INotifyPropertyChanged --------------
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
