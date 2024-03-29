﻿using HtmlAgilityPack;
using Markdig;
using Scombroid.LINQPadBlog.Utils;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace Scombroid.LINQPadBlog.ScriptTransformers
{
    public class FileSystemLinqScriptTransformer : ILinqScriptTransformer
    {
        private DirectoryInfo _resourcesDir { get; set; }
        private string LINQPadCssRelativePath { get { return $"{_resourcesDir.Name}/{Globals.FileSystem.LINQPadCssFileName}"; } }
        private string LINQPadJsRelativePath { get { return $"{_resourcesDir.Name}/{Globals.FileSystem.LINQPadJsFileName}"; } }
        private DirectoryInfo _blogDir;

        public FileSystemLinqScriptTransformer(DirectoryInfo blogDir)
        {
            if (blogDir == null)
                throw new ArgumentException($"{nameof(blogDir)} can't be null");

            _blogDir = blogDir;
        }

        public IScriptTransformResult Transform(LinqPadScriptInfo scriptInfo, IScriptTransformParams scriptParams)
        {
            var result = new ScriptTransformResult();

            // Create blog directory if it doesn't exist
            if (!_blogDir.Exists)
            {
                _blogDir.Create();
            }

            CreateWebPageResourceFiles(scriptInfo, _blogDir);

            // Convert markdown to html
            var convertedScript = ConvertScriptContentsToHtml(scriptInfo);

            // Get the output file path
            var outfile = new FileInfo(Path.Combine(_blogDir.FullName, Path.ChangeExtension(scriptInfo.ProcessedArgs.FilePath.Name, "html")));

            var output = BuildHtmlContents(scriptInfo, convertedScript, true);
            File.WriteAllText(outfile.FullName, output.DocumentNode.OuterHtml);
            result.Location = outfile.FullName;

            return result;
        }

        private void CreateWebPageResourceFiles(LinqPadScriptInfo scriptInfo, DirectoryInfo blogDir)
        {
            // Create the resources folder if it doesn't exist
            _resourcesDir = new DirectoryInfo(Path.Combine(blogDir.FullName, Globals.FileSystem.ResourcesFolderName));
            if (!_resourcesDir.Exists)
            {
                _resourcesDir.Create();
            }

            File.WriteAllText(Path.Combine(_resourcesDir.FullName, Globals.FileSystem.LINQPadCssFileName), scriptInfo.LinqPadWebResources.Styles);
            File.WriteAllText(Path.Combine(_resourcesDir.FullName, Globals.FileSystem.LINQPadJsFileName), scriptInfo.LinqPadWebResources.Scripts);
        }

        private HtmlDocument BuildHtmlContents(LinqPadScriptInfo scriptInfo, string scriptHtml, bool replaceResources)
        {
            var htmlDoc = scriptInfo.LinqPadWebResources.LinqPadHtmlDoc;
            htmlDoc.OptionWriteEmptyNodes = true;

            HtmlNode head = htmlDoc.DocumentNode.SelectSingleNode(Globals.DOM.HeadNodePath);

            var titleNode = htmlDoc.CreateElement("title");
            titleNode.InnerHtml = Path.GetFileNameWithoutExtension(scriptInfo.ProcessedArgs.FilePath.Name);
            head.ChildNodes.Add(titleNode);

            if (replaceResources)
            {
                // Remove the style section and script section generated by LINQPad,
                // and replace with links to resource files
                var styleNode = htmlDoc.DocumentNode.SelectSingleNode(Globals.DOM.StyleNodePath);
                var replacementStyleNode = htmlDoc.CreateElement("link");
                replacementStyleNode.SetAttributeValue("rel", "stylesheet");
                replacementStyleNode.SetAttributeValue("type", "text/css");
                replacementStyleNode.SetAttributeValue("href", LINQPadCssRelativePath);
                head.ReplaceChild(replacementStyleNode, styleNode);

                var scriptNode = htmlDoc.DocumentNode.SelectSingleNode(Globals.DOM.ScriptNodePath);
                var replacementScriptNode = htmlDoc.CreateElement("script");
                replacementScriptNode.SetAttributeValue("src", LINQPadJsRelativePath);
                head.ReplaceChild(replacementScriptNode, scriptNode);
            }

            var linkHighlightJsCss = htmlDoc.CreateElement("link");
            linkHighlightJsCss.SetAttributeValue("rel", "stylesheet");
            linkHighlightJsCss.SetAttributeValue("type", "text/css");
            linkHighlightJsCss.SetAttributeValue("href", Globals.FileSystem.HighlightJsCssCdnUri);
            head.AppendChild(linkHighlightJsCss);

            var scriptHighlightJs = htmlDoc.CreateElement("script");
            scriptHighlightJs.SetAttributeValue("src", Globals.FileSystem.HighlightJsCdnUri);
            head.AppendChild(scriptHighlightJs);

            var scriptHighlightJsLoad = htmlDoc.CreateElement("script");
            scriptHighlightJsLoad.InnerHtml = Globals.FileSystem.HighlightJsLoadStatement;
            head.AppendChild(scriptHighlightJsLoad);

            var scriptMathJax = htmlDoc.CreateElement("script");
            scriptMathJax.SetAttributeValue("src", Globals.FileSystem.MathJaxCdnUri);
            head.AppendChild(scriptMathJax);

            var body = htmlDoc.DocumentNode.SelectSingleNode(Globals.DOM.BodyNodePath);

            var mainBody = htmlDoc.CreateTextNode(scriptHtml);
            body.AppendChild(mainBody);

            var dumpBody = htmlDoc.CreateTextNode(scriptInfo.ScriptOutput);
            body.AppendChild(dumpBody);

            return htmlDoc;
        }

        private string ConvertScriptContentsToHtml(LinqPadScriptInfo scriptInfo)
        {
            var result = new StringBuilder();
            var codeSectionStart = string.Format(Globals.HighlightJs.CodeSectionWithLangStart, scriptInfo.GetScriptLangName());

            foreach (var section in scriptInfo.ScriptContents)
            {
                if (String.IsNullOrWhiteSpace(section.Contents))
                    continue;

                switch (section.ContentType)
                {
                    case ScriptContentSectionType.CompiledCode:
                        result.Append(codeSectionStart);
                        result.Append(WebUtility.HtmlEncode(section.Contents));
                        result.AppendLine(Globals.HighlightJs.CodeSectionEnd);
                        break;
                    case ScriptContentSectionType.NonCompiledCode:
                        if (!string.IsNullOrWhiteSpace(section.CodeClass))
                        {
                            result.Append(string.Format(Globals.HighlightJs.CodeSectionWithLangStart, section.CodeClass));
                        }
                        else
                        {
                            result.Append(codeSectionStart);
                        }
                        
                        result.Append(WebUtility.HtmlEncode(section.Contents));
                        result.AppendLine(Globals.HighlightJs.CodeSectionEnd);
                        break;
                    case ScriptContentSectionType.DumpOutput:
                        // TODO: Implement dump lookup
                        break;
                    case ScriptContentSectionType.MarkdownComment:
                        result.Append(Markdown.ToHtml(section.Contents));
                        break;
                }
            }

            return result.ToString();
        }
    }
}
