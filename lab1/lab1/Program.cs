using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace lab1
{
    class Program
    {

        static FileStream wavOut;
        static short [] mixaj;

        static void Main(string[] args)
        {

            wavOut = new FileStream("out.wav", FileMode.Create);

            FileStream file1 = new FileStream("wave1_44KHzMono.wav", FileMode.Open);
            byte[] data1;

            FileStream file2 = new FileStream("wave2_44KHzMono.wav", FileMode.Open);
            byte[] data2;

            file1.Seek(0, SeekOrigin.End);
            data1 = new byte[file1.Position];

            file2.Seek(0, SeekOrigin.End);
            data2 = new byte[file2.Position];

            //citim in array fisierele

            file1.Seek(0, SeekOrigin.Begin);
            file2.Seek(0, SeekOrigin.Begin);
            file1.Read(data1, 0, data1.Length);
            file2.Read(data2, 0, data2.Length);

            file1.Close();
            file2.Close();

            wavOut.Write(data1, 0, 44);

            short[] samples1 = new short[(data1.Length - 44) / 2];
            short[] samples2 = new short[(data2.Length - 44) / 2];

            for (int i = 0; i < samples1.Length; i++)
            {
                samples1[i] = BitConverter.ToInt16(data1, 44 + i * 2);
            }

            for (int i = 0; i < samples2.Length; i++)
            {
                samples2[i] = BitConverter.ToInt16(data2, 44 + i * 2);
            }

            mixaj = MixajStreams(samples1, samples2, 0.5);

            wavOut.Write(Short2ByteArray(mixaj), 0, mixaj.Length);
            wavOut.Close();


            //testing wav file class

            WAVFile testWav = new WAVFile();
            testWav.Open("wave1_44KHzMono.wav");
            testWav.UpSample(4);
            testWav.PrintWAVInfo();
        }

        public static byte[] Short2ByteArray(short[] sArray)
        {
            byte[] rez = new byte[sArray.Length * 2];
            int cnt = 0;
            foreach (short sword in sArray ){
                Int16 sample = (Int16)sword;
                byte lo = Convert.ToByte(sample & 0xff);
                byte hi = Convert.ToByte((sample >> 8) & 0xff);

                rez[cnt] = lo;
                rez[cnt + 1] = hi;
                cnt += 2;
            }
            return rez;
        } 

        public static short[] MixajStreams(short[] wavstream1, short[] wavstream2, double factorMix)
        {
            short[] mix;

            if (wavstream1.Length > wavstream2.Length)
            {
                mix = new short[wavstream1.Length];
                int i, j;

                for (i = 0; i < wavstream2.Length; i++)
                {
                    mix[i] = (short)(wavstream1[i] * factorMix + wavstream2[i] * (1 - factorMix));
                }

                for (j = i; j < wavstream1.Length; j++)
                {
                    mix[j] = wavstream1[j];
                }
            }else{
                mix = new short[wavstream2.Length];
                int i, j;

                for (i = 0; i < wavstream1.Length; i++)
                {
                    mix[i] = (short)(wavstream1[i] * factorMix + wavstream2[i] * (1 - factorMix));
                }

                for (j = i; j < wavstream2.Length; j++)
                {
                    mix[j] = wavstream2[j];
                }
            }
            return mix;
        }


    }
}
