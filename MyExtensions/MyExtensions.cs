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
    
    public static List<string> GetBlogPostMethodNames() => new List<string> {
        nameof(CreateFileSystemBlogPost),
        nameof(CreateWordPressDotComBlogPost),
        nameof(CreateWordPressBlogPost)
    };

    public static void CreateFileSystemBlogPost(DirectoryInfo postDir)
    {
        var args = Scombroid.LINQPadBlog.Utils.ProcessedArgs.ProcessScriptArgs(new string[] { Util.CurrentQueryPath });
        var transformer = new Scombroid.LINQPadBlog.ScriptTransformers.FileSystemLinqScriptTransformer(postDir);

        Scombroid.LINQPadBlog.ScriptTransformers.IScriptTransformResult result = null;
        using (var trans = new Scombroid.LINQPadBlog.ScriptTransformer(transformer, args, null, GetBlogPostMethodNames()))
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
            BaseUrl = @"https://arbaureal.wordpress.com",
            BlogId = 113075450,
            PostTitle = postTitle,
            PostType = "post",
            PostStatus = "publish",
            Username = "spoida",
            Password = Util.GetPassword("wordpress.com"),
            PostID = null
        };

        Scombroid.LINQPadBlog.ScriptTransformers.IScriptTransformResult result = null;
        using (var trans = new Scombroid.LINQPadBlog.ScriptTransformer(transformer, args, postParams, GetBlogPostMethodNames()))
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

	// Turns a linq file into an html page and uploads it to self-hosted full WordPress site
	public static void CreateWordPressBlogPost(string postTitle, List<string> postCategories)
	{
		var args = Scombroid.LINQPadBlog.Utils.ProcessedArgs.ProcessScriptArgs(new string[] { Util.CurrentQueryPath });
		var transformer = new Scombroid.LINQPadBlog.ScriptTransformers.WordPressLinqScriptTransformer();
		
		var postInfoFilePath = Path.ChangeExtension(args.FilePath.FullName, "postinfo");
		var postInfoFile = new FileInfo(postInfoFilePath);
		
		int? existingPostId = null;
		if (postInfoFile.Exists)
		{
			var postInfoContents = File.ReadAllText(postInfoFilePath);
			if (Int32.TryParse(postInfoContents, out int postId))
			{
				existingPostId = postId;
			}
		}

		var postParams = new Scombroid.LINQPadBlog.ScriptTransformers.WordPressParams()
		{
			BaseApiUrl = @"https://arbaureal.com/wp-json/",
			PostTitle = postTitle,
			PostDateUtc = DateTime.UtcNow,
			PostStatus = "publish",
			Username = "spoida",
			Password = Util.GetPassword("arbaureal.com"),
			Format = "standard",
			Categories = postCategories,
			PostID = existingPostId
		};

		Scombroid.LINQPadBlog.ScriptTransformers.IScriptTransformResult result = null;
		using (var trans = new Scombroid.LINQPadBlog.ScriptTransformer(transformer, args, postParams, GetBlogPostMethodNames()))
		{
			// Run the script through LINQPad and extract any output
			var scriptOutput = Util.Cmd("lprun.exe", $@"-format=htmlfrag ""{ trans.GetTempFilePath }""", true).LastOrDefault() ?? string.Empty;
			result = trans.Transform(scriptOutput, GetLINQPadOutputDOM());
		}
		
		if (!existingPostId.HasValue && result.PostId.HasValue)
		{
			File.WriteAllText(postInfoFilePath, result.PostId.Value.ToString());
		}
		
		var webBrowser = new WebBrowser();
		webBrowser.ScriptErrorsSuppressed = true;
		webBrowser.Navigate(result.Location);
		PanelManager.DisplayControl(webBrowser, "Blog Post");
	}
}
