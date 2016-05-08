namespace WeblogAddin
{
    public class WeblogInfo
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ApiUrl { get; set; }

        public WeblogTypes Type { get; set; } = WeblogTypes.MetaWeblogApi;

        /// <summary>
        /// Url used to preview the post. The postId can be embedded into 
        /// the value by using {0}.
        /// </summary>
        public string PreviewUrl { get; set; }
    }

    public enum WeblogTypes
    {
        MetaWeblogApi,
        Wordpress,
        Unknown
    }
}