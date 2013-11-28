using System;
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
        private byte[] wavHeader = new byte[44];
        private short[] wavSamples;

        // Audio format information
        
        private short wavNumChannels;      // The # of channels (1 or 2)
        private int wavSampleRateHz;      // The audio sample rate (Hz)
        private int wavBytesPerSec;       // Bytes per second
        private short wavBitsPerSample;   // # of bits per sample
        private int wavSubChunk2Size;
        private int wavDataSizeBytes;     // The data size (bytes)


        /// <summary>
        /// Constructor
        /// </summary>
        public WAVFile()
        {
            initVariables();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// /// <param name="pathToFile">The wav file's path</param>
        public WAVFile(String pathToFile)
        {
            initVariables();
            wavFilename = pathToFile;
            
            
            Open(wavFilename);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~WAVFile()
        {
            Close();
        }


        /// <summary>
        /// Opens the wav file and extracts the necessary information.
        /// </summary>
        /// <param name="fileName">The wav file name</param>
        public void Open(String fileName)
        {
            try
            {
                wavFilename = fileName;
                if (!File.Exists(fileName))
                    Console.WriteLine("File does not exist: " + fileName);

                wavFileStream = new FileStream(fileName, FileMode.Open);

                //reading the wav header
                wavFileStream.Seek(0, SeekOrigin.Begin);
                wavFileStream.Read(wavHeader, 0, 44);

                
                wavFileStream.Seek(0, SeekOrigin.End);
                wavContent = new byte[wavFileStream.Position];

                //reading the wav file's content
                wavFileStream.Seek(44, SeekOrigin.Begin);
                wavFileStream.Read(wavContent, 0, wavContent.Length - 44);


                //extracting the necessary information
                // # of channels (2 bytes)
                byte[] buffer = new byte[4];

                // # of channels (2 bytes)
                wavFileStream.Seek(20, SeekOrigin.Begin);
                wavFileStream.Read(buffer, 0, 4);
                wavNumChannels = (BitConverter.IsLittleEndian ? buffer[2] : buffer[3]);

                // Sample rate (4 bytes)
                wavFileStream.Read(buffer, 0, 4);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(buffer);
                //wavSampleRateHz = BitConverter.ToInt32(buffer, 0);

                // Bytes per second (4 bytes)
                wavFileStream.Read(buffer, 0, 4);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(buffer);
                wavBytesPerSec = BitConverter.ToInt32(buffer, 0);

                // Bits per sample (2 bytes)
                byte[] buffer2 = new byte[2];
                wavFileStream.Seek(34, SeekOrigin.Begin);
                wavFileStream.Read(buffer2, 0, 2);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(buffer2, 0, 2);
                wavBitsPerSample = BitConverter.ToInt16(buffer2, 0);

                //reading subchunk2size
                wavFileStream.Seek(40, SeekOrigin.Begin);
                wavFileStream.Read(buffer, 0, 4);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(buffer);
                wavSubChunk2Size = BitConverter.ToInt32(buffer, 0);
                /*
                PrintWAVInfo();
                */

                //size of the data
                wavDataSizeBytes = wavContent.Length;
          

                //setting up wavSamples
                wavSamples = new short[wavDataSizeBytes / 2]; //becasue short = 2 bytes :)
                for (int i = 0; i < wavSamples.Length; i++)
                {
                    wavSamples[i] = BitConverter.ToInt16(wavContent,i * 2);
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

        /// <summary>
        /// Up sampling the wav file.
        /// </summary>
        /// <param name="factor">The upsampling factor</param>
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
 
        
        /// <summary>
        /// Creates a wav file from various parts.
        /// </summary>
        /// <param name="header">The byte[] array that is contains the header information</param>
        /// <param name="samples">The short[] array that is contains the wav samples</param>
        /// <param name="fileName">The output file name</param>
        /// <returns>
        /// Returns the path of the created file.
        /// </returns>
        public string OutputWAVFile(byte [] header, short[] samples, String fileName)
        {
            string path;

            path = System.Web.Hosting.HostingEnvironment.MapPath("~/TransientStorage/") + fileName;

            FileStream wavOut;

            wavOut = new FileStream(path, FileMode.Create);
            wavOut.Write(header, 0, 44);
            wavOut.Write(Short2ByteArray(samples),0,samples.Length * 2);
            wavOut.Close();

            return path;
        }

        /// <summary>
        /// Prints wav file's info for debugging.
        /// </summary>
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

        /// <summary>
        /// Initializes the wav file's parameters
        /// </summary>
        private void initVariables()
        {
            wavFilename = null;
            wavFileStream = null;
            wavDataSizeBytes = 0;
       
            // These audio format defaults correspond to the standard for
            // CD audio.
            wavNumChannels = 2;
            wavSampleRateHz = 44100;
            wavBytesPerSec = 176400;
            wavBitsPerSample = 16;
        }


        /// <summary>
        /// Converts a short array to byte array.
        /// </summary>
        /// <param name="sArray">The short[] array that is being converted</param>
        /// <returns>
        /// A byte array
        /// </returns>
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
                          (wavBitsPerSample == pWAVFile.BitsPerSample));

            return retval;
        }


        /// <summary>
        /// Gets the audio file's number of channels
        /// </summary>
        public short NumChannels
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
