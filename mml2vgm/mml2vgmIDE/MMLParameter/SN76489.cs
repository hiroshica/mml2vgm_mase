﻿using Core;
using System;
using System.Linq;

namespace mml2vgmIDE.MMLParameter
{
    public class SN76489 : Instrument
    {
        public SN76489() : base(4)
        {
        }

        public override string Name => "SN76489";

        public override void SetParameter(outDatum od, int cc)
        {
            switch (od.type)
            {
                case enmMMLType.Instrument:
                    envelope[od.linePos.ch] = (int)od.args[1];
                    break;
                case enmMMLType.Octave:
                    octave[od.linePos.ch] = (int)od.args[0];
                    break;
                case enmMMLType.OctaveDown:
                    octave[od.linePos.ch]--;
                    break;
                case enmMMLType.OctaveUp:
                    octave[od.linePos.ch]++;
                    break;
                case enmMMLType.Note:
                    if (od.args != null && od.args.Count > 0)
                    {
                        Core.Note nt = (Core.Note)od.args[0];
                        int shift = nt.shift;
                        string f = Math.Sign(shift) >= 0 ? string.Concat(Enumerable.Repeat("+", shift)) : string.Concat(Enumerable.Repeat("-", -shift));
                        notecmd[od.linePos.ch] = string.Format("o{0}{1}{2}", octave[od.linePos.ch], nt.cmd, f);
                        length[od.linePos.ch] = string.Format("{0:0.##}(#{1:d})", 1.0 * cc / nt.length, nt.length);
                    }
                    break;
                case enmMMLType.Rest:
                    Core.Rest rs = (Core.Rest)od.args[0];
                    notecmd[od.linePos.ch] = "r";
                    length[od.linePos.ch] = string.Format("{0:0.##}(#{1:d})", 1.0 * cc / rs.length, rs.length);
                    break;
                case enmMMLType.Volume:
                    if (od.linePos != null)
                        vol[od.linePos.ch] = (int)od.args[0];
                    break;
                case enmMMLType.Pan:
                    int n = (int)od.args[0];
                    pan[od.linePos.ch] = n == 0 ? "-" : (n == 1 ? "R" : (n == 2 ? "L" : "C"));
                    break;

            }
        }

    }
}
