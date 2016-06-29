<%@ WebHandler Language="C#" Class="BugReportService" %>

using System;
using System.IO;
using System.Web;
using Westwind.Web;
using Westwind.Web.JsonSerializers;
using Westwind.Utilities;
using System.Web.Routing;

public class BugReportService : CallbackHandler
{
    public static bool FirstAccess;
    static string BugReportFilePath;

    static BugReportService()
    {
        JSONSerializer.DefaultJsonParserType = SupportedJsonParserTypes.JsonNet;
        BugReportFilePath = HttpContext.Current.Server.MapPath("~/bugreport/bugreports.txt");
    }

    public BugReportService()
    {
        FirstAccess = false;
    }

    [CallbackMethod(RouteUrl = "bugreport")]
    public Bug ReportBug(Bug bug)
    {
        string msg = $@"{bug.Message}    
{bug.Product} v{bug.Version}
{bug.StackTrace}
";

        StringUtils.LogString(msg, BugReportFilePath);

        return bug;
    }

    [CallbackMethod()]
    public bool TruncateFile()
    {
        long cutoff = 100000;
        var fi = new FileInfo(BugReportFilePath);
        if (fi.Length < cutoff)
            return true;

        using (MemoryStream ms = new MemoryStream((int)cutoff))
        {
            using (FileStream s = new FileStream(BugReportFilePath, FileMode.Open, FileAccess.ReadWrite))
            {
                s.Seek(cutoff, SeekOrigin.End);
                s.CopyTo(ms);
                s.SetLength(cutoff);
                s.Position = 0;
                ms.CopyTo(s);
            }
        }

        return true;
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