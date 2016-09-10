using System.Text;

namespace Scombroid.LINQPadBlog.Utils
{
    public class ScriptContentSection
    {
        private readonly StringBuilder _lines;
        public ScriptContentSectionType ContentType { get; private set; }
        public string Contents => _lines.ToString();

        public ScriptContentSection(ScriptContentSectionType contentType)
        {
            _lines = new StringBuilder();
            ContentType = contentType;
        }

        public void AppendLine(string line)
        {
            _lines.AppendLine(line);
        }
    }
}