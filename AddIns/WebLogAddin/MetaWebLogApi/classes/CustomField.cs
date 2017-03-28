using System.Diagnostics;

namespace WebLogAddin.MetaWebLogApi
{
    /// <summary>
    /// Custom field info attached to a blog item.
    /// </summary>
    [DebuggerDisplay("{Key},{Value}")]
    public class CustomField
    {
        public string Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
