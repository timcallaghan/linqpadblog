﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Scombroid.LINQPadBlog.Utils
{
    public class ScriptContentParser
    {
        public List<ScriptContentSection> ScriptContentSections { get; }
        private readonly string _codeCommentStart;
        private readonly string _codeCommentEnd;

        public ScriptContentParser(string codeCommentStart, string codeCommentEnd, string script, List<string> stripFromFile)
        {
            ScriptContentSections = new List<ScriptContentSection>();
            _codeCommentStart = codeCommentStart;
            _codeCommentEnd = codeCommentEnd;
            ParseContents(script, stripFromFile);
        }

        private void ParseContents(string input, List<string> stripFromFile)
        {
            stripFromFile = stripFromFile?.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            ScriptContentSection currentSection = null;
            var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var sectionStack = new Stack<ScriptContentSectionType>();
            Regex nonCompiledCodeRegex = new Regex(Globals.Comments.NonCompiledCodeRegexStart, RegexOptions.None);

            for (var lineIndex = 0; lineIndex < lines.Length; ++lineIndex)
            {
                var line = lines[lineIndex];
                var lineTrimmed = line.Trim();
                if (lineTrimmed == _codeCommentStart)
                {
                    if (currentSection != null)
                    {
                        if (sectionStack.Pop() != ScriptContentSectionType.CompiledCode)
                            throw new ScriptContentParseException($"Unexpected {_codeCommentStart} found on line {lineIndex + 1}.");

                        ScriptContentSections.Add(currentSection);
                    }

                    currentSection = new ScriptContentSection(ScriptContentSectionType.MarkdownComment);
                    sectionStack.Push(ScriptContentSectionType.MarkdownComment);
                }
                else if (lineTrimmed == _codeCommentEnd)
                {
                    if (currentSection != null)
                    {
                        if (sectionStack.Peek() != ScriptContentSectionType.MarkdownComment)
                            throw new ScriptContentParseException($"Unexpected {_codeCommentEnd} found on line {lineIndex + 1}. Expected to see {GetClosingTagForSectionType(sectionStack.Peek())}.");

                        ScriptContentSections.Add(currentSection);
                    }

                    currentSection = null;
                    sectionStack.Pop();
                }
                else if (nonCompiledCodeRegex.IsMatch(lineTrimmed))
                {
                    if (sectionStack.Peek() != ScriptContentSectionType.MarkdownComment)
                        throw new ScriptContentParseException($"Unexpected {Globals.Comments.NonCompiledCodeStart} found on line {lineIndex + 1}. Expected to see {GetClosingTagForSectionType(sectionStack.Peek())}.");

                    if (currentSection != null)
                    {
                        ScriptContentSections.Add(currentSection);
                    }

                    var matchInfo = nonCompiledCodeRegex.Match(lineTrimmed);
                    currentSection = new ScriptContentSection(ScriptContentSectionType.NonCompiledCode, matchInfo.Groups["lang"].Value);
                    sectionStack.Push(ScriptContentSectionType.NonCompiledCode);
                }
                else if (lineTrimmed == Globals.Comments.NonCompiledCodeEnd)
                {
                    if (sectionStack.Pop() != ScriptContentSectionType.NonCompiledCode)
                        throw new ScriptContentParseException($"Unexpected {Globals.Comments.NonCompiledCodeEnd} found on line {lineIndex + 1}. Expected to have previously seen {Globals.Comments.NonCompiledCodeStart}.");

                    ScriptContentSections.Add(currentSection);
                    currentSection = null;
                }
                else if (lineTrimmed == Globals.Comments.DumpStart)
                {
                    if (sectionStack.Peek() != ScriptContentSectionType.MarkdownComment)
                        throw new ScriptContentParseException($"Unexpected {Globals.Comments.DumpStart} found on line {lineIndex + 1}. Expected to see {GetClosingTagForSectionType(sectionStack.Peek())}.");

                    if (currentSection != null)
                    {
                        ScriptContentSections.Add(currentSection);
                    }

                    currentSection = new ScriptContentSection(ScriptContentSectionType.DumpOutput);
                    sectionStack.Push(ScriptContentSectionType.DumpOutput);
                }
                else if (lineTrimmed == Globals.Comments.DumpEnd)
                {
                    if (sectionStack.Pop() != ScriptContentSectionType.DumpOutput)
                        throw new ScriptContentParseException($"Unexpected {Globals.Comments.DumpEnd} found on line {lineIndex + 1}. Expected to have previously seen {Globals.Comments.DumpStart}.");

                    ScriptContentSections.Add(currentSection);
                    currentSection = null;
                }
                else
                {
                    if (currentSection == null)
                    {
                        if (sectionStack.Any() && sectionStack.Peek() == ScriptContentSectionType.MarkdownComment)
                        {
                            currentSection = new ScriptContentSection(ScriptContentSectionType.MarkdownComment);
                        }
                        else
                        {
                            if (sectionStack.Any())
                                throw new ScriptContentParseException($"Expected to be inside code block on line {lineIndex + 1} but was instead inside {sectionStack.Peek()}");

                            currentSection = new ScriptContentSection(ScriptContentSectionType.CompiledCode);
                            sectionStack.Push(ScriptContentSectionType.CompiledCode);
                        }
                    }

                    if 
                    (
                        sectionStack.Contains(ScriptContentSectionType.MarkdownComment) 
                        || stripFromFile == null
                        || !stripFromFile.Any() 
                        || stripFromFile.All(s => !line.Contains(s))
                    )
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
                    throw new ScriptContentParseException($"Expected to be inside a single code section on last line of file but nothing was found on the stack");

                if (sectionStack.Count > 1)
                    throw new ScriptContentParseException($"Expected to be inside a single code section on last line of file but {sectionStack.Count} sections were found on the stack");

                var finalSectionType = sectionStack.Pop();
                if (finalSectionType != currentSection.ContentType)
                    throw new ScriptContentParseException($"Expected to be inside a {finalSectionType} section but was instead inside a {currentSection.ContentType} section on last line of file");

                ScriptContentSections.Add(currentSection);
            }
        }

        private string GetClosingTagForSectionType(ScriptContentSectionType sectionType)
        {
            switch (sectionType)
            {
                case ScriptContentSectionType.MarkdownComment:
                    return _codeCommentEnd;
                case ScriptContentSectionType.NonCompiledCode:
                    return Globals.Comments.NonCompiledCodeEnd;
                case ScriptContentSectionType.DumpOutput:
                    return Globals.Comments.DumpEnd;
            }

            return sectionType.ToString();
        }
    }
}