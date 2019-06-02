namespace Scombroid.LINQPadBlog
{
    public class Globals
    {
        public const string AppName = "LINQPadBlog";

        // DOM manipulation
        public class DOM
        {
            public const string HeadNodePath = "/html/head";
            public const string BodyNodePath = "/html/body";
            public const string StyleNodePath = "/html/head/style";
            public const string ScriptNodePath = "/html/head/script";
        }

        // LINQPad
        public class LINQPad
        {
            public const string QueryKindAttributeName = "Kind";

            public class QueryKind
            {
                // LINQPad Query kind can currently be one of:
                // Expression, Statements, Program, VBExpression, VBStatements, VBProgram, FSharpExpression, FSharpProgram, SQL, ESQL
                public const string CSharpExpression = "Expression";
                public const string CSharpStatements = "Statements";
                public const string CSharpProgram = "Program";
                public const string VBExpression = "VBExpression";
                public const string VBStatements = "VBStatements";
                public const string VBProgram = "VBProgram";
                public const string FSharpExpression = "FSharpExpression";
                public const string FSharpProgram = "FSharpProgram";
                public const string SQL = "SQL";
                public const string ESQL = "ESQL";
            }
        }

        public class Comments
        {
            // C# multi-line comments
            public const string CSharpStart = "/*";
            public const string CSharpEnd = "*/";
            // F# multi-line comments
            public const string FSharpStart = "(*";
            public const string FSharpEnd = "*)";
            // Allows embedding of code blocks in comments (code won't be compiled/processed by LINQPad)
            public const string NonCompiledCodeStart = "[[Code]]";
            public const string NonCompiledCodeEnd = "[[/Code]]";
            // Allows specification of where dump output will appear in the generated file
            public const string DumpStart = "[[Dump]]";
            public const string DumpEnd = "[[/Dump]]";
        }

        // LATEX/MathJax
        public class LATEX
        {
            public const string IntermediateDollarSign = "d_o_l_l_a_r";
            public const string InlineMathStart = @"\\(";
            public const string InlineMathEnd = @"\\)";
            public const string StandAloneMathMarker = "$$";
        }

        // FileSystemLinqScriptTransformer
        public class FileSystem
        {
            public const string LINQPadCssFileName = "linqpad.css";
            public const string LINQPadJsFileName = "linqpad.js";
            public const string ResourcesFolderName = "resources";
            public const string CodeSectionStart = @"<pre><code>";
            public const string CodeSectionEnd = @"</code></pre>";
            public const string HighlightJsCssCdnUri = @"https://cdnjs.cloudflare.com/ajax/libs/highlight.js/9.15.8/styles/default.min.css";
            public const string HighlightJsCdnUri = @"https://cdnjs.cloudflare.com/ajax/libs/highlight.js/9.15.8/highlight.min.js";
            public const string HighlightJsLoadStatement = @"hljs.initHighlightingOnLoad();";
            public const string MathJaxCdnUri = @"https://cdnjs.cloudflare.com/ajax/libs/mathjax/2.7.5/latest.js?config=TeX-MML-AM_CHTML";
        }

        // WordPressDotComLinqScriptTransformer
        public class WordPressCom
        {
            public const string LATEX = "latex";
            public const string LATEXMarker = "$";
            public const string CodeSectionStart = @"[code language=""{0}""]";
            public const string CodeSectionEnd = @"[/code]";
            public const string BaseAPIUri = @"https://public-api.wordpress.com/rest/v1.1/sites/";

            public class Syntax
            {
                public const string csharp = "csharp";
                public const string vb = "vb";
                public const string fsharp = "fsharp";
                public const string sql = "sql";
            }
        }   
    }
}
