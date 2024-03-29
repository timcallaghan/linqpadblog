﻿using HtmlAgilityPack;
using Markdig;
using Scombroid.LINQPadBlog.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using WordPressPCL;
using WordPressPCL.Models;

namespace Scombroid.LINQPadBlog.ScriptTransformers
{
    public class WordPressLinqScriptTransformer : ILinqScriptTransformer
    {
        public WordPressLinqScriptTransformer() { }

        public IScriptTransformResult Transform(LinqPadScriptInfo scriptInfo, IScriptTransformParams scriptParams)
        {
            if (!(scriptParams is WordPressParams postParams))
                throw new ArgumentException($"{nameof(scriptParams)} must be an instance of {nameof(WordPressParams)}");

            // Convert markdown to html
            var convertedScript = ConvertScriptContentsToHtml(scriptInfo);

            var output = BuildHtmlContents(scriptInfo, convertedScript);

            return UploadBlogPostToWordPress(output, postParams);
        }

        private HtmlDocument BuildHtmlContents(LinqPadScriptInfo scriptInfo, string scriptHtml)
        {
            var htmlDoc = scriptInfo.LinqPadWebResources.LinqPadHtmlDoc;
            var body = htmlDoc.DocumentNode.SelectSingleNode(Globals.DOM.BodyNodePath);

            var mainBody = htmlDoc.CreateTextNode(scriptHtml);
            body.AppendChild(mainBody);

            var scopedOutput = $@"<div id=""linqpadoutput"">{scriptInfo.ScriptOutput}</div>";
            var dumpBody = htmlDoc.CreateTextNode(scopedOutput);
            body.AppendChild(dumpBody);

            return htmlDoc;
        }

        ScriptTransformResult UploadBlogPostToWordPress(HtmlDocument htmlDoc, WordPressParams postParams)
        {
            // TODO: Find a way to make this method async...
            var client = new WordPressClient(postParams.BaseApiUrl)
            {
                AuthMethod = AuthMethod.JWT
            };
            client.RequestJWToken(postParams.Username, postParams.Password).Wait();
            // check if authentication has been successful 
            // TODO handle this when error
            client.IsValidJWToken().Wait();

            // Resolve the categories supplied for the post
            var categoryIds = new List<int>();
            if (postParams.Categories.Any())
            {
                var categoriesApiResult = client.Categories.GetAll();
                categoriesApiResult.Wait();
                var existingCategories = categoriesApiResult.Result.ToList();

                foreach (var categoryName in postParams.Categories.Select(c => c.Trim()).Distinct())
                {
                    var foundExistingCategory = false;
                    foreach (var existingCategory in existingCategories)
                    {
                        if (existingCategory.Name.Equals(categoryName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            categoryIds.Add(existingCategory.Id);
                            foundExistingCategory = true;
                        }
                    }

                    if (!foundExistingCategory)
                    {
                        var createCategoryApiResult = client.Categories.Create(new Category(categoryName));
                        createCategoryApiResult.Wait();
                        categoryIds.Add(createCategoryApiResult.Result.Id);
                    }
                }
            }

            var post = new Post
            {
                Title = new Title(postParams.PostTitle),
                Content = new Content(htmlDoc.DocumentNode.SelectSingleNode(Globals.DOM.BodyNodePath).InnerHtml),
                Status = Status.Publish, // TODO:Fix postParams.PostStatus
                Format = postParams.Format
            };

            if (categoryIds.Any())
            {
                post.Categories = categoryIds.ToArray();
            }

            var result = new ScriptTransformResult();

            // If postID is not null, ensure we "update" rather than create
            if (postParams.PostID.HasValue)
            {
                post.Id = postParams.PostID.Value;
                var updatePostApiResult = client.Posts.Update(post);
                updatePostApiResult.Wait();

                result.Location = updatePostApiResult.Result.Link;
                result.PostId = updatePostApiResult.Result.Id;
            }
            else
            {
                post.DateGmt = DateTime.UtcNow;
                var createPostApiResult = client.Posts.Create(post);
                createPostApiResult.Wait();

                result.Location = createPostApiResult.Result.Link;
                result.PostId = createPostApiResult.Result.Id;
            }

            return result;
        }

        private string ConvertScriptContentsToHtml(LinqPadScriptInfo scriptInfo)
        {
            var result = new StringBuilder();
            var codeSectionStart = string.Format(Globals.HighlightJs.CodeSectionWithLangStart, scriptInfo.GetScriptLangName());

            foreach (var section in scriptInfo.ScriptContents)
            {
                if (string.IsNullOrWhiteSpace(section.Contents))
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
                            result.Append(Globals.HighlightJs.CodeSectionStart);
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
