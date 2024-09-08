using System;

namespace libCanOpenSimple
{
    public class ConnectionChangedEventArgs : EventArgs
    {
        private bool _connecting;

        public bool connecting { get { return _connecting; } set { } }

        public ConnectionChangedEventArgs(bool connecting)
        {
            _connecting = connecting;
        }
    }
}
