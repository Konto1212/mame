// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Smf;
using zanac.MAmidiMEmo.ComponentModel;

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
            if (periodicAction != null)
                InstrumentManager.UnsetPeriodicCallback(periodicAction);

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
            ParentManager.AddKeyOnSound(this);

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

            SoundDriverEnabled = Timbre.SDS.Enable;
            if (SoundDriverEnabled)
            {
                adsr = new ADSR();
                adsr.SetAttackRate(Math.Pow(10d * (127d - Timbre.SDS.AR) / 127d, 2));
                adsr.SetDecayRate(Math.Pow(10d * (Timbre.SDS.DR / 127d), 2));
                adsr.SetReleaseRate(Math.Pow(60d * (Timbre.SDS.RR / 127d), 2));
                adsr.SetSustainLevel((127d - Timbre.SDS.SL) / 127d);
                adsr?.Gate(true);
            }
        }

        /// <summary>
        ///キーオフ
        /// </summary>
        public virtual void KeyOff()
        {
            IsKeyOff = true;
            adsr?.Gate(false);

            if (CurrentDriverEnvelopeState == EnvelopeState.Idle)
                SoundOff();
        }

        /// <summary>
        ///キーオフ
        /// </summary>
        public virtual void SoundOff()
        {
            IsSoundOff = true;
            adsr?.Reset();
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
            double d = d1 + ModultionTotalLevel + PortamentoDeltaNoteNumber;

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

            if (adsr != null)
                v *= adsr.GetOutputLevel();

            return v;
        }

        private Action periodicAction;

        /// <summary>
        /// 
        /// </summary>
        private void updatePeriodicAction()
        {
            if (ModulationEnabled || PortamentoEnabled || SoundDriverEnabled)
            {
                if (periodicAction == null)
                    periodicAction = new Action(OnPeriodicAction);
                InstrumentManager.SetPeriodicCallback(periodicAction);
            }
            else
            {
                InstrumentManager.UnsetPeriodicCallback(periodicAction);
                periodicAction = null;
            }
        }

        /// <summary>
        /// モジュレーション値を更新する
        /// 10msごとに呼ばれる
        /// </summary>
        public virtual void OnPeriodicAction()
        {
            if (ModulationEnabled)
            {
                double radian = 2 * Math.PI * (modulationStep / InstrumentManager.TIMER_HZ);

                double mdepth = 0;
                if (ParentModule.ModulationDepthes[NoteOnEvent.Channel] > 64)
                {
                    if (modulationStartCounter < 10d * InstrumentManager.TIMER_HZ)
                        modulationStartCounter += 1.0;

                    if (modulationStartCounter > ParentModule.GetModulationDelaySec(NoteOnEvent.Channel) * InstrumentManager.TIMER_HZ)
                        mdepth = (double)ParentModule.ModulationDepthes[NoteOnEvent.Channel] / 127d;
                }
                //急激な変化を抑制
                var mv = ((double)ParentModule.Modulations[NoteOnEvent.Channel] / 127d) + mdepth;
                if (mv != modultionLevel)
                    modultionLevel += (mv - modultionLevel) / 1.25;

                double modHz = radian * ParentModule.GetModulationRateHz(NoteOnEvent.Channel);
                ModultionTotalLevel = modultionLevel * Math.Sin(modHz);
                ModultionTotalLevel *= ((double)ParentModule.ModulationDepthRangesNote[NoteOnEvent.Channel] +
                    ((double)ParentModule.ModulationDepthRangesCent[NoteOnEvent.Channel] / 127d));

                modulationStep += 1.0;
                if (modHz > 2 * Math.PI)
                    modulationStep = 0;
            }

            if (PortamentoEnabled)
            {
                if (PortamentoDeltaNoteNumber != 0)
                {
                    double delta = -portStartNoteDeltSign * 12d / Math.Pow(((double)ParentModule.PortamentoTimes[NoteOnEvent.Channel] / 2d) + 1d, 1.25);
                    PortamentoDeltaNoteNumber += delta;

                    if (portStartNoteDeltSign < 0 && PortamentoDeltaNoteNumber >= 0)
                        PortamentoDeltaNoteNumber = 0;
                    else if (portStartNoteDeltSign > 0 && PortamentoDeltaNoteNumber <= 0)
                        PortamentoDeltaNoteNumber = 0;

                    if (PortamentoDeltaNoteNumber == 0)
                        PortamentoEnabled = false;
                }
            }

            if (SoundDriverEnabled)
            {
                adsr.Process();
                if (adsr.GetCurrentEnvelopeState() == EnvelopeState.Idle)
                {
                    SoundDriverEnabled = false;
                    SoundOff();
                }
            }


            if (ModulationEnabled || PortamentoEnabled)
                UpdatePitch();

            if (SoundDriverEnabled)
                UpdateVolume();
        }

        /// <summary>
        /// 
        /// </summary>
        public EnvelopeState CurrentDriverEnvelopeState
        {
            get
            {
                if (adsr != null)
                {
                    return adsr.GetCurrentEnvelopeState();
                }
                else
                {
                    if (!IsKeyOff)
                        return EnvelopeState.Sustain;
                    else
                        return EnvelopeState.Idle;
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
        public double ModultionTotalLevel
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
                        updatePeriodicAction();
                    }
                    else
                    {
                        if (ParentModule.ModulationDepthes[NoteOnEvent.Channel] > 64)
                            return;

                        f_modulationEnabled = value;
                        updatePeriodicAction();
                    }
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
                    updatePeriodicAction();
                }
            }
        }

        private bool f_soundDriverEnabled;

        /// <summary>
        /// サウンドドライバの有効無効
        /// </summary>
        public bool SoundDriverEnabled
        {
            get
            {
                return f_soundDriverEnabled;
            }
            set
            {
                if (value != f_soundDriverEnabled)
                {
                    f_soundDriverEnabled = value;
                    updatePeriodicAction();
                }
            }
        }

        #endregion

        #region envelope

        private ADSR adsr;

        //https://www.earlevel.com/main/2013/06/03/envelope-generators-adsr-code/
        /// <summary>
        /// 
        /// </summary>
        private class ADSR
        {
            EnvelopeState state;
            double output;
            double attackRate;
            double decayRate;
            double releaseRate;
            double attackCoef;
            double decayCoef;
            double releaseCoef;
            double sustainLevel;
            double targetRatioA;
            double targetRatioDR;
            double attackBase;
            double decayBase;
            double releaseBase;

            public ADSR()
            {
                Reset();
                SetAttackRate(0);
                SetDecayRate(0);
                SetReleaseRate(30);
                SetSustainLevel(1.0);
                SetTargetRatioA(0.3);
                SetTargetRatioDR(0.0001);
            }

            /// <summary>
            /// set attack rate
            /// </summary>
            /// <param name="rate">[sec]</param>
            public void SetAttackRate(double rate)
            {
                attackRate = rate * InstrumentManager.TIMER_HZ;
                attackCoef = calcCoef(rate, targetRatioA);
                attackBase = (1.0 + targetRatioA) * (1.0 - attackCoef);
            }

            /// <summary>
            /// set decay rate
            /// </summary>
            /// <param name="rate">[sec]</param>
            public void SetDecayRate(double rate)
            {
                decayRate = rate * InstrumentManager.TIMER_HZ;
                decayCoef = calcCoef(rate, targetRatioDR);
                decayBase = (sustainLevel - targetRatioDR) * (1.0 - decayCoef);
            }

            /// <summary>
            /// set release rate
            /// </summary>
            /// <param name="rate">[sec]</param>
            public void SetReleaseRate(double rate)
            {
                releaseRate = rate * InstrumentManager.TIMER_HZ;
                releaseCoef = calcCoef(rate, targetRatioDR);
                releaseBase = -targetRatioDR * (1.0 - releaseCoef);
            }

            private double calcCoef(double rate, double targetRatio)
            {
                return (rate <= 0) ? 0.0 : Math.Exp(-Math.Log((1.0 + targetRatio) / targetRatio) / rate);
            }

            /// <summary>
            /// set sustain level
            /// </summary>
            /// <param name="level"></param>
            public void SetSustainLevel(double level)
            {
                sustainLevel = level;
                decayBase = (sustainLevel - targetRatioDR) * (1.0 - decayCoef);
            }

            /// <summary>
            /// Adjust the curves of the Attack, or Decay and Release segments,
            /// from the initial default values (small number such as 0.0001 to 0.01 for mostly-exponential,
            /// large numbers like 100 for virtually linear):
            /// </summary>
            /// <param name="targetRatio">0.0001 to 0.01</param>
            public void SetTargetRatioA(double targetRatio)
            {
                if (targetRatio < 0.000000001)
                    targetRatio = 0.000000001;  // -180 dB
                targetRatioA = targetRatio;
                attackCoef = calcCoef(attackRate, targetRatioA);
                attackBase = (1.0 + targetRatioA) * (1.0 - attackCoef);
            }

            /// <summary>
            /// Adjust the curves of the Attack, or Decay and Release segments,
            /// from the initial default values (small number such as 0.0001 to 0.01 for mostly-exponential,
            /// large numbers like 100 for virtually linear):
            /// </summary>
            /// <param name="targetRatio">0.0001 to 0.01</param>
            public void SetTargetRatioDR(double targetRatio)
            {
                if (targetRatio < 0.000000001)
                    targetRatio = 0.000000001;  // -180 dB
                targetRatioDR = targetRatio;
                decayCoef = calcCoef(decayRate, targetRatioDR);
                releaseCoef = calcCoef(releaseRate, targetRatioDR);
                decayBase = (sustainLevel - targetRatioDR) * (1.0 - decayCoef);
                releaseBase = -targetRatioDR * (1.0 - releaseCoef);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public double Process()
            {
                switch (state)
                {
                    case EnvelopeState.Idle:
                        break;
                    case EnvelopeState.Attack:
                        output = attackBase + output * attackCoef;
                        if (output >= 1.0)
                        {
                            output = 1.0;
                            state = EnvelopeState.Decay;
                        }
                        break;
                    case EnvelopeState.Decay:
                        output = decayBase + output * decayCoef;
                        if (output <= sustainLevel)
                        {
                            output = sustainLevel;
                            state = EnvelopeState.Sustain;
                        }
                        break;
                    case EnvelopeState.Sustain:
                        break;
                    case EnvelopeState.Release:
                        output = releaseBase + output * releaseCoef;
                        if (output <= 0.0)
                        {
                            output = 0.0;
                            state = EnvelopeState.Idle;
                        }
                        break;
                }
                return output;
            }


            public void Gate(bool keyOn)
            {
                if (keyOn)
                    state = EnvelopeState.Attack;
                else if (state != EnvelopeState.Idle)
                    state = EnvelopeState.Release;
            }

            public EnvelopeState GetCurrentEnvelopeState()
            {
                return state;
            }

            public void Reset()
            {
                state = EnvelopeState.Idle;
                output = 0.0;
            }

            public double GetOutputLevel()
            {
                return output;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public enum EnvelopeState
        {
            Idle = 0,
            Attack,
            Decay,
            Sustain,
            Release
        };

        #endregion


    }
}
