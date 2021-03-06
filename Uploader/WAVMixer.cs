﻿using System;
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

        /// <summary>
        /// Constructor.
        /// </summary>
        public WAVMixer()
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pathToFile1">Path to first file</param>
        /// <param name="pathToFile2">Path to second file</param>
        public WAVMixer(String pathToFile1, String pathToFile2)
        {
            wav1 = new WAVFile(pathToFile1);
            wav2 = new WAVFile(pathToFile2);
        }

        /// <summary>
        /// Begins mixing the files
        /// </summary>
        /// <returns>
        /// Returns the errorCode and message/pathToFile.
        /// </returns>
        public string[] startMixing()
        {
            string[] ret = new string[2];            
            string path;

            //check format match
            if (!wav1.FormatMatches(wav2))
            {
                ret[0] ="1";
                ret[1] = "Files do not match.";
                return ret;
            }

            //check bitrate
            if (!checkBitRate())
            {
                ret[0] ="1";
                ret[1] = "Please select 16-bit files.";
                return ret;
            }

            //check Tracks
            if (!checkAndUpSampleTracks())
            {
                ret[0] ="1";
                ret[1] = "Files do not have proportionate sample rates.";
                return ret;
            }
            
            //begin mixing
            mix(0.5);
 
            //write to .wav file 
            path = createFile("output.wav");

            ret[0] ="0";
            ret[1] = path;
            return ret;
        }

        /// <summary>
        /// Checks bitrate compatibility between the two files.
        /// </summary>
        private bool checkBitRate()
        {
            if (wav1.SampleRateHz == 8 || wav1.SampleRateHz == 8)
                return false;

            return true;
        }

        /// <summary>
        /// Checks the sample rate and does the up-sampling if necessary.
        /// </summary>
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


        /// <summary>
        /// Mixes the two files according to the mix factor.
        /// </summary>
        /// <param name="factorMix">The mix-factor variable</param>
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

        /// <summary>
        /// Outputs the file.
        /// </summary>
        /// <param name="filename">Output file's name</param>
        /// <returns>The path to the mixed file</returns>
        private string createFile(String filename)
        {
            byte [] outputHeader;
            string path;
            mixedWAV = new WAVFile();

            
            if (wav1.SubChunk2Size <= wav2.SubChunk2Size)
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
