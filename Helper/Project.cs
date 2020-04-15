using System.Collections.Generic;
using System.IO;
using Helper.Checkers;
using Newtonsoft.Json;

namespace Helper
{
    public class Project
    {
        private static readonly JsonSerializer JsonSerializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
        });

        [JsonIgnore]
        public IReadOnlyCollection<IChecker> AllCheckers => Checkers.ChatAvailableChecker;

        public AllCheckers Checkers { get; set; }

        public void Save(Stream stream)
        {
            using var writer = new StreamWriter(stream);
                JsonSerializer.Serialize(writer, this);
        }

        public static Project Load(Stream stream)
        {
            using var reader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(reader);
            return JsonSerializer.Deserialize<Project>(jsonReader);
        }
    }

    public class AllCheckers
    {
        public ChatAvailableChecker[] ChatAvailableChecker { get; set; }
    }
}
