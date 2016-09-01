using Scombroid.LINQPadBlog.ScriptTransformers;
using Scombroid.LINQPadBlog.Utils;

namespace Scombroid.LINQPadBlog
{
    public static class ScriptTransformer
    {
        public static void Transform
        (
            ILinqScriptTransformer transformer, 
            ProcessedArgs processedArgs, 
            IScriptTransformParams scriptParams, 
            string stripMeFromFile
        )
        {
            var scriptInfo = LinqPadScriptUtils.LoadLINQPadScriptInfo(processedArgs, stripMeFromFile);
            transformer.Transform(scriptInfo, scriptParams);
        }
    }
}
