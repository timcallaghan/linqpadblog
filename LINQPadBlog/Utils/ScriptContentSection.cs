using System.Text;

namespace Scombroid.LINQPadBlog.Utils
{
    public class ScriptContentSection
    {
        private readonly StringBuilder _lines;
        public ScriptContentSectionType ContentType { get; private set; }
        public string Contents => _lines.ToString();

        public string CodeClass { get; private set; }

        public ScriptContentSection(ScriptContentSectionType contentType, string codeClass = null)
        {
            _lines = new StringBuilder();
            ContentType = contentType;
            CodeClass = !string.IsNullOrWhiteSpace(codeClass) ? codeClass : null;
        }

        public void AppendLine(string line)
        {
            _lines.AppendLine(line);
        }
    }
}