using System;
using System.Collections;

namespace MediaCompression
{
    public class ImageMath
    {
        // Method converts from RGB to YCbCr.
        public int[] RGBToYCbCr(int r1, int g1, int b1)
        {
            double y = (0.299 * r1) + (0.587 * g1) + (0.114 * b1);
            double cb = -(0.168736 * r1) - (0.331264 * g1) + (0.5 * b1) + 128;
            double cr = (0.5 * r1) - (0.418688 * g1) - (0.081312 * b1) + 128;

            int[] array = { (int)y, (int)cb, (int)cr };

            return array;
        }

        // Method converts from YCbCr to RGB.
        public int[] YCbCrToRGB(int y1, int cb1, int cr1)
        {
            double r = y1 + 1.402 * (cr1 - 128);
            double g = y1 - 0.344136 * (cb1 - 128) - 0.714136 * (cr1 - 128);
            double b = y1 + 1.772 * (cb1 - 128);

            r = between0and255(r);
            b = between0and255(b);
            g = between0and255(g);

            int[] array = { (int)r, (int)g, (int)b };

            return array;
        }

        public double between0and255(double color)
        {
            if (color < 0)
                return 0;
            if (color > 255)
                return 255;
            return color;
        }

        // Uses array to quantize an 8x8 block.
        public int[,] QuantizeL(int[,] F)
        {
            int[,] q = {{16,11,10,16,24,40,51,61 },
         { 12,12,14,19,26,58,60,55 },
         { 14,13,16,24,40,57,69,56 },
         { 14,17,22,29,51,87,80,62 },
         { 18,22,37,56,68,109,103,77 },
         { 24,35,55,64,81,104,113,92 },
         { 49,64,78,87,103,121,120,101 },
         { 72,92,95,98,112,100,103,99 } };
            int[,] QF = new int[8, 8];
            int i, j;
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    QF[i, j] = F[i, j] / q[i, j];
                }
            }
            return QF;
        }

        public int[,] QuantizeLD(int[,] F)
        {
            int[,] q = {{16,11,10,16,24,40,51,61 },
         { 12,12,14,19,26,58,60,55 },
         { 14,13,16,24,40,57,69,56 },
         { 14,17,22,29,51,87,80,62 },
         { 18,22,37,56,68,109,103,77 },
         { 24,35,55,64,81,104,113,92 },
         { 49,64,78,87,103,121,120,101 },
         { 72,92,95,98,112,100,103,99 } };
            int[,] QF = new int[8, 8];
            int i, j;
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    QF[i, j] = F[i, j] * q[i, j];
                }
            }
            return QF;
        }
        public int[,] QuantizeC(int[,] F)
        {
            int[,] q = {{17,18,24,47,99,99,99,99},
         {18,21,26,66,99,99,99,99},
         {24,26,56,99,99,99,99,99},
         {47,66,99,99,99,99,99,99},
         {99,99,99,99,99,99,99,99},
         {99,99,99,99,99,99,99,99},
         {99,99,99,99,99,99,99,99},
         {99,99,99,99,99,99,99,99}};
            int[,] QF = new int[8, 8];
            int i, j;
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    QF[i, j] = F[i, j] / q[i, j];
                }
            }
            return QF;
        }
        public int[,] QuantizeCD(int[,] F)
        {
            int[,] q = {{17,18,24,47,99,99,99,99},
         {18,21,26,66,99,99,99,99},
         {24,26,56,99,99,99,99,99},
         {47,66,99,99,99,99,99,99},
         {99,99,99,99,99,99,99,99},
         {99,99,99,99,99,99,99,99},
         {99,99,99,99,99,99,99,99},
         {99,99,99,99,99,99,99,99}};
            int[,] QF = new int[8, 8];
            int i, j;
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    QF[i, j] = F[i, j] * q[i, j];
                }
            }
            return QF;
        }

        public int[] zz =
        {
                 0, 1, 8, 16, 9, 2, 3, 10 ,
                 17, 24, 32, 25, 18, 11, 4, 5,
                 12, 19, 26, 33, 40, 48, 41, 34,
                 27, 20, 13, 6, 7, 14, 21, 28,
                 35, 42, 49, 56, 57, 50, 43, 36,
                 29, 22, 15, 23, 30, 37, 44, 51,
                 58, 59, 52, 45, 38, 31, 39, 46,
                 53, 60, 61, 54, 47, 55, 62, 63
        };

        public Byte[] zigZag(int[,] block)
        {
            Byte[] singleArray = new Byte[64];
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                {
                    int bx = zz[x + y * 8] % 8;
                    int by = (int)Math.Floor((float)zz[x + y * 8] / 8.0);
                    singleArray[x + y * 8] = (Byte)block[by, bx];
                }
            return singleArray;
        }

        public int[,] reverseZigZag(Byte[] input)
        {
            int[,] block = new int[8, 8];
            for (int i = 0; i < 64; i++)
            {
                int x = zz[i]%8;
                int y = (int)Math.Floor((float)zz[i]/8.0);
                block[y, x] = input[i];
            }
            return block;
        }

        void Encode(int[] RL, int rl)
        {

        }

        float C(int u)
        {
            if (u == 0)
                return (float)(1.0 / Math.Sqrt(2.0));
            else
                return 1.0f;
        }
        float CD(int u)
        {
            if (u == 0)
                return (float)(1.0 / Math.Sqrt(8.0));
            else
                return (1.0f / 2.0f);
        }
        public int[,] DCT(int[,] input)
        {
            float pi = (float)Math.PI;
            int[,] ret = new int[8, 8];
            float a;
            for (int u = 0; u < 8; u++)
                for (int v = 0; v < 8; v++)
                {
                    a = 0.0f;
                    for (int x = 0; x < 8; x++)
                        for (int y = 0; y < 8; y++)
                            a += (float)((float)input[x, y]
                                * (float)Math.Cos((2.0 * (float)(x) + 1.0) * (float)(u) * pi / 16.0)
                                * (float)Math.Cos((2.0 * (float)(y) + 1.0) * (float)(v) * pi / 16.0));
                    ret[u, v] = (int)(0.25 * C(u) * C(v) * a);
                }
            return ret;
        }
        public int[,] IDCT(int[,] input)
        {
            float pi = (float)Math.PI;
            int[,] ret = new int[8, 8];
            float a;
            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                {
                    a = 0.0f;
                    for (int u = 0; u < 8; u++)
                        for (int v = 0; v < 8; v++)
                            a += (float)(C(u) * C(v) * (float)(input[u, v])
                                * Math.Cos((2.0 * (float)(x) + 1.0) * (float)(u) * pi / 16.0)
                                * Math.Cos((2.0 * (float)(y) + 1.0) * (float)(v) * pi / 16.0));

                    ret[x, y] = (int)between0and255((Math.Round(a * 0.25)));
                }
            return ret;
        }

        public ArrayList RLE(int[] ZZ) // Rewrite to store in a byte array
        {
            ArrayList myAL = new ArrayList();
            int count = 0;
            for (int i = 0; i < 64; i++)
            {
                if (ZZ[i] != 0) // if next number is not zero...
                {
                    if (count == 0) // and no zeroes occurred previously
                    {
                        myAL.Add(ZZ[i]); // add the number to the arraylist
                    }
                    else// else a non zero occurred and you have a string of zeroes before
                    {
                        myAL.Add(0);
                        myAL.Add(count);
                        myAL.Add(ZZ[i]);
                        count = 0; // Reset count;
                    }
                }
                else if (i < 63) // else zero occurred and not end of the array
                {
                    count++; // add to count
                }
                else // zero occurred and end of the array
                {
                    count++;
                    myAL.Add(0);
                    myAL.Add(count);
                    count = 0;
                }
            }
            // Copies the elements of the ArrayList to a byte array.
            return myAL;
        }

        public ArrayList Compress(int[,] f)
        {
            int[,] F = new int[8, 8];
            //DCT(f, F);
            int[,] QF = new int[8, 8];
            //Quantize(F, QF);

            int newDC = QF[0, 0];

            int[] ZZ = new int[64];
            //zigZag(QF, ZZ);

            return RLE(ZZ);
        }

    }
}
