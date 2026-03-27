using System.Reflection;

namespace superint.ProjectBootstrapper.Infrastructure.Helpers
{
    public static class TemplateLoader
    {
        private const string BaseResourcePrefix = "superint.ProjectBootstrapper.Infrastructure.Templates.";

        public static string LoadTemplate(string templatePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{BaseResourcePrefix}{templatePath}";

            using var stream = assembly.GetManifestResourceStream(resourceName)
                               ?? throw new FileNotFoundException($"Template não encontrado: {resourceName}");

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static string LoadDockerComposeTemplateContent(string templateName) => LoadTemplate($"docker_compose.{templateName}");

        public static string LoadJenkinsTemplate(string templateName) => LoadTemplate($"jenkins.{templateName}");

        public static string LoadJenkinsfileTemplate(string templateName) => LoadTemplate($"jenkinsfile.{templateName}");
    }
}