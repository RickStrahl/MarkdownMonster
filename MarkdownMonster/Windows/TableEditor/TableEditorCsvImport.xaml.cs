using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MarkdownMonster.Annotations;
using Microsoft.Win32;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for TableEditorCsvImport.xaml
    /// </summary>
    public partial class TableEditorCsvImport : MetroWindow, INotifyPropertyChanged
    {
        public AppModel AppModel { get; set; }


        public string CsvFilename
        {
            get { return _csvFilename; }
            set
            {
                if (value == _csvFilename) return;
                _csvFilename = value;
                OnPropertyChanged(nameof(CsvFilename));
                OnPropertyChanged(nameof(IsFilename));
            }
        }
        private string _csvFilename;


        public bool IsFilename => string.IsNullOrEmpty(_csvFilename);

        public bool ImportFromCsv
        {
            get { return _ImportFromCsv; }
            set
            {
                if (value == _ImportFromCsv) return;
                _ImportFromCsv = value;
                _csvFilename = null;
                OnPropertyChanged(nameof(ImportFromCsv));
                OnPropertyChanged(nameof(IsFilename));
            }
        }
        private bool _ImportFromCsv = true;


        public bool IsCancelled { get; set; } = true;


        public bool ImportFromClipboard
        {
            get { return _ImportFromClipboard; }
            set
            {
                if (value == _ImportFromClipboard) return;
                _ImportFromClipboard = value;
                OnPropertyChanged(nameof(ImportFromClipboard));
            }
        }
        private bool _ImportFromClipboard;


        public string CsvSeparator
        {
            get { return _CsvSeparator; }
            set
            {
                if (value == _CsvSeparator) return;
                _CsvSeparator = value;
                OnPropertyChanged(nameof(CsvSeparator));
            }
        }

        private string _CsvSeparator = ",";


        public TableEditorCsvImport()
        {
            InitializeComponent();

            AppModel = mmApp.Model;
            mmApp.SetThemeWindowOverride(this);

            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RadioFilename_Click(object sender, RoutedEventArgs e)
        {
            SelectFileName();
        }

        private void RadioClipboard_Click(object sender, RoutedEventArgs e)
        {
            // Try to detect the CSV delimiter
            var csv = ClipboardHelper.GetText();
            var lines = StringUtils.GetLines(csv.Trim());

            int tabCount = 0;
            int commaCount = 0;
            int listDelimiterCount = 0;
            string listSep = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            foreach (var line in lines)
            {
                if (line.Contains("\t"))
                    tabCount++;
                if (line.Contains(","))
                    commaCount++;
                if (line.Contains(listSep))
                    listDelimiterCount++;
            }

            if (tabCount >= lines.Length)
                CsvSeparator = "\\t";
            else if (commaCount >= lines.Length)
                CsvSeparator = ",";
            else if (listDelimiterCount >= lines.Length)
                CsvSeparator = listSep;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ImportFromCsv && string.IsNullOrEmpty(CsvFilename))
                SelectFileName();

            if (ImportFromCsv && string.IsNullOrEmpty(CsvFilename))
            {
                e.Handled = false;
                return;
            }

            IsCancelled = false;
            //e.Handled = true;
            Close();
        }

        void SelectFileName()
        {

            var fd = new OpenFileDialog
            {
                DefaultExt = ".csv",
                Filter = "Csv files (*.csv)|*.csv|" +
                         "All files (*.*)|*.*",
                CheckFileExists = true,
                RestoreDirectory = true,
                Multiselect = false,
                Title = "Open CsvFile"
            };
            if (!string.IsNullOrEmpty(AppModel.Configuration.LastFolder))
                fd.InitialDirectory = AppModel.Configuration.LastFolder;

            var res = fd.ShowDialog();
            if (res == null || !res.Value)
                return;

            CsvFilename = fd.FileName;
        }


    }
}
