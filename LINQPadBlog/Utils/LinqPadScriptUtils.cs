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
            File.Copy(processedArgs.FilePath.FullName, tempFile.FullName, true);

            try
            {
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
                if (doc.Root == null)
                    throw new NotSupportedException($"{Globals.AppName} error: The supplied linq file could not be parsed. Make sure the file is a valid LINQPad file.");

                linqPadScriptInfo.QueryKind = doc.Root.Attribute(Globals.LINQPad.QueryKindAttributeName).Value;

                if (!IsQuerySupported(linqPadScriptInfo.QueryKind))
                    throw new NotSupportedException($"{Globals.AppName} does not support query kind of {linqPadScriptInfo.QueryKind}");

                var scriptContentParser = new ScriptContentParser(
                    GetCommentStartTag(linqPadScriptInfo.QueryKind),
                    GetCommentEndTag(linqPadScriptInfo.QueryKind),
                    fileContentLines.ToString(),
                    stripMeFromFile);
                linqPadScriptInfo.ScriptContents = scriptContentParser.ScriptContentSections;

                // Update tempFile contents with just the executable parts of the script
                var executableScript = new StringBuilder(linqPadScriptInfo.Header);
                executableScript.AppendLine();
                foreach (var section in linqPadScriptInfo.ScriptContents)
                {
                    if (section.ContentType == ScriptContentSectionType.CompiledCode)
                    {
                        executableScript.AppendLine(section.Contents);
                    }
                }

                File.WriteAllText(tempFile.FullName, executableScript.ToString());

                // Run the *.linq file and capture the html output fragments from calls to .Dump() within the file
                linqPadScriptInfo.ScriptOutput = Util.Run(
                    tempFile.FullName,
                    QueryResultFormat.HtmlFragment,
                    processedArgs.AdditionalArgs
                ).AsString();

                // Extract the standard LINQPad css and javascript text
                linqPadScriptInfo.LinqPadWebResources = LinqPadWebResources.Generate();

                return linqPadScriptInfo;
            }
            finally
            {
                tempFile.Delete();
            }
        }

        private static bool IsQuerySupported(string queryKind)
        {
            return queryKind == Globals.LINQPad.QueryKind.CSharpExpression
                   || queryKind == Globals.LINQPad.QueryKind.CSharpStatements
                   || queryKind == Globals.LINQPad.QueryKind.CSharpProgram
                   || queryKind == Globals.LINQPad.QueryKind.FSharpExpression
                   || queryKind == Globals.LINQPad.QueryKind.FSharpProgram;
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

            if
            (
                queryKind == Globals.LINQPad.QueryKind.FSharpExpression
                || queryKind == Globals.LINQPad.QueryKind.FSharpProgram
            )
            {
                return Globals.Comments.FSharpStart;
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

            if
            (
                queryKind == Globals.LINQPad.QueryKind.FSharpExpression
                || queryKind == Globals.LINQPad.QueryKind.FSharpProgram
            )
            {
                return Globals.Comments.FSharpEnd;
            }

            throw new NotSupportedException($"{Globals.AppName} does not support query kind of {queryKind}");
        }
    }
}
