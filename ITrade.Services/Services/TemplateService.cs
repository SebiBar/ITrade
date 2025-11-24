using ITrade.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Razor.Templating.Core;
using System.Text;

namespace ITrade.Services.Services
{
    public class TemplateService(
        IHostEnvironment env
        ) : ITemplateService
    {

        public async Task<string> RenderAsync(string relativePath, IReadOnlyDictionary<string, string> model)
        {
            var path = NormalizePath(relativePath);

            if (path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
            {
                return await RazorTemplateEngine.RenderAsync(path, model);
            }

            var physical = ToPhysicalPath(path);
            if (!File.Exists(physical))
                throw new FileNotFoundException($"Text template not found: {physical}");

            var text = await File.ReadAllTextAsync(physical, Encoding.UTF8);
            foreach (var kv in model)
            {
                text = text.Replace("{{" + kv.Key + "}}", kv.Value ?? string.Empty, StringComparison.Ordinal);
            }
            return text;
        }

        private string NormalizePath(string input)
        {
            var p = input.Replace('\\', '/').Trim();

            if (p.StartsWith("/", StringComparison.Ordinal) || p.StartsWith("~/", StringComparison.Ordinal))
                return p.StartsWith("~/") ? "/" + p[2..] : p;

            if (!Path.HasExtension(p))
                p += ".cshtml";

            return "/Views/" + p.TrimStart('/');
        }

        private string ToPhysicalPath(string absolutePath)
        {
            var rel = absolutePath.TrimStart('/', '\\')
                                   .Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(env.ContentRootPath, rel);
        }
    }
}
