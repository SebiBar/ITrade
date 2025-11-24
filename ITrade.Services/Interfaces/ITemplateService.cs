namespace ITrade.Services.Interfaces
{
    public interface ITemplateService
    {
        Task<string> RenderAsync(string relativePath, IReadOnlyDictionary<string, string> model);
    }
}
