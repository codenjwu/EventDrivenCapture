using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventDrivenCapture
{
    internal class CaptureHandler
    {
        AutoResetEvent _captureResetEvent = new AutoResetEvent(true);
        bool _captureInPorcess = false;
        CaptureEventArgs[] _captureArgs = null;
        CancellationTokenSource _capturecancelSource = new CancellationTokenSource();

        EventHandler<CaptureEventArgs[]> _cHandler;
        public event EventHandler<CaptureEventArgs[]> CapturedHandler
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
        void OnCapturedEvent(CaptureEventArgs[] e)
        {
            _cHandler?.Invoke(this, e);
        }

        EventHandler<CaptureEventArgs[]> _epHandler;
        public event EventHandler<CaptureEventArgs[]> ExceptionHandler
        {
            add
            {
                _epHandler -= value;
                _epHandler += value;
            }
            remove
            {
                _epHandler -= value;
            }
        }
        void OnExceptionEvent(CaptureEventArgs[] e)
        {
            _epHandler?.Invoke(this, e);
        }

        EventHandler<CaptureEventArgs[]> _clHandler;
        public event EventHandler<CaptureEventArgs[]> CancelHandler
        {
            add
            {
                _clHandler -= value;
                _clHandler += value;
            }
            remove
            {
                _clHandler -= value;
            }
        }
        void OnCancelEvent(CaptureEventArgs[] e)
        {
            _clHandler?.Invoke(this, e);
        }

        EventHandler<CaptureEventArgs[]> _ieplHandler;
        public event EventHandler<CaptureEventArgs[]> InnerExceptionHandler
        {
            add
            {
                _ieplHandler -= value;
                _ieplHandler += value;
            }
            remove
            {
                _ieplHandler -= value;
            }
        }
        void OnInnerExceptionEvent(CaptureEventArgs[] e)
        {
            _ieplHandler?.Invoke(this, e);
        }

        private void CaptureFrame()
        {
            var token = _capturecancelSource.Token;
            _captureInPorcess = true;
            var task = Task.Run(() =>
             {
                 try
                 {
                     token.ThrowIfCancellationRequested();
                     while (_captureResetEvent.WaitOne())
                     {
                         for (int i = 0; i < _captureArgs.Length; i++)
                         {
                             int width = _captureArgs[i].Capture.CaptureSetting.Width;
                             int height = _captureArgs[i].Capture.CaptureSetting.Height;

                             Graphics captureGraphics = Graphics.FromImage(_captureArgs[i].Capture.CapturedImage);
                             captureGraphics.CopyFromScreen(_captureArgs[i].Capture.CaptureSetting.StartX, _captureArgs[i].Capture.CaptureSetting.StartY, 0, 0, new Size(width, height));
                         }
                         token.ThrowIfCancellationRequested();
                         try
                         {
                             OnCapturedEvent(_captureArgs);
                         }
                         catch (Exception e)
                         {
                             OnExceptionEvent(_captureArgs);
                             continue;
                         }
                     }
                 }
                 catch (OperationCanceledException ex)
                 {
                     _captureInPorcess = false;
                     _captureResetEvent.Reset();
                     _capturecancelSource.Dispose();
                     _capturecancelSource = new CancellationTokenSource();
                     OnCancelEvent(_captureArgs);
                 }
                 catch (Exception e)
                 {
                     _captureInPorcess = false;
                     _captureResetEvent.Reset();
                     _capturecancelSource.Dispose();
                     _capturecancelSource = new CancellationTokenSource();
                     OnInnerExceptionEvent(_captureArgs);
                 }
             }, token);
        }

        internal void Start(Capture[] captures)
        {
            if (_captureArgs == null || !_captureArgs.Any())
                _captureArgs = captures.Select(s => new CaptureEventArgs { Capture = s }).ToArray();
            if (!_captureInPorcess)
                CaptureFrame();
        }

        internal void Stop() => _capturecancelSource.Cancel();

        internal void NextFrame() => _captureResetEvent.Set();

        public void CancelCapture() => _capturecancelSource.Cancel();
    }
}
