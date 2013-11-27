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
        static void Main(string[] args)
        {
            
            //testing the mixer class
           // WAVMixer testMixer = new WAVMixer("wave1_44KHzMono.wav", "wave2_44KHzMono.wav");
            WAVMixer testMixer = new WAVMixer("roland.wav", "yamaha.wav");
            testMixer.startMixing();
        }


    }
}
