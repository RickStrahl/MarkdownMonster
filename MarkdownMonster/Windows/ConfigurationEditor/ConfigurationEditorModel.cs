using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
            EditorWindow.StatusBar.ShowStatusProgress("Loading settings...");
            WindowUtilities.DoEvents();

            var parser = new ConfigurationParser();
            parser.ParseAllConfigurationObjects();

            var listFilteredProperties = parser.FindProperty(SearchText);

            DisplayCount = listFilteredProperties.Count;

            EditorWindow.PropertiesPanel.Children.Clear();
            EditorWindow.SectionsPanel.Children.Clear();

            await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                RenderPropertyFields(panel, listFilteredProperties);
                EditorWindow.StatusBar.ShowStatus();
            });

        }
        private void RenderPropertyFields(StackPanel panel, List<ConfigurationPropertyItem> listFilteredProperties)
        {
            string section = null;
            object instance = null;

            foreach (var item in listFilteredProperties)
            {
                if (item.Section == "WindowPosition" || item.Section == "ApplicationUpdates")
                    continue;

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
                        FontSize = 17,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = Brushes.LightSteelBlue,
                        Margin = new Thickness(0, 5, 20, 5),
                        TextAlignment = TextAlignment.Right,
                        Text = item.SectionDisplayName
                    };
                    EditorWindow.SectionsPanel.Children.Add(sectionHeader2);
                    sectionHeader2.PreviewMouseLeftButtonDown += SectionHeader2_PreviewMouseLeftButtonDown;
                    

                    section = item.Section;
                }


                // set up a binding for the value
                var valueBinding = new Binding {Source = mmApp.Model.Configuration, Mode = BindingMode.TwoWay};
                if (item.Section == "ApplicationUpdates" || item.Section == "WindowPosition")
                    continue;

                if (item.Section == "Application")
                    valueBinding.Path = new PropertyPath(item.Property.Name);
                else
                    valueBinding.Path = new PropertyPath(item.Section + "." + item.Property.Name);

                // skip over config objects
                if (item.Property.Type.EndsWith("Configuration"))
                    continue;

                if (item.Property.Type == "bool")
                {
                    // Render Checkbox with label for Bool
                    var checkbox = new CheckBox() {
                        Margin = new Thickness(0, 15, 0, 3),
                        FontSize= 16.5,
                        Foreground = Brushes.Silver,
                        FontWeight = FontWeights.SemiBold,
                        Content = StringUtils.FromCamelCase(item.Property.Name)
                    };
                    BindingOperations.SetBinding(checkbox, CheckBox.IsCheckedProperty, valueBinding);
                    panel.Children.Add(checkbox);
                }
                else
                {
                    // Just render a header and add the field below
                    var header = new TextBlock()
                    {
                        FontSize = 16.5,
                        Foreground = Brushes.Silver,
                        FontWeight = FontWeights.SemiBold,
                        Margin = new Thickness(0, 15, 0, 3),
                        Text = StringUtils.FromCamelCase(item.Property.Name)
                    };
                    panel.Children.Add(header);
                }

                
                if (item.Property.Type == "string")
                {
                    var textBox = new TextBox() {Margin = new Thickness(0, 5, 0, 10)};
                    BindingOperations.SetBinding(textBox, TextBox.TextProperty, valueBinding);
                    panel.Children.Add(textBox);
                }

                else if (item.Property.Type == "int" || item.Property.Type == "decimal" ||
                         item.Property.Type == "double")
                {
                    var textBox = new TextBox()
                    {
                        Margin = new Thickness(0, 5, 0, 10),
                        Width = 80,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        TextAlignment = TextAlignment.Right
                    };
                    BindingOperations.SetBinding(textBox, TextBox.TextProperty, valueBinding);
                    panel.Children.Add(textBox);
                }
                else if (item.Property.Type == "bool")
                {
                    // do nothing
                }
                else
                {
                    // Enums
                    var type = item.Property.Type;
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

        private void SectionHeader2_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var clickedSection = sender as TextBlock;
            var clickedSectionText = clickedSection.Text;
            foreach (TextBlock tb in EditorWindow.SectionsPanel.Children)
            {
                if (clickedSection.Text == "Application")
                {
                    EditorWindow.PropertiesScrollContainer.ScrollToHome();
                    return;
                }

                if(clickedSectionText == tb.Text)
                {
                    tb.Focus();

                    var header = EditorWindow.PropertiesPanel.Children
                        .OfType<object>()
                        .FirstOrDefault(tb2 =>
                        {
                            var tb3 = tb2 as TextBlock;
                            if (tb3 == null || tb3.Text == null)
                                return false;
                            
                            if (tb3.Text == clickedSection.Text)
                                return true;

                            return false;
                        }) as TextBlock;


                    Point relativeLocation = header.TranslatePoint(new Point(0, 0),EditorWindow.PropertiesPanel);
                    EditorWindow.PropertiesScrollContainer.ScrollToVerticalOffset(relativeLocation.Y);
                }
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
