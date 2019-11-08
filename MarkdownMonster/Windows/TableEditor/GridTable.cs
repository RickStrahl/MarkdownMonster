using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MarkdownMonster.Annotations;

namespace MarkdownMonster.Windows
{

    /// <summary>
    /// Custom bound 'grid' of textboxes
    /// </summary>
    public class GridTable : Grid
    {
        public Window ParentWindow { get; set; }
        public AppModel AppModel { get; set; }


        public GridTable()
        {
            VerticalAlignment = VerticalAlignment.Top;
            HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        public ObservableCollection<ObservableCollection<CellContent>> TableSource
        {
            get { return (ObservableCollection<ObservableCollection<CellContent>>)GetValue(TableSourceProperty); }
            set { SetValue(TableSourceProperty, value); }
        }

        
        // Using a DependencyProperty as the backing store for TableSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TableSourceProperty =
            DependencyProperty.Register("TableSource", typeof(ObservableCollection<ObservableCollection<CellContent>>), typeof(GridTable), new PropertyMetadata(null, TableSourceChanged));

        private Size _lastMeasureSize = Size.Empty;
        private Size _lastArrangeSize = Size.Empty;

        internal bool PreventRecursiveUpdates { get; set; } = false;

        private static void TableSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var table = d as GridTable;
            if (table == null) return;
            var data = e.NewValue as ObservableCollection<ObservableCollection<CellContent>>;
            if (data == null) return;

            table.RepopulateChildren(data);

            //data.CollectionChanged += (s, e2) =>
            //{

            //    Debug.WriteLine("Collection Changed...");
            //    if (!table.PreventRecursiveUpdates)
            //        table.RepopulateChildren(data);
            //};
            //foreach (var row in data)
            //    row.CollectionChanged += (s, e2) =>
            //    {
            //        if (!table.PreventRecursiveUpdates)
            //            table.RepopulateChildren(data);
            //    };
        }

        private void RepopulateChildren(ObservableCollection<ObservableCollection<CellContent>> data)
        {
            //Debug.WriteLine("RepopulateChildren called.");

            bool origRecursiveUpdates = PreventRecursiveUpdates;
            PreventRecursiveUpdates = true;

            Children.Clear();
            RowDefinitions.Clear();
            ColumnDefinitions.Clear();
            
            var contextMenu = ParentWindow.Resources["ColumnContextMenu"] as ContextMenu;

            //var rect = new Rectangle { Fill = Brushes.Gray };
            //Grid.SetColumnSpan(rect, 10000);
            //Children.Add(rect);

            if (data.Count < 1) return;

            RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var columnCounter = 0;
            foreach (var header in data[0])
            {                
                ColumnDefinitions.Add(new ColumnDefinition { });

                var columnText = NewTextBox();
                columnText.Background = new BrushConverter().ConvertFromString("#777") as Brush;
                columnText.Tag = new TablePosition { Column = columnCounter, Row = 0 };
                columnText.ContextMenu = contextMenu;

                var binding = new Binding("Text") { Source = header };
                columnText.SetBinding(TextBox.TextProperty, binding);

                Grid.SetColumn(columnText, columnCounter);
                Children.Add(columnText);

                columnCounter++;                
            }
            

            if (data.Count > 50)
                mmApp.Model.Window.ShowStatusProgress("Binding table data...");

            var rowCount = 1;
            foreach (var row in data.Skip(1))
            {
                RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                columnCounter = 0;

                foreach (var column in row)
                {
                    var columnText = NewTextBox();
                    columnText.Tag = new TablePosition { Column = columnCounter, Row = rowCount };

                    var binding = new Binding("Text") { Source = column, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.TwoWay };
                    columnText.SetBinding(TextBox.TextProperty, binding);

                    //columnText.Text = column.Text;

                    columnText.ContextMenu = contextMenu;
                    Grid.SetColumn(columnText, columnCounter);
                    Grid.SetRow(columnText, rowCount);

                    Children.Add(columnText);

                    columnCounter++;
                }
                rowCount++;
                if (rowCount % 8 == 0)
                {
                    PreventRecursiveUpdates = false;
                    WindowUtilities.DoEvents();
                    PreventRecursiveUpdates = true;
                }
            }

            // Last Cell in Last Row - insert a new row on tab
            var lastText = Children.OfType<TextBox>().LastOrDefault();
            if (lastText != null)
            {                
                lastText.PreviewKeyDown += (s, e) =>
                {
                    // Tab Key Insertion
                    if (e.Key == Key.Tab)
                    {
                        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                            return;

                        var textBox = s as TextBox;
                        var pos = textBox.Tag as TablePosition;

                        AddRow(pos.Row, 0, RowInsertLocation.Below);
                    }
                };
            }

            PreventRecursiveUpdates = origRecursiveUpdates;
            if (!PreventRecursiveUpdates) InvalidateVisual();
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (PreventRecursiveUpdates && _lastArrangeSize != Size.Empty) return arrangeSize;
            _lastArrangeSize = base.ArrangeOverride(arrangeSize);
            return _lastArrangeSize;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (PreventRecursiveUpdates && _lastMeasureSize != Size.Empty) return _lastMeasureSize;
            _lastMeasureSize = base.MeasureOverride(constraint);
            return _lastMeasureSize;
        }

        void KeyUpHandler(object o, KeyEventArgs args)
        {
            bool isShifted = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
                

            if (args.Key == Key.Up && !isShifted)
            {
                var textBox = o as TextBox;
                var pos = textBox.Tag as TablePosition;

                SelectColumn(pos.Row - 1, pos.Column);
            }
            else if (args.Key == Key.Down && !isShifted)
            {
                var textBox = o as TextBox;
                var pos = textBox.Tag as TablePosition;

                SelectColumn(pos.Row + 1, pos.Column);
            }
        }


        #region Helpers

        public void SelectColumn(int row, int col)
        {
            // needs to run out of band so that any previous rendering has occurred.
            Dispatcher.InvokeAsync(() =>
            {
                var data = TableSource;
                if (data.Count < 1)
                    return;

                var colCount = data[0].Count;

                if (row < 0)
                    row = 0;
                if (row >= TableSource.Count)
                    row = TableSource.Count - 1;
               if (col < 0)
                    col = 0;
               
                var skipBy = row * colCount + col;
                if (skipBy < 0)
                    return;

                var newText = Children.OfType<TextBox>()
                    .Skip(skipBy)
                    .FirstOrDefault();

                newText?.Focus();
            },DispatcherPriority.Normal);
        }

        public void AddColumn(int currentRow, int currentColumn, ColumnInsertLocation insertLocation)
        {
            PreventRecursiveUpdates = true;

            foreach (var row in TableSource)
            {
                var newCell = new CellContent(string.Empty);
                if (insertLocation == ColumnInsertLocation.Right)
                {
                    row.Insert(currentColumn + 1, newCell);                                                     
                }
                else
                {
                    row.Insert(currentColumn, newCell);                                        
                }
            }
            PreventRecursiveUpdates = false;

            RepopulateChildren(TableSource);

            if (insertLocation == ColumnInsertLocation.Right)
                SelectColumn(currentRow, currentColumn + 1);
            else
                SelectColumn(currentRow, currentColumn);
        }

        public void AddRow(int currentRow, int currentColumn, RowInsertLocation insertLocation)
        {
            var row = TableSource[currentRow] as ObservableCollection<CellContent>;
            if (row == null)
                return;

            PreventRecursiveUpdates = true;

            var newCell = new CellContent(string.Empty);
            if (insertLocation == RowInsertLocation.Below)
            {
                var newRow = new ObservableCollection<CellContent>();
                for (int i = 0; i < row.Count; i++)
                    newRow.Add(new CellContent(string.Empty));

                TableSource.Insert(currentRow + 1, newRow);
            }
            else
            {
                var newRow = new ObservableCollection<CellContent>();
                for (int i = 0; i < row.Count; i++)
                    newRow.Add(new CellContent(string.Empty));

                TableSource.Insert(currentRow, newRow);
            }


            PreventRecursiveUpdates = false;

            RepopulateChildren(TableSource);

            if (insertLocation == RowInsertLocation.Below)
                SelectColumn(currentRow + 1, 0);
            else
                SelectColumn(currentRow, 0);
        }

        public void DeleteRow(int currentRow, int currentColumn)
        {
            var row = TableSource[currentRow] as ObservableCollection<CellContent>;
            if (row == null)
                return;
            PreventRecursiveUpdates = true;
            TableSource.Remove(row);
            PreventRecursiveUpdates = false;

            RepopulateChildren(TableSource);
            SelectColumn(currentRow, currentColumn);
        }



        public void DeleteColumn(int currentRow, int currentColumn)
        {

            PreventRecursiveUpdates = true;
            foreach (var row in TableSource)
            {
                var item = row[currentColumn];
                row.Remove(item);
            }
            PreventRecursiveUpdates = false;

            RepopulateChildren(TableSource);
            
            SelectColumn(currentRow, currentColumn);
        }

        public TextBox NewTextBox() //(int rowIndex, int columnIndex)
        {
            var tb = new TextBox();

            tb.KeyUp += KeyUpHandler;

            //columnText.SpellCheck.IsEnabled = true; // VERY SLOW - use events instead
            tb.GotFocus += (s, args) => { tb.SpellCheck.IsEnabled = true; };
            tb.LostFocus += (s, args) => { tb.SpellCheck.IsEnabled = false; };

            return tb;
        }
        #endregion

    }

    [DebuggerDisplay("{Text} {Row}:{Column} ")]
    public class CellContent : INotifyPropertyChanged
    {
        public string Text
        {
            get { return _text; }
            set
            {
                if (value == _text) return;
                _text = value;
                OnPropertyChanged();
            }
        }
        private string _text;



        public int Row
        {
            get { return _row; }
            set
            {
                if (value == _row) return;
                _row = value;
                OnPropertyChanged();
            }
        }
        private int _row;

        
        public int Column
        {
            get { return _column; }
            set
            {
                if (value == _column) return;
                _column = value;
                OnPropertyChanged();
            }
        }
        private int _column;

        public string[] Lines { get; set; }

        public CellContent(string text)
        {
            Text = text;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TablePosition
    {
        public int Row;
        public int Column;
    }

    public enum ColumnInsertLocation
    {
        Left,
        Right
    }

    public enum RowInsertLocation
    {
        Above,
        Below
    }
}
