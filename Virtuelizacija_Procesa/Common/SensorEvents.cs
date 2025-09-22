using System;

namespace Common
{
    // Delegati za postojeće događaje
    public delegate void TransferStartedHandler();
    public delegate void SampleReceivedHandler(SensorSample sample);
    public delegate void TransferCompletedHandler();
    public delegate void WarningRaisedHandler(string message, SensorSample sample);

    // Novi delegati za analitiku 1
    public delegate void VolumeSpikeHandler(string message, SensorSample sample, double deltaV);
    public delegate void OutOfBandWarningHandler(string message, SensorSample sample, double avg);

    public class SensorEvents
    {
        //  Postojeći događaji
        public event TransferStartedHandler OnTransferStarted;
        public event SampleReceivedHandler OnSampleReceived;
        public event TransferCompletedHandler OnTransferCompleted;
        public event WarningRaisedHandler OnWarningRaised;

        //  Novi događaji 
        public event VolumeSpikeHandler OnVolumeSpike;
        public event OutOfBandWarningHandler OnOutOfBandWarning;

        //  Metode za podizanje događaja
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


        public void RaiseVolumeSpike(string message, SensorSample sample, double deltaV)
        {
            if (OnVolumeSpike != null)
                OnVolumeSpike(message, sample, deltaV);
        }

        public void RaiseOutOfBandWarning(string message, SensorSample sample, double avg)
        {
            if (OnOutOfBandWarning != null)
                OnOutOfBandWarning(message, sample, avg);
        }
    }
}
