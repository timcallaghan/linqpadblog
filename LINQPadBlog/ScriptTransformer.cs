using Scombroid.LINQPadBlog.ScriptTransformers;
using Scombroid.LINQPadBlog.Utils;

namespace Scombroid.LINQPadBlog
{
    public static class ScriptTransformer
    {
        public static void Transform(ILinqScriptTransformer transformer, ProcessedArgs processedArgs, IScriptTransformParams scriptParams)
        {
            var scriptInfo = LinqPadScriptUtils.LoadLINQPadScriptInfo(processedArgs);
            transformer.Transform(scriptInfo, scriptParams);
        }
    }
}
