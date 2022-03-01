using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Helper.Tests")]

namespace Helper.Jobs.Impl
{
    public class SyncFilesJob: IJob
    {
        private readonly JobHistory _history = new JobHistory();

        public string Name => "Синхронизация файлов";

        public bool IsDisabled { get; set; }

        public string[] ExcludeFilters { get; } = { ".ini" };

        public bool CompareBeforeCopy { get; } = true;
        
        public TimeSpan? Interval => TimeSpan.FromMinutes(5);

        public IHistory History => _history;
        
        public Action<IJob, string> Message { get; set; }

        public string[] RootFolders { get; set; } = Array.Empty<string>();
        
        public async Task Run(CancellationToken cancellationToken)
        {
            try
            {
                var roots = RootFolders.Select(rf => new RootInfo(rf)).ToArray();
                
                var paths = GetPaths(roots);

                foreach (var path in paths)
                {
                    var actual = GetMostActual(path, roots);
                    foreach (var root in roots.Where(r => r != actual.Item1))
                        Copy(path, actual.Item1, root);
                }

                _history.Add(true);
            }
            catch (Exception e)
            {
                _history.Add(e);
            }
        }

        private IReadOnlyCollection<RelativePath> GetPaths(IEnumerable<RootInfo> roots)
        {
            var result = new List<RelativePath>();

            foreach (var root in roots)
            {
                var paths = GetPaths(root.Directory, root);
                foreach (var path in paths)
                    if (!result.Any(r => r.AreEquals(path)))
                        result.Add(path);
            }

            return result;
        }

        private IReadOnlyCollection<RelativePath> GetPaths(DirectoryInfo directoryInfo, RootInfo root)
        {
            var result = new List<RelativePath>();

            foreach (var fileInfo in directoryInfo.GetFiles())
            {
                if (ExcludeFilters.Any(ef => fileInfo.Name.Contains(ef, StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                var folder = directoryInfo.FullName.Substring(root.Directory.FullName.Length);
                var relativePath = new RelativePath(folder, fileInfo.Name);
                if (!result.Any(r => r.AreEquals(relativePath)))
                    result.Add(relativePath);
            }

            foreach (var directory in directoryInfo.GetDirectories())
            {
                if (ExcludeFilters.Any(ef => directory.Name.Contains(ef, StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                var children = GetPaths(directory, root);
                foreach (var child in children)
                    if (!result.Any(r => r.AreEquals(child)))
                        result.Add(child);
            }

            return result;
        }

        private void Copy(RelativePath path, RootInfo from, RootInfo to)
        {
            var fiFrom = path.GetFileInfo(from);
            var fiTo = path.GetFileInfo(to);

            if (fiTo.Exists)
                if (CompareBeforeCopy)
                    if (AreEquals(fiFrom, fiTo))
                        return;

            if (!fiTo.Directory.Exists)
                fiTo.Directory.Create();

            File.Copy(fiFrom.FullName, fiTo.FullName, true);
        }

        private static bool AreEquals(FileInfo fi1, FileInfo fi2)
        {
            var data1 = File.ReadAllBytes(fi1.FullName);
            var data2 = File.ReadAllBytes(fi2.FullName);

            if (data1.Length != data2.Length)
                return false;

            if (data1.Length == 0)
                return true;

            return data1.SequenceEqual(data2);
        }

        private Tuple<RootInfo, FileInfo> GetMostActual(RelativePath path, IReadOnlyCollection<RootInfo> roots)
        {
            FileInfo actualFile = null;
            RootInfo rt = null;
            var modifyDate = DateTime.MinValue;
            foreach (var root in roots)
            {
                var fileInfo = new FileInfo(path.GetFullName(root));
                if (fileInfo.Exists)
                    if (fileInfo.LastWriteTimeUtc > modifyDate)
                    {
                        actualFile = fileInfo;
                        rt = root;
                        modifyDate = fileInfo.LastWriteTimeUtc;
                    }
            }

            return new Tuple<RootInfo, FileInfo>(rt, actualFile);
        }

        [DebuggerDisplay("{Directory}")]
        internal class RootInfo
        {
            public DirectoryInfo Directory { get; }

            public RootInfo(string path)
            {
                Directory = new DirectoryInfo(path.ToLowerInvariant());
            }
        }

        [DebuggerDisplay("{Folder} \\ {FileName}")]
        internal class RelativePath
        {
            public string Folder { get; }

            public string FileName { get; }

            public RelativePath(string folder, string fileName)
            {
                Folder = folder;
                FileName = fileName;
            }

            public bool AreEquals(RelativePath value)
            {
                if (!value.Folder.Equals(Folder, StringComparison.InvariantCultureIgnoreCase))
                    return false;

                if (!value.FileName.Equals(FileName, StringComparison.InvariantCultureIgnoreCase))
                    return false;

                return true;
            }

            public string GetFullName(RootInfo root)
            {
                return string.Join(@"\", root.Directory.FullName, Folder, FileName);
            }

            public FileInfo GetFileInfo(RootInfo root)
            {
                return new FileInfo(GetFullName(root));
            }
        }
    }
}
