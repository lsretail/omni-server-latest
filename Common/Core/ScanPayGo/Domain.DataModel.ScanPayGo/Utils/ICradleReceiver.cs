using System;
using System.Collections.Generic;
using System.Text;

namespace LSRetail.Omni.Domain.DataModel.ScanPayGo.Utils
{
    public enum CradleState
    {
        Unknown = 0,
        OutOfCradle = 1,
        InCradle = 2,
    }

    public class CradleStateChangedEventArgs : EventArgs
    {
        public CradleState State { get; set; }
    }

    public interface ICradleStateReceiver
    {
        CradleState CradleState { get; }

        event EventHandler<CradleStateChangedEventArgs> CradleStateChanged;

        void StartListening();

        void StopListening();
    }
}
