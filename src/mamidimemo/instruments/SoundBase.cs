// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Smf;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Instruments.Envelopes;

namespace zanac.MAmidiMEmo.Instruments
{

    /// <summary>
    /// 
    /// </summary>
    public abstract class SoundBase : IDisposable
    {
        public InstrumentBase ParentModule
        {
            get;
            private set;
        }

        public SoundManagerBase ParentManager
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public TimbreBase Timbre
        {
            get;
            private set;
        }

        public bool IsDisposed
        {
            get;
            private set;
        }

        public bool IsKeyOff
        {
            get;
            private set;
        }

        public bool IsSoundOff
        {
            get;
            private set;
        }

        /// <summary>
        /// チップ上の物理的なチャンネル(MIDI chと区別するためスロットとする)
        /// </summary>
        public int Slot
        {
            get;
            private set;
        }

        /// <summary>
        /// MIDIイベント
        /// </summary>
        public NoteOnEvent NoteOnEvent
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slot">チップ上の物理的なチャンネル(MIDI chと区別するためスロットとする)</param>
        protected SoundBase(InstrumentBase parentModule, SoundManagerBase manager, TimbreBase timbre, NoteOnEvent noteOnEvent, int slot)
        {
            NoteOnEvent = noteOnEvent;
            Slot = slot;
            ParentModule = parentModule;
            ParentManager = manager;
            Timbre = timbre;
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
            if (!IsDisposed)
            {
                KeyOff();
                SoundOff();
            }

            IsDisposed = true;
        }

        /// <summary>
        /// サウンドオン
        /// </summary>
        public virtual void KeyOn()
        {
            if (ParentModule.ModulationDepthes[NoteOnEvent.Channel] > 64 ||
                ParentModule.Modulations[NoteOnEvent.Channel] > 0)
                ModulationEnabled = true;

            if (ParentModule.Portamentos[NoteOnEvent.Channel] >= 64)
            {
                int ln = ParentManager.GetLastNoteNumber(NoteOnEvent.Channel);
                if (ln >= 0)
                {
                    PortamentoDeltaNoteNumber = ln - NoteOnEvent.NoteNumber;
                    portStartNoteDeltSign = Math.Sign(PortamentoDeltaNoteNumber);

                    PortamentoEnabled = true;
                }
            }

            var adsrs = Timbre.SDS.ADSR;
            EnableADSR = adsrs.Enable;
            if (EnableADSR)
            {
                AdsrEngine = new AdsrEngine();
                AdsrEngine.SetAttackRate(Math.Pow(10d * (127d - adsrs.AR) / 127d, 2));
                AdsrEngine.SetDecayRate(Math.Pow(10d * (adsrs.DR / 127d), 2));
                AdsrEngine.SetReleaseRate(Math.Pow(60d * (adsrs.RR / 127d), 2));
                AdsrEngine.SetSustainLevel((127d - adsrs.SL) / 127d);
                AdsrEngine.Gate(true);
            }

            var efs = Timbre.SDS.FxS;
            if (efs != null)
                EnableFx = efs.Enable;
        }

        /// <summary>
        ///キーオフ
        /// </summary>
        public virtual void KeyOff()
        {
            if (IsKeyOff)
                return;

            IsKeyOff = true;
            AdsrEngine?.Gate(false);

            if (CurrentAdsrState == AdsrState.SoundOff)
                SoundOff();
        }

        /// <summary>
        ///キーオフ
        /// </summary>
        public virtual void SoundOff()
        {
            IsSoundOff = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void UpdateVolume()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void UpdatePanpot()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void UpdatePitch()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected double CalcCurrentFrequency()
        {
            double d = CalcCurrentPitch();

            double noteNum = Math.Pow(2.0, ((double)NoteOnEvent.NoteNumber + d - 69.0) / 12.0);
            double freq = 440.0 * noteNum;
            return freq;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected double CalcCurrentPitch()
        {
            var pitch = (int)ParentModule.Pitchs[NoteOnEvent.Channel] - 8192;
            var range = (int)ParentModule.PitchBendRanges[NoteOnEvent.Channel];

            double d1 = ((double)pitch / 8192d) * range;
            double d = d1 + ModultionDeltaNoteNumber + PortamentoDeltaNoteNumber + ArpeggiateDeltaNoteNumber;

            if (FxEngine != null)
                d += FxEngine.DeltaNoteNumber;

            return d;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected double CalcCurrentVolume()
        {
            double v = 1;

            v *= ParentModule.Expressions[NoteOnEvent.Channel] / 127d;
            v *= ParentModule.Volumes[NoteOnEvent.Channel] / 127d;
            v *= NoteOnEvent.Velocity / 127d;

            if (AdsrEngine != null)
                v *= AdsrEngine.OutputLevel;

            if (FxEngine != null)
                v *= FxEngine.OutputLevel;

            v *= ArpeggiateLevel;

            return v;
        }

        private double f_ArpeggiateDeltaNoteNumber;

        /// <summary>
        /// 
        /// </summary>
        internal protected double ArpeggiateDeltaNoteNumber
        {
            get
            {
                return f_ArpeggiateDeltaNoteNumber;
            }
            internal set
            {
                if (f_ArpeggiateDeltaNoteNumber != value)
                {
                    f_ArpeggiateDeltaNoteNumber = value;
                    UpdatePitch();
                }
            }
        }

        private double f_ArpeggiateVolume = 1d;

        /// <summary>
        /// 
        /// </summary>
        internal protected double ArpeggiateLevel
        {
            get
            {
                return f_ArpeggiateVolume;
            }
            internal set
            {
                if (f_ArpeggiateVolume != value)
                {
                    f_ArpeggiateVolume = value;
                    UpdateVolume();
                }
            }
        }

        private double processFx(object state)
        {
            if (!IsDisposed && !IsSoundOff && EnableFx && FxEngine != null)
            {
                FxEngine.Process(this, IsKeyOff, IsSoundOff);

                UpdatePitch();
                UpdateVolume();

                EnableFx = FxEngine.Active;

                if (EnableFx)
                    return FxEngine.Settings.EnvelopeInterval;
            }
            return -1;
        }

        private double processAdsr(object state)
        {
            if (!IsDisposed && !IsSoundOff && EnableADSR && AdsrEngine != null)
            {
                AdsrEngine.Process();

                UpdateVolume();

                if (AdsrEngine.AdsrState != AdsrState.SoundOff)
                    return HighPrecisionTimer.TIMER_BASIC_INTERVAL;

                EnableADSR = false;
                SoundOff();
            }
            return -1;
        }

        private double processPortamento(object state)
        {
            if (!IsDisposed && !IsSoundOff && PortamentoEnabled && PortamentoDeltaNoteNumber != 0)
            {
                double delta = -portStartNoteDeltSign * 12d / Math.Pow(((double)ParentModule.PortamentoTimes[NoteOnEvent.Channel] / 2d) + 1d, 1.25);
                PortamentoDeltaNoteNumber += delta;

                if (portStartNoteDeltSign < 0 && PortamentoDeltaNoteNumber >= 0)
                    PortamentoDeltaNoteNumber = 0;
                else if (portStartNoteDeltSign > 0 && PortamentoDeltaNoteNumber <= 0)
                    PortamentoDeltaNoteNumber = 0;

                UpdatePitch();

                if (PortamentoDeltaNoteNumber != 0)
                    return HighPrecisionTimer.TIMER_BASIC_INTERVAL;

                PortamentoEnabled = false;
            }
            return -1;
        }

        private double processModulation(object state)
        {
            if (!IsDisposed && !IsSoundOff && ModulationEnabled)
            {
                double radian = 2 * Math.PI * (modulationStep / HighPrecisionTimer.TIMER_BASIC_HZ);

                double mdepth = 0;
                if (ParentModule.ModulationDepthes[NoteOnEvent.Channel] > 64)
                {
                    if (modulationStartCounter < 10d * HighPrecisionTimer.TIMER_BASIC_HZ)
                        modulationStartCounter += 1.0;

                    if (modulationStartCounter > ParentModule.GetModulationDelaySec(NoteOnEvent.Channel) * HighPrecisionTimer.TIMER_BASIC_HZ)
                        mdepth = (double)ParentModule.ModulationDepthes[NoteOnEvent.Channel] / 127d;
                }
                //急激な変化を抑制
                var mv = ((double)ParentModule.Modulations[NoteOnEvent.Channel] / 127d) + mdepth;
                if (mv != modultionLevel)
                    modultionLevel += (mv - modultionLevel) / 1.25;

                double modHz = radian * ParentModule.GetModulationRateHz(NoteOnEvent.Channel);
                ModultionDeltaNoteNumber = modultionLevel * Math.Sin(modHz);
                ModultionDeltaNoteNumber *= ((double)ParentModule.ModulationDepthRangesNote[NoteOnEvent.Channel] +
                    ((double)ParentModule.ModulationDepthRangesCent[NoteOnEvent.Channel] / 127d));

                modulationStep += 1.0;
                if (modHz > 2 * Math.PI)
                    modulationStep = 0;

                UpdatePitch();

                return HighPrecisionTimer.TIMER_BASIC_INTERVAL;
            }
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        public AdsrState CurrentAdsrState
        {
            get
            {
                if (AdsrEngine != null)
                {
                    return AdsrEngine.AdsrState;
                }
                else
                {
                    if (!IsKeyOff)
                        return AdsrState.Sustain;
                    else
                        return AdsrState.SoundOff;
                }
            }
        }

        #region modulation

        /// <summary>
        /// モジュレーションの進角
        /// </summary>
        public double modulationStep;

        /// <summary>
        /// モジュレーションの開始カウンタ
        /// </summary>
        public double modulationStartCounter;

        /// <summary>
        /// モジュレーションの効き具合(0-1.0)
        /// </summary>
        public double ModultionDeltaNoteNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// モジュレーションホイール値(0-1.0)
        /// </summary>
        private double modultionLevel;

        private bool f_modulationEnabled;

        /// <summary>
        /// モジュレーションの有効/無効
        /// </summary>
        public bool ModulationEnabled
        {
            get
            {
                return f_modulationEnabled;
            }
            set
            {
                if (value != f_modulationEnabled)
                {
                    if (value)
                    {
                        f_modulationEnabled = value;
                    }
                    else
                    {
                        if (ParentModule.ModulationDepthes[NoteOnEvent.Channel] > 64)
                            return;

                        f_modulationEnabled = value;
                    }
                    if (f_modulationEnabled)
                        HighPrecisionTimer.SetFixedPeriodicCallback(new Func<object, double>(processModulation), null);
                    //HighPrecisionTimer.SetPeriodicCallback(new Func<object, double>(processModulation), HighPrecisionTimer.TIMER_BASIC_INTERVAL, null);
                }
            }
        }

        #endregion

        #region portamento

        public double PortamentoDeltaNoteNumber
        {
            get;
            private set;
        }

        private double portStartNoteDeltSign;

        private bool f_portamentoEnabled;

        /// <summary>
        /// ポルタメントの有効無効
        /// </summary>
        public bool PortamentoEnabled
        {
            get
            {
                return f_portamentoEnabled;
            }
            set
            {
                if (value != f_portamentoEnabled)
                {
                    f_portamentoEnabled = value;

                    if (f_portamentoEnabled)
                        HighPrecisionTimer.SetFixedPeriodicCallback(new Func<object, double>(processPortamento), null);
                    //HighPrecisionTimer.SetPeriodicCallback(new Func<object, double>(processPortamento), HighPrecisionTimer.TIMER_BASIC_INTERVAL, null);
                }
            }
        }

        #endregion

        private bool f_AdsrEnabled;

        /// <summary>
        /// ADSRの有効無効
        /// </summary>
        public bool EnableADSR
        {
            get
            {
                return f_AdsrEnabled;
            }
            set
            {
                if (value != f_AdsrEnabled)
                {
                    f_AdsrEnabled = value;

                    if (f_AdsrEnabled)
                        HighPrecisionTimer.SetFixedPeriodicCallback(new Func<object, double>(processAdsr), null);
                    //HighPrecisionTimer.SetPeriodicCallback(new Func<object, double>(processAdsr), HighPrecisionTimer.TIMER_BASIC_INTERVAL, null);
                }
            }
        }

        private bool f_FxEnabled;

        /// <summary>
        /// Fxの有効無効
        /// </summary>
        public bool EnableFx
        {
            get
            {
                return f_FxEnabled;
            }
            set
            {
                if (value != f_FxEnabled)
                {
                    f_FxEnabled = value;

                    if (f_FxEnabled)
                    {
                        var efs = Timbre.SDS.FxS;
                        if (efs != null)
                        {
                            FxEngine = efs.CreateEngine();
                            HighPrecisionTimer.SetPeriodicCallback(new Func<object, double>(processFx), efs.EnvelopeInterval, null);
                        }
                    }
                }
            }
        }

        protected AdsrEngine AdsrEngine { get; private set; }

        protected AbstractFxEngine FxEngine { get; private set; }

    }
}
