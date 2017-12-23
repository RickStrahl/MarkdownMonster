using System;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MarkdownMonster.Annotations;

namespace MarkdownMonster.Windows
{

    /// <summary>
    /// Custom bound 'grid' of textboxes
    /// </summary>
    public class GridTable : Grid
    {
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

        private static void TableSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var table = d as GridTable;
            if (table == null) return;
            var data = e.NewValue as ObservableCollection<ObservableCollection<CellContent>>;
            if (data == null) return;

            table.RepopulateChildren(data);

            data.CollectionChanged += (s, e2) => { table.RepopulateChildren(data); };
            foreach (var row in data)
                row.CollectionChanged += (s, e2) => { table.RepopulateChildren(data); };
        }

        private void RepopulateChildren(ObservableCollection<ObservableCollection<CellContent>> data)
        {
            Children.Clear();
            RowDefinitions.Clear();
            ColumnDefinitions.Clear();

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
                columnText.Tag = new TablePosition { Column = columnCounter, Row = 0};

                var binding = new Binding("Text") { Source = header };
                columnText.SetBinding(TextBox.TextProperty, binding);

                Children.Add(columnText);
                Grid.SetColumn(columnText, columnCounter);
                columnCounter++;

                columnText.KeyUp += KeyUpAndDownHandler;
            }

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
                    Children.Add(columnText);
                    Grid.SetColumn(columnText, columnCounter);
                    Grid.SetRow(columnText, rowCount);
                    columnCounter++;

                    columnText.KeyUp += KeyUpAndDownHandler;
                }
                rowCount++;
            }

            var lastText = Children.OfType<TextBox>().LastOrDefault();
            if (lastText != null)
            {                
                lastText.PreviewKeyDown += (s, e) =>
                {
                    if (e.Key == Key.Tab)
                    {
                        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                            return;
                       
                        var newRow = new ObservableCollection<CellContent>();
                        foreach (var header in TableSource[0])
                            newRow.Add(new CellContent(string.Empty));

                        var textBox = s as TextBox;
                        var pos = textBox.Tag as TablePosition;

                        TableSource.Add(newRow);

                        SelectColumn(pos.Row + 1, 0);
                    }
                };
            }
        }
        
        void KeyUpAndDownHandler(object o, KeyEventArgs args)
        {
            if (args.Key == Key.Up)
            {
                var textBox = o as TextBox;
                var pos = textBox.Tag as TablePosition;
                SelectColumn(pos.Row - 1, pos.Column + 1);
            }
            else if (args.Key == Key.Down)
            {
                var textBox = o as TextBox;
                var pos = textBox.Tag as TablePosition;
                SelectColumn(pos.Row + 1, pos.Column + 1);
            }         
        }

        public void SelectColumn(int row, int col)
        {
            var data = this.TableSource;
            if (data.Count < 1)
                return;
            var colCount = data[0].Count;

            var skipBy = row * colCount + col - 1;
            if (skipBy < 0)
                return;

            var newText = Children.OfType<TextBox>()
                .Skip(skipBy)
                .FirstOrDefault();

            newText?.Focus();
        }
 

        public TextBox NewTextBox() //(int rowIndex, int columnIndex)
        {
            var text = new TextBox();
            text.GotFocus += (s, e) =>
            {

            };
            return text;
        }
    }

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

}
