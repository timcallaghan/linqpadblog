# README #

LINQPadBlog generates a blog post from valid LINQPad files (*.linq). It transforms markdown and LATEX embedded in code comments into html, and also includes the output of calls to LINQPad's Dump() method. It currently supports output to the file system and WordPress.com blogs.

### What is this repository for? ###

* The goal is to effortlessly turn LINQPad scripts into blog post
* Version

### How do I get set up? ###

* Summary of set up
* Configuration
* Dependencies
* Database configuration
* How to run tests
* Deployment instructions

### How does it work? ###

The process is:

* Use LINQPad to write some code
* Any text should be placed between multi-line comment markers
* Text can include Markdown and LATEX
* At the end of the executable code section, place a call to a static helper method to perform the transform
* The static helper method feeds the current *.linq file into the transformation engine
* The transformation engine parses the file and transforms and Markdown into html, as well as encoding LATEX for the output target
* The *.linq file is run (via LINQPad's Util.Run method) and all output is captured
* An html document is generated with the transformed text contents, code sections, and any output
* Depending on the output target, the html document is either written to the local file system or uploaded to wordpress.com

### Contribution guidelines ###

Contributions are most welcome.  Make some changes and then submit a pull request.