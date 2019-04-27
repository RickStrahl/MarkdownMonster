using System;
using System.Reflection;
using MarkdownMonster.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Westwind.Utilities;

namespace MarkdownMonster
{

    /// <summary>
    /// Wrapper around the Ace Editor JavaScript COM interface that
    /// allows calling into the editor to perform various operations
    /// </summary>
    public class AceEditorCom
    {
        /// <summary>
        /// The actual raw COM instance of the `te` instance
        /// inside of  `editor.js`. The internal members use
        /// this instance to access the members of the underlying
        /// JavaScript object.
        ///
        /// You can use ReflectionUtils to further use Reflection
        /// on this instance to automate Ace Editor.
        ///
        /// Example:
        /// var udm = ReflectionUtils.InvokeMethod(AceEditor.Instance,"editor.session.getUndoManager",false)
        /// RelectionManager.Invoke(udm,"undo",false);
        ///
        /// Note methods with no parameters should pass `false`
        /// </summary>
        public object Instance { get; }

        public Type InstanceType { get;  }

        /// <summary>
        /// This is a Write Only property that  allows you to update
        /// the dirty status inside of the editor. This is not used
        /// frequently as the editor IsDirty flag is internally updated
        /// but if you need to force some action in the editor (like a spellcheck)
        /// to fire then this is the property to set.
        /// </summary>
        public bool IsDirty
        {
            set { SetIsDirty(value); }
        }

        public AceEditorCom(object instance)
        {
            Instance = instance;
            InstanceType = instance.GetType();
        }

        #region Selections

        public void SetFocus()
        {
            Invoke("setfocus", false);
        }

        public void SetSelectionRange(int startRow, int startColumn, int endRow, int endColumn)
        {
            Invoke("setSelectionRange", startRow, startColumn, endRow, endColumn);
        }

        public SelectionRange GetSelectionRange()
        {
            var range =  Invoke("getselectionrange");
            if (range == null)
                return null;

            return new SelectionRange
            {
                StartRow = (int)ReflectionUtils.GetPropertyCom(range, "startRow"),
                EndRow = (int)ReflectionUtils.GetPropertyCom(range, "endRow"),
                StartColumn = (int)ReflectionUtils.GetPropertyCom(range, "startColumn"),
                EndColumn = (int)ReflectionUtils.GetPropertyCom(range, "endColumn")
            };
        }


        public string GetSelection()
        {
            return Invoke("getselection", false) as string;
        }

        public void SetSelection(string text)
        {
            Invoke("setselection", text);
        }

        public void ReplaceContent(string text)
        {
            Invoke("replaceContent", text);
        }

        public void FindAndReplaceText(string search, string replace)
        {
            Invoke("findAndReplaceText",search, replace);
        }

        public void DeleteCurrentLine()
        {
            Invoke("deleteCurrentLine", false);
        }

        public void FindAndReplaceTextInCurrentLine(string search, string replace)
        {
            Invoke("findAndReplaceTextInCurrentLine", search, replace);
        }

        /// <summary>
        /// Forces the cursor position to be set to the mouse
        /// position.
        /// </summary>
        public void SetSelPositionFromMouse()
        {
            Invoke("setselpositionfrommouse",false);
        }
        #endregion

        #region Navigation

        /// <summary>
        /// Sets the cursor position
        /// </summary>
        /// <param name="pos">Previously captured position (column, row props)</param>
        public void SetCursorPosition(object pos)
        {
            Invoke("setCursorPosition", pos);
        }

        /// <summary>
        /// Sets the cursor position
        /// </summary>
        /// <param name="row">row to to goto</param>
        /// <<param name="col">column to goto</param>
        public void SetCursorPosition(int row, int col)
        {
            Invoke("setCursorPosition", row, col);
        }

        /// <summary>
        /// Returns the current cursor position as an object
        /// </summary>
        /// <returns></returns>
        public object GetCursorPosition()
        {
            return Invoke("getCursorPosition");
        }

        /// <summary>
        /// Goes to the bottom of the editor
        /// </summary>
        public void GotoBottom()
        {
            Invoke("gotoBottom", false);
        }

        /// <summary>
        /// Goes to the specific line in the editor
        /// </summary>
        public void GotoLine(int line, bool noRefresh, bool noSelection)
        {
            Invoke("gotoLine", line, noRefresh, noSelection);
        }

        /// <summary>
        /// Sets scroll position from a scroll object (captured via COM)
        /// </summary>
        /// <param name="scroll">Previously captured scroll position</param>
        public void SetScrollTop(object scroll)
        {
            Invoke("setscrolltop", scroll);
        }

        /// <summary>
        /// Returns current editor scoll position
        /// </summary>
        /// <returns></returns>
        public object GetScrollTop()
        {
            return Invoke("getscrolltop");
        }

        /// <summary>
        /// Returns the text of the currently active line in the editor
        /// </summary>
        /// <returns></returns>
        public string GetCurrentLine()
        {
            return Invoke("getCurrentLine", false) as string;
        }

        /// <summary>
        /// Returns the text of the currently active line in the editor
        /// </summary>
        /// <returns></returns>
        public string GetLine(int row)
        {
            return Invoke("getLine", row) as string;
        }

        public int GetLineNumber()
        {
            return (int) Invoke("getLineNumber", false);
        }

        public void MoveCursorLeft(int count)
        {
            Invoke("moveCursorLeft", count);
        }

        public void MoveCursorRight(int count)
        {
            Invoke("moveCursorRight", count);
        }

        public void MoveCursorUp(int count)
        {
            Invoke("moveCursorUp", count);
        }

        public void MoveCursorDown(int count)
        {
            Invoke("moveCursorDown", count);
        }
        #endregion

        #region Values and Stats
        /// <summary>
        /// Set the value of the Editor
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(string value, object position, bool keepUndoBuffer)
        {
            Invoke("setvalue", value, position, keepUndoBuffer);
        }

        /// <summary>
        /// Retrieves the Editor value
        /// </summary>
        public string GetValue()
        {
            return Invoke("getvalue", false) as string;
        }


        public void UpdateDocumentStats()
        {
            Invoke("updateDocumentStats", false);
        }

        #endregion

        #region Document State

        /// <summary>
        /// Sets the editors internal dirty flag
        /// </summary>
        /// <param name="isDirty"></param>
        private void SetIsDirty(bool isDirty)
        {
            Set("isDirty", isDirty);
        }

        /// <summary>
        /// Spellchecks the document explicitly.
        /// </summary>
        /// <param name="force">if true forces the document to be rechecked otherwise dirty flag and timing is checked and may not actually spell check.</param>
        public void SpellCheckDocument(bool force)
        {
            Invoke("spellcheckDocument",force);
        }


        /// <summary>
        /// Forces suggestions to be shown for the currently spell error selected in the editor
        /// </summary>
        public void ShowSuggestions()
        {
            Invoke("showSuggestions", false);
        }
        #endregion

        #region Editor Styling

        /// <summary>
        /// Method used to send Configuration to the editor.
        /// Sets things like font sizes, Word Wrap, padding etc.
        /// Called from Markdown Document.
        ///
        /// Use <seealso cref="MarkdownDocumentEditor.RestyleEditor" /> instead.
        /// </summary>
        public void SetEditorStyling()
        {
            Invoke("setEditorStyle", GetJsonStyleInfo());
        }

        /// <summary>
        /// Gets a JSON string of all the settings that are exported to the
        /// ACE Editor instance when styling the editor.
        ///
        /// This object is passed down to ACE which can then uses these settings
        /// to update the editor styling in `setEditorStyle` and also with
        /// a few additional settings.
        /// </summary>
        /// <returns>JSON string of all settings set by the editor</returns>
        public static string GetJsonStyleInfo()
        {
            // determine if we want to rescale the editor fontsize
            // based on DPI Screen Size
            decimal dpiRatio = 1;
            try
            {
                dpiRatio = WindowUtilities.GetDpiRatio(mmApp.Model.Window);
            }
            catch
            {
            }

            var fontSize = mmApp.Configuration.Editor.FontSize *
                           ((decimal)mmApp.Configuration.Editor.ZoomLevel / 100) * dpiRatio;

            var config = mmApp.Configuration;

            // CenteredModeMaxWidth is rendered based on Centered Mode only
            int maxWidth = config.Editor.CenteredModeMaxWidth;
            if (!config.Editor.CenteredMode)
                maxWidth = 0;  // 0 means stretch full width

            var style = new
            {
                Theme = config.EditorTheme,
                FontSize = (int)fontSize,
                MaxWidth = maxWidth,

                config.Editor.Font,
                config.Editor.LineHeight,
                config.Editor.Padding,
                config.Editor.HighlightActiveLine,
                config.Editor.WrapText,
                config.Editor.ShowLineNumbers,
                config.Editor.ShowInvisibles,
                config.Editor.ShowPrintMargin,
                config.Editor.PrintMargin,
                config.Editor.WrapMargin,
                config.Editor.KeyboardHandler,
                config.Editor.EnableBulletAutoCompletion,
                config.Editor.TabSize,
                config.Editor.UseSoftTabs,
                config.Editor.RightToLeft
            };

            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
            return JsonConvert.SerializeObject(style, settings);
        }



        public void EnableSpellChecking(bool enable , string dictionary = "en-us")
        {
            Invoke("enablespellchecking", enable, dictionary);
        }

        /// <summary>
        /// Sets the language syntax for the document
        /// </summary>
        /// <param name="syntax">Syntax like markdown, xmls, csharp and so on</param>
        public void SetLanguage(string syntax)
        {
            Invoke("setlanguage", syntax);
        }


        /// <summary>
        /// Returns a font object
        /// </summary>
        /// <returns></returns>
        public object GetFontSize()
        {
            return Invoke("getfontsize",false);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="show"></param>
        public void SetShowLineNumbers(bool show)
        {
            Invoke("setShowLineNumbers", show);
        }

        public void SetShowInvisibles(bool show)
        {
            Invoke("setShowInvisibles", show);
        }

        public void SetReadOnly(bool show)
        {
            Invoke("setReadOnly", show);
        }
        public void SetWordWrap(bool enable)
        {
            Invoke("setWordWrap", enable);
        }

        public void AdjustPadding(bool forceRefresh)
        {
            Invoke("adjustPadding", forceRefresh);
        }

        /// <summary>
        /// Sets the editor split mode
        /// - Beside, Below, None
        /// </summary>
        /// <param name="location"></param>
        public void Split(string location)
        {
            Invoke("split", location);
        }

        public void ExecCommand(string action, string parm)
        {
            Invoke("execcommand", action, parm);
        }



        #endregion

        //        /// <summary>
        //        /// Sets the flag that determines whether line numbers are shown
        //        /// </summary>
        //        /// <param name="showLineNumbers"></param>
        //        public void SetShowLineNumbers(bool showLineNumbers)
        //        {
        //            Invoke("setShowLineNumbers", showLineNumbers);
        //        }


        #region UndoManager
        /// <summary>
        /// Returns the JavaScript UndoManager
        /// </summary>
        /// <returns></returns>
        public object GetUndoManager()
        {
            var session = ReflectionUtils.GetPropertyExCom(Instance, "editor.session");
            return ReflectionUtils.CallMethodCom(session, "getUndoManager",false);
        }

        public bool HasUndo(object undoManager = null)
        {
            if (undoManager == null)
                undoManager = GetUndoManager();
            return (bool) ReflectionUtils.CallMethodCom(undoManager, "hasUndo", false);
        }
        public bool HasRedo(object undoManager = null)
        {
            if (undoManager == null)
                undoManager = GetUndoManager();
            return (bool)ReflectionUtils.CallMethodCom(undoManager, "hasRedo", false);
        }

        public void Undo()
        {
            ReflectionUtils.CallMethodExCom(Instance, "editor.undo",false);
        }

        public void Redo()
        {
            ReflectionUtils.CallMethodExCom(Instance, "editor.redo",false);
        }

        #endregion

        #region COM Invocation

        private const BindingFlags flags =
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Static | BindingFlags.Instance |
            BindingFlags.IgnoreCase;

        /// <summary>
        /// Invokes a method on the editor by name with parameters
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object Invoke(string method, params object[] parameters)
        {
            // Instance methods have to have a parameter to be found (arguments array)
            if (parameters == null)
                parameters = new object[] {false};
#if DEBUG
            try
            {
#endif
                return InstanceType.InvokeMember(method, flags | BindingFlags.InvokeMethod, null, Instance,
                     parameters );
#if DEBUG
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.GetBaseException().Message);
                throw ex;
            }
#endif
            //return ReflectionUtils.CallMethod(Instance, method, parameters);
        }


        /// <summary>
        /// Invokes a method on the editor by name with parameters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T Invoke<T>(string method, params object[] parameters)
        {
            // Instance methods have to have a parameter to be found (arguments array)
            if (parameters == null)
                parameters = new object[] {false};

            //var res = ReflectionUtils.CallMethod(Instance, method, parameters);
            var res = InstanceType.InvokeMember(method, flags | BindingFlags.InvokeMethod, null, Instance,
                new object[] { parameters });
            if (res == null || res == DBNull.Value)
                return default(T);

            return (T)res;
        }

        /// <summary>
        /// Extended method invocation that can use . syntax to walk
        /// an object property hierarchy. Slower, but provides support
        /// for . and indexer [] syntax.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public object InvokeEx(string method, params object[] parameters)
        {
            // Instance methods have to have a parameter to be found (arguments array)
            if (parameters == null)
                parameters = new object[] {false};

            return ReflectionUtils.CallMethodExCom(Instance, method, parameters);
        }


        /// <summary>
        /// Extended method invocation that can use . syntax to walk
        /// an object property hierarchy.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        /// <<param name="parameters">optional list of parameters. Leave blank if no parameters</param>
        /// <returns></returns>
        public object InvokeEx(object instance, string method, params object[] parameters)
        {
            return ReflectionUtils.CallMethodExCom(instance, method, parameters);
        }


        /// <summary>
        /// Retrieves a property value from the editor by name
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public object Get(string propertyName)
        {
            return InstanceType.InvokeMember(propertyName, flags | BindingFlags.GetProperty, null, Instance, null);
        }


        /// <summary>
        /// Retrieves a property from the editor by name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public T Get<T>(string propertyName)
        {
            var res = InstanceType.InvokeMember(propertyName, flags | BindingFlags.GetProperty, null, Instance, null);
            if (res == null || res == DBNull.Value)
                return default(T);

            return (T) res;
        }

        /// <summary>
        /// Sets a property on the editor by name
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public void Set(string propertyName, object value)
        {
            InstanceType.InvokeMember(propertyName, flags | BindingFlags.SetProperty, null, Instance, new [] { value });
            //ReflectionUtils.SetProperty(Instance, property, value);
        }
        #endregion
    }
}

