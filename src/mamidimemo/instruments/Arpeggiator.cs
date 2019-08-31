// copyright-holders:K.Ito
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Instruments
{
    //https://cdn.korg.com/jp/support/download/files/6e1e2515cd8d82ff484b6ec38bc91906.pdf?response-content-disposition=inline%3Bfilename*%3DUTF-8''MS2000_OM_J3.pdf&response-content-type=application%2Fpdf%3B
    //https://github.com/wesen/mididuino/blob/develop/hardware/libraries/MidiTools/Arpeggiator.cpp

    public class Arpeggiator
    {
        private static Random random = new Random();

        private List<NoteOnEvent> orderedNotes = new List<NoteOnEvent>();

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<NoteOnEvent> ArpeggioNotes
        {
            get
            {
                return orderedNotes.AsEnumerable();
            }
        }

        private List<NoteOnEvent> arpNotes;

        private int arpOctaveCount;

        private int arpStep;


        /// <summary>
        /// 
        /// </summary>
        public ArpStepStyle StepStyle
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public RetriggerType RetriggerType
        {
            get;
            set;
        }

        public int f_Range = 1;

        /// <summary>
        /// 
        /// </summary>
        public int Range
        {
            get
            {
                return f_Range;
            }
            set
            {
                if (f_Range != value && value >= 1 && value <= 4)
                {
                    f_Range = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double GateCounter
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public double Counter
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public double CounterStep
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public int ArpNotesCount
        {
            get
            {
                return arpNotes.Count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public NoteOnEvent LastNote
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Arpeggiator()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearNotes()
        {
            orderedNotes.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ResetNextNoteOn
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="noteOn"></param>
        public void AddNote(NoteOnEvent noteOn)
        {
            for (int i = 0; i < orderedNotes.Count; i++)
            {
                if (orderedNotes[i].NoteNumber == noteOn.NoteNumber)
                {
                    orderedNotes[i].Velocity = noteOn.Velocity;
                    return;
                }
            }

            orderedNotes.Add(noteOn);

            calculateArp();

            if (RetriggerType == RetriggerType.Note)// || orderedNotes.Count == 1)
                Retrigger();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="noteOff"></param>
        public bool RemoveNote(NoteOffEvent noteOff)
        {
            bool result = true;
            for (int i = 0; i < orderedNotes.Count; i++)
            {
                if (orderedNotes[i].NoteNumber == noteOff.NoteNumber)
                {
                    orderedNotes.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            if (result)
                calculateArp();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public NoteOnEvent Next()
        {
            if (arpStep >= arpNotes.Count)
            {
                arpStep = 0;
                arpOctaveCount++;
                if (arpOctaveCount >= Range)
                    arpOctaveCount = 0;
            }
            NoteOnEvent an = null;
            if (StepStyle != ArpStepStyle.Random)
                an = arpNotes[arpStep];
            else
                an = arpNotes[random.Next(arpNotes.Count)];
            arpStep++;

            an = new NoteOnEvent(an.NoteNumber, an.Velocity) { Channel = an.Channel };

            if (arpOctaveCount > 0)
            {
                int oc = arpOctaveCount;
                if (StepStyle == ArpStepStyle.Random)
                    oc = random.Next(Range - 1);
                oc *= 12;
                if (an.NoteNumber + oc < 128)
                    an.NoteNumber += (SevenBitNumber)oc;
            }
            LastNote = an;
            return an;
        }


        public void Retrigger()
        {
            arpStep = 0;
            arpOctaveCount = 0;
            if (StepStyle == ArpStepStyle.RandomOnce)
                calculateArp();
        }

        private void calculateArp()
        {
            arpStep = 0;
            switch (StepStyle)
            {
                case ArpStepStyle.Up:
                    sortUp();
                    arpNotes = new List<NoteOnEvent>(orderedNotes);
                    break;

                case ArpStepStyle.Down:
                    sortDown();
                    arpNotes = new List<NoteOnEvent>(orderedNotes);
                    break;

                case ArpStepStyle.Order:
                    arpNotes = new List<NoteOnEvent>(orderedNotes);
                    break;

                case ArpStepStyle.UpDown:
                    {
                        var num = orderedNotes.Count;
                        if (num > 1)
                        {
                            sortUp();
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                            for (int i = 0; i < num - 2; i++)
                                arpNotes.Add(orderedNotes[num - 2 - i]);
                        }
                        else
                        {
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                    }
                    break;

                case ArpStepStyle.DownUp:
                    {
                        var num = orderedNotes.Count;
                        if (num > 1)
                        {
                            sortDown();
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                            for (int i = 0; i < num - 2; i++)
                                arpNotes.Add(orderedNotes[num - 2 - i]);
                        }
                        else
                        {
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                    }
                    break;

                case ArpStepStyle.UpAndDown:
                    {
                        var num = orderedNotes.Count;
                        if (num > 1)
                        {
                            sortUp();
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                            for (int i = 0; i < num; i++)
                                arpNotes.Add(orderedNotes[num - 1 - i]);
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                        else
                        {
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                    }
                    break;

                case ArpStepStyle.DownAndUp:
                    {
                        var num = orderedNotes.Count;
                        if (num > 1)
                        {
                            sortDown();
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                            for (int i = 0; i < num; i++)
                                arpNotes.Add(orderedNotes[num - 1 - i]);
                        }
                        else
                        {
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                    }
                    break;

                case ArpStepStyle.Converge:
                    {
                        var num = orderedNotes.Count;
                        sortUp();
                        if (num > 1)
                        {
                            arpNotes.Clear();
                            for (int i = 0; i < (num >> 1); i++)
                            {
                                arpNotes.Add(orderedNotes[i]);
                                arpNotes.Add(orderedNotes[num - i - 1]);
                            }
                            if ((byte)(num & 1) == 1)
                            {
                                arpNotes.Add(orderedNotes[(num >> 1)]);
                            }
                        }
                        else
                        {
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                    }
                    break;

                case ArpStepStyle.Diverge:
                    {
                        var num = orderedNotes.Count;
                        sortUp();
                        if (num > 1)
                        {
                            if ((byte)(num & 1) == 1)
                            {
                                arpNotes.Add(orderedNotes[num >> 1]);
                            }
                            for (int i = (num >> 1) - 1; i >= 0; i--)
                            {
                                arpNotes.Add(orderedNotes[i]);
                                arpNotes.Add(orderedNotes[num - i - 1]);
                            }
                        }
                        else
                        {
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                    }
                    break;

                case ArpStepStyle.ConAndDiverge:
                    {
                        var num = orderedNotes.Count;
                        sortUp();
                        if (num > 1)
                        {
                            for (int i = 0; i < (num >> 1); i++)
                            {
                                arpNotes.Add(orderedNotes[i]);
                                arpNotes.Add(orderedNotes[num - i - 1]);
                            }
                            if ((byte)(num & 1) == 1)
                            {
                                arpNotes.Add(orderedNotes[(num >> 1)]);
                            }
                            for (int i = (num >> 1) - 1; i >= 0; i--)
                            {
                                arpNotes.Add(orderedNotes[i]);
                                arpNotes.Add(orderedNotes[num - i - 1]);
                            }
                        }
                        else
                        {
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                    }
                    break;

                case ArpStepStyle.PinkyUp:
                    {
                        var num = orderedNotes.Count;
                        sortUp();
                        if (num > 1)
                        {
                            for (int i = 0; i < num - 1; i++)
                            {
                                arpNotes.Add(orderedNotes[num - 1]);
                                arpNotes.Add(orderedNotes[i]);
                            }
                        }
                        else
                        {
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                    }
                    break;

                case ArpStepStyle.PinkyUpDown:
                    {
                        var num = orderedNotes.Count;
                        sortUp();
                        if (num > 1)
                        {
                            for (int i = 0; i < num - 1; i++)
                            {
                                arpNotes.Add(orderedNotes[num - 1]);
                                arpNotes.Add(orderedNotes[i]);
                            }
                            for (int i = 0; i < num - 1; i++)
                            {
                                arpNotes.Add(orderedNotes[num - 1]);
                                arpNotes.Add(orderedNotes[num - i - 2]);
                            }
                        }
                        else
                        {
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                    }
                    break;

                case ArpStepStyle.ThumbUp:
                    {
                        var num = orderedNotes.Count;
                        sortUp();
                        if (num > 1)
                        {
                            for (int i = 1; i < num; i++)
                            {
                                arpNotes.Add(orderedNotes[0]);
                                arpNotes.Add(orderedNotes[i]);
                            }
                        }
                        else
                        {
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                    }
                    break;

                case ArpStepStyle.ThumbUpDown:
                    {
                        var num = orderedNotes.Count;
                        sortUp();
                        if (num > 1)
                        {
                            for (int i = 1; i < num; i++)
                            {
                                arpNotes.Add(orderedNotes[0]);
                                arpNotes.Add(orderedNotes[i]);
                            }
                            for (int i = 0; i < num - 1; i++)
                            {
                                arpNotes.Add(orderedNotes[0]);
                                arpNotes.Add(orderedNotes[num - i - 1]);
                            }
                        }
                        else
                        {
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                    }
                    break;

                case ArpStepStyle.Random:
                    arpNotes = new List<NoteOnEvent>(orderedNotes);
                    break;

                case ArpStepStyle.RandomOnce:
                    {
                        var num = orderedNotes.Count;
                        arpNotes = new List<NoteOnEvent>(orderedNotes);
                        for (int i = 0; i < num; i++)
                        {
                            byte rand = (byte)(random.Next() % num);
                            var tmp = arpNotes[i];
                            arpNotes[i] = arpNotes[rand];
                            arpNotes[rand] = tmp;
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        private void sortUp()
        {
            orderedNotes.Sort((a, b) => a.NoteNumber - b.NoteNumber);
        }

        private void sortDown()
        {
            orderedNotes.Sort((a, b) => b.NoteNumber - a.NoteNumber);
        }


    }


    public enum RetriggerType
    {
        Off = 0,
        Note,
        Beat,
        //Cnt
    }

    public enum ArpStepStyle
    {
        Up = 0,
        Down,
        UpDown,
        DownUp,
        UpAndDown,
        DownAndUp,
        Converge,
        Diverge,
        ConAndDiverge,
        PinkyUp,
        PinkyUpDown,
        ThumbUp,
        ThumbUpDown,
        Random,
        RandomOnce,
        Order,
        //Cnt
    }

    public enum ArpType
    {
        Dynamic,
        Static
    }

    public enum ArpMethod
    {
        NoteOn,
        Pitch
    }

    public enum ArpResolution
    {
        QuarterNote = 1,
        QuarterTriplet = 3,
        EighthNote = 2,
        EighthTriplet = 6,
        SixteenthNote = 4,
        SixteenthTriplet = 12,
        ThirtySecondNote = 8,
    }

}
