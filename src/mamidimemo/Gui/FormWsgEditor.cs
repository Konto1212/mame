using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormWsgEditor : Form
    {

        public IWsgEditorByteCapable ByteInstance
        {
            get
            {
                return graphControl.ByteInstance;
            }
            set
            {
                graphControl.ByteInstance = value;
            }
        }

        public IWsgEditorSbyteCapable SbyteInstance
        {
            get
            {
                return graphControl.SbyteInstance;
            }
            set
            {
                graphControl.SbyteInstance = value;
            }
        }


        public byte[] ByteWsgData
        {
            get
            {
                return graphControl.ResultOfWsgData;
            }
        }

        public sbyte[] SbyteWsgData
        {
            get
            {
                int max = ((1 << graphControl.SbyteInstance.WsgBitWide) - 1) / 2;
                sbyte[] data = new sbyte[graphControl.SbyteInstance.WsgData.Length];
                for (int i = 0; i < data.Length; i++)
                    data[i] = (sbyte)(graphControl.ResultOfWsgData[i] - max);
                return data;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FormWsgEditor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        private class GraphControl : UserControl
        {

            private IWsgEditorByteCapable f_ByteInstance;

            private byte[] f_ResultOfWsgData = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

            public byte[] ResultOfWsgData
            {
                get
                {
                    return f_ResultOfWsgData;
                }
                set
                {
                    f_ResultOfWsgData = value;
                }
            }

            private int wsgLen = 16;

            private int wsgBitWide = 4;

            private int f_WsgMaxValue = 15;

            public int WsgMaxValue
            {
                get
                {
                    return f_WsgMaxValue;
                }
                set
                {
                    f_WsgMaxValue = value;
                }
            }

            public IWsgEditorByteCapable ByteInstance
            {
                get
                {
                    return f_ByteInstance;
                }
                set
                {
                    if (value == null)
                        return;

                    f_ByteInstance = value;
                    ResultOfWsgData = new byte[value.WsgData.Length];
                    Array.Copy(value.WsgData, ResultOfWsgData, value.WsgData.Length);
                    wsgLen = ResultOfWsgData.Length;
                    wsgBitWide = value.WsgBitWide;
                    WsgMaxValue = (1 << wsgBitWide) - 1;

                    updateText();
                }
            }

            private IWsgEditorSbyteCapable f_SbyteInstance;

            public IWsgEditorSbyteCapable SbyteInstance
            {
                get
                {
                    return f_SbyteInstance;
                }
                set
                {
                    if (value == null)
                        return;

                    f_SbyteInstance = value;
                    ResultOfWsgData = new byte[value.WsgData.Length];
                    wsgLen = ResultOfWsgData.Length;
                    wsgBitWide = value.WsgBitWide;
                    WsgMaxValue = (1 << wsgBitWide) - 1;
                    for (int i = 0; i < value.WsgData.Length; i++)
                        ResultOfWsgData[i] = (byte)((int)value.WsgData[i] + (WsgMaxValue / 2));

                    updateText();
                }
            }

            private void updateText()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < ResultOfWsgData.Length; i++)
                {
                    if (sb.Length != 0)
                        sb.Append(' ');
                    sb.Append(ResultOfWsgData[i].ToString((IFormatProvider)null));
                }
                try
                {
                    ((FormWsgEditor)Parent).suspendWsgDataTextChange = true;
                    ((FormWsgEditor)Parent).textBoxWsgDataText.Text = sb.ToString();
                }
                finally
                {
                    ((FormWsgEditor)Parent).suspendWsgDataTextChange = false;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public GraphControl()
            {

            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="e"></param>
            protected override void OnPaint(PaintEventArgs e)
            {
                // Call the OnPaint method of the base class.  
                base.OnPaint(e);

                Graphics g = e.Graphics;
                Size sz = this.ClientSize;
                Size dotSz = Size.Empty;
                dotSz = new Size(sz.Width / wsgLen, sz.Height / (WsgMaxValue + 1));

                //fill bg
                using (SolidBrush sb = new SolidBrush(Color.Black))
                    g.FillRectangle(sb, e.ClipRectangle);
                //draw grid
                using (Pen pen = new Pen(Color.DarkGray))
                {
                    using (Pen pen2 = new Pen(Color.White, 3))
                    {
                        for (int x = 0; x < wsgLen; x++)
                        {
                            Pen dp = x + 1 == wsgLen / 2 ? pen2 : pen;
                            g.DrawLine(dp, (x * dotSz.Width) + dotSz.Width - 1, 0, (x * dotSz.Width) + dotSz.Width - 1, sz.Height);
                        }
                        for (int y = 0; y < (WsgMaxValue + 1); y++)
                        {
                            Pen dp = y + 1 == (WsgMaxValue + 1) / 2 ? pen2 : pen;
                            g.DrawLine(dp,
                                0, sz.Height - ((y * dotSz.Height) + dotSz.Height),
                                sz.Width, sz.Height - ((y * dotSz.Height) + dotSz.Height));
                        }
                    }
                }
                //draw dot
                using (SolidBrush sb = new SolidBrush(Color.Green))
                {
                    for (int x = 0; x < wsgLen; x++)
                    {
                        g.FillRectangle(sb,
                            x * dotSz.Width,
                            sz.Height - (ResultOfWsgData[x] * dotSz.Height) - dotSz.Height + 1,
                            dotSz.Width - 1, dotSz.Height - 1);
                    }
                }
            }

            protected override void OnClientSizeChanged(EventArgs e)
            {
                base.OnClientSizeChanged(e);
                Invalidate();
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                processMouse(e);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);

                processMouse(e);
            }

            private void processMouse(MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left)
                    return;

                int len = 0;
                int bw = 0;
                if (ByteInstance != null)
                {
                    len = ByteInstance.WsgData.Length;
                    bw = ByteInstance.WsgBitWide;
                }
                else if (SbyteInstance != null)
                {
                    len = SbyteInstance.WsgData.Length;
                    bw = SbyteInstance.WsgBitWide;
                }

                Size sz = this.ClientSize;
                Point pt = e.Location;
                Size dotSz = new Size(sz.Width / len, sz.Height / (WsgMaxValue + 1));

                Point wxv = new Point(pt.X / dotSz.Width, (sz.Height - pt.Y) / dotSz.Height);

                if (0 <= wxv.X & wxv.X < len && 0 <= wxv.Y && wxv.Y <= WsgMaxValue)
                {
                    if (ResultOfWsgData[wxv.X] != (byte)wxv.Y)
                    {
                        Invalidate(new Rectangle(wxv.X * dotSz.Width, sz.Height - (ResultOfWsgData[wxv.X] * dotSz.Height) - dotSz.Height + 1,
                            dotSz.Width - 1, dotSz.Height - 1));

                        ResultOfWsgData[wxv.X] = (byte)wxv.Y;

                        Invalidate(new Rectangle(wxv.X * dotSz.Width, sz.Height - (ResultOfWsgData[wxv.X] * dotSz.Height) - dotSz.Height + 1,
                            dotSz.Width - 1, dotSz.Height - 1));

                        updateText();
                    }
                }
            }
        }

        private bool suspendWsgDataTextChange = false;

        private void textBoxWsgDataText_TextChanged(object sender, EventArgs e)
        {
            if (suspendWsgDataTextChange)
                return;

            string[] vals = textBoxWsgDataText.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<byte> vs = new List<byte>();
            foreach (var val in vals)
            {
                byte v = 0;
                if (byte.TryParse(val, out v))
                    vs.Add(v);
            }

            for (int i = 0; i < Math.Min(ByteWsgData.Length, vs.Count); i++)
                ByteWsgData[i] = vs[i] > graphControl.WsgMaxValue ? (byte)graphControl.WsgMaxValue : vs[i];

            graphControl.Invalidate();
        }
    }
}
