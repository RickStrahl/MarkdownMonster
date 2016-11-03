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
    static string TelemetryFilePath;

    static BugReportService()
    {
        JSONSerializer.DefaultJsonParserType = SupportedJsonParserTypes.JsonNet;
        BugReportFilePath = HttpContext.Current.Server.MapPath("~/bugreport/bugreports.txt");
        TelemetryFilePath = HttpContext.Current.Server.MapPath("~/bugreport/telemetry.txt");
    }

    public BugReportService()
    {
        FirstAccess = false;
    }

    [CallbackMethod(RouteUrl = "bugreport")]
    public Bug ReportBug(Bug bug)
    {
        string msg = $@"{bug.Message}    
{bug.Product} v{bug.Version} - {Context.Request.ServerVariables["REMOTE_ADDR"]}
{bug.StackTrace}
";
        bug.TimeStamp = DateTime.Now;

        StringUtils.LogString(msg, BugReportFilePath);

        return bug;
    }

    [CallbackMethod(RouteUrl = "Telemetry")]
    public string Telemetry(Telemetry telemetry)
    {
        StringUtils.LogString( telemetry.Version + " - " +
                               telemetry.Operation + " - " +
                               (telemetry.Registered ? "YES" : "no") + " - " +
                               telemetry.Access + " - " +
                               Context.Request.ServerVariables["REMOTE_ADDR"]   + " - " +
                               telemetry.Time.ToString("n0") + "s -" +
                               telemetry.Data,
                               TelemetryFilePath);

        return "ok";
    }

    [CallbackMethod()]
    public bool TruncateFile()
    {
        var filename = BugReportFilePath;
        if (!string.IsNullOrEmpty(Context.Request.QueryString["telemetry"]))
            filename = TelemetryFilePath;

        long cutoff = 100000;
        var fi = new FileInfo(filename);
        if (fi.Length < cutoff)
            return true;

        using (MemoryStream ms = new MemoryStream((int)cutoff))
        {
            using (FileStream s = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite))
            {
                long loc = s.Seek(cutoff * -1, SeekOrigin.End);
                s.CopyTo(ms);
                s.SetLength(0);
                s.Flush();

                ms.Position = 0;                
                ms.CopyTo(s);
                s.Flush();
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

public class Telemetry
{
    public string Version { get; set; }
    public bool Registered { get; set; }
    public string Operation { get; set;  }
    public string Data { get; set; }
    public int Time { get; set; }
    public int Access { get; set; }
}