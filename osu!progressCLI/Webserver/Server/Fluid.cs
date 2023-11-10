using Fluid;
using Microsoft.Extensions.FileProviders;
using Parlot.Fluent;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;

namespace osu_progressCLI.Webserver.Server
{
    public class FluidRenderer
    {
       public static readonly List<KeyValuePair<string, IFluidTemplate>> templates = new List<KeyValuePair<string, IFluidTemplate>>();

        public static async Task setup(FluidParser parser)
        {
            TemplateOptions.Default.MemberAccessStrategy = new UnsafeMemberAccessStrategy(); //Allow all data to be read (its gonna be fine);
            IFileProvider fileProvider = new PhysicalFileProvider(Path.GetFullPath("Webserver/Fluid"));

            TemplateOptions.Default.FileProvider = fileProvider;

            string[] filepath = Directory.GetFiles(DEFAULTFOLDER);
            string[] files = filepath.Select(path => Path.GetFileName(path)).ToArray();


            foreach (string filename in files)
            {
                if (!parser.TryParse(await File.ReadAllTextAsync(DEFAULTFOLDER + filename), out IFluidTemplate template, out var error))
                {
                    throw new InvalidOperationException($"Failed to parse template: {error}", new FileNotFoundException("The specified template file was not found or could not be read.", filename));
                }

                templates.Add(new KeyValuePair<string, IFluidTemplate>(filename, template));
                Console.WriteLine($"{filename} succsefully parsed and added!");
            }

            //iterate over list of to cache tempaltes

        }
        private static readonly string DEFAULTFOLDER = "Webserver/Fluid/";
        public static async Task<string> RenderTemplateAsync<T>(string staticFilePath, T data, FluidParser parser) where T : class
        {

            if (!parser.TryParse(await File.ReadAllTextAsync(DEFAULTFOLDER + staticFilePath), out var template, out var error))
            {
                throw new InvalidOperationException($"Failed to parse template: {error}", new FileNotFoundException("The specified template file was not found or could not be read.", staticFilePath));
            }

            var context = new TemplateContext(data);
            context.SetValue(data.GetType().Name, data);
            Console.WriteLine(data.GetType().Name);
            return template.Render(context);
        }

        public static async Task<string> RenderTemplateListAsync<T>(string staticFilePath, T data, FluidParser parser) where T : class
        {
            if (!parser.TryParse(await File.ReadAllTextAsync(DEFAULTFOLDER + staticFilePath, Encoding.UTF8), out var template, out var error))
            {
                throw new InvalidOperationException($"Failed to parse template: {error}", new FileNotFoundException("The specified template file was not found or could not be read.", staticFilePath));
            }
            var context = new TemplateContext(data);
            context.SetValue("List", data);
            return template.Render(context);
        }
    }
}
