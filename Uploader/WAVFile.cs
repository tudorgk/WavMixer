using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace lab1
{
    class WAVFile
    {
        private String wavFilename;
        private FileStream wavFileStream;
        private byte[] wavContent;
        private byte[] wavHeader;
        private short[] wavSamples;

        // Audio format information
        
        private byte wavNumChannels;      // The # of channels (1 or 2)
        private int wavSampleRateHz;      // The audio sample rate (Hz)
        private int wavBytesPerSec;       // Bytes per second
        private short wavBytesPerSample;  // # of bytes per sample (1=8 bit Mono, 2=8 bit Stereo or 16 bit Mono, 4=16 bit Stereo)
        private short wavBitsPerSample;   // # of bits per sample
        private int wavSubChunk2Size;
        private int wavDataSizeBytes;     // The data size (bytes)

        //private int mDataBytesWritten;  // Used in write mode for keeping track of
        // the number of bytes written
        private int mNumSamplesRemaining; // When in read mode, this is used for keeping track of how many audio
        // samples remain.  This is updated in GetNextSample_ByteArray().

        public WAVFile()
        {
            initVariables();
        }

        public WAVFile(String pathToFile)
        {
            initVariables();
            wavFilename = pathToFile;
            
            
            Open(wavFilename);
        }

        ~WAVFile()
        {
            Close();
        }

        public void Open(String fileName)
        {
            try
            {
                wavFilename = fileName;
                if (!File.Exists(fileName))
                    Console.WriteLine("File does not exist: " + fileName);

                wavFileStream = new FileStream(fileName, FileMode.Open);
                
                //setting up the wavContent byte array
                wavFileStream.Seek(0, SeekOrigin.End);
                wavContent = new byte[wavFileStream.Position];

                //reading the wav header
                wavHeader = new byte[44];
                wavFileStream.Seek(0, SeekOrigin.Begin);
                wavFileStream.Read(wavHeader, 0, 44);


                //reading the wav file's content
                wavFileStream.Seek(0, SeekOrigin.Begin);
                wavFileStream.Read(wavContent, 0, wavContent.Length);

                //extracting the necessary information
                // # of channels (2 bytes)
                byte[] buffer = new byte[4];

                // # of channels (2 bytes)
                wavFileStream.Seek(20, SeekOrigin.Begin);
                wavFileStream.Read(buffer, 0, 4);
                wavNumChannels= (BitConverter.IsLittleEndian ? buffer[2] : buffer[3]);

                // Sample rate (4 bytes)
                wavFileStream.Read(buffer, 0, 4);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(buffer);
                wavSampleRateHz = BitConverter.ToInt32(buffer, 0);

                // Bytes per second (4 bytes)
                wavFileStream.Read(buffer, 0, 4);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(buffer);
                wavBytesPerSec = BitConverter.ToInt32(buffer, 0);

                // Bits per sample (2 bytes)
                byte [] buffer2 = new byte [2];
                wavFileStream.Seek(34,SeekOrigin.Begin);
                wavFileStream.Read(buffer2, 0, 2);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(buffer2,0,2);
                wavBitsPerSample = BitConverter.ToInt16(buffer2,0);

                //reading subchunk2size
                wavFileStream.Seek(40, SeekOrigin.Begin);
                wavFileStream.Read(buffer, 0, 4);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(buffer);
                wavSubChunk2Size= BitConverter.ToInt32(buffer, 0);

                PrintWAVInfo();

                //size of the data
                wavDataSizeBytes = wavContent.Length - 44;

                //setting up wavSamples
                wavSamples = new short[wavDataSizeBytes / 2]; //becasue short = 2 bytes :)
                for (int i = 0; i < wavSamples.Length; i++)
                {
                    wavSamples[i] = BitConverter.ToInt16(wavContent, 44 + i * 2);
                }
               
                
                //closing file stream
                Close();               
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }


        public void UpSample (int factor){
            int ptr = 0;
            short [] resultWavSamples = new short[wavSamples.Length * factor];

            for (int i = 0; i < wavSamples.Length; i++) 
                for (int j = 0; j < factor; j++) 
                    resultWavSamples[ptr++] = wavSamples[i];

            //setting new wav file data
            wavSamples = resultWavSamples;
            wavSampleRateHz *= factor;
        }

        //create a wav file from various parts
        public string OutputWAVFile(byte [] header, short[] samples, String fileName)
        {
            string path;

            path = System.Web.Hosting.HostingEnvironment.MapPath("~/TransientStorage/") + fileName;

            FileStream wavOut;

            wavOut = new FileStream(path, FileMode.Create);
            wavOut.Write(header, 0, header.Length);
            wavOut.Write(Short2ByteArray(samples),0,samples.Length * 2);
            wavOut.Close();

            return path;
        }

        public void PrintWAVInfo()
        {
            Debug.WriteLine("Number of channels = " + wavNumChannels);
            Debug.WriteLine("Sample rate = " + wavSampleRateHz);
            Debug.WriteLine("Bytes/sec = " + wavBytesPerSec);
            Debug.WriteLine("Bits/sample = " + wavBitsPerSample);
            Debug.WriteLine("SubChunk2Size = " + wavSubChunk2Size);
        }

        /// <summary>
        /// Closes the file
        /// </summary>
        public void Close()
        {
            if (wavFileStream != null)
            {
                wavFileStream.Close();
                wavFileStream.Dispose();
                wavFileStream = null;
            }

            // Reset the members back to defaults
            //initVariables();
        }

        private void initVariables()
        {
            wavFilename = null;
            wavFileStream = null;
            wavDataSizeBytes = 0;
            wavBytesPerSample = 0;
       
            // These audio format defaults correspond to the standard for
            // CD audio.
            wavNumChannels = 2;
            wavSampleRateHz = 44100;
            wavBytesPerSec = 176400;
            wavBitsPerSample = 16;
            mNumSamplesRemaining = 0;
        }

        public static byte[] Short2ByteArray(short[] sArray)
        {
            byte[] rez = new byte[sArray.Length * 2];
            int cnt = 0;
            foreach (short sword in sArray)
            {
                Int16 sample = (Int16)sword;
                byte lo = Convert.ToByte(sample & 0xff);
                byte hi = Convert.ToByte((sample >> 8) & 0xff);

                rez[cnt] = lo;
                rez[cnt + 1] = hi;
                cnt += 2;
            }
            return rez;
        } 

        /// <summary>
        /// Returns whether or not the WAV file format (mono/stereo,
        /// sample rate, and bits per sample) match another WAV file's
        /// format.
        /// </summary>
        /// <param name="pWAVFile">Another WAVFile object to compare with</param>
        /// <returns></returns>
        public bool FormatMatches(WAVFile pWAVFile)
        {
            bool retval = false;

            if (pWAVFile != null)
                retval = ((wavNumChannels == pWAVFile.NumChannels) &&
                          (wavSampleRateHz == pWAVFile.SampleRateHz) &&
                          (wavBitsPerSample == pWAVFile.BitsPerSample));

            return retval;
        }


        /// <summary>
        /// Gets the audio file's number of channels
        /// </summary>
        public byte NumChannels
        {
            get { return wavNumChannels; }
        }

        /// <summary>
        /// Gets whether or not the file is stereo.
        /// </summary>
        public bool IsStereo
        {
            get { return (wavNumChannels == 2); }
        }

        /// <summary>
        /// Gets the audio file's sample rate (in Hz)
        /// </summary>
        public int SampleRateHz
        {
            get { return wavSampleRateHz; }
        }

        /// <summary>
        /// Gets the number of bytes per second for the audio file
        /// </summary>
        public int BytesPerSec
        {
            get { return wavBytesPerSec; }
        }

        /// <summary>
        /// Gets the number of bits per sample for the audio file
        /// </summary>
        public short BitsPerSample
        {
            get { return wavBitsPerSample; }
        }

        /// <summary>
        /// Gets the data wav samples
        /// </summary>
        public short [] WAVSamples
        {
            get { return wavSamples; }
        }

        /// <summary>
        /// Gets the header
        /// </summary>
        public byte[] WAVHeader
        {
            get { return wavHeader; }
        }

        /// <summary>
        /// Gets the SubChunk2Size
        /// </summary>
        public int SubChunk2Size
        {
            get { return wavSubChunk2Size; }
        }

    }
}
