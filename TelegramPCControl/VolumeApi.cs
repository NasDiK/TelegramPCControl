using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TelegramPCControl
{
    public static class WinAPI
    {
        [DllImport("winmm.dll")]
        static extern int waveOutGetVolume(IntPtr hwo, out uint pdwVolume);

        [DllImport("winmm.dll")]
        static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);
        public static int Volume
        {
            get
            {

                uint CurrVol = 0;
                waveOutGetVolume(IntPtr.Zero, out CurrVol);
                ushort CalcVol = (ushort)(CurrVol & 0x0000ffff);
                return CalcVol / (ushort.MaxValue / 100);
            }
            set
            {
                if (value < 0 || value > 100)
                    throw new ArgumentOutOfRangeException();
                int NewVolume = ((ushort.MaxValue / 100) * value);
                uint NewVolumeAllChannels = (((uint)NewVolume & 0x0000ffff) | ((uint)NewVolume << 16));
                Console.WriteLine(waveOutSetVolume(IntPtr.Zero, NewVolumeAllChannels));
            }
        }

        /// <summary>
        /// Returns current volume
        /// </summary>
        /// <returns>
        /// item1 is Left channel volume. Item2 is Right channel volumes</returns>
        public static (int,int) ShowVolume()
        {
            uint volume;
            waveOutGetVolume(IntPtr.Zero, out volume);
            int left = (int)(volume & 0xFFFF);
            int right = (int)((volume >> 16) & 0xFFFF);
            return (left,right);
        }
    }
}
