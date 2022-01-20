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
        int NumberOfFrames = 1;
        List<float> all = new List<float>();
        for (int i = 0; i < NumberOfFrames; i++)
        {
            all.AddRange(DrawFrameNTSC(image));
        }
        float[] hsb = all.ToArray();
        int samplerate = 100000000;
        WriteArrayToWAV(hsb, "test.wav", samplerate);
    }

    static float[] DrawFrameNTSC(Bitmap input)
    {
        int samplerate = 100000000;
        float frontporchTime = 0.0000015f;
        float SyncDipTime = 0.0000047f;
        float verticalBlankTime = 0.0000047f;
        float LineTime = 0.0000526f;

        float SyncLevel = -0.2857f;
        float BlankingLevel = 0f;
        float ReferenceBlack = 0.05357f;
        float ReferenceWhite = 0.7143f;

        int lineSamples = (int)Math.Round((float)samplerate * LineTime);

        int frontporchsamples = (int)Math.Round((float)samplerate * frontporchTime);
        int syncdipsamples = (int)Math.Round((float)samplerate * SyncDipTime);
        int verticalblanksamples = (int)Math.Round((float)samplerate * verticalBlankTime);
        int ExtraSamples = syncdipsamples + verticalblanksamples + frontporchsamples;


        int lines = 259;
        int VertSyncLines = 1;
        Bitmap bmp = new Bitmap(input, new Size(lineSamples, lines));



        float[] hsb = new float[(bmp.Width * bmp.Height) + (bmp.Height * ExtraSamples) + (6066 * 100)]; //Add a few samples, will be replaced by list later
        int ArrayPosition = 0;
        for (int i = 0; i < VertSyncLines; i++) //Even Sync
        {
            float equalizingpulseTime = 0.0000023f;
            int equalizingpulsesamples = (int)Math.Round((float)samplerate * equalizingpulseTime);

            float LinePeriodTime = 0.0000635555f;
            int LinePeriodsamples = (int)Math.Round((float)samplerate * LinePeriodTime);

            Console.WriteLine("Line Period: " + LinePeriodsamples);
            Console.WriteLine("Equalizing Pulse: " + equalizingpulsesamples);
            //First 6 thingys
            for (int jx = 0; jx < 6; jx++)
            {
                //Equalizing pulse
                for (int j = 0; j < equalizingpulsesamples; j++)
                {
                    hsb[ArrayPosition] = SyncLevel;
                    ArrayPosition++;
                }
                //Blanking
                for (int j = 0; j < ((LinePeriodsamples / 2) - equalizingpulsesamples); j++)
                {
                    hsb[ArrayPosition] = BlankingLevel;
                    ArrayPosition++;
                }
            }
            Console.WriteLine("eqp1: " + ArrayPosition);

            float FieldSyncPulseTime = 0.0000271f;
            int FieldSyncPulsesamples = (int)Math.Round((float)samplerate * FieldSyncPulseTime);
            Console.WriteLine("Field Sync Pulse: " + FieldSyncPulsesamples);
            Console.WriteLine("Sync Dip: " + syncdipsamples);

            //Second 6 thingys
            for (int jx = 0; jx < 6; jx++)
            {
                //Field Sync Pulse
                for (int j = 0; j < FieldSyncPulsesamples; j++)
                {
                    hsb[ArrayPosition] = SyncLevel;
                    ArrayPosition++;
                }
                //Interval between Field Sync Pulses
                for (int j = 0; j < syncdipsamples; j++)
                {
                    hsb[ArrayPosition] = BlankingLevel;
                    ArrayPosition++;
                }
            }
            Console.WriteLine("fsp: " + ArrayPosition);

            //Third 6 thingys (Once again)
            for (int jx = 0; jx < 6; jx++)
            {
                //Equalizing pulse
                for (int j = 0; j < equalizingpulsesamples; j++)
                {
                    hsb[ArrayPosition] = SyncLevel;
                    ArrayPosition++;
                }
                //Blanking
                for (int j = 0; j < ((LinePeriodsamples / 2) - equalizingpulsesamples); j++)
                {
                    hsb[ArrayPosition] = BlankingLevel;
                    ArrayPosition++;
                }
            }
            Console.WriteLine("ep2: " + ArrayPosition);

            //10 normal blanked lines
            for (int jx = 0; jx < 10; jx++)
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
                for (int j = 0; j < bmp.Width; j++)
                {
                    hsb[ArrayPosition] = BlankingLevel;
                    ArrayPosition++;
                }
            }
            Console.WriteLine("nbl: " + ArrayPosition);


        }

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
            for (int j = 0; j < bmp.Width; j++)
            {
                Color c = bmp.GetPixel(j, i);
                hsb[ArrayPosition] = (((float)(c.R + c.G + c.B) / 765) * (ReferenceWhite - ReferenceBlack)) + ReferenceBlack;
                ArrayPosition++;
            }
        }

        for (int i = 0; i < hsb.Length; i++)
        {
            //hsb[i] = hsb[i] * -1; //Invert signal
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
