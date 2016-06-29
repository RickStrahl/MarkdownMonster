<%@ WebHandler Language="C#" Class="BugReportService" %>

using System;
using System.Web;
using Westwind.Web;
using Westwind.Web.JsonSerializers;
using Westwind.Utilities;
using System.Web.Routing;

public class BugReportService : CallbackHandler {
    public static bool FirstAccess;

    static BugReportService()
    {
        JSONSerializer.DefaultJsonParserType = SupportedJsonParserTypes.JsonNet;
     }

    public BugReportService()
    {
        FirstAccess = false;
    }

    [CallbackMethod(RouteUrl="bugreport")]
    public Bug ReportBug(Bug bug)
    {
        string msg = $@"{bug.Message}    
{bug.Product} v{bug.Version}
{bug.StackTrace}
";


        StringUtils.LogString(msg, Context.Server.MapPath("~/bugreports.txt"));

        return bug;
    }
}

public class Bug
{
    public DateTime TimeStamp { get; set; }
    public string Message { get; set; }
    public string Product { get; set; }
    public string Version { get; set; }
    public string StackTrace { get; set; }
}