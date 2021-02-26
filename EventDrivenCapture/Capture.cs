using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace EventDrivenCapture
{
    public partial class Capture : IDisposable
    {
        CaptureSetting _setting;
        public Capture(CaptureSetting setting)
        {
            _setting = setting;
            CapturedImage = new Bitmap(setting.Width, setting.Height, PixelFormat.Format32bppArgb); // for reusing bitmap 
        }

        public CaptureSetting CaptureSetting { get => _setting; }

        public Bitmap CapturedImage { get; private set; }

        EventHandler<CaptureEventArgs> _cHandler;
        public event EventHandler<CaptureEventArgs> CapturedEventHandler
        {
            add
            {
                _cHandler -= value;
                _cHandler += value;
            }
            remove
            {
                _cHandler -= value;
            }
        }

        public void OnCapturedEvent(CaptureEventArgs e) => _cHandler?.Invoke(this, e);

        EventHandler<CaptureEventArgs> _pHandler;
        public event EventHandler<CaptureEventArgs> PausedEventHandler
        {
            add
            {
                _pHandler -= value;
                _pHandler += value;
            }
            remove
            {
                _pHandler -= value;
            }
        }

        public void OnPausedEvent(CaptureEventArgs e) => _pHandler?.Invoke(this, e);

        EventHandler<CaptureEventArgs> _exHandler;
        public event EventHandler<CaptureEventArgs> ExceptionEventHandler
        {
            add
            {
                _exHandler -= value;
                _exHandler += value;
            }
            remove
            {
                _exHandler -= value;
            }
        }

        public void OnExceptionEvent(CaptureEventArgs e) => _exHandler?.Invoke(this, e);

        #region dispose
        private bool _disposed = false;
        ~Capture() => Dispose(false);
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                CapturedImage?.Dispose();
            }

            _disposed = true;
        }
        #endregion
    }
}
