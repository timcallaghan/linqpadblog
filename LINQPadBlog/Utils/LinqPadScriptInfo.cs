using System.Collections.Generic;

namespace Scombroid.LINQPadBlog.Utils
{
    public class LinqPadScriptInfo
    {
        public ProcessedArgs ProcessedArgs { get; set; }
        public LinqPadWebResources LinqPadWebResources { get; set; }
        public string Header { get; set; }
        public string QueryKind { get; set; }
        public List<ScriptContentSection> ScriptContents { get; set; }
        public string ScriptOutput { get; set; }
    }
}
