using System;
using System.Collections.Generic;

namespace Scombroid.LINQPadBlog.Utils
{
    public class ScriptContentParser
    {
        public List<ScriptContentSection> ScriptContentSections { get; }
        private readonly string _codeCommentStart;
        private readonly string _codeCommentEnd;

        public ScriptContentParser(string codeCommentStart, string codeCommentEnd, string script, string stripMeFromFile)
        {
            ScriptContentSections = new List<ScriptContentSection>();
            _codeCommentStart = codeCommentStart;
            _codeCommentEnd = codeCommentEnd;
            ParseContents(script, stripMeFromFile);
        }

        private void ParseContents(string input, string stripMeFromFile)
        {
            ScriptContentSection currentSection = null;
            var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var isInsideComment = false;
            foreach (var line in lines)
            {
                if (line == _codeCommentStart)
                {
                    if (currentSection != null)
                    {
                        ScriptContentSections.Add(currentSection);
                    }
                    currentSection = new ScriptContentSection(ScriptContentSectionType.MarkdownComment);
                    isInsideComment = true;
                }
                else if (line == _codeCommentEnd)
                {
                    ScriptContentSections.Add(currentSection);
                    currentSection = null;
                    isInsideComment = false;
                }
                else if (line == Globals.Comments.NonCompiledCodeStart)
                {
                    if (currentSection != null)
                    {
                        ScriptContentSections.Add(currentSection);
                    }
                    currentSection = new ScriptContentSection(ScriptContentSectionType.NonCompiledCode);
                }
                else if (line == Globals.Comments.NonCompiledCodeEnd)
                {
                    ScriptContentSections.Add(currentSection);
                    currentSection = null;
                }
                else if (line == Globals.Comments.DumpStart)
                {
                    if (currentSection != null)
                    {
                        ScriptContentSections.Add(currentSection);
                    }
                    currentSection = new ScriptContentSection(ScriptContentSectionType.DumpOutput);
                }
                else if (line == Globals.Comments.DumpEnd)
                {
                    ScriptContentSections.Add(currentSection);
                    currentSection = null;
                }
                else
                {
                    if (currentSection == null)
                    {
                        currentSection = isInsideComment ? new ScriptContentSection(ScriptContentSectionType.MarkdownComment) : new ScriptContentSection(ScriptContentSectionType.CompiledCode);
                    }

                    if (isInsideComment || String.IsNullOrWhiteSpace(stripMeFromFile) || !line.Contains(stripMeFromFile))
                    {
                        currentSection.AppendLine(line);
                    }
                }
            }

            if (currentSection != null)
            {
                ScriptContentSections.Add(currentSection);
            }
        }
    }
}