using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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


        public string SearchText
        {
            get => _searchText;
            set
            {
                if (value == _searchText)
                    return;
                _searchText = value;
                OnPropertyChanged();
            }
        }


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
        private string _searchText;


        public ConfigurationEditorModel()
        {
            AppModel = mmApp.Model;
            Window = AppModel.Window;
        }


        public async System.Threading.Tasks.Task AddConfigurationsAsync(StackPanel panel)
        {
            var parser = new ConfigurationParser();
            parser.ParseAllConfigurationObjects();

            var listFilteredProperties = parser.FindProperty(SearchText);

            DisplayCount = listFilteredProperties.Count;

            EditorWindow.PropertiesPanel.Children.Clear();
            EditorWindow.SectionsPanel.Children.Clear();

            await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                RenderPropertyFields(panel, listFilteredProperties);
            });

        }

        private void RenderPropertyFields(StackPanel panel, List<ConfigurationPropertyItem> listFilteredProperties)
        {
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
                        Margin = new Thickness(0, 20, 0, 5),
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


                // set up a binding for the value
                var valueBinding = new Binding
                {
                    Source = mmApp.Model.Configuration,
                    Mode = BindingMode.TwoWay
                };
                if (item.Section == "ApplicationUpdates" || item.Section == "WindowPosition")
                    continue;

                if (item.Section == "Application")
                    valueBinding.Path = new PropertyPath(item.Property.Name);
                else
                    valueBinding.Path = new PropertyPath(item.Section + "." + item.Property.Name);

                var header = new TextBlock()
                {
                    FontSize = 16.5,
                    Foreground = Brushes.Silver,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 15, 0, 3),
                    Text = StringUtils.FromCamelCase(item.Property.Name)
                };

                // bool uses a checkbox before the Header label
                if (item.Property.Type == "bool")
                {
                    var tpanel = new StackPanel {Orientation = Orientation.Horizontal};
                    var checkbox = new CheckBox() {Margin = new Thickness(0, 15, 0, 0)};
                    BindingOperations.SetBinding(checkbox, CheckBox.IsCheckedProperty, valueBinding);
                    tpanel.Children.Add(checkbox);
                    tpanel.Children.Add(header);
                    panel.Children.Add(tpanel);
                }
                else
                    // otherwise just render the header
                    panel.Children.Add(header);


                if (item.Property.Type == "string")
                {
                    var textBox = new TextBox() {Margin = new Thickness(0, 5, 0, 10)};
                    BindingOperations.SetBinding(textBox, TextBox.TextProperty, valueBinding);
                    panel.Children.Add(textBox);
                }

                else if (item.Property.Type == "int" || item.Property.Type == "decimal" || item.Property.Type == "double")
                {
                    var textBox = new TextBox()
                    {
                        Margin = new Thickness(0, 5, 0, 10),
                        Width = 120,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        TextAlignment = TextAlignment.Right
                    };
                    BindingOperations.SetBinding(textBox, TextBox.TextProperty, valueBinding);
                    panel.Children.Add(textBox);
                }
                else
                {
                    // Enums
                    int x = 1;
                }


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
                if (val == null)
                    val = string.Empty;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
