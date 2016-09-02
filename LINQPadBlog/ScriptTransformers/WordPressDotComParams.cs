namespace Scombroid.LINQPadBlog.ScriptTransformers
{
    public class WordPressDotComParams : IScriptTransformParams
    {
        public string PostTitle { get; set; }
        public int? PostID { get; set; }
        public string BaseUrl { get; set; }
        public int BlogId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PostStatus { get; set; }
        public string PostType { get; set; }
    }
}
