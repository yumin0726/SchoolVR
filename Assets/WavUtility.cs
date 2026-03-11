using UnityEngine;
using System;
using System.IO;

// 這是一個靜態工具類別，不需要掛載在物件上，放著就能用
public static class WavUtility
{
    public static byte[] FromAudioClip(AudioClip clip)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            WriteWavFile(clip, stream);
            return stream.ToArray();
        }
    }

    private static void WriteWavFile(AudioClip clip, MemoryStream stream)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        Int16[] intData = new Int16[samples.Length];
        Byte[] bytesData = new Byte[samples.Length * 2];

        // 轉換音訊採樣數據
        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * 32767);
            byte[] byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        // 寫入 WAV 檔案標頭 (Header)
        stream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
        stream.Write(BitConverter.GetBytes(bytesData.Length + 36), 0, 4);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);
        stream.Write(BitConverter.GetBytes(16), 0, 4);
        stream.Write(BitConverter.GetBytes((short)1), 0, 2);
        stream.Write(BitConverter.GetBytes((short)clip.channels), 0, 2);
        stream.Write(BitConverter.GetBytes(clip.frequency), 0, 4);
        stream.Write(BitConverter.GetBytes(clip.frequency * clip.channels * 2), 0, 4);
        stream.Write(BitConverter.GetBytes((short)(clip.channels * 2)), 0, 2);
        stream.Write(BitConverter.GetBytes((short)16), 0, 2);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
        stream.Write(BitConverter.GetBytes(bytesData.Length), 0, 4);
        stream.Write(bytesData, 0, bytesData.Length);
    }
}