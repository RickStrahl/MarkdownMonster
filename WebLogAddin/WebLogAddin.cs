using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FontAwesome.WPF;
using MarkdownMonster.AddIns;

namespace WebLogAddin
{
    public class WebLogAddin :  MarkdownMonsterAddin, IMarkdownMonsterAddin
    {
        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            var menuItem = new AddInMenuItem()
            {
                Caption = "Publish to WebLog",
                EditorCommand = "weblog",
                FontawesomeIcon = FontAwesomeIcon.Wordpress
            };
            menuItem.Execute = new Action<object>(WebLogAddin_Execute);

            this.MenuItems.Add(menuItem);
        }

        public void WebLogAddin_Execute(object sender)
        {
            var editor = Model.ActiveEditor;
            if (editor == null)
                return;

            MessageBox.Show("WebLogAddin Fired and ready to publish this post to your Web log","Markdown Monster",MessageBoxButton.OK,MessageBoxImage.Information);
        }
    }

}
