using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DataGridGroupAndSort
{
    public enum Group
    {
        Today = 0,
        Yesterday = 1,
        EarlierThisWeek = 2,
        LastWeek = 3,
        EarlierThisMonth = 4,
        LastMonth = 5,
        EarlierThisYear = 6,
        ALongTimeAgo = 7 
    }
    
    public class File : INotifyPropertyChanged
    {
        private string _name;
        private DateTime _date;
        private float _size;

        public string Name
        {
            get => _name;
            set {
                _name = value;
                OnPropertyChanged();
            }
        }

        public DateTime Date
        {
            get => _date;
            set {
                _date = value;
                OnPropertyChanged();
            }
        }

        public float Size
        {
            get => _size;
            set {
                _size = value;
                OnPropertyChanged();
            }
        }

        public Group Group
        {
            get
            {
                var today = DateTime.Today;

                var sundayLastWeek = today.AddDays(-(int)today.DayOfWeek - 7);
                    
                if (today.Date == _date.Date) return Group.Today;
                if (today.Subtract(_date).TotalDays <= 1) return Group.Yesterday;
                if (today.Subtract(_date).TotalDays <= 7 && _date.DayOfWeek < today.DayOfWeek) return Group.EarlierThisWeek;
                if (today.Year == _date.Year && today.Month == _date.Month && _date >= sundayLastWeek) return Group.LastWeek;
                if (today.Year == _date.Year && today.Month == _date.Month) return Group.EarlierThisMonth;
                if (today.Year == _date.Year && today.Month == _date.Month + 1) return Group.LastMonth;
                if (today.Year == _date.Year) return Group.EarlierThisYear;
                return Group.ALongTimeAgo;
            }
        }

        public File(string name, DateTime date, float size)
        {
            _name = name;
            _date = date;
            _size = size;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<File> Files { get; } = new ();

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
            "Filter", typeof(string), typeof(MainWindow), new PropertyMetadata(default(string), FilterChanged));
        
        private static void FilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var collectionView = (d as MainWindow)?.FilesDataGrid?.ItemsSource as ListCollectionView;
            if (collectionView == null) throw new ArgumentException("FilterChanged expected valid collection view!");
            
            using (collectionView.DeferRefresh()) {
                collectionView.SortDescriptions.Clear();
                collectionView.SortDescriptions.Add(new SortDescription("Group", ListSortDirection.Ascending));
            }
            collectionView.Refresh();
        }

        public string Filter
        {
            get => (string)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }
        
        public MainWindow()
        {
            InitializeComponent();

            var now = DateTime.Now;
            
            Files.Add(new File("Apple", now, 100));
            Files.Add(new File("Pear", now.AddDays(-1), 23));
            Files.Add(new File("Cranberry", now.AddDays(-2), 29));
            Files.Add(new File("Peach", now.AddDays(-3), 50));
            Files.Add(new File("Blueberry", now.AddDays(-4), 76));
            Files.Add(new File("Coconut", now.AddDays(-5), 89));
            Files.Add(new File("Durian", now.AddDays(-6), 93));
            Files.Add(new File("Conkerberry", now.AddDays(-7), 48));
            Files.Add(new File("Honeydew", now.AddDays(-8), 68));
            Files.Add(new File("Canistel", now.AddMonths(-1), 18));
            Files.Add(new File("Naranjilla", now.AddMonths(-2), 25));
            Files.Add(new File("Grapple", now.AddMonths(-3), 56));
            Files.Add(new File("Naranjilla", now.AddMonths(-4), 36));
            Files.Add(new File("Mountain pepper", now.AddMonths(-5), 41));
            Files.Add(new File("Greengage", now.AddMonths(-6), 39));
            Files.Add(new File("Dekopon", now.AddMonths(-7), 74));
            Files.Add(new File("Madrono", now.AddMonths(-8), 61));
            Files.Add(new File("Jabotacaba", now.AddMonths(-9), 22));
            Files.Add(new File("Feijoa", now.AddMonths(-10), 44));
            Files.Add(new File("Dekopon", now.AddMonths(-11), 99));
            Files.Add(new File("Neem", now.AddMonths(-12), 80));
            Files.Add(new File("MelonPear", now.AddMonths(-13), 60));
            Files.Add(new File("Jambul fruit", now.AddYears(-1), 33));
            Files.Add(new File("Date plum", now.AddYears(-1), 5));
        }
        
        private void CollectionViewSource_Filter(object sender, System.Windows.Data.FilterEventArgs e) {
            dynamic item = e.Item;

            string search = Filter;

            if (string.IsNullOrWhiteSpace(search)) {
                e.Accepted = true;
                return;
            }

            string name = item.Name;

            e.Accepted = name.Contains(search, StringComparison.InvariantCultureIgnoreCase);
        }


        private void ProductsList_OnSorting(object sender, DataGridSortingEventArgs e) =>
            SortGroupedDataGrid("Date", "Group", e);

        private static ListSortDirection Invert(ListSortDirection sortDirection) =>
            sortDirection == ListSortDirection.Descending
                ? ListSortDirection.Ascending
                : ListSortDirection.Descending;

        private void SortGroupedDataGrid(string reversingProperty, string groupProperty, DataGridSortingEventArgs e) {
            var source = (ListCollectionView) FilesDataGrid.ItemsSource;

            var isReversingGroups = e.Column.SortMemberPath.Equals(reversingProperty);

            var propertySortDirection = Invert(e.Column.SortDirection ?? ListSortDirection.Ascending);

            var groupSortDirection = isReversingGroups ? propertySortDirection : ListSortDirection.Ascending;

            using (source.DeferRefresh()) {
                source.SortDescriptions.Clear();
                source.SortDescriptions.Add(new SortDescription(groupProperty, groupSortDirection));
                source.SortDescriptions.Add(new SortDescription(e.Column.SortMemberPath,
                    isReversingGroups ? Invert(propertySortDirection) : propertySortDirection));
            }

            e.Column.SortDirection = propertySortDirection;

            source.Refresh();

            e.Handled = true;
        }

    }
}