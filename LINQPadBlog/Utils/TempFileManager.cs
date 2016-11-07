using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scombroid.LINQPadBlog.Utils
{
    public class TempFileManager : IDisposable
    {
        private FileInfo TempFile { get; }
        public string TempFilePath => TempFile.FullName;

        public TempFileManager(FileInfo fileToCopy)
        {
            try
            {
                TempFile = new FileInfo(Path.Combine(Path.GetTempPath(), fileToCopy.Name));
                File.Copy(fileToCopy.FullName, TempFile.FullName, true);
            }
            catch (Exception)
            {
                if (TempFile != null && TempFile.Exists)
                {
                    TempFile.Delete();
                }
                throw;
            }
        }

        public void ReplaceFileContents(string contents)
        {
            File.WriteAllText(TempFilePath, contents);
        }

        public void Dispose()
        {
            if (TempFile != null && TempFile.Exists)
            {
                TempFile.Delete();
            }
        }
    }
}
