using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Collections.Generic;

class program
{
    static void Main(string[] args)
    {
        Bitmap image = new Bitmap("test.png");
        int NumberOfFrames = 5;
        List<float> all = new List<float>();
        for(int i = 0; i < NumberOfFrames; i++)
        {
            all.AddRange(DrawFrame(image));
        }
        float[] hsb = all.ToArray();
        int samplerate = 10000000;
        WriteArrayToWAV(hsb, "test.wav", samplerate);

    }

    static float[] DrawFrame(Bitmap input)
    {
        Bitmap bmp = new Bitmap(input,new Size(576, 576));
        Console.WriteLine("Height: " + bmp.Height.ToString());
        int samplerate = 10000000;
        float frontporchTime = 0;
        float SyncDipTime = 0.0000047f; //NTSC: 0.0000047
        float verticalBlankTime = 0.00000735f; //NTSC: 0.0000014

        float SyncLevel = -0.2857f;
        float BlankingLevel = 0f;
        float ReferenceBlack = 0.05357f;
        float ReferenceWhite = 0.7143f;


        int frontporchsamples = (int)Math.Round((float)samplerate * frontporchTime);
        int syncdipsamples = (int)Math.Round((float)samplerate * SyncDipTime);
        int verticalblanksamples = (int)Math.Round((float)samplerate * verticalBlankTime);
        int ExtraSamples = syncdipsamples + verticalblanksamples + frontporchsamples;

        float[] hsb = new float[(bmp.Width * bmp.Height) + (bmp.Height * ExtraSamples)]; //Add sync pulse, 10 samples per line in this case
        int ArrayPosition = 0;
        for (int i = 0; i < bmp.Height; i++)
        {
            //Front porch
            for (int j = 0; j < frontporchsamples; j++)
            {
                hsb[ArrayPosition] = BlankingLevel;
                ArrayPosition++;
            }
            //Sync
            for (int j = 0; j < syncdipsamples; j++)
            {
                hsb[ArrayPosition] = SyncLevel;
                ArrayPosition++;
            }
            //Blanking / Backporch
            for (int j = 0; j < verticalblanksamples; j++)
            {
                hsb[ArrayPosition] = BlankingLevel;
                ArrayPosition++;
            }
            //Brightness data
            for (int j = 0 - 0; j < bmp.Width; j++)
            {
                Color c = bmp.GetPixel(j, i);
                hsb[ArrayPosition] = mapRange((float)(c.R + c.G + c.B) / 765, 0, 1, ReferenceBlack, ReferenceWhite);
                ArrayPosition++;
            }
        }


        for(int i = 0; i < hsb.Length; i++)
        {
            hsb[i] = hsb[i] * -1; //Invert signal
        }
        
        return hsb;
    }

    static float mapRange(float input, float input_start, float input_end, float output_start, float output_end)
    {
        float output = output_start + ((output_end - output_start) / (input_end - input_start)) * (input - input_start);
        return output;
    }

    static void WriteArrayToWAV(float[] data, string filename, int samplerate)
    {
        FileStream f = new FileStream(filename, FileMode.Create);
        BinaryWriter wr = new BinaryWriter(f);
        int fileSize = 44 + data.Length * 2;

        int channels = 1;

        // header:

        wr.Write(Encoding.ASCII.GetBytes("RIFF"));  // "RIFF"
        wr.Write((Int32)fileSize);                  // size of entire file with 16-bit data
        wr.Write(Encoding.ASCII.GetBytes("WAVE"));  // "WAVE"

        // chunk 1:
        wr.Write(Encoding.ASCII.GetBytes("fmt "));  // "fmt "
        wr.Write((Int32)16);                        // size of chunk in bytes
        wr.Write((Int16)1);                         // 1 - for PCM
        wr.Write((Int16)channels);                         // only Stereo files in this version
        wr.Write((Int32)samplerate);          // sample rate per second (usually 44100)
        wr.Write((Int32)((channels * 2) * samplerate));    // bytes per second (usually 176400)
        wr.Write((Int16)(2 * channels));                         // data align 4 bytes (2 bytes sample stereo)
        wr.Write((Int16)16);                        // only 16-bit in this version

        // chunk 2:
        wr.Write(Encoding.ASCII.GetBytes("data"));  // "data"
        wr.Write((Int32)(data.Length * 2));   // size of audio data 16-bit

        // audio data:
        
        for (int i = 0; i < data.Length; i++)
        {
            wr.Write((Int16)mapRange(data[i], -1, 1, -32768, 32768));
        }

    }

}