using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MarkdownMonster.Annotations;
using Westwind.Utilities;

namespace MarkdownMonster.Windows.ConfigurationEditor
{
    public class ConfigurationEditorModel : INotifyPropertyChanged
    {
        public AppModel AppModel { get; set; }
        public MainWindow Window { get; set; }

        public ConfigurationEditorWindow EditorWindow {
            get;
            set;
        }

        public ConfigurationParser ConfigurationParser { get; set; }


        public string SearchText { get; set; }


        public int DisplayCount
        {
            get { return _DisplayCount; }
            set
            {
                if (value == _DisplayCount) return;
                _DisplayCount = value;
                OnPropertyChanged(nameof(DisplayCount));
            }
        }
        private int _DisplayCount;


        public ConfigurationEditorModel()
        {
            AppModel = mmApp.Model;
            Window = AppModel.Window;
        }


        public async void AddConfigurationsAsync(StackPanel panel)
        {
            var parser = new ConfigurationParser();
            parser.ParseAllConfigurationObjects();

            var listFilteredProperties = parser.FindProperty(SearchText);

            DisplayCount = listFilteredProperties.Count;
            
            await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                panel.Children.Clear();
                EditorWindow.SectionsPanel.Children.Clear();

                string section = null;
                object instance = null;

                foreach (var item in listFilteredProperties)
                {
                    if (item.Section == "Application")
                        instance = mmApp.Configuration;
                    else
                    {
                        try
                        {
                            instance = ReflectionUtils.GetProperty(
                                mmApp.Configuration, item.Section); 
                        }
                        catch
                        {
                            instance = null;
                        }
                    }

                    if (instance == null)
                        continue;



                    if (item.Section != section)
                    {
                        
                        var sectionHeader = new TextBlock()
                        {
                            FontSize = 27,
                            FontWeight = FontWeights.SemiBold,
                            Foreground = Brushes.LightSteelBlue,
                            Margin = new Thickness(0, 20, 0, 15),
                            Text = item.SectionDisplayName
                        };
                        panel.Children.Add(sectionHeader);


                        var sectionHeader2 = new TextBlock()
                        {
                            FontSize = 18,
                            FontWeight = FontWeights.SemiBold,
                            Foreground = Brushes.LightSteelBlue,
                            Margin = new Thickness(0, 3, 0, 3),
                            Text = item.SectionDisplayName
                        };                        
                        EditorWindow.SectionsPanel.Children.Add(sectionHeader2);

                        section = item.Section;
                    }

                   

                    var header = new TextBlock()
                    {
                        FontSize = 16.5,
                        Foreground = Brushes.Silver,
                        FontWeight = FontWeights.SemiBold,
                        Margin = new Thickness(0, 10, 0, 3),
                        Text = StringUtils.FromCamelCase(item.Property.Name)
                    };
                    panel.Children.Add(header);

                    if (!string.IsNullOrEmpty(item.Property.HelpText))
                    {
                        var description = new TextBlock()
                        {
                            FontStyle = FontStyles.Italic,
                            FontSize = 12.4,
                            Text = item.Property.HelpText
                        };
                        panel.Children.Add(description);
                    }


                    var val = ReflectionUtils.GetProperty(instance, item.Property.Name);

                    var textBox = new TextBox() { Text = val.ToString(), Margin = new Thickness(0, 5, 0, 10) };
                    panel.Children.Add(textBox);

                }
            });

        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
