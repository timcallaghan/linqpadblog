namespace Scombroid.LINQPadBlog.ScriptTransformers
{
    public interface IScriptTransformResult
    {
        string Location { get; }
        int? PostId { get; }
    }
}
