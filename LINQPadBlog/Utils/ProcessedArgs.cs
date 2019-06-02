using System;
using System.IO;
using System.Linq;

namespace Scombroid.LINQPadBlog.Utils
{
    public class ProcessedArgs
    {
        public FileInfo FilePath { get; private set; }
        public string[] AdditionalArgs { get; private set; }

        public static ProcessedArgs ProcessScriptArgs(string[] args)
        {
            // First arg is the linq file path, all remaining args are passed on
            // as parameters to the linq file.
            if (args == null || args.Length < 1)
            {
                throw new ArgumentException($"{Globals.AppName} requires at least one argument");
            }

            var filePath = new FileInfo(args[0]);
            if (!filePath.Exists)
            {
                throw new ArgumentException($"The first argument to {Globals.AppName} must be a valid file path");
            }

            string[] scriptArgs = null;
            if (args.Length > 1)
            {
                scriptArgs = args.Skip(1).ToArray();
            }

            return new ProcessedArgs() { FilePath = filePath, AdditionalArgs = scriptArgs };
        }
    }
}
