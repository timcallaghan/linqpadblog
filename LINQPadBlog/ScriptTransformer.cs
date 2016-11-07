using System;
using System.IO;
using Scombroid.LINQPadBlog.ScriptTransformers;
using Scombroid.LINQPadBlog.Utils;

namespace Scombroid.LINQPadBlog
{
    public class ScriptTransformer : IDisposable
    {
        private readonly ILinqScriptTransformer _transformer;
        private readonly IScriptTransformParams _scriptParams;
        private readonly LinqPadScriptInfo _scriptInfo;
        private readonly TempFileManager _tempFile;
        public string GetTempFilePath => _tempFile.TempFilePath;

        public ScriptTransformer
        (
            ILinqScriptTransformer transformer,
            ProcessedArgs processedArgs,
            IScriptTransformParams scriptParams,
            string stripMeFromFile
        )
        {
            _transformer = transformer;
            _scriptParams = scriptParams;

            // Take a copy of the file
            _tempFile = new TempFileManager(processedArgs.FilePath);
            _scriptInfo = LinqPadScriptUtils.LoadLINQPadScriptInfo(_tempFile, processedArgs, stripMeFromFile);
        }

        public IScriptTransformResult Transform
        (
            string scriptOutput,
            // ReSharper disable once InconsistentNaming
            string linqPadOutputDOM
        )
        {
            _scriptInfo.ScriptOutput = scriptOutput;
            _scriptInfo.LinqPadWebResources = LinqPadWebResources.Generate(linqPadOutputDOM);
            return _transformer.Transform(_scriptInfo, _scriptParams);
        }

        public void Dispose()
        {
            _tempFile?.Dispose();
        }
    }
}
