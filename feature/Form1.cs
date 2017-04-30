using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Imaging.Filters;
using AForge;
using System.IO;

namespace feature
{
    public partial class Form1 : Form
    {
        Bitmap originalImage;
        Bitmap skinImage;
        Bitmap monoImage;
        Bitmap morphImage;
        Bitmap biggestBlobsImage, k;
        Bitmap resultImage, fd;
        List<int> featureVector = new List<int>();

        public Form1()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            var returnValue = openDialog.ShowDialog();
            if (returnValue == DialogResult.OK)
            {
                //originalImage = new Bitmap(openDialog.FileName);
                resultImage = new Bitmap(new Bitmap(openDialog.FileName), new Size(300, 300));
                // pictureBox1.Image = originalImage;
                skinImage = new Bitmap(new Bitmap(openDialog.FileName), new Size(300, 300)); //load,resize and assign to skin image
                pictureBox1.Image = skinImage;
            }

        }

        private void skinColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int i, j, r, g, b, min, max;
            for (i = 0; i < skinImage.Width; i++)
            {
                for (j = 0; j < skinImage.Height; j++)
                {
                    Color pixelColor = (skinImage.GetPixel(i, j));
                    r = pixelColor.R;
                    g = pixelColor.G;
                    b = pixelColor.B;
                    max = Math.Max(r, Math.Max(g, b));
                    min = Math.Min(r, Math.Min(g, b));

                    if (r > 95 && g > 40 && b > 20 && (max - min) > 15 && max == r && r - g > 15)
                    {
                        continue;
                    }
                    else
                        skinImage.SetPixel(i, j, Color.Black);


                }
            }


            pictureBox2.Image = skinImage;

        }

        private void fillHoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // create grayscale filter (BT709)
            Grayscale filter = new Grayscale(0.2125, 0.7154, 0.0721);
            // apply the filter
            Bitmap grayImage = filter.Apply(skinImage);

            // create filter
            Threshold filter2 = new Threshold(90);

            filter2.ApplyInPlace(grayImage);
            //monoImage = filter2.Apply(grayImage);
            //pictureBox3.Image = monoImage;
            Closing close = new Closing();
            // apply the filter
            morphImage = close.Apply(grayImage);
            // create filter
            Opening open = new Opening();
            // apply the filter
            k = open.Apply(morphImage);
            pictureBox3.Image = k;


        }

        private void blobToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExtractBiggestBlob filter = new ExtractBiggestBlob();
            // apply the filter
            biggestBlobsImage = filter.Apply(k);
            pictureBox4.Image = biggestBlobsImage;
            IntPoint blobPosition = filter.BlobPosition;
            Console.WriteLine(blobPosition.X);
            Console.WriteLine(blobPosition.Y);
            Console.WriteLine(biggestBlobsImage.Width);
            Console.WriteLine(biggestBlobsImage.Height);
            fd = new Bitmap(biggestBlobsImage);
            int i, j;
            for (i = 0; i < (biggestBlobsImage.Width); i++)
            {
                for (j = 0; j < (biggestBlobsImage.Height); j++)
                {

                    Color hg = (resultImage.GetPixel(i + blobPosition.X, j + blobPosition.Y));
                    int r = fd.GetPixel(i, j).R;
                    int g = fd.GetPixel(i, j).G;
                    int b = fd.GetPixel(i, j).B;
                    if ((r = g = b) == 255)
                    {
                        fd.SetPixel(i, j, hg);
                    }
                    else
                        fd.SetPixel(i, j, Color.Black);


                }
            }

        }

        private void aNDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox5.Image = fd;


        }

        

        private void openFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
          /*  List<String> imagesList = new List<String>();
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();

            DialogResult result = folderDialog.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderDialog.SelectedPath))
            {
                DirectoryInfo directoryName = new DirectoryInfo(folderDialog.SelectedPath);
                string dname = directoryName.ToString();
                Console.WriteLine(directoryName);
                string[] path = Directory.GetFiles(dname, "*");
                List<Bitmap> images = new List<Bitmap>();
                foreach(var path in path)
                {
                    Bitmap image = new Bitmap(path);
                    images.Add(image);
             
                }


                List<double[]> features = new List<double[]>();
                foreach(var image in images)
                {
                    double[] features = automate(image);
                    features.Add(feature);
                }

                double[][] featuresArray = features.ToArray();
            } */
        }

        private void cannyEdgeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap grayImage = Grayscale.CommonAlgorithms.BT709.Apply(fd);
            CannyEdgeDetector canny = new CannyEdgeDetector(0, 0, 1.4);
            Bitmap edgeImage = canny.Apply(grayImage);

            pictureBox6.Image = edgeImage;
            // Console.WriteLine(edgeImage.Width);

            int i, j, x, y, u, v;
            double con1=0;
            Color blackC = Color.FromArgb(Color.Black.ToArgb());
            u = edgeImage.Width / 6;
            v = edgeImage.Height / 6;
            //Console.WriteLine(u);
            //Console.WriteLine(v);

            for (i = 0; i < 6; i++)
            {
                for (j = 0; j < 6; j++)
                {
                    int count = 0;
                    for (x = i * u; x < ((i * u) + u); x++)
                    {
                        for (y = j * v; y < ((j * v) + v); y++)
                        {

                            Color c = (edgeImage.GetPixel(x, y));
                            //Console.WriteLine(c);
                            if (c != blackC)
                            {
                                count++;

                                con1++;
                            }


                        }
                    }
                    
                    featureVector.Add(count);
                   //Console.WriteLine(count);

                }
            }
           
            
            //Calculate Normalization and add the value to the featureNormVector
            List<double> featureNormVector = new List<double>();

            //Total Pixel Count
            //Console.WriteLine(con1);

            int z;
            //Normalization
            for (z = 0; z < featureVector.Count; z++)
            {
                double normalizedValue = featureVector[z] / con1;
                Console.WriteLine(normalizedValue);
                featureNormVector.Add(normalizedValue);
            }


        }

        private void doitallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            skinColorToolStripMenuItem_Click(sender, e);
           
            fillHoleToolStripMenuItem_Click(sender, e);
            blobToolStripMenuItem_Click(sender, e);
            aNDToolStripMenuItem_Click(sender,e);
            cannyEdgeToolStripMenuItem_Click(sender, e);


        }
    }
}
