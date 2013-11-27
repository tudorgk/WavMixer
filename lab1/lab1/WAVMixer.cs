using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace lab1
{
    class WAVMixer
    {
        WAVFile wav1;
        WAVFile wav2;
        WAVFile mixedWAV;
        short[] mixedWAVSamples;

        public WAVMixer()
        {

        }

        public WAVMixer(String pathToFile1, String pathToFile2)
        {
            wav1 = new WAVFile(pathToFile1);
            wav2 = new WAVFile(pathToFile2);
        }

        public void startMixing()
        {
            if (!wav1.FormatMatches(wav2))
            {
                Debug.WriteLine("Files do not match");
                return;
            }

            //check Tracks
            if (!checkAndUpSampleTracks())
            {
                return;
            }
            
            //begin mixing
            mix(0.5);
 
            //write to .wav file 
            createFile("output.wav");
        }

        private bool checkAndUpSampleTracks()
        {
            bool ret = false;

            WAVFile maxSampleWAV, minSampleWAV;
            //checking sample rates
            if (wav1.SampleRateHz > wav2.SampleRateHz)
            {
                maxSampleWAV = wav1;
                minSampleWAV = wav2;
            }
            else
            {
                maxSampleWAV = wav2;
                minSampleWAV = wav1;
            }

            //Up sampling the lowest bitrate track
            if (maxSampleWAV.SampleRateHz % minSampleWAV.SampleRateHz == 0 && maxSampleWAV.SampleRateHz != minSampleWAV.SampleRateHz)
            {
                ret = true;
                minSampleWAV.UpSample(maxSampleWAV.SampleRateHz / minSampleWAV.SampleRateHz);
            }
            else if (maxSampleWAV.SampleRateHz == minSampleWAV.SampleRateHz)
            {
                ret = true;
            }
            else
            {
                ret = false;
                Debug.WriteLine("Files do not have proportionate sample rate!");
            }

            return ret;
        }

        private void mix(double factorMix)
        {
            WAVFile maxSampleWAV, minSampleWAV;

            if (wav1.WAVSamples.Length > wav2.WAVSamples.Length)
            {
                maxSampleWAV = wav1;
                minSampleWAV = wav2;
            }
            else
            {
                maxSampleWAV = wav2;
                minSampleWAV = wav1;
            }

            mixedWAVSamples = new short[maxSampleWAV.WAVSamples.Length];
            int i, j;

            for (i = 0; i < minSampleWAV.WAVSamples.Length; i++)
            {
                mixedWAVSamples[i] = (short)(maxSampleWAV.WAVSamples[i] * factorMix + minSampleWAV.WAVSamples[i] * (1 - factorMix));
            }

            for (j = i; j < maxSampleWAV.WAVSamples.Length; j++)
            {
                mixedWAVSamples[j] = maxSampleWAV.WAVSamples[j];
            }


        }


        private void createFile(String filename)
        {
            byte [] outputHeader;
            mixedWAV = new WAVFile();

            if (wav1.SubChunk2Size > wav2.SubChunk2Size)
            {
                outputHeader = wav1.WAVHeader;
            }
            else
            {
                outputHeader = wav2.WAVHeader;
            }

            mixedWAV.OutputWAVFile(outputHeader, mixedWAVSamples, filename);
        }
    }
}
