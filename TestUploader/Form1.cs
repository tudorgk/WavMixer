using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Net;

namespace TestUploader
{
    /// <summary>
    /// A test form used to upload a file from a windows application using
    /// the Uploader Web Service
    /// </summary>
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            // do nothing
        }


        /// <summary>
        /// Upload any file to the web service; this function may be
        /// used in any application where it is necessary to upload
        /// a file through a web service
        /// </summary>
        /// <param name="filename">Pass the file path to upload</param>
        private void UploadFile(string filename1,string filename2)
        {
            try
            {
                // get the exact file name from the path
                String strFile1 = System.IO.Path.GetFileName(filename1);
                String strFile2 = System.IO.Path.GetFileName(filename2);

                // create an instance fo the web service
                TestUploader.Uploader.FileUploader srv = new TestUploader.Uploader.FileUploader();

                // get the file information form the selected file
                FileInfo fInfo1 = new FileInfo(filename1);
                FileInfo fInfo2 = new FileInfo(filename2);

                // get the length of the file to see if it is possible
                // to upload it (with the standard 4 MB limit)
                long numBytes1 = fInfo1.Length;
                double dLen1 = Convert.ToDouble(fInfo1.Length / 1000000);
                long numBytes2 = fInfo1.Length;
                double dLen2 = Convert.ToDouble(fInfo2.Length / 1000000);

                Debug.WriteLine(strFile1);
                Debug.WriteLine(strFile2);
                // Default limit of 4 MB on web server
                // have to change the web.config to if
                // you want to allow larger uploads
                if (dLen1 < 40 && dLen2 < 40)
                {
                    // set up a file stream and binary reader for the 
                    // selected file
                    FileStream fStream1 = new FileStream(filename1, FileMode.Open, FileAccess.Read);
                    BinaryReader br1 = new BinaryReader(fStream1);
                    FileStream fStream2 = new FileStream(filename2, FileMode.Open, FileAccess.Read);
                    BinaryReader br2 = new BinaryReader(fStream2);


                    // convert the file to a byte array
                    byte[] data1 = br1.ReadBytes((int)numBytes1);
                    br1.Close();
                    byte[] data2 = br2.ReadBytes((int)numBytes2);
                    br2.Close();

                    // pass the byte array (file) and file name to the web service
                    string sTmp = srv.MixWavFiles(data1, strFile1,data2,strFile2);

                    fStream1.Close();
                    fStream1.Dispose();
                    fStream2.Close();
                    fStream2.Dispose();


                    // this will always say OK unless an error occurs,
                    // if an error occurs, the service returns the error message
                    MessageBox.Show("File Upload Status: " + sTmp );

                    WebClient webClient = new WebClient();
                    webClient.DownloadFile(sTmp, "output.wav");



                }
                else
                {
                    // Display message if the file was too large to upload
                    MessageBox.Show("The file selected exceeds the size limit for uploads.", "File Size");
                }
            }
            catch (Exception ex)
            {
                // display an error message to the user
                MessageBox.Show(ex.Message.ToString(), "Upload Error");
            }
        }


        /// <summary>
        /// Allow the user to browse for a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Open File";
            openFileDialog1.Filter = "All Files|*.*";
            openFileDialog1.FileName = "";

            try
            {
                openFileDialog1.InitialDirectory = "C:\\Temp";
            }
            catch
            {
                // skip it 
            }

            openFileDialog1.ShowDialog();

            if (openFileDialog1.FileName == "")
                return;
            else
                txtFileName1.Text = openFileDialog1.FileName;

        }



        /// <summary>
        /// If the user has selected a file, send it to the upload method, 
        /// the upload method will convert the file to a byte array and
        /// send it through the web service
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (txtFileName1.Text != string.Empty && txtFileName2.Text != string.Empty)
            {
                this.UploadFile(txtFileName1.Text,txtFileName2.Text);
            } 
            else
                MessageBox.Show("You must select both files.", "No Files Selected");
        }


        //second button
        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog2.Title = "Open File";
            openFileDialog2.Filter = "All Files|*.*";
            openFileDialog2.FileName = "";

            try
            {
                openFileDialog2.InitialDirectory = "C:\\Temp";
            }
            catch
            {
                // skip it 
            }

            openFileDialog2.ShowDialog();

            if (openFileDialog2.FileName == "")
                return;
            else
                txtFileName2.Text = openFileDialog2.FileName;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}