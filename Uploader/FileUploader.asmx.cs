using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using lab1;

namespace Uploader
{
    /// <summary>
    /// This web method will provide an web method to load any
    /// file onto the server; the UploadFile web method
    /// will accept the report and store it in the local file system.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class FileUploader : System.Web.Services.WebService
    {

        string wav1Filename, wav2Filename;
        WAVFile wav1, wav2;

        [WebMethod]
        public string UploadFile(byte[] f1, string fileName1, byte[] f2, string fileName2)
        {
            // the byte array argument contains the content of the file
            // the string argument contains the name and extension
            // of the file passed in the byte array
            try
            {
                // instance a memory stream and pass the
                // byte array to its constructor
                MemoryStream ms1 = new MemoryStream(f1);
                MemoryStream ms2 = new MemoryStream(f2);

                // instance a filestream pointing to the 
                // storage folder, use the original file name
                // to name the resulting file
                FileStream fs1 = new FileStream
                    (System.Web.Hosting.HostingEnvironment.MapPath("~/TransientStorage/") + 
                    fileName1, FileMode.Create);
                FileStream fs2 = new FileStream
                   (System.Web.Hosting.HostingEnvironment.MapPath("~/TransientStorage/") +
                   fileName2, FileMode.Create);

                // write the memory stream containing the original
                // file as a byte array to the filestream
                ms1.WriteTo(fs1);
                ms2.WriteTo(fs2);

                // clean up
                ms1.Close();
                fs1.Close();
                fs1.Dispose();

                ms2.Close();
                fs2.Close();
                fs2.Dispose();

                // return OK if we made it this far
                return "OK";
            }
            catch (Exception ex)
            {
                // return the error message if the operation fails
                return ex.Message.ToString();
            }
        }

        [WebMethod]
        public string RunMixer()
        {   
            string path1,path2;

            path1 = System.Web.Hosting.HostingEnvironment.MapPath("~/TransientStorage/") +
                   wav1Filename;
            path2 = System.Web.Hosting.HostingEnvironment.MapPath("~/TransientStorage/") +
                   wav2Filename;
           
            Debug.WriteLine(wav1Filename);
            Debug.WriteLine(path1);

            WAVMixer mixer = new WAVMixer(path1, path2);
            return mixer.startMixing();
        }

        [WebMethod]
        public string MixWavFiles(byte[] f1, string fileName1, byte[] f2, string fileName2)
        {
            string outputPath;
            
            wav1Filename = fileName1;
            wav2Filename = fileName2;


            UploadFile(f1, wav1Filename, f2, wav2Filename);

            outputPath=RunMixer();

            return outputPath;
        }
        
    }
}
