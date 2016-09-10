using LINQPad;
using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Scombroid.LINQPadBlog.Utils
{
    public static class LinqPadScriptUtils
    {
        public static LinqPadScriptInfo LoadLINQPadScriptInfo(ProcessedArgs processedArgs, string stripMeFromFile)
        {
            // Take a copy of the file
            var tempFile = new FileInfo(Path.Combine(Path.GetTempPath(), processedArgs.FilePath.Name));

            string fileContents = null;
            if (string.IsNullOrWhiteSpace(stripMeFromFile))
            {
                fileContents = File.ReadAllText(processedArgs.FilePath.FullName);
            }
            else
            {
                // Strip out passed string from file contents (prevent circular call cycle)
                // Probably a better way to do this using CallContext
                var fileContentBuilder = new StringBuilder();
                foreach (var line in File.ReadLines(processedArgs.FilePath.FullName))
                {
                    if (!line.Contains(stripMeFromFile))
                    {
                        fileContentBuilder.AppendLine(line);
                    }
                }

                fileContents = fileContentBuilder.ToString();
            }

            File.WriteAllText(tempFile.FullName, fileContents);

            // LINQPad files consist of an xml header section, followed by a blank line separator, followed by the script/code.
            // Typical file contents look like:
            // 	<Query Kind="Program">
            // 	...
            // 	</Query>
            //
            // 	Script content starts here...

            var fileHeaderLines = new StringBuilder();
            var fileContentLines = new StringBuilder();
            bool foundSeparator = false;

            using (var sr = new StreamReader(tempFile.FullName))
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
                Header = fileHeaderLines.ToString()
            };

            // Query kind will be one of:
            // Expression, Statements, Program, VBExpression, VBStatements, VBProgram, FSharpExpression, FSharpProgram, SQL, ESQL
            XDocument doc = XDocument.Parse(linqPadScriptInfo.Header);
            if (doc == null)
                throw new NotSupportedException($"{Globals.AppName} error: The supplied linq file could not be parsed. Make sure the file is a valid LINQPad file.");

            linqPadScriptInfo.QueryKind = doc.Root.Attribute(Globals.LINQPad.QueryKindAttributeName).Value;

            if (!IsQuerySupported(linqPadScriptInfo.QueryKind))
                throw new NotSupportedException($"{Globals.AppName} does not support query kind of {linqPadScriptInfo.QueryKind}");

            var scriptContentParser = new ScriptContentParser(
                GetCommentStartTag(linqPadScriptInfo.QueryKind), 
                GetCommentEndTag(linqPadScriptInfo.QueryKind), 
                fileContentLines.ToString());
            linqPadScriptInfo.ScriptContents = scriptContentParser.ScriptContentSections;

            // Run the *.linq file and capture the html output fragments from calls to .Dump() within the file
            linqPadScriptInfo.ScriptOutput = Util.Run(
                tempFile.FullName,
                QueryResultFormat.HtmlFragment,
                processedArgs.AdditionalArgs
            ).AsString();

            // Extract the standard LINQPad css and javascript text
            linqPadScriptInfo.LinqPadWebResources = LinqPadWebResources.Generate();

            tempFile.Delete();

            return linqPadScriptInfo;
        }

        private static bool IsQuerySupported(string queryKind)
        {
            return queryKind == Globals.LINQPad.QueryKind.CSharpExpression
                || queryKind == Globals.LINQPad.QueryKind.CSharpStatements
                || queryKind == Globals.LINQPad.QueryKind.CSharpProgram;
        }

        private static string GetCommentStartTag(string queryKind)
        {
            if
                (
                queryKind == Globals.LINQPad.QueryKind.CSharpExpression
                || queryKind == Globals.LINQPad.QueryKind.CSharpStatements
                || queryKind == Globals.LINQPad.QueryKind.CSharpProgram
                )
            {
                return Globals.Comments.CSharpStart;
            }

            throw new NotSupportedException($"{Globals.AppName} does not support query kind of {queryKind}");
        }

        private static string GetCommentEndTag(string queryKind)
        {
            if
                (
                queryKind == Globals.LINQPad.QueryKind.CSharpExpression
                || queryKind == Globals.LINQPad.QueryKind.CSharpStatements
                || queryKind == Globals.LINQPad.QueryKind.CSharpProgram
                )
            {
                return Globals.Comments.CSharpEnd;
            }

            throw new NotSupportedException($"{Globals.AppName} does not support query kind of {queryKind}");
        }
    }
}
