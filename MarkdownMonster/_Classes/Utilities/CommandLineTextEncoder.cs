using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Westwind.Utilities;

namespace MarkdownMonster.Utilities
{
    /// <summary>
    /// Handles command line text encoding for files that
    /// can be opened via the command line.
    ///
    /// Format:
    ///
    ///
    /// untitled.base64,base64text
    /// untitled.urlencoded,urlencodedText
    /// untitled.text,text
    /// </summary>
    public class CommandLineTextEncoder
    {

        public static string CreateEncodedCommandLineFilename(string text, int line = 0,
            CommandLineTextEncodingFormats format = CommandLineTextEncodingFormats.Base64)
        {
            if (string.IsNullOrEmpty(text))
                return "untitled";

            string encodedFile = null;

            if (format == CommandLineTextEncodingFormats.Base64)
                encodedFile = "untitled.base64," + ToBase64(text);
            else if(format == CommandLineTextEncodingFormats.UrlEncoded)
                encodedFile = "untitled.urlencoded," + WebUtility.UrlEncode(text);
            else if(format == CommandLineTextEncodingFormats.Json)
                encodedFile = "untitled.json," + JsonConvert.SerializeObject(text);
            else if(format == CommandLineTextEncodingFormats.PlainText)
                encodedFile = "untitled.text," + text;
            else if(format == CommandLineTextEncodingFormats.PreEncodedBase64)
                encodedFile = "untitled.base64," + text;

            if(line > 0)
                encodedFile += "," + line;

            return encodedFile;
        }


        /// <summary>
        /// Parses out the decoded text from the URL formatted text
        /// </summary>
        /// <returns></returns>
        public static string ParseUntitledString(string untitledText)
        {
            string text = null;

            try
            {
                if (untitledText.StartsWith("untitled.base64,", StringComparison.OrdinalIgnoreCase))
                {
                    text = untitledText.Substring("untitled.base64,".Length);
                    text = FromBase64(text);
                }
                else if (untitledText.StartsWith("untitled.urlencoded,", StringComparison.OrdinalIgnoreCase))
                {
                    text = untitledText.Substring("untitled.urlencoded,".Length);
                    text = FromBase64(WebUtility.UrlDecode(text));
                }
                else if (untitledText.StartsWith("untitled.text,", StringComparison.OrdinalIgnoreCase))
                {
                    text = untitledText.Substring("untitled.text,".Length);
                }
                else if (untitledText.StartsWith("untitled.json,", StringComparison.OrdinalIgnoreCase))
                {
                    text = untitledText.Substring("untitled.json,".Length);
                    if (!text.StartsWith("\'") && !text.StartsWith("\""))
                        text = "\"" + text + "\"";
                    text = JsonSerializationUtils.Deserialize(text, typeof(string)) as string;
                }
                
            }
            catch
            {
                text = "Invalid text formatting for imported text.";
            }

            return text;
        }

        #region Conversions

        
        /// <summary>
        /// Encodes a string into a Base64 string.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="encoding">If not specified Encoding.UTF8 is used</param>
        /// <returns></returns>
        public static string ToBase64(string text, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return Convert.ToBase64String(encoding.GetBytes(text));
        }

        /// <summary>
        /// Decodes a base64 string back to a plain string
        /// </summary>
        /// <param name="base64Text"></param>
        /// <param name="encoding">If not specified Encoding.UTF8 is used</param>
        /// <returns></returns>
        public static string FromBase64(string base64Text, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(base64Text))
                return base64Text;
            if (encoding == null)
                encoding = Encoding.UTF8;

            return encoding.GetString(Convert.FromBase64String(base64Text));
        }
        
        #endregion 
    }

    public enum CommandLineTextEncodingFormats
    {
        // Text is encoded to base64
        Base64,

        // Text is encoded to UrlEncoded 
        UrlEncoded,

        // JSON string with or without quotes
        Json,

        // Text is embedded as is. Not recommended as it can't express line breaks and other characters easily.
        // Use for internal exposure only, not for command line passing
        PlainText,
        // Text is passed as base64 and not encoded
        PreEncodedBase64
    }
}
