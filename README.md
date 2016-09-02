# README #

LINQPadBlog generates a blog post from valid LINQPad files (*.linq). It transforms markdown and LATEX embedded in code comments into html, and also includes the output of calls to LINQPad's Dump() method. It currently supports output to the file system and WordPress.com blogs.

### What is this repository for? ###

* The goal is to effortlessly turn a LINQPad script into a blog post
* Ideally it is used from within LINQPad with the aid of static helper methods added to LINQPad's static MyExtensions class

### Limitations ###

* Currently only supports one-time upload to wordpress.com (if you need to edit an uploaded post you'll first need to delete the original to avoid duplicates)
* Currently only supports C# comments
* Doesn't work with LINQPad's Dump() for images
* Doesn't have robust exception handling

The plan is to fix all of these issues over time.

### How do I get set up? ###

The setup process is slightly convoluted so suggestions to improve this process are welcome.

* Open LINQPad and then open the inbuilt `My Extensions` file
* Add the following static methods to the `MyExtensions` class

~~~~
	// Turns a linq file into an html page and saves it to the local file system
	public static void CreateFileSystemBlogPost(DirectoryInfo postDir)
	{
		var args = Scombroid.LINQPadBlog.Utils.ProcessedArgs.ProcessScriptArgs(new string[] { Util.CurrentQueryPath });
		var transformer = new Scombroid.LINQPadBlog.ScriptTransformers.FileSystemLinqScriptTransformer(postDir);
		var result = Scombroid.LINQPadBlog.ScriptTransformer.Transform(transformer, args, null, nameof(CreateFileSystemBlogPost));

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
			BaseUrl = @"https://your_site.wordpress.com",
			BlogId = 123456789,
			PostTitle = postTitle,
			PostType = "post",
			PostStatus = "publish",
			Username = "your_username",
			Password = "your_password",
			PostID = null
		};

		var result = Scombroid.LINQPadBlog.ScriptTransformer.Transform(transformer, args, postParams, nameof(CreateWordPressDotComBlogPost));

		var webBrowser = new WebBrowser();
		webBrowser.ScriptErrorsSuppressed = true;
		webBrowser.Navigate(result.Location);
		PanelManager.DisplayControl(webBrowser, "Blog Post");
	}
~~~~

* Open the `Query Properties` dialog (F4) and add a reference to the LINQPadBlog nuget package
* In the `Query Properties` dialog, also add a reference to `System.Windows.Forms.dll`
* Click `OK`

You should now have LINQPadBlog accessible by all scripts that you write.

### How do I use it? ###

Here's an example
~~~~
/*
You place you text between multi-line comment markers.  
Standard Markdown is supported (via MarkdownSharp) so things like *italicized* and **bold** work as expected.  
You can also include mathematics. For example, when \\(a \ne 0\\), there are two solutions to \\(ax^2 + bx + c = 0\\) and they are
$$x = {-b \pm \sqrt{b^2-4ac} \over 2a}.$$
*/
void Main()
{
	// Single line comments will not be transformed
	var a = 40;
	var b = 2;
	var c = a + b;

	c.Dump("Meaning of life");
	// At the end of your executable code, call one of the static helper methods to perform the transformation and then hit F5 to see the results.
	// Note that this call will be stripped from the file (to prevent an infinite call cycle!)
	MyExtensions.CreateFileSystemBlogPost(new DirectoryInfo(@"D:\Temp\BlogOutput"));
}
/*
It's also ok to add comments after the code.  
Any output from calls to .Dump() will be placed at the end of the file.
*/
~~~~

### How does it work? ###

The process is:

* Use LINQPad to write some code
* Any text should be placed between multi-line comment markers
* Text can include Markdown and LATEX
* At the end of the executable code section, place a call to a static helper method to perform the transform
* The static helper method feeds the current *.linq file into the transformation engine
* The transformation engine parses the file and transforms any Markdown into html, as well as correctly escaping LATEX for the output target
* The *.linq file is run (via LINQPad's Util.Run method) and all output is captured
* An html document is generated with the transformed text contents, code sections, and any output
* Depending on the output target, the html document is either written to the local file system or uploaded to wordpress.com

### Contribution guidelines ###

Contributions are most welcome.  Make some changes and then submit a pull request.