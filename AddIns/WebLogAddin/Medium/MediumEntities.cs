using System.Collections.Generic;

namespace WebLogAddin.Medium
{


    public class MediumUserResult
    {
        public MediumUser data { get; set; }
    }

    public class MediumUser
    {
        public string id { get; set; }
        public string username { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string imageUrl { get; set; }
    }


    public class MediumPublicationsResult
    {
        public List<MediumPublication> data { get; set; }
    }
    public class MediumPublication
    {
        public string id { get; set; }
        public string userId { get; set; }
        public string role { get; set; }

        public string name { get; set; }
        public string description { get; set; }
        public string imageUrl { get; set; }    
    }


    public class MediumPostResult
    {
        public MediumPost data { get; set; }
    }
    public class MediumPost
    {
        public string id { get; set; }

        public string title { get; set; }
        public string contentFormat { get; set; }
        public string content { get; set; }
        public string canonicalUrl { get; set; }

        public string url { get; set; }

        public string[] tags { get; set; }

        /// <summary>
        /// public, draft, unlisted
        /// </summary>
        public string publishStatus { get; set; } = "public";

        /// <summary>
        /// “all-rights-reserved”, “cc-40-by”, “cc-40-by-sa”, “cc-40-by-nd”, “cc-40-by-nc”, “cc-40-by-nc-nd”, “cc-40-by-nc-sa”, “cc-40-zero”, “public-domain”. The default is “all-rights-reserved”.
        /// </summary>
        public string license { get; set; } = "all-rights-reserved";

        public bool notifyFollowers { get; set; }
    }


    public class MediumImageResult
    {
        public MediumImage data { get; set; }
    }

    public class MediumImage
    {
        public string url { get; set; }
        public string md5 { get; set; }
    }
    
}