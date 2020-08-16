﻿using Core;
using musicDriverInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mml2vgmIDE
{
    public class pmdM : baseDriver
    {
        public uint YM2608ClockValue { get; internal set; } = 7987200;
        private musicDriverInterface.MmlDatum[] mBuf = null;
        private PMDManager pm = null;
        bool initPhase = true;
        List<SoundManager.PackData> pd = new List<SoundManager.PackData>();
        long count = 0;
        private SoundManager.Chip chipYM2608;
        private string filename = "";

        public override GD3 getGD3Info(byte[] buf, uint vgmGd3)
        {
            return null;
        }

        public override bool init(outDatum[] vgmBuf, ChipRegister chipRegister, EnmChip[] useChip, uint latency, uint waitTime, long jumpPointClock)
        {
            return true;
        }

        public override void oneFrameProc()
        {
            if (initPhase)
            {
                initPhase = false;
                chipRegister.YM2608SetRegister(null, count, chipYM2608, pd.ToArray());
                return;
            }

            pm.Rendering();
            count++;
        }

        public bool init(MmlDatum[] mBuf, string mWorkPath, PMDManager pmdManager, ChipRegister chipRegister, EnmChip[] enmChips, uint v1, uint v2, string mFileName)
        {
            if (pmdManager == null) return false;

            this.vgmBuf = null;
            this.mBuf = mBuf;
            this.chipRegister = chipRegister;
            this.useChip = useChip;
            this.latency = latency;
            this.waitTime = waitTime;
            this.pm = pmdManager;
            chipYM2608 = chipRegister.YM2608[0];
            filename = mFileName;

            Counter = 0;
            TotalCounter = 0;
            LoopCounter = 0;
            vgmCurLoop = 0;
            Stopped = false;
            vgmFrameCounter = -latency - waitTime;
            vgmSpeed = 1;
            vgmSpeedCounter = 0;

            initPhase = true;
            pd = new List<SoundManager.PackData>();

            //Driverの初期化
            pm.InitDriver(
                System.IO.Path.Combine(mWorkPath, "dummy")
                , OPNAInitialWrite
                , OPNAWaitSend
                , PPZ8Write
                , PPSDRVWrite
                , false
                , mBuf
                , true
                , false
                );

            pm.StartRendering((int)Common.SampleRate, (int)YM2608ClockValue);
            pm.MSTART(0);

            return true;
        }

        private int PPSDRVWrite(ChipDatum arg)
        {
            if (arg == null) return 0;

            outDatum od = null;
            if (arg.port == 0x05)
            {
                //chipRegister.PPSDRVLoad(od, count, 0, (byte[])arg.addtionalData);
            }
            else
            {
                //chipRegister.PPSDRVWrite(od, count, 0, arg.port, arg.address, arg.data);
            }

            return 0;
        }

        private int PPZ8Write(ChipDatum arg)
        {
            if (arg == null) return 0;

            outDatum od = null;
            if (arg.port == 0x03)
            {
                //chipRegister.PPZ8LoadPcm(od, count, 0, (byte)arg.address, (byte)arg.data, (byte[])arg.addtionalData);
            }
            else
            {
                //chipRegister.PPZ8Write(od, count, 0, arg.port, arg.address, arg.data);
            }

            return 0;
        }

        private void OPNAWaitSend(long arg1, int arg2)
        {
            return;
        }

        private void OPNAWrite(ChipDatum dat)
        {
            //Log.WriteLine(LogLevel.TRACE, string.Format("FM P{2} Out:Adr[{0:x02}] val[{1:x02}]", (int)dat.address, (int)dat.data, dat.port));
            //Console.WriteLine("FM P{2} Out:Adr[{0:x02}] val[{1:x02}]", (int)dat.address, (int)dat.data, dat.port);
            outDatum od = null;
            if (dat.addtionalData != null)
            {
                if (dat.addtionalData is MmlDatum)
                {
                    MmlDatum md = (MmlDatum)dat.addtionalData;
                    if (md.linePos != null) md.linePos.srcMMLID = filename;
                    od = new outDatum(md.type, md.args, md.linePos, (byte)md.dat);
                }

            }

            //if (od != null && od.linePos != null)
            //{
            //Console.WriteLine("{0}", od.linePos.col);
            //}

            //chipRegister.YM2608SetRegister(od, (long)dat.time, 0, dat.port, dat.address, dat.data);
            chipRegister.YM2608SetRegister(od, count, 0, dat.port, dat.address, dat.data);
        }


        private void OPNAInitialWrite(musicDriverInterface.ChipDatum dat)
        {
            if (!initPhase)
            {
                OPNAWrite(dat);
                return;
            }

            SoundManager.PackData p = new SoundManager.PackData(null, null, EnmDataType.Block, dat.port * 0x100 + dat.address, dat.data, null);
            pd.Add(p);
        }
    }
}