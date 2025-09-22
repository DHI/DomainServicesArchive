namespace DHI.Services.Samples.Physics.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using PhysUnit = DHI.Physics.Unit;

    public sealed class PhysicsViewModel : INotifyPropertyChanged
    {
        private string _searchText = "";
        private string _inputValue = "1";
        private string _resultText = "—";
        private string _dimensionText = "";
        private string _errorText = "";
        private bool _hasError;
        private int _decimals = 4;

        private UnitDisplay _selectedFrom;
        private UnitDisplay _selectedTo;

        private UnitDisplay _customFrom;
        private UnitDisplay _customTo;
        private string _customA = "1";
        private string _customB = "0";

        public ObservableCollection<UnitDisplay> AllUnits { get; } = new();
        public ObservableCollection<UnitDisplay> FromUnits { get; } = new();
        public ObservableCollection<UnitDisplay> ToUnits { get; } = new();

        public ICollectionView UnitsView { get; }

        private readonly Dictionary<(string from, string to), Func<double, double>> _custom = new();

        public PhysicsViewModel()
        {
            foreach (var u in GetAllDefaultUnits())
                AllUnits.Add(new UnitDisplay(u));

            foreach (var u in AllUnits) FromUnits.Add(u);
            UpdateToUnitsFilter(null);

            _selectedFrom = FromUnits.FirstOrDefault();
            _selectedTo = ToUnits.FirstOrDefault(u => u.DimensionEquals(_selectedFrom?.Unit)) ?? ToUnits.FirstOrDefault();

            UnitsView = CollectionViewSource.GetDefaultView(AllUnits);
            UnitsView.Filter = UnitFilter;

            ConvertCommand = new RelayCommand(_ => Convert(), _ => CanConvert());
            SwapCommand = new RelayCommand(_ => Swap(), _ => SelectedFromUnit != null && SelectedToUnit != null);
            ResetSearchCommand = new RelayCommand(_ => SearchText = "");
            RegisterCustomConversionCommand = new RelayCommand(_ => RegisterCustom(), _ => CustomFromUnit != null && CustomToUnit != null);
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value ?? ""; OnPropertyChanged(); UnitsView.Refresh(); }
        }

        public string UnitCountText => $"{AllUnits.Count:N0} units";

        public string InputValue
        {
            get => _inputValue;
            set { _inputValue = value; OnPropertyChanged(); }
        }

        public int Decimals
        {
            get => _decimals;
            set { _decimals = Math.Clamp(value, 0, 12); OnPropertyChanged(); }
        }

        public UnitDisplay SelectedFromUnit
        {
            get => _selectedFrom;
            set
            {
                if (Set(ref _selectedFrom, value))
                {
                    UpdateToUnitsFilter(value?.Unit);
                    DimensionText = value?.Unit?.Dimension?.ToString() ?? "";
                }
            }
        }

        public UnitDisplay SelectedToUnit
        {
            get => _selectedTo;
            set { Set(ref _selectedTo, value); }
        }

        public string ResultText
        {
            get => _resultText;
            private set { _resultText = value; OnPropertyChanged(); }
        }

        public string DimensionText
        {
            get => _dimensionText;
            private set { _dimensionText = value; OnPropertyChanged(); }
        }

        public string ErrorText
        {
            get => _errorText;
            private set { _errorText = value; OnPropertyChanged(); }
        }

        public bool HasError
        {
            get => _hasError;
            private set { _hasError = value; OnPropertyChanged(); }
        }

        public UnitDisplay CustomFromUnit
        {
            get => _customFrom;
            set { Set(ref _customFrom, value); }
        }

        public UnitDisplay CustomToUnit
        {
            get => _customTo;
            set { Set(ref _customTo, value); }
        }

        public string CustomA
        {
            get => _customA;
            set { _customA = value; OnPropertyChanged(); }
        }

        public string CustomB
        {
            get => _customB;
            set { _customB = value; OnPropertyChanged(); }
        }

        public ICommand ConvertCommand { get; }
        public ICommand SwapCommand { get; }
        public ICommand ResetSearchCommand { get; }
        public ICommand RegisterCustomConversionCommand { get; }

        private void UpdateToUnitsFilter(PhysUnit dimensionOwner)
        {
            ToUnits.Clear();

            IEnumerable<UnitDisplay> pick = AllUnits;
            if (dimensionOwner != null)
            {
                pick = pick.Where(u => u.DimensionEquals(dimensionOwner));
            }

            foreach (var u in pick)
                ToUnits.Add(u);

            if (!ToUnits.Contains(SelectedToUnit))
                SelectedToUnit = ToUnits.FirstOrDefault();
        }

        private bool UnitFilter(object obj)
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return true;
            var text = SearchText.Trim();
            if (obj is UnitDisplay u)
            {
                return (u.Unit.Id?.IndexOf(text, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0
                    || (u.Unit.Abbreviation?.IndexOf(text, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0
                    || (u.Unit.Description?.IndexOf(text, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0;
            }
            return false;
        }

        private bool CanConvert()
            => SelectedFromUnit != null && SelectedToUnit != null && double.TryParse(InputValue, NumberStyles.Float, CultureInfo.InvariantCulture, out _);

        private void Convert()
        {
            HasError = false;
            ErrorText = "";
            ResultText = "—";

            if (!double.TryParse(InputValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
            {
                HasError = true;
                ErrorText = "Invalid number.";
                return;
            }
            if (SelectedFromUnit == null || SelectedToUnit == null) return;

            var from = SelectedFromUnit.Unit;
            var to = SelectedToUnit.Unit;

            try
            {
                double result;
                if (_custom.TryGetValue((from.Id, to.Id), out var f))
                {
                    result = f(val);
                }
                else
                {
                    result = PhysUnit.Convert(val, from, to);
                }

                ResultText = $"{val.ToString("0." + new string('#', Decimals), CultureInfo.InvariantCulture)} {from.Abbreviation} = " +
                             $"{Math.Round(result, Decimals).ToString("0." + new string('#', Decimals), CultureInfo.InvariantCulture)} {to.Abbreviation}";
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorText = ex.Message;
            }
        }

        private void Swap()
        {
            (SelectedFromUnit, SelectedToUnit) = (SelectedToUnit, SelectedFromUnit);
            Convert();
        }

        private void RegisterCustom()
        {
            if (!double.TryParse(CustomA, NumberStyles.Float, CultureInfo.InvariantCulture, out var a))
            {
                HasError = true; ErrorText = "Invalid a (slope)."; return;
            }
            if (!double.TryParse(CustomB, NumberStyles.Float, CultureInfo.InvariantCulture, out var b))
            {
                HasError = true; ErrorText = "Invalid b (offset)."; return;
            }
            if (CustomFromUnit == null || CustomToUnit == null) return;

            _custom[(CustomFromUnit.Unit.Id, CustomToUnit.Unit.Id)] = x => a * x + b;
            HasError = false;
            ErrorText = "";
        }

        private static IEnumerable<PhysUnit> GetAllDefaultUnits()
        {
            var props = typeof(DHI.Physics.Units)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.PropertyType == typeof(PhysUnit));

            foreach (var p in props)
            {
                if (p.GetValue(null) is PhysUnit u)
                    yield return u;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        private bool Set<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value; OnPropertyChanged(name); return true;
        }
    }

    public sealed class UnitDisplay
    {
        public UnitDisplay(PhysUnit unit) => Unit = unit;
        public PhysUnit Unit { get; }
        public string Display => $"{Unit.Abbreviation}  —  {Unit.Description} ({Unit.Id})";
        public bool DimensionEquals(PhysUnit other) => other != null && Unit.Dimension == other.Dimension;

        public string Id => Unit.Id;
        public string Abbreviation => Unit.Abbreviation;
        public string Description => Unit.Description;
    }

    public sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> _exec;
        private readonly Predicate<object?>? _can;
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        { _exec = execute ?? throw new ArgumentNullException(nameof(execute)); _can = canExecute; }
        public bool CanExecute(object? parameter) => _can?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _exec(parameter);
        public event EventHandler? CanExecuteChanged
        { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }
    }

    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public static readonly BooleanToVisibilityConverter Instance = new BooleanToVisibilityConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value is bool b && b) ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
