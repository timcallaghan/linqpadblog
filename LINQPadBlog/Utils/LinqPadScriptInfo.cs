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

        public string GetScriptLangName()
        {
            var lang = Globals.HighlightJs.Syntax.csharp;
            switch (QueryKind)
            {
                case Globals.LINQPad.QueryKind.VBExpression:
                case Globals.LINQPad.QueryKind.VBStatements:
                case Globals.LINQPad.QueryKind.VBProgram:
                    lang = Globals.HighlightJs.Syntax.vb;
                    break;
                case Globals.LINQPad.QueryKind.FSharpExpression:
                case Globals.LINQPad.QueryKind.FSharpProgram:
                    lang = Globals.HighlightJs.Syntax.fsharp;
                    break;
                case Globals.LINQPad.QueryKind.SQL:
                    lang = Globals.HighlightJs.Syntax.sql;
                    break;
            }

            return lang;
        }
    }
}
