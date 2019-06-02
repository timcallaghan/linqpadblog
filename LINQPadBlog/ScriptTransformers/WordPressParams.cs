using System;
using System.Collections.Generic;

namespace Scombroid.LINQPadBlog.ScriptTransformers
{
    public class WordPressParams : IScriptTransformParams
    {
        public string PostTitle { get; set; }
        public int? PostID { get; set; }
        public DateTime PostDateUtc { get; set; }
        public string BaseApiUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PostStatus { get; set; }
        public string Format { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
    }
}
