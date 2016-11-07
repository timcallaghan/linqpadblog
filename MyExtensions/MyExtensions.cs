void Main()
{
	// Write code to test your extensions here. Press F5 to compile and run.
}

public static class MyExtensions
{
	// Write custom extension methods here. They will be available to all queries.

    public static string GetLINQPadOutputDOM()
    {
        string html = null;
        using (var tw = Util.CreateXhtmlWriter(true, false))
        {
            // LINQPad (v4) CreateXhtmlWriter appears to have a "bug" in ToString(), in that 
            // it returns empty if nothing has been written yet.  To get around this, we write "true" before calling ToString()
            tw.Write(true);

            // LINQPad (v5) appears to have a "bug" in ToString(), in that 
            // it duplicates the html document inside the body tag 
            // (perhaps Tostring() is not meant to be called on it?).
            // To get around this, we simply find the first occurence of <body>
            // and take all text up to that point (plus additional text to make the DOM well formed)
            html = tw.ToString();
            html = html.Substring(0, html.IndexOf(@"<body>")) + @"<body></body></html>";
        }

        return html;
    }

    public static void CreateFileSystemBlogPost(DirectoryInfo postDir)
    {
        var args = Scombroid.LINQPadBlog.Utils.ProcessedArgs.ProcessScriptArgs(new string[] { Util.CurrentQueryPath });
        var transformer = new Scombroid.LINQPadBlog.ScriptTransformers.FileSystemLinqScriptTransformer(postDir);

        Scombroid.LINQPadBlog.ScriptTransformers.IScriptTransformResult result = null;
        using (var trans = new Scombroid.LINQPadBlog.ScriptTransformer(transformer, args, null, nameof(CreateFileSystemBlogPost)))
        {
            // Run the script through LINQPad and extract any output
            var scriptOutput = Util.Cmd("lprun.exe", $@"-format=htmlfrag ""{ trans.GetTempFilePath }""", true).LastOrDefault() ?? string.Empty;
            result = trans.Transform(scriptOutput, GetLINQPadOutputDOM());
        }

        var webBrowser = new WebBrowser();
        webBrowser.ScriptErrorsSuppressed = true;
        webBrowser.Navigate($"file:///{result.Location}");
        PanelManager.DisplayControl(webBrowser, "Blog Post");
    }

	// Turns a linq file into an html page and uploads it to wordpress.com
	public static void CreateWordPressDotComBlogPost(string postTitle)
	{
		var args = Scombroid.LINQPadBlog.Utils.ProcessedArgs.ProcessScriptArgs(new string[] { Util.CurrentQueryPath });
		var transformer = new Scombroid.LINQPadBlog.ScriptTransformers.WordPressDotComLinqScriptTransformer();

        var postParams = new Scombroid.LINQPadBlog.ScriptTransformers.WordPressDotComParams()
        {
            BaseUrl = @"https://yoursite.wordpress.com",
            BlogId = 12345678,
            PostTitle = postTitle,
            PostType = "post",
            PostStatus = "publish",
            Username = "username",
            Password = "password",
            PostID = null
        };

        Scombroid.LINQPadBlog.ScriptTransformers.IScriptTransformResult result = null;
        using (var trans = new Scombroid.LINQPadBlog.ScriptTransformer(transformer, args, postParams, nameof(CreateWordPressDotComBlogPost)))
        {
            // Run the script through LINQPad and extract any output
            var scriptOutput = Util.Cmd("lprun.exe", $@"-format=htmlfrag ""{ trans.GetTempFilePath }""", true).LastOrDefault() ?? string.Empty;
            result = trans.Transform(scriptOutput, GetLINQPadOutputDOM());
        }

		var webBrowser = new WebBrowser();
		webBrowser.ScriptErrorsSuppressed = true;
		webBrowser.Navigate(result.Location);
		PanelManager.DisplayControl(webBrowser, "Blog Post");
	}
}
