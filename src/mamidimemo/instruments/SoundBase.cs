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
                KeyOff();

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

            SoundDriverEnabled = Timbre.SDP.Enable;
        }

        /// <summary>
        ///キーオフ
        /// </summary>
        public virtual void KeyOff()
        {
            IsKeyOff = true;
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

        private Action periodicAction;

        /// <summary>
        /// 
        /// </summary>
        private void updatePeriodicAction()
        {
            if (ModulationEnabled || PortamentoEnabled)
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

            }

            if (ModulationEnabled || PortamentoEnabled)
                UpdatePitch();

            if (SoundDriverEnabled)
                UpdateVolume();
        }

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

    }
}
