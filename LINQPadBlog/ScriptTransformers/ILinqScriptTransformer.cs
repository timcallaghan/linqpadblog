using Scombroid.LINQPadBlog.Utils;

namespace Scombroid.LINQPadBlog.ScriptTransformers
{
    public interface ILinqScriptTransformer
    {
        void Transform(LinqPadScriptInfo scriptInfo, IScriptTransformParams scriptParams);
    }
}
