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

    // novi delegati za Analitiku 2
    public delegate void TemperatureSpikeDHTHandler(string message, SensorSample sample, double deltaTdht);
    public delegate void TemperatureSpikeBMPHandler(string message, SensorSample sample, double deltaTbmp);

    public class SensorEvents
    {
        //  Postojeći događaji
        public event TransferStartedHandler OnTransferStarted;
        public event SampleReceivedHandler OnSampleReceived;
        public event TransferCompletedHandler OnTransferCompleted;
        public event WarningRaisedHandler OnWarningRaised;

        //  novi događaji 
        public event VolumeSpikeHandler OnVolumeSpike;
        public event OutOfBandWarningHandler OnOutOfBandWarning;

        //  novi događaji 
        public event TemperatureSpikeDHTHandler OnTemperatureSpikeDHT;
        public event TemperatureSpikeBMPHandler OnTemperatureSpikeBMP;


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

        // analitika 1
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

        // analitika 2
        public void RaiseTemperatureSpikeDHT(string message, SensorSample sample, double deltaTdht)
        {
            if (OnTemperatureSpikeDHT != null)
                OnTemperatureSpikeDHT(message, sample, deltaTdht);
        }

        public void RaiseTemperatureSpikeBMP(string message, SensorSample sample, double deltaTbmp)
        {
            if (OnTemperatureSpikeBMP != null)
                OnTemperatureSpikeBMP(message, sample, deltaTbmp);
        }
    }
}
