namespace Scombroid.LINQPadBlog.Utils
{
    public class LinqPadScriptInfo
    {
        public ProcessedArgs ProcessedArgs { get; set; }
        public LinqPadWebResources LinqPadWebResources { get; set; }
        public string CurrentQueryPath { get; set; }
        public string Path { get; set; }
        public string Header { get; set; }
        public string QueryKind { get; set; }
        public string ScriptContents { get; set; }
        public string ScriptOutput { get; set; }
    }
}
