using System;
using System.Collections.Generic;
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

        public string startMixing()
        {
            string path;
            if (!wav1.FormatMatches(wav2))
            {
               return "Files do not match.";
            }

            //check bitrate
            if (!checkBitRate())
            {
                return "Please select 16-bit files.";
            }

            //check Tracks
            if (!checkAndUpSampleTracks())
            {
                return "Please check sample rates.";
            }
            
            //begin mixing
            mix(0.5);
 
            //write to .wav file 
            path = createFile("output.wav");

            return path;
        }

        private bool checkBitRate()
        {
            if (wav1.SampleRateHz == 8 || wav1.SampleRateHz == 8)
                return false;

            return true;
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


        private string createFile(String filename)
        {
            byte [] outputHeader;
            string path;
            mixedWAV = new WAVFile();

            if (wav1.SubChunk2Size > wav2.SubChunk2Size)
            {
                outputHeader = wav1.WAVHeader;
            }
            else
            {
                outputHeader = wav2.WAVHeader;
            }

            path = mixedWAV.OutputWAVFile(outputHeader, mixedWAVSamples, filename);

            return path;
        }
    }
}
