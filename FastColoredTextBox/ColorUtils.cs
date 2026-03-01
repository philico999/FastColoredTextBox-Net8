using System;
using System.Drawing;
using System.Windows.Forms;

namespace FastColoredTextBoxNS
{

    internal static class ColorUtils
    {
        public static void ApplyStyles(Form frm, Control parent)
        {
            frm.ForeColor = parent.ForeColor;
            frm.BackColor = parent.BackColor;

            foreach (Control ctl in frm.Controls)
            {
                if (ctl is Button bt)
                {
                    bt.ForeColor = Color.White;
                    bt.BackColor = Color.Gray;
                }
            }
        }
   }
}