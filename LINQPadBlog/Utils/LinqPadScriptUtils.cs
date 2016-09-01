using LINQPad;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Scombroid.LINQPadBlog.Utils
{
    public static class LinqPadScriptUtils
    {
        public static LinqPadScriptInfo LoadLINQPadScriptInfo(ProcessedArgs processedArgs)
        {
            // LINQPad files consists of an xml header section, followed by a blank line separator, followed by the script/code.
            // Typical file contents look like:
            // 	<Query Kind="Program">
            // 	...
            // 	</Query>
            //
            // 	Script content starts here...

            var fileHeaderLines = new StringBuilder();
            var fileContentLines = new StringBuilder();
            bool foundSeparator = false;

            using (var sr = new StreamReader(processedArgs.FilePath.FullName))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (!foundSeparator)
                    {
                        if (line.Equals(string.Empty))
                        {
                            foundSeparator = true;
                        }
                        else
                        {
                            fileHeaderLines.AppendLine(line);
                        }
                    }
                    else
                    {
                        fileContentLines.AppendLine(line);
                    }
                }
            }

            var linqPadScriptInfo = new LinqPadScriptInfo()
            {
                ProcessedArgs = processedArgs,
                CurrentQueryPath = Util.CurrentQueryPath,
                Path = processedArgs.FilePath.FullName,
                Header = fileHeaderLines.ToString(),
                ScriptContents = fileContentLines.ToString()
            };

            // Query kind will be one of:
            // Expression, Statements, Program, VBExpression, VBStatements, VBProgram, FSharpExpression, FSharpProgram, SQL, ESQL
            XDocument doc = XDocument.Parse(linqPadScriptInfo.Header);
            linqPadScriptInfo.QueryKind = doc.Root.Attribute(Globals.LINQPad.QueryKindAttributeName).Value;

            // Run the *.linq file and capture the html output fragments from calls to .Dump() within the file
            linqPadScriptInfo.ScriptOutput = Util.Run(
                processedArgs.FilePath.FullName,
                QueryResultFormat.HtmlFragment,
                processedArgs.AdditionalArgs
            ).AsString();

            // Extract the standard LINQPad css and javascript text
            linqPadScriptInfo.LinqPadWebResources = LinqPadWebResources.Generate();

            return linqPadScriptInfo;
        }
    }
}
