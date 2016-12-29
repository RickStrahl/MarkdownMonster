<%@ WebHandler Language="C#" Class="MediumApiCallback" %>

using System;
using System.IO;
using System.Text;
using System.Web;
using Westwind.Web;
using Westwind.Web.JsonSerializers;
using Westwind.Utilities;
using System.Web.Routing;

public class MediumApiCallback : CallbackHandler
{
    static MediumApiCallback()
    {
        JSONSerializer.DefaultJsonParserType = SupportedJsonParserTypes.JsonNet;
    }

}