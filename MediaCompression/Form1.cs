using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaCompression
{
    public partial class Form1 : Form
    {
        Bitmap loadedBitmap; // Loading the bitmap from file
        Bitmap yCbCrBitmap;
        Bitmap RGBBitmap;
        int[] yArray; // Used when taking values from the bitmap
        int[] CbArray; // Used when taking values from the bitmap
        int[] CrArray; // Used when taking values from the bitmap
        int[] yArraySubsampled;
        int[] CbArraySubsampled;
        int[] CrArraySubsampled;
        int[][,] yArrayQuantized;
        int[][,] CbArrayQuantized;
        int[][,] CrArrayQuantized;
        int[,] y64; // 8x8 array for y
        int[,] Cb64; // 8x8 array for Cr
        int[,] Cr64; // 8x8 array for Cb
        int width;
        int height;
        int subWidth;
        int subHeight;
        ImageMath im;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            im = new ImageMath();
        }

        // Load picture button. Displays the bmp in the picturebox. bmp saved as local variable b.
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Picture files|*.BMP";
            openFileDialog1.Title = "Select a BMP File";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                loadedBitmap = new Bitmap(openFileDialog1.OpenFile());
                height = loadedBitmap.Height;
                width = loadedBitmap.Width;
                pictureBox1.Image = loadedBitmap;
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        // CONVERT button takes what is in the buffer and converts to format specified in the dropbox.
        private void button2_Click(object sender, EventArgs e)
        {
            // Displays a SaveFileDialog so the user can save the Image  
            // assigned to Button2.  
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.  
            if (saveFileDialog1.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method.  
                System.IO.FileStream fs =
                   (System.IO.FileStream)saveFileDialog1.OpenFile();
                // Saves the Image in the appropriate ImageFormat based upon the  
                // File type selected in the dialog box.  
                // NOTE that the FilterIndex property is one-based.  
                switch (saveFileDialog1.FilterIndex)
                {
                    case 1:
                        loadedBitmap.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;

                    case 2:
                        loadedBitmap.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Bmp);
                        break;

                    case 3:
                        loadedBitmap.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                }

                fs.Close();
            }
        }
        private void convertToYCbCr()
        {
            yArray = new int[width * height];
            CbArray = new int[width * height];
            CrArray = new int[width * height];

            //yCbCrBitmap = new Bitmap(width, height);
            for(int j = 0; j < width; j++)
                for(int k = 0; k < height; k++)
                {
                    Color c = loadedBitmap.GetPixel(j, k); // Get a pixel
                    int[] x = im.RGBToYCbCr((int)c.R, (int)c.G, (int)c.B); // Create an array for YCbCr pixel
                    //Console.WriteLine(c);
                    yArray[j + k * width] = x[0]; // Always add y value
                    CbArray[j + k * width] = x[1]; // Take the value of Cb
                    CrArray[j + k * width] = x[2]; // and Cr
                }

        }

        private void convertTORGB()
        {
            RGBBitmap = new Bitmap(width, height);

            for (int j = 0; j < width; j++)
                for (int k = 0; k < height; k++)
                {
                    
                    int[] yCbCRcolor = new int[3];
                    //Console.WriteLine(c);
                    yCbCRcolor[0] = yArray[j + k * width]; // Always add y value
                    yCbCRcolor[1] = CbArray[j + k * width]; // Take the value of Cb
                    yCbCRcolor[2] = CrArray[j + k * width]; // and Cr

                    int[] RGBcolor = im.YCbCrToRGB(yCbCRcolor[0], yCbCRcolor[1], yCbCRcolor[2]);

                    Color color = Color.FromArgb(RGBcolor[0], RGBcolor[1], RGBcolor[2]);

                    RGBBitmap.SetPixel(j, k, color);
                }
        }

        private void subsample()
        {
            subWidth = (int)Math.Ceiling((float)width / 2);
            subHeight = (int)Math.Ceiling((float)height / 2);

            yArraySubsampled = yArray;
            CbArraySubsampled = new int[subWidth * subHeight];
            CrArraySubsampled = new int[subWidth * subHeight];

            for (int j = 0; j < width; j+=2)
                for (int k = 0; k < height; k+=2)
                {
                    CbArraySubsampled[j/2 + (k/2)*subWidth] = CbArray[j + k * width];
                    CrArraySubsampled[j/2 + (k/2)*subWidth] = CrArray[j + k * width];
                }
        }

        private void unsubsample()
        {

            yArray = yArraySubsampled;
            CbArray = new int[yArray.Length];
            CrArray = new int[yArray.Length];

            for (int j = 0; j < width; j++)
                for (int k = 0; k < height; k++)
                {
                    int subX = (int)Math.Floor((float)j / 2);
                    int subY = (int)Math.Floor((float)k / 2);
                    CbArray[j + k * width] = CbArraySubsampled[subX + subY * subWidth];
                    CrArray[j + k * width] = CrArraySubsampled[subX + subY * subWidth];
                }
        }

        // Iterate this
        private void addToByteArray(int currentW, int currentH)
        {
            //iterate8x8(currentW, currentH);

            im.Compress(y64);
            im.Compress(Cb64);
            im.Compress(Cr64);
        }

        // Blocksampling 8x8
        private int[,] iterate8x8(int currentW, int currentH, int[] yArrayParam)
        {
            int[,] eightByEight = new int[8, 8];
            
            for (int j = 0; j < 8; j++)
            {
                for (int k = 0; k < 8; k++)
                {
                    if(currentW + j <= width)
                        if(currentH + k <= height)
                        {
                            eightByEight[j,k] = yArrayParam[currentW + j + (k + currentH) * width];
                            //Cb64[j, k] = CbArrayParam[currentW + j + (k + currentH) * width];
                            //Cr64[j, k] = CrArrayParam[currentW + j + (k + currentH) * width];
                        }
                }
            }

            return eightByEight;
        }

        private void uniterate(int currentW, int currentH, ref int[] yArrayParam, int[,] eightByEight)
        {
            for (int j = 0; j < 8; j++)
            {
                for (int k = 0; k < 8; k++)
                {
                    if (currentW + j <= width)
                        if (currentH + k <= height)
                        {
                            yArrayParam[currentW + j + (k + currentH) * width] = eightByEight[j, k];
                            //Cb64[j, k] = CbArrayParam[currentW + j + (k + currentH) * width];
                            //Cr64[j, k] = CrArrayParam[currentW + j + (k + currentH) * width];
                        }
                }
            }
        }

        private void traverseBitmap() // DCT across bitmap
        {
            int xLength = (int)Math.Ceiling((float)(width / 8));
            int yLength = (int)Math.Ceiling((float)(height / 8));

            int xSubLength = (int)Math.Ceiling((float)(subWidth / 8));
            int ySubLength = (int)Math.Ceiling((float)(subHeight / 8));

            yArrayQuantized = new int[xLength*yLength][,];
            CbArrayQuantized = new int[xSubLength*ySubLength][,];
            CrArrayQuantized = new int[xSubLength*ySubLength][,];

            for (int j = 0; j < width; j += 8)
            {
                for (int k = 0; k < height; k += 8)
                {
                    var block = iterate8x8(j, k, yArraySubsampled);
                    block = im.DCT(block);
                    block = im.QuantizeL(block);
                    yArrayQuantized[j + k*width] = block;
                }
            }

            for (int j = 0; j < subWidth; j += 8)
            {
                for (int k = 0; k < subHeight; k += 8)
                {
                    var blockCr = iterate8x8(j, k, CrArraySubsampled);
                    var blockCb = iterate8x8(j, k, CbArraySubsampled);
                    blockCr = im.DCT(blockCr);
                    blockCb = im.DCT(blockCb);
                    blockCr = im.QuantizeL(blockCr);
                    blockCb = im.QuantizeL(blockCb);
                    CrArrayQuantized[j + k * width] = blockCr;
                    CbArrayQuantized[j + k * width] = blockCb;
                }
            }
        }

        private void reverseBitmap()
        {
            for (int j = 0; j < width; j += 8)
            {
                for (int k = 0; k < height; k += 8)
                {
                    var block = yArrayQuantized[j + k * width];
                    block = im.QuantizeLD(block);
                    block = im.IDCT(block);

                }
            }

            for (int j = 0; j < subWidth; j += 8)
            {
                for (int k = 0; k < subHeight; k += 8)
                {
                    var blockCr = iterate8x8(j, k, CrArraySubsampled);
                    var blockCb = iterate8x8(j, k, CbArraySubsampled);
                    blockCr = im.DCT(blockCr);
                    blockCb = im.DCT(blockCb);
                    blockCr = im.QuantizeL(blockCr);
                    blockCb = im.QuantizeL(blockCb);
                    CrArrayQuantized[j + k * width] = blockCr;
                    CbArrayQuantized[j + k * width] = blockCb;
                }
            }
        }

        // Pixel button. Starts the conversion process.
        private void button3_Click(object sender, EventArgs e)
        {
            convertToYCbCr();
            MessageBox.Show("YbCbCr conversion complete.");

        }

        private void button4_Click(object sender, EventArgs e)
        {
            convertTORGB();
            MessageBox.Show("RGB conversion complete.");
            pictureBox1.Image = RGBBitmap;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            subsample();
            MessageBox.Show("Subsample complete.");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            unsubsample();
            MessageBox.Show("unsubsample conversion complete.");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            traverseBitmap();
            MessageBox.Show("DCT conversion complete.");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            MessageBox.Show("inverse DCT conversion complete.");
        }
    }
}
