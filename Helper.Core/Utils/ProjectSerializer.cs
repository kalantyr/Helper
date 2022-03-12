using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Helper.Core.Utils
{
    public class ProjectSerializer
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            IgnoreReadOnlyProperties = true
        };

        public void Save(Project project, Stream stream)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var json = JsonSerializer.Serialize(project, SerializerOptions);
            using var writer = new StreamWriter(stream);
            writer.Write(json);
        }

        public Project Load(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            return JsonSerializer.Deserialize<Project>(json, SerializerOptions);
        }
    }
}
