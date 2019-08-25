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

        public bool IsDisposed
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
        protected SoundBase(InstrumentBase parentModule, SoundManagerBase manager, NoteOnEvent noteOnEvent, int slot)
        {
            NoteOnEvent = noteOnEvent;
            Slot = slot;
            this.ParentModule = parentModule;
            this.ParentManager = manager;
        }

        /// <summary>
        /// サウンドオン
        /// </summary>
        public virtual void On()
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
        }

        /// <summary>
        /// サウンドオフ
        /// </summary>
        public virtual void Off()
        {
        }

        /// <summary>
        /// 0.0 - 100.0
        /// </summary>
        public double modulationStep;

        /// <summary>
        /// </summary>
        public double modulationStart;

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

        private bool f_modulationEnabled;

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
                    if (modulationStart < 10d * InstrumentManager.TIMER_HZ)
                        modulationStart += 1.0;

                    if (modulationStart > ParentModule.GetModulationDelaySec(NoteOnEvent.Channel) * InstrumentManager.TIMER_HZ)
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
                    double delta = -portStartNoteDeltSign * 12d / Math.Pow(((double)ParentModule.PortamentoTimes[NoteOnEvent.Channel]/2d) + 1d, 1.25);
                    PortamentoDeltaNoteNumber += delta;

                    if (portStartNoteDeltSign < 0 && PortamentoDeltaNoteNumber >= 0)
                        PortamentoDeltaNoteNumber = 0;
                    else if (portStartNoteDeltSign > 0 && PortamentoDeltaNoteNumber <= 0)
                        PortamentoDeltaNoteNumber = 0;

                    if (PortamentoDeltaNoteNumber == 0)
                        PortamentoEnabled = false;
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
        /// 
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

        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
            if (periodicAction != null)
                InstrumentManager.UnsetPeriodicCallback(periodicAction);

            if (!IsDisposed)
                Off();
        }

    }
}
