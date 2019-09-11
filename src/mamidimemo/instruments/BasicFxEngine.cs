using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Instruments
{

    /// <summary>
    /// 
    /// </summary>
    public class BasicFxEngine : AbstractFxEngine
    {
        private BasicFxSettings settings;

        public override AbstractFxSettingsBase Settings
        {
            get
            {
                return settings;
            }
        }

        private double f_OutputLevel;

        /// <summary>
        /// 
        /// </summary>
        public override double OutputLevel
        {
            get => f_OutputLevel;
        }

        private double f_DeltaNoteNumber;

        /// <summary>
        /// 
        /// </summary>
        public override double DeltaNoteNumber
        {
            get => f_DeltaNoteNumber;
        }

        private bool f_Active;

        /// <summary>
        /// エフェクトが動作しているかどうか falseなら終了
        /// </summary>
        public override bool Active
        {
            get
            {
                return f_Active;
            }
            protected set
            {
                f_Active = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        public BasicFxEngine(BasicFxSettings settings)
        {
            this.settings = settings;

            f_OutputLevel = 1d;
            f_DeltaNoteNumber = 0d;
        }

        public uint volumeCounter;

        public uint pitchCounter;

        /// <summary>
        /// 
        /// </summary>
        protected virtual void ProcessCore(SoundBase sound, bool isKeyOff, bool isSoundOff)
        {
            //volume
            if (settings.VolumeEnvelopesNums.Length > 0)
            {
                if (!isKeyOff)
                {
                    var vm = settings.VolumeEnvelopesNums.Length;
                    if (settings.VolumeEnvelopesReleasePoint >= 0)
                        vm = settings.VolumeEnvelopesReleasePoint;
                    if (volumeCounter >= vm)
                    {
                        if (settings.VolumeEnvelopesRepeatPoint >= 0)
                            volumeCounter = (uint)settings.VolumeEnvelopesRepeatPoint;
                        else
                            volumeCounter = (uint)vm;

                        if (volumeCounter >= settings.VolumeEnvelopesNums.Length)
                            volumeCounter = (uint)(settings.VolumeEnvelopesNums.Length - 1);
                    }
                }
                else
                {
                    if (settings.VolumeEnvelopesRepeatPoint < 0)
                        volumeCounter = (uint)settings.VolumeEnvelopesNums.Length;

                    if (volumeCounter >= settings.VolumeEnvelopesNums.Length)
                        volumeCounter = (uint)(settings.VolumeEnvelopesNums.Length - 1);
                }
                int vol = settings.VolumeEnvelopesNums[volumeCounter++];

                f_OutputLevel = vol / 127d;
            }

            //pitch
            if (settings.PitchEnvelopesNums.Length > 0)
            {
                if (!isKeyOff)
                {
                    var vm = settings.PitchEnvelopesNums.Length;
                    if (settings.PitchEnvelopesReleasePoint >= 0)
                        vm = settings.PitchEnvelopesReleasePoint;
                    if (pitchCounter >= vm)
                    {
                        if (settings.PitchEnvelopesRepeatPoint >= 0)
                            pitchCounter = (uint)settings.PitchEnvelopesRepeatPoint;
                        else
                            pitchCounter = (uint)vm;

                        if (pitchCounter >= settings.PitchEnvelopesNums.Length)
                            pitchCounter = (uint)(settings.PitchEnvelopesNums.Length - 1);
                    }
                }
                else
                {
                    if (settings.PitchEnvelopesRepeatPoint < 0)
                        pitchCounter = (uint)settings.PitchEnvelopesNums.Length;

                    if (pitchCounter >= settings.PitchEnvelopesNums.Length)
                        pitchCounter = (uint)(settings.PitchEnvelopesNums.Length - 1);
                }
                double pitch = settings.PitchEnvelopesNums[pitchCounter++];
                double range = settings.PitchEnvelopeRange;

                f_DeltaNoteNumber += ((double)pitch / 8192d) * range;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Process(SoundBase sound, bool isKeyOff, bool isSoundOff)
        {
            f_Active = true;

            if (!settings.Enable || isSoundOff)
            {
                f_Active = false;
                return;
            }

            ProcessCore(sound, isKeyOff, isSoundOff);
        }
    }
}
