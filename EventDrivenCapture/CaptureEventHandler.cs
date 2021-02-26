using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventDrivenCapture
{
    public class CaptureEventHandler
    {
        CaptureHandler _helper;
        public CaptureEventHandler(Capture[] captures)
        {
            _captures = captures;
            _helper = new CaptureHandler();
        }
        Capture[] _captures;
        AutoResetEvent _captureEventResetEvent = new AutoResetEvent(false);
        public void Start()
        {
            if (!StartFlag)
            {
                StartFlag = true;
                PauseFlag = false;
                _helper.ExceptionHandler += OnException;
                _helper.CapturedHandler += OnCaptured;
                _helper.CancelHandler += OnCanceled;
                _helper.Start(_captures);
            }
        }
        public void Pause() => PauseFlag = true; 
        public void Continue()
        {
            PauseFlag = false;
            _captureEventResetEvent.Set();
        }
        public void Stop()
        {
            PauseFlag = false;
            StartFlag = false;
            _captureEventResetEvent.Reset();
            _helper.Stop();
        }

        private void OnCaptured(object sender, CaptureEventArgs[] args)
        {
            if (!PauseFlag)
            {
                foreach (var arg in args)
                {
                    arg.Capture.OnCapturedEvent(arg);
                }
                OnCapturedEvent(args);
                _helper.NextFrame();
            }
            else
            {
                foreach (var arg in args)
                {
                    arg.Capture.OnPausedEvent(arg);
                }
                _captureEventResetEvent.WaitOne();
                _helper.NextFrame();
            }
        }
        private void OnException(object sender, CaptureEventArgs[] args)
        {
            foreach (var arg in args)
            {
                arg.Capture.OnExceptionEvent(arg);
            }
            OnExceptionEvent(args);
            Stop();
        }
        private void OnCanceled(object sender, CaptureEventArgs[] args)
        {
            OnCanceledEvent(args);
        }

        EventHandler<CaptureEventArgs[]> _clHandler;
        public event EventHandler<CaptureEventArgs[]> CanceledEventHandler
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
        void OnCanceledEvent(CaptureEventArgs[] e) => _clHandler?.Invoke(this, e);

        EventHandler<CaptureEventArgs[]> _ceHandler;
        public event EventHandler<CaptureEventArgs[]> CapturedEventHandler
        {
            add
            {
                _ceHandler -= value;
                _ceHandler += value;
            }
            remove
            {
                _ceHandler -= value;
            }
        }
        void OnCapturedEvent(CaptureEventArgs[] e) => _ceHandler?.Invoke(this, e);

        EventHandler<CaptureEventArgs[]> _epHandler;
        public event EventHandler<CaptureEventArgs[]> ExceptionEventHandler
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
        void OnExceptionEvent(CaptureEventArgs[] e) => _epHandler?.Invoke(this, e);

        bool StartFlag // true is paused
        {
            get
            {
                return _startflag == 1;  
            }
            set
            {
                if (value)
                {

                    int newFlagValue = 1;
                    int initialFlagValue;
                    do
                    {
                        initialFlagValue = _startflag;
                    } while (initialFlagValue != Interlocked.CompareExchange(
                        ref _startflag, newFlagValue, initialFlagValue));
                }
                else
                {
                    int newFlagValue = 0;
                    int initialFlagValue;
                    do
                    {
                        initialFlagValue = _startflag;
                    } while (initialFlagValue != Interlocked.CompareExchange(
                        ref _startflag, newFlagValue, initialFlagValue));
                }
            }
        }
        private volatile int _startflag;
        bool PauseFlag // true is paused
        {
            get
            {
                return _pauseflag == 1;  
            }
            set
            {
                if (value)
                {

                    int newFlagValue = 1;
                    int initialFlagValue;
                    do
                    {
                        initialFlagValue = _pauseflag;
                    } while (initialFlagValue != Interlocked.CompareExchange(
                        ref _pauseflag, newFlagValue, initialFlagValue));
                }
                else
                {
                    int newFlagValue = 0;
                    int initialFlagValue;
                    do
                    {
                        initialFlagValue = _pauseflag;
                    } while (initialFlagValue != Interlocked.CompareExchange(
                        ref _pauseflag, newFlagValue, initialFlagValue));
                }
            }
        }
        private volatile int _pauseflag;
    }
}
