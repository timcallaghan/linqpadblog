using System;
using System.Collections.Generic;
using System.Linq;

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
            var sectionStack = new Stack<ScriptContentSectionType>();
            var isInsideComment = false;

            for (var lineIndex = 0; lineIndex < lines.Length; ++lineIndex)
            {
                var line = lines[lineIndex];
                if (line == _codeCommentStart)
                {
                    if (currentSection != null)
                    {
                        if (sectionStack.Pop() != ScriptContentSectionType.CompiledCode)
                            throw new ScriptContentParseException($"Unexpected {_codeCommentStart} found on line {lineIndex + 1}");

                        ScriptContentSections.Add(currentSection);
                    }

                    currentSection = new ScriptContentSection(ScriptContentSectionType.MarkdownComment);
                    sectionStack.Push(ScriptContentSectionType.MarkdownComment);
                    isInsideComment = true;
                }
                else if (line == _codeCommentEnd)
                {
                    if (currentSection != null)
                    {
                        if (sectionStack.Pop() != ScriptContentSectionType.MarkdownComment)
                            throw new ScriptContentParseException($"Unexpected {_codeCommentEnd} found on line {lineIndex + 1}");

                        ScriptContentSections.Add(currentSection);
                    }

                    currentSection = null;
                    isInsideComment = false;
                }
                else if (line == Globals.Comments.NonCompiledCodeStart)
                {
                    if (sectionStack.Peek() != ScriptContentSectionType.MarkdownComment)
                        throw new ScriptContentParseException($"Unexpected {Globals.Comments.NonCompiledCodeStart} found on line {lineIndex + 1}");

                    if (currentSection != null)
                    {
                        ScriptContentSections.Add(currentSection);
                    }

                    currentSection = new ScriptContentSection(ScriptContentSectionType.NonCompiledCode);
                    sectionStack.Push(ScriptContentSectionType.NonCompiledCode);
                }
                else if (line == Globals.Comments.NonCompiledCodeEnd)
                {
                    if (sectionStack.Pop() != ScriptContentSectionType.NonCompiledCode)
                        throw new ScriptContentParseException($"Unexpected {Globals.Comments.NonCompiledCodeEnd} found on line {lineIndex + 1}");

                    ScriptContentSections.Add(currentSection);
                    currentSection = null;
                }
                else if (line == Globals.Comments.DumpStart)
                {
                    if (sectionStack.Peek() != ScriptContentSectionType.MarkdownComment)
                        throw new ScriptContentParseException($"Unexpected {Globals.Comments.DumpStart} found on line {lineIndex + 1}");

                    if (currentSection != null)
                    {
                        ScriptContentSections.Add(currentSection);
                    }

                    currentSection = new ScriptContentSection(ScriptContentSectionType.DumpOutput);
                    sectionStack.Push(ScriptContentSectionType.DumpOutput);
                }
                else if (line == Globals.Comments.DumpEnd)
                {
                    if (sectionStack.Pop() != ScriptContentSectionType.DumpOutput)
                        throw new ScriptContentParseException($"Unexpected {Globals.Comments.DumpEnd} found on line {lineIndex + 1}");

                    ScriptContentSections.Add(currentSection);
                    currentSection = null;
                }
                else
                {
                    if (currentSection == null)
                    {
                        if (isInsideComment)
                        {
                            if (sectionStack.Peek() != ScriptContentSectionType.MarkdownComment)
                                throw new ScriptContentParseException($"Expected comment resumption on line {lineIndex + 1} but did not find it");

                            currentSection = new ScriptContentSection(ScriptContentSectionType.MarkdownComment);
                        }
                        else
                        {
                            if (sectionStack.Any())
                                throw new ScriptContentParseException($"Expected code block commencement on line {lineIndex + 1} but did not find it");

                            currentSection = new ScriptContentSection(ScriptContentSectionType.CompiledCode);
                            sectionStack.Push(ScriptContentSectionType.CompiledCode);
                        }
                    }

                    if (isInsideComment || String.IsNullOrWhiteSpace(stripMeFromFile) || !line.Contains(stripMeFromFile))
                    {
                        currentSection.AppendLine(line);
                    }
                }
            }

            if (currentSection != null)
            {
                // Must be the case that this is code (all existing comments should have been closed already (otherwise the script is invalid)
                if (currentSection.ContentType != ScriptContentSectionType.CompiledCode)
                    throw new ScriptContentParseException($"Expected to be inside a code block at end of file but was instead inside a {currentSection.ContentType} section");

                if (sectionStack.Count == 0)
                    throw new ScriptContentParseException($"Expected to be inside a single code section on last line of file but nothing was found");

                if (sectionStack.Count > 1)
                    throw new ScriptContentParseException($"Expected to be inside a single code section on last line of file but {sectionStack.Count} were found on the stack");

                var finalSectionType = sectionStack.Pop();
                if (finalSectionType != currentSection.ContentType)
                    throw new ScriptContentParseException($"Expected to be inside a {finalSectionType} section but was instead inside a {currentSection.ContentType} section on last line of file");

                ScriptContentSections.Add(currentSection);
            }
        }
    }
}