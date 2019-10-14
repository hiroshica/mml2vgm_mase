﻿using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mml2vgmIDE.Driver.ZGM.ZgmChip
{
    public class MidiGM:ZgmChip
    {
        public MidiGM(ChipRegister chipRegister, Setting setting, outDatum[] vgmBuf)
        {
            this.chipRegister = chipRegister;
            this.setting = setting;
            this.vgmBuf = vgmBuf;

            Use = true;
            Device = EnmDevice.MIDIGM;
            name = "GeneralMIDI";
            Model = EnmModel.None;
            Number = 0;
            Hosei = 0;
        }

        public override void Setup(int chipIndex, ref uint dataPos, ref Dictionary<int, Driver.ZGM.zgm.RefAction<outDatum, uint>> cmdTable)
        {
            base.Setup(chipIndex, ref dataPos, ref cmdTable);

            if (cmdTable.ContainsKey(defineInfo.commandNo)) cmdTable.Remove(defineInfo.commandNo);
            cmdTable.Add(defineInfo.commandNo, SendPort);

        }

        private void SendPort(outDatum od, ref uint vgmAdr)
        {
            int len = (int)(vgmBuf[vgmAdr + 1].val);
            byte[] dat = new byte[len];
            for (int i = 0; i < len; i++) dat[i] = (byte)(vgmBuf[vgmAdr + 2 + i].val);
            chipRegister.MIDISetRegister(od, Audio.DriverSeqCounter, Index, 0, dat);
            vgmAdr += (uint)(len + 2);
        }

    }
}
