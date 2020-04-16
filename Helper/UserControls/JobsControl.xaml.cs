using System.Linq;

namespace Helper.UserControls
{
    public partial class JobsControl
    {
        private Project _project;

        public Project Project
        {
            get => _project;
            set
            {
                if (value == Project)
                    return;

                _project = value;
                TuneControls();
            }
        }

        public JobsControl()
        {
            InitializeComponent();
        }

        private void TuneControls()
        {
            if (Project == null)
            {
                _lb.ItemsSource = null;
                return;
            }

            var checkerControls = Project.AllJobs
                .OrderBy(ch => ch.Name)
                .Select(job => new JobControl { Job = job })
                .ToArray();
            _lb.ItemsSource = checkerControls;
        }
    }
}
