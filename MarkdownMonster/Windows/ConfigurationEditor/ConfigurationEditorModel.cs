using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using MahApps.Metro.Controls;
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


        public async Task AddConfigurationsAsync(StackPanel panel)
        {
            EditorWindow.StatusBar.ShowStatusProgress("Loading settings...");
            await Dispatcher.CurrentDispatcher.InvokeAsync( () =>
            {
                var parser = new ConfigurationParser();
                parser.ParseAllConfigurationObjects();

                var listFilteredProperties = parser.FindProperty(SearchText);

                DisplayCount = listFilteredProperties.Count;

                EditorWindow.PropertiesPanel.Children.Clear();
                EditorWindow.SectionsPanel.Children.Clear();

                Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                {
                    RenderPropertyFields(panel, listFilteredProperties);
                    EditorWindow.StatusBar.ShowStatus();
                },DispatcherPriority.Normal);
            },DispatcherPriority.Render);
        }
        private void RenderPropertyFields(StackPanel panel, List<ConfigurationPropertyItem> listFilteredProperties)
        {
            string section = null;
            object configInstance = null;

            Brush headerColor = mmApp.Configuration.ApplicationTheme == Themes.Dark ? Brushes.LightSteelBlue : Brushes.SteelBlue;
            Brush sectionColor = mmApp.Configuration.ApplicationTheme == Themes.Dark ? Brushes.LightSlateGray : Brushes.DarkSlateBlue;

            foreach (var item in listFilteredProperties)
            {
                if (item.Section == "WindowPosition" || item.Section == "ApplicationUpdates" || item.Property.Scope != "public")
                    continue;

                if (item.Section == "Application")
                    configInstance = mmApp.Configuration;
                else
                {
                    try
                    {
                        configInstance = ReflectionUtils.GetProperty(
                            mmApp.Configuration, item.Section);
                    }
                    catch
                    {
                        configInstance = null;
                    }
                }

                if (configInstance == null)
                    continue;

                if (item.Section != section)
                {
                    var sectionHeader = new TextBlock()
                    {
                        FontSize = 28,
                        FontWeight = FontWeights.Bold,
                        Foreground = sectionColor,
                        Margin = new Thickness(0, 15, 0, 0),
                        Text = item.SectionDisplayName
                    };
                    panel.Children.Add(sectionHeader);


                    var sectionLink = new Button()
                    {
                        FontSize = 18,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = sectionColor,
                        Margin = new Thickness(0, 5, 20, 8),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Content = item.SectionDisplayName
                    };
                    sectionLink.SetResourceReference(TextBlock.StyleProperty, "LinkButtonStyle");
                    EditorWindow.SectionsPanel.Children.Add(sectionLink);
                    sectionLink.PreviewMouseLeftButtonDown += SectionHeader2_PreviewMouseLeftButtonDown;
                    
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
                        Foreground = headerColor,
                        FontWeight = FontWeights.SemiBold,
                        Content = StringUtils.FromCamelCase(item.Property.Name)
                    };

                    checkbox.Checked += SaveAndRestyle;  // (object s, RoutedEventArgs a) =>  mmApp.Model.ActiveEditor?.RestyleEditor();
                    checkbox.Unchecked += SaveAndRestyle; // (object s, RoutedEventArgs a) => mmApp.Model.ActiveEditor?.RestyleEditor();

                    BindingOperations.SetBinding(checkbox, CheckBox.IsCheckedProperty, valueBinding);
                    panel.Children.Add(checkbox);
                }
                else
                {
                    // Just render a header and add the field below
                    var header = new TextBlock()
                    {
                        FontSize = 16.5,
                        Foreground = headerColor,
                        FontWeight = FontWeights.SemiBold,
                        Margin = new Thickness(0, 15, 0, 3),
                        Text = StringUtils.FromCamelCase(item.Property.Name)
                    };
                    panel.Children.Add(header);

                }

                
                if (item.Property.Type == "string")
                {
                    var textBox = new TextBox() {Margin = new Thickness(0, 5, 0, 10)};
                    textBox.LostFocus += SaveAndRestyle;
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
                    textBox.LostFocus += SaveAndRestyle; 

                    BindingOperations.SetBinding(textBox, TextBox.TextProperty, valueBinding);
                    panel.Children.Add(textBox);
                }
                else if (item.Property.Type == "bool")
                {
                    // do nothing - already added the control
                }
                else
                {
                    var val = ReflectionUtils.GetProperty(configInstance, item.Property.Name);
                    if (val == null)
                        continue;

                    var type = val.GetType();
                    if (val is IDictionary<string,string>)
                    {
                        var button = new Button()
                        {
                            FontWeight = FontWeights.SemiBold,
                            Margin = new Thickness(10, 5, 0, 15),
                            Content="Please edit this list of values in the JSON file",
                            Command = mmApp.Model.Commands.SettingsCommand
                        };
                        button.SetResourceReference(TextBlock.StyleProperty, "LinkButtonStyle");
                        panel.Children.Add(button);
                        
                        //var grid = new DataGrid
                        //{
                        //    ItemsSource = val as Dictionary<string, string>,
                        //    Height = 100,
                        //    Margin = new Thickness(10, 5, 0, 8),
                        //    HeadersVisibility = DataGridHeadersVisibility.None,
                        //};
                        //panel.Children.Add(grid);

                    }
                    if (type.IsEnum)
                    {
                        var enumItems = ReflectionUtils.GetEnumList(type);

                        var combo = new ComboBox()
                        {
                            ItemsSource = enumItems,
                            DisplayMemberPath = "Value",
                            Margin = new Thickness(0, 5, 0, 8)
                        };
                        combo.SelectedItem = enumItems.First(kv => kv.Key == val.ToString());
                        combo.Tag = configInstance;
                        combo.SelectionChanged += (s,a) =>
                        {
                            var cmbo = s as ComboBox;
                            var key = (KeyValuePair<string,string>) cmbo.SelectedValue;
                            var enumVal = Enum.Parse(type, key.Key);
                            var prop = cmbo.Tag.GetType().GetProperty(item.Property.Name,
                                BindingFlags.Public | BindingFlags.Instance);
                            prop.SetValue(cmbo.Tag, enumVal, null);
                            //ReflectionUtils.SetPropertyEx(configInstance, , enumVal);

                            Dispatcher.CurrentDispatcher.Invoke(() => SaveAndRestyle(null,null));
                        };
                        panel.Children.Add(combo);
                    }
                    else if (item.Property.Type == "DateTime")
                    {
                        var timePicker = new DateTimePicker {Margin = new Thickness(0, 5, 0, 10)};
                        BindingOperations.SetBinding(timePicker, DateTimePicker.SelectedDateProperty,valueBinding);
                        panel.Children.Add(timePicker);
                    }
                    else if (type.IsClass)
                    {
                        var childPanel = new StackPanel {Margin = new Thickness(20, 30, 10, 0)};

                    }
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
                else
                    Debug.WriteLine("Missing Configuration Help Property: " + item.Property.Name);
            }
        }

        private void SaveAndRestyle(object sender, RoutedEventArgs e)
        {
            mmApp.Configuration.Write();
            mmApp.Model.ActiveEditor?.RestyleEditor();
        }

        private void SectionHeader2_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var clickedSection = sender as Button;
            var clickedSectionText = clickedSection.Content as string;

            foreach (Button tb in EditorWindow.SectionsPanel.Children)
            {
                if (clickedSectionText == "Application")
                {
                    EditorWindow.PropertiesScrollContainer.ScrollToHome();
                    return;
                }

                if(clickedSectionText == tb.Content as string)
                {
                    tb.Focus();

                    var header = EditorWindow.PropertiesPanel.Children
                        .OfType<object>()
                        .FirstOrDefault(tb2 =>
                        {
                            var tb3 = tb2 as TextBlock;
                            if (tb3 == null || tb3.Text == null)
                                return false;
                            
                            if (tb3.Text == clickedSectionText)
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
