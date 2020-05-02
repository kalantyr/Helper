using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Helper.Model;

namespace Helper.Tools
{
    public class RemoveUnusedProjectsTool
    {
        public async Task<ActionResult> Run(CancellationToken cancellationToken)
        {
            try
            {
                var links = new List<string> { RootProjectName };
                SearchReferences(RootProjectName, links, Solution.Projects);
                var toRemove = Solution.Projects.Where(p => !links.Contains(p.Name)).ToArray();

                cancellationToken.ThrowIfCancellationRequested();

                Solution.Remove(toRemove);

                return ActionResult.Success($"Projects removed ({toRemove.Length})",
                    toRemove.Select(p => ActionResult.Success($"{p.Name} removed")).ToArray());
            }
            catch (Exception e)
            {
                return ActionResult.FromError(e.GetBaseException().Message);
            }
        }

        private static void SearchReferences(string projName, ICollection<string> projNames, IReadOnlyCollection<Helper.Model.Project> projects)
        {
            var project = projects.FirstOrDefault(pr => pr.Name == projName);
            if (project == null)
                throw new ArgumentOutOfRangeException(nameof(projName));

            var references = project.GetReferences().OfType<ProjectReference>().Select(r => r.ProjectName);
            foreach (var id in references)
                if (!projNames.Contains(id))
                {
                    projNames.Add(id);
                    SearchReferences(id, projNames, projects);
                }
        }

        public Solution Solution { get; set; }
        
        public string RootProjectName { get; set; }
    }
}
