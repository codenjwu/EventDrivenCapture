using System;

namespace EventDrivenCapture
{
    public partial class CaptureEventArgs : EventArgs
    {
        public Capture Capture { get; set; }
    }
}
