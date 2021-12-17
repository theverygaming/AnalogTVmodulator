using System;
using System.Drawing;
using System.IO;
using System.Text;

class program
{
    static void Main(string[] args)
    {
        Bitmap bmp = new Bitmap("test.png");
        int samplerate = 1000000;
        float SyncDip = 0;
        float verticalBlank = 0;
        float frontporch = 0;

        int syncdipsamples = 1;
        int verticalblanksamples = 2;
        int frontporchsamples = 3;
        int ExtraSamples = syncdipsamples + verticalblanksamples + frontporchsamples;

        float[] hsb = new float[(bmp.Width * bmp.Height) + bmp.Width * ExtraSamples]; //Add sync pulse, 10 samples per line in this case
        int ArrayPosition = 0;
        for (int i = 0; i < bmp.Height; i++)
        {
            //Sync
            for(int j = 0; j < syncdipsamples; j++)
            {
                hsb[ArrayPosition] = -1;
                ArrayPosition++;
            }
            //Blanking
            for(int j = 0; j < verticalblanksamples; j++)
            {
                hsb[ArrayPosition] = 0;
                ArrayPosition++;
            }
            //Brightness data
            for (int j = 0 - 0; j < bmp.Width; j++)
            {
                Color c = bmp.GetPixel(j, i);
                hsb[ArrayPosition] = ((float)(c.R + c.G + c.B) / 3) / 255;
                ArrayPosition++;
            }
            //Front porch
            for(int j = 0; j < frontporchsamples; j++)
            {
                hsb[ArrayPosition] = 0;
                ArrayPosition++;
            }
        }


        WriteArrayToWAV(hsb, "test.wav", samplerate);

    }


    static void WriteArrayToWAV(float[] data, string filename, int samplerate)
    {
        FileStream f = new FileStream(filename, FileMode.Create);
        BinaryWriter wr = new BinaryWriter(f);
        for(int i = 0; i < data.Length; i++)
        {
            wr.Write(data[i]);
        }

    }

}