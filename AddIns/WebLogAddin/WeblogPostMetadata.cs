using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using MarkdownMonster;
using WebLogAddin.MetaWebLogApi;
using Westwind.Utilities;


namespace WeblogAddin
{
    public class WeblogPostMetadata : INotifyPropertyChanged
    {
        private string _title;
        private string _abstract;
        private bool _isDraft;

        /// <summary>
        /// The post id - empty on new entries, and set to 
        /// the id generated on the server after a post 
        /// was uploaded.
        /// </summary>
        public string PostId { get; set; }


        /// <summary>
        /// The title of the post derived from the document's header
        /// </summary>
        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        /// <summary>
        /// This should hold the sanitized markdown text
        /// stripped of the config data.
        /// </summary>
        public string MarkdownBody { get; set; }

        /// <summary>
        /// Url that is mapped to wp_thumbnail
        /// </summary>
        public string FeaturedImageUrl { get; set; }

        /// <summary>
        /// Featured Image Id used for Wordpress
        /// </summary>
        public string FeatureImageId { get; set; }

        /// <summary>
        /// This should hold the raw markdown text retrieved
        /// from the editor which will contain the meta post data
        /// </summary>
        public string RawMarkdownBody { get; set; }

        /// <summary>
        /// Short abstract text for the post.
        /// </summary>
        public string Abstract
        {
            get { return _abstract; }
            set
            {
                if (value == _abstract) return;
                _abstract = value;
                OnPropertyChanged(nameof(Abstract));
            }
        }

        /// <summary>
        /// Keywords that are added to the post when published.
        /// Typically these are added to the document meta data.
        /// </summary>
        public string Keywords { get; set; }

        /// <summary>
        /// Categories that this post is associated with
        /// </summary>
        public string Categories { get; set; }

        /// <summary>
        /// The Weblog that this post is to be sent to
        /// </summary>
        public string WeblogName { get; set; }


        /// <summary>
        /// A collection of custom fields that are uploaded to the server
        /// </summary>
        public IDictionary<string,CustomField> CustomFields { get; set;}

        public bool IsDraft
        {
            get { return _isDraft; }
            set
            {
                if (value == _isDraft) return;
                _isDraft = value;
                OnPropertyChanged(nameof(IsDraft));
            }
        }


        /// <summary>
        /// Strips the Markdown Meta data from the message and populates
        /// the post structure with the meta data values.
        /// </summary>
        /// <param name="markdown"></param>
        /// <param name="post"></param>
        /// <param name="weblogInfo"></param>
        /// <returns></returns>
        public static WeblogPostMetadata GetPostConfigFromMarkdown(string markdown, Post post, WeblogInfo weblogInfo)
        {
            var meta = new WeblogPostMetadata()
            {
                RawMarkdownBody = markdown,
                MarkdownBody = markdown,
                WeblogName = WeblogAddinConfiguration.Current.LastWeblogAccessed,
                CustomFields = new Dictionary<string, CustomField>()
            };

            // check for title in first line and remove it 
            // since the body shouldn't render the title
            var lines = StringUtils.GetLines(markdown, 20);
            if (lines.Length > 0 && lines[0].StartsWith("# "))
            {
                if (weblogInfo.Type != WeblogTypes.Medium) // medium wants the header in the text
                    meta.MarkdownBody = meta.MarkdownBody.Replace(lines[0], "").Trim();

                meta.Title = lines[0].Trim().Substring(2);
            }
            else if (lines.Length > 2 && lines[0] == "---" && meta.MarkdownBody.Contains("layout: post"))
            {
                var block = mmFileUtils.ExtractString(meta.MarkdownBody, "---", "---", returnDelimiters: true);
                if (!string.IsNullOrEmpty(block))
                {
                    meta.Title = StringUtils.ExtractString(block, "title: ", "\n").Trim();
                    meta.MarkdownBody = meta.MarkdownBody.Replace(block, "").Trim();
                }
            }


            string configString = StringUtils.ExtractString(markdown,
                "<!-- Post Configuration -->",
                "<!-- End Post Configuration -->",
                caseSensitive: false, allowMissingEndDelimiter: true, returnDelimiters: true);

            if (string.IsNullOrEmpty(configString))
                return meta;

            // strip the config section
            meta.MarkdownBody = meta.MarkdownBody.Replace(configString, "");

            configString = StringUtils.ExtractString(configString,
                "\n```xml",
                "```",
                caseSensitive: false, allowMissingEndDelimiter: true, returnDelimiters: false);

            var config = XElement.Parse(configString);

            string title = config.Element("title")?.Value.Trim();
            if (string.IsNullOrEmpty(meta.Title))
                meta.Title = title;
            meta.Abstract = config.Element("abstract")?.Value.Trim();
            meta.Keywords = config.Element("keywords")?.Value.Trim();
            meta.Categories = config.Element("categories")?.Value.Trim();
            meta.PostId = config.Element("weblogs")?.Element("postid")?.Value.Trim();
            string strIsDraft = config.Element("isDraft")?.Value.Trim();
            bool isDraft;
            if (strIsDraft != null && bool.TryParse(strIsDraft, out isDraft))
                meta.IsDraft = isDraft;
            string weblogName = config.Element("weblogs")?.Element("weblog")?.Value.Trim();
            if (!string.IsNullOrEmpty(weblogName))
                meta.WeblogName = weblogName;

            string featuredImageUrl = config.Element("featuredImage")?.Value.Trim();
            if (!string.IsNullOrEmpty(featuredImageUrl))
                meta.FeaturedImageUrl = featuredImageUrl;

            var customFields = config.Element("customFields");
            if (customFields != null)
            {
                foreach (var customField in customFields.Elements("customField"))
                {
                    var key = customField.Element("key")?.Value;
                    var value = customField?.Element("value")?.Value;
                    var id = customField?.Element("id")?.Value;
                    meta.CustomFields.Add(key, new CustomField { Key = key, Value = value, ID = id });
                }
            }

            post.Title = meta.Title;
            post.Categories = meta.Categories.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            post.Tags = meta.Keywords.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            post.mt_excerpt = meta.Abstract;
            post.mt_keywords = meta.Keywords;

            return meta;
        }

        /// <summary>
        /// This method sets the RawMarkdownBody
        /// </summary>
        /// <returns>Updated Markdown - also sets the RawMarkdownBody and MarkdownBody</returns>
        public string SetConfigInMarkdown()
        {
            var meta = this;
            string markdown = meta.RawMarkdownBody;


            var config = new XElement("blogpost",
                new XElement("title", meta.Title),
                new XElement("abstract", meta.Abstract),
                new XElement("categories", meta.Categories),
                new XElement("keywords", meta.Keywords),
                new XElement("isDraft", meta.IsDraft),
                new XElement("featuredImage", meta.FeaturedImageUrl),
                GetCustomFieldsXml(meta.CustomFields),
                new XElement("weblogs",
                    new XElement("postid", meta.PostId),
                    new XElement("weblog", meta.WeblogName)));

            string origConfig = StringUtils.ExtractString(markdown, "<!-- Post Configuration -->", "<!-- End Post Configuration -->", false, false, true);
            string newConfig = $@"<!-- Post Configuration -->
<!--
```xml
{config}
```
-->
<!-- End Post Configuration -->";

            if (string.IsNullOrEmpty(origConfig))
            {
                markdown += "\r\n" + newConfig;
            }
            else
                markdown = markdown.Replace(origConfig, newConfig);

            meta.RawMarkdownBody = markdown;
            meta.MarkdownBody = meta.RawMarkdownBody.Replace(newConfig, "");

            return markdown;
        }

        private static XElement GetCustomFieldsXml(IDictionary<string, CustomField> customFields)
        {
            if (customFields == null)
                return null;

            return new XElement(
                "customFields",
                from cf in customFields.Values
                select new XElement(
                    "customField",
                    new XElement("key", cf.Key),
                    new XElement("value", cf.Value),
                    string.IsNullOrEmpty(cf.ID) ? null : new XElement("id", cf.ID)));
        }

        #region INotify Property Changed

        public event PropertyChangedEventHandler PropertyChanged;

        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}