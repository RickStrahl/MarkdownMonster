using System.Collections.Generic;

namespace MarkdownMonster.AddIns
{
    public interface IMarkdownMonsterAddin
    {
        List<AddInMenuItem> MenuItems { get; set; }

        AppModel Model { get; set; }
        
        void OnAfterOpenFile();
        void OnAfterSaveFile();
        void OnApplicationShutdown();
        void OnApplicationStart();
        bool OnBeforeOpenFile();
        bool OnBeforeSaveFile();
        void OnDocumentActivated();
    }
}