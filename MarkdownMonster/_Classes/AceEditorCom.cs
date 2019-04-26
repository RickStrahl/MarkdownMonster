using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using Westwind.Utilities;

namespace MarkdownMonster
{

    /// <summary>
    /// Wrapper around the Ace Editor COM interface
    /// </summary>
    public class AceEditorCom
    {
        public object Instance { get; }

        public Type InstanceType { get;  }

        public AceEditorCom(object instance)
        {
            Instance = instance;
            InstanceType = instance.GetType();
        }

        #region Selections

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
        /// <param name="pos">Previously captured position (column, row props)</param>
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


        /// <summary>
        /// Method used to send Configuration to the editor.
        /// Sets things like font sizes, Word Wrap, padding etc.
        /// Called from Markdown Document.
        ///
        /// Use <seealso cref="MarkdownDocumentEditor.RestyleEditor" /> instead.
        /// </summary>
        /// <param name="jsonStyleInfo"></param>
        public void SetEditorStyle(string jsonStyleInfo)
        {
            Invoke("setEditorStyle", jsonStyleInfo);
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
        public void SetIsDirty(bool isDirty)
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

            //var res = ReflectionUtils.CallMethod(Instance, method, parameters);
            var res = InstanceType.InvokeMember(method, flags | BindingFlags.InvokeMethod, null, Instance,
                new object[] { parameters });
            if (res == null || res == DBNull.Value)
                return default(T);

            return (T)res;
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

        public void AdjustPadding(bool forceRefresh)
        {
            Invoke("adjustPadding", forceRefresh);
        }

        public void SetFocus()
        {
            Invoke("setfocus", false);
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
    }
}

