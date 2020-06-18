using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace NetWorkFrame
{
    public class MiniConverter
    {
        public static int BytesToInt(byte[] bytes, int startIndex)
        {
            Array.Reverse(bytes, startIndex, 4);
            return BitConverter.ToInt32(bytes, startIndex);
        }

        public static byte[] IntToBytes(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return bytes;
        }

        public static long TimeJavaToCSharp(long java_time) {
            DateTime dt_1970 = new DateTime(1970, 1, 1, 0, 0, 0);
            long ticks_1970 = dt_1970.Ticks;
            long time_ticks = ticks_1970 + java_time * 10000;
            DateTime dt = new DateTime(time_ticks).AddHours(8);
            return dt.Ticks;
        }
    }

    public class ToolForProtobuf
    {
        #region proto3序列化与反序列化
        public static byte[] Serialize(IMessage msg)
        {
            using (MemoryStream rawOutput = new MemoryStream())
            {
                CodedOutputStream output = new CodedOutputStream(rawOutput);
                //output.WriteRawVarint32((uint)len);
                output.WriteMessage(msg);
                output.Flush();
                byte[] result = rawOutput.ToArray();

                return result;
            }
        }
        public static T Deserialize<T>(byte[] dataBytes) where T : IMessage, new()
        {
            CodedInputStream stream = new CodedInputStream(dataBytes);
            T msg = new T();
            stream.ReadMessage(msg);
            //msg= (T)msg.Descriptor.Parser.ParseFrom(dataBytes);
            return msg;
        }
        #endregion
    }
}
