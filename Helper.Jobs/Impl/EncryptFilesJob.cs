using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Helper.Jobs.Impl
{
    public class EncryptFilesJob: IJob
    {
        private readonly JobHistory _history = new JobHistory();

        public string Name => "Encrypt";

        public string[] ExcludeFilters { get; } = { ".ini", ".tmp." };

        public bool IsDisabled { get; set; }

        public TimeSpan? Interval => TimeSpan.FromMinutes(15);

        public IHistory History => _history;

        public EncryptOptions Options { get; set; } = new EncryptOptions();

        public Action<IJob, string> Message { get; set; }

        public async Task Run(CancellationToken cancellationToken)
        {
            try
            {
                Encrypt(new DirectoryInfo(Options.SourceFolder), new DirectoryInfo(Options.DestFolder), new CryptoEngine(Options.Password));
                _history.Add(true);
            }
            catch (Exception e)
            {
                _history.Add(e);
            }
        }

        private void Encrypt(DirectoryInfo sourceFolder, DirectoryInfo destFolder, ICryptoEngine cryptoEngine)
        {
            if (!destFolder.Exists)
                destFolder.Create();

            foreach (var file in sourceFolder.GetFiles())
            {
                if (ExcludeFilters.Any(ef => file.Name.Contains(ef, StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                byte[] data;
                using (var f = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new BinaryReader(f))
                    data = reader.ReadBytes((int)f.Length);

                data = Options.Decrypt
                    ? cryptoEngine.Decrypt(data)
                    : cryptoEngine.Encrypt(data);

                var destFileName = Path.Combine(destFolder.FullName, file.Name);
                using (var f = new FileStream(destFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    f.Write(data);
            }

            foreach (var directory in sourceFolder.GetDirectories())
            {
                if (ExcludeFilters.Any(ef => directory.Name.Contains(ef, StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                var childFolderName = Path.Combine(destFolder.FullName, directory.Name);
                Encrypt(directory, new DirectoryInfo(childFolderName), cryptoEngine);
            }
        }

        public class EncryptOptions
        {
            public string SourceFolder { get; set; }

            public string DestFolder { get; set; }

            public string Password { get; set; }

            public bool Decrypt { get; set; } = false;
        }
    }
}
