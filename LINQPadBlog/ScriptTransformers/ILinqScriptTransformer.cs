using Scombroid.LINQPadBlog.Utils;

namespace Scombroid.LINQPadBlog.ScriptTransformers
{
    public interface ILinqScriptTransformer
    {
        IScriptTransformResult Transform(LinqPadScriptInfo scriptInfo, IScriptTransformParams scriptParams);
    }
}
