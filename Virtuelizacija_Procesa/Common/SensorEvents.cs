using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    // Delegati za događaje
    public delegate void TransferStartedHandler();
    public delegate void SampleReceivedHandler(SensorSample sample);
    public delegate void TransferCompletedHandler();
    public delegate void WarningRaisedHandler(string message, SensorSample sample);
    public class SensorEvents
    {
        public event TransferStartedHandler OnTransferStarted;
        public event SampleReceivedHandler OnSampleReceived;
        public event TransferCompletedHandler OnTransferCompleted;
        public event WarningRaisedHandler OnWarningRaised;

        public void RaiseTransferStarted()
        {
            if (OnTransferStarted != null)
                OnTransferStarted();
        }

        public void RaiseSampleReceived(SensorSample sample)
        {
            if (OnSampleReceived != null)
                OnSampleReceived(sample);
        }

        public void RaiseTransferCompleted()
        {
            if (OnTransferCompleted != null)
                OnTransferCompleted();
        }

        public void RaiseWarning(string message, SensorSample sample)
        {
            if (OnWarningRaised != null)
                OnWarningRaised(message, sample);
        }
    }
}
