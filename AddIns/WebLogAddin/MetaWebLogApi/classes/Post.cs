using System;
using System.Collections.Generic;
using System.Linq;

namespace WebLogAddin.MetaWebLogApi
{
    public class Post
    {
        public Post()
        {
            DateCreated = DateTime
                .Now;
        }

        public object PostId { get; set; }

        public DateTime DateCreated { get; set; }
        public string Body { get; set; }
        public string Title { get; set; }
        public string Permalink { get; set; }
        public string[] Categories { get; set; }
        public string[] Tags { get; set; }
        public CustomField[] CustomFields { get; set; }
        public Term[] Terms { get; set; }
        public string PostType { get; set; }

        public string mt_excerpt { get; set; }
        public string mt_keywords { get; set; }

        public string mt_text_more { get; set; }

        public string wp_post_thumbnail { get; set; }
        public string post_content { get; set; }

        public string Url { get; set; }
        public override string ToString()
        {
            return Body;
        }


        /// <summary>
        /// Gets the custom field for a given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public CustomField GetCustomField(string key)
        {
            return CustomFields?.FirstOrDefault(cf => cf.Key == key);
        }


        /// <summary>
        /// Adds or updates a custom field to the existing custom fields list
        /// </summary>
        /// <param name="customField"></param>
        public void SetCustomField(CustomField customField)
        {
            var list = new List<CustomField>();
            if (CustomFields != null)
                list.AddRange(CustomFields);

            var field = list.FirstOrDefault(cf => cf == customField);
            if (field != null)
            {
                field.ID = customField.ID;
                field.Value = customField.Value;
                return;
            }
            
            list.Add(customField);
            CustomFields = list.ToArray();
        }
    }
}