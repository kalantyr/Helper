using Helper.Events;

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
                if (value != null)
                    _tbName.Text = value.Name;
                else
                    _tbName.Text = null;
            }
        }

        public EventControl()
        {
            InitializeComponent();
        }
    }
}
