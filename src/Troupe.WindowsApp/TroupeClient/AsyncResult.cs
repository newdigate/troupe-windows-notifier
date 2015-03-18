using System;
using System.Threading;

namespace Troupe.WindowsApp.TroupeClient {
    public class AsyncResult : IAsyncResult {
        private bool _isCompleted;

        private WaitHandle _asyncWaitHandle = new AutoResetEvent(false);

        private object _asyncState;

        private bool _completedSynchronously;

        public void SetAsyncState(object asyncState) {
            _asyncState = asyncState;
        }

        public bool IsCompleted {
            get { return _isCompleted; }
        }

        public WaitHandle AsyncWaitHandle {
            get { return _asyncWaitHandle; }
        }

        public object AsyncState {
            get { return _asyncState; }
        }

        public bool CompletedSynchronously {
            get { return _completedSynchronously; }
        }
    }
}