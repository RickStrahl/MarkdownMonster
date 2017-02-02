<%@ WebHandler Language="C#" Class="MarkdownMonsterService" %>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using Westwind.Web;
using Westwind.Web.JsonSerializers;
using Westwind.Utilities;
using System.Web.Routing;
using MarkdownMonster.AddIns;

public class MarkdownMonsterService : CallbackHandler
{
    public static bool FirstAccess;

    static MarkdownMonsterService()
    {

    }

    public MarkdownMonsterService()
    {
        FirstAccess = false;
    }

    [CallbackMethod(RouteUrl = "Addins")]
    public List<AddinItem> AddinList()
    {
        var manager = new AddinManager();
        var addins = manager.GetAddinListAsync().Result;
        return addins;
    }
}