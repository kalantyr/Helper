using Helper.Core.Events;

namespace Helper.UserControls
{
    public partial class EventControl
    {
        private IEvent _event;

        public IEvent Event
        {
            get => _event;
            set
            {
                _event = value;
                _tbName.Text = value?.Name;
            }
        }

        public EventControl()
        {
            InitializeComponent();
        }
    }
}
