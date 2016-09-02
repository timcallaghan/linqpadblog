using Scombroid.LINQPadBlog.ScriptTransformers;
using Scombroid.LINQPadBlog.Utils;
using System.Threading.Tasks;

namespace Scombroid.LINQPadBlog
{
    public static class ScriptTransformer
    {
        public static IScriptTransformResult Transform
        (
            ILinqScriptTransformer transformer, 
            ProcessedArgs processedArgs, 
            IScriptTransformParams scriptParams, 
            string stripMeFromFile
        )
        {
            var scriptInfo = LinqPadScriptUtils.LoadLINQPadScriptInfo(processedArgs, stripMeFromFile);
            return transformer.Transform(scriptInfo, scriptParams);
        }
    }
}
