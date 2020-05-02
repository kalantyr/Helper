using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Helper.Model
{
    public class Solution
    {
        public string File { get; private set; }

        public IReadOnlyCollection<Project> Projects { get; private set; }

        public static Solution FromFile(string fileName)
        {
            var projects = new List<Project>();

            using var file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(file);
            var text = reader.ReadToEnd();
            var lines = text
                .Split("Project", StringSplitOptions.RemoveEmptyEntries)
                .Where(s => s.StartsWith("(\"{"));
            foreach (var line in lines)
                if (line.Contains("proj\", \"{"))
                {
                    var parts = line.Split(",");
                    var filename = Path.Combine(Path.GetDirectoryName(fileName), parts[1].Trim().Trim('"'));
                    projects.Add(new Project(filename));
                }

            return new Solution
            {
                File = fileName,
                Projects = projects.OrderBy(pr => pr.Name).ToArray()
            };
        }

        public void Remove(IEnumerable<Project> projects)
        {
            using var file = new FileStream(File, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(file);
            var text = reader.ReadToEnd();

            throw new NotImplementedException();
        }
    }

    public class Project
    {
        private string _name;

        public string File { get; }

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                    _name = Path.GetFileNameWithoutExtension(File);
                return _name;
            }
        }

        public Project(string filename)
        {
            File = filename;
        }

        public override string ToString()
        {
            return Name;
        }

        public IReadOnlyCollection<Reference> GetReferences()
        {
            using var file = new FileStream(File, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(file);
            var text = reader.ReadToEnd();
            var xml = XElement.Parse(text);
            var projectReferences = xml
                .Elements(XName.Get("ItemGroup", xml.Name.NamespaceName))
                .SelectMany(x => x.Elements(XName.Get("ProjectReference", xml.Name.NamespaceName)))
                .Select(x => x.Element(XName.Get("Name", xml.Name.NamespaceName)))
                .Select(x => new ProjectReference(x.Value))
                .ToArray();

            return projectReferences;
        }
    }

    public abstract class Reference
    {
    }

    public class ProjectReference: Reference
    {
        public string ProjectName { get; }

        public ProjectReference(string projectName)
        {
            ProjectName = projectName;
        }
    }
}
