namespace MarkdownMonster.Windows
{
    public class TableEditorCommands
    {
        private TableEditorHtml Window { get; }

        public TableEditorCommands(TableEditorHtml window)
        {
            Window = window;

            EmbedTable();
        }

        public CommandBase EmbedTableCommand { get; set; }

        void EmbedTable()
        {
            EmbedTableCommand = new CommandBase((parameter, command) =>
            {
                Window.TableData = Window.Interop.GetJsonTableData();

                var parser = new TableParserHtml();

                if (Window.TableMode == "Grid Table")
                    Window.TableHtml = parser.ToGridTableMarkdown(Window.TableData);
                else if(Window.TableMode == "HTML Table")
                    Window.TableHtml = parser.ToTableHtml(Window.TableData);
                else
                    Window.TableHtml = parser.ToPipeTableMarkdown(Window.TableData);


                mmApp.Model.ActiveEditor.SetSelectionAndFocus(Window.TableHtml);
                Window.Close();
            }, (p, c) => true);
        }

    }
}
