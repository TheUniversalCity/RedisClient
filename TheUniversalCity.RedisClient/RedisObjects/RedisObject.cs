using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using TheUniversalCity.RedisClient.RedisObjects.Agregates;

namespace TheUniversalCity.RedisClient.RedisObjects
{
    public abstract class RedisObject
    {
        public RedisAttributeType Attribute { get; private set; }

        protected RedisObject()
        {

        }

        public void SetAttribute(RedisAttributeType attribute)
        {
            Attribute = attribute;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[][] ReadBlobEofCrLf(IEnumerator<byte> enumerator, long length)
        {
            long containerSize = length / int.MaxValue + Math.Sign(length % int.MaxValue);
            var byteContainer = new byte[containerSize][];

            for (int i = 0; i < containerSize; i++)
            {
                var bufferLength = Math.Min(length, int.MaxValue);
                var buffer = new byte[bufferLength];

                for (long j = 0; j < bufferLength; j++)
                {
                    enumerator.MoveNext();
                    buffer[j] = enumerator.Current;
                }

                byteContainer[i] = buffer;
                length -= bufferLength;
            }

            enumerator.MoveNext(); // CR
            enumerator.MoveNext(); // LF

            return byteContainer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[] ReadBlobEofCrLf(IEnumerator<byte> enumerator, long length, Encoding encoding)
        {
            var byteContainer = ReadBlobEofCrLf(enumerator, length);
            var stringContainer = new string[byteContainer.Length];

            for (int i = 0; i < byteContainer.Length; i++)
            {
                stringContainer[i] = encoding.GetString(byteContainer[i]);
#if DEBUG
                Console.WriteLine($"{nameof(ReadBlobEofCrLf)} : Value =>{stringContainer[i]}, i=> {i}");
#endif
            }

            return stringContainer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadLineEofCrLf(IEnumerator<byte> enumerator, Encoding encoding)
        {
            var list = new List<byte>();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current == RedisObjectDeterminator.CR)
                {
                    enumerator.MoveNext();

                    if (enumerator.Current == RedisObjectDeterminator.LF)
                    {
                        break;
                    }

                    throw new InvalidOperationException();
                }

                list.Add(enumerator.Current);
            }

            return encoding.GetString(list.ToArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadLineEof40BytesMarker(IEnumerator<byte> enumerator, byte[] marker, Encoding encoding)
        {
            var list = new List<byte>();

            while (enumerator.MoveNext())
            {
                #region FASTEST MARKER MATCHER
                if (enumerator.Current == marker[0])
                {
                    enumerator.MoveNext();

                    if (enumerator.Current == marker[1])
                    {
                        enumerator.MoveNext();

                        if (enumerator.Current == marker[2])
                        {
                            enumerator.MoveNext();

                            if (enumerator.Current == marker[3])
                            {
                                enumerator.MoveNext();

                                if (enumerator.Current == marker[4])
                                {
                                    enumerator.MoveNext();

                                    if (enumerator.Current == marker[5])
                                    {
                                        enumerator.MoveNext();

                                        if (enumerator.Current == marker[6])
                                        {
                                            enumerator.MoveNext();

                                            if (enumerator.Current == marker[7])
                                            {
                                                enumerator.MoveNext();

                                                if (enumerator.Current == marker[8])
                                                {
                                                    enumerator.MoveNext();

                                                    if (enumerator.Current == marker[9])
                                                    {
                                                        enumerator.MoveNext();

                                                        if (enumerator.Current == marker[10])
                                                        {
                                                            enumerator.MoveNext();

                                                            if (enumerator.Current == marker[11])
                                                            {
                                                                enumerator.MoveNext();

                                                                if (enumerator.Current == marker[12])
                                                                {
                                                                    enumerator.MoveNext();

                                                                    if (enumerator.Current == marker[13])
                                                                    {
                                                                        enumerator.MoveNext();

                                                                        if (enumerator.Current == marker[14])
                                                                        {
                                                                            enumerator.MoveNext();

                                                                            if (enumerator.Current == marker[15])
                                                                            {
                                                                                enumerator.MoveNext();

                                                                                if (enumerator.Current == marker[16])
                                                                                {
                                                                                    enumerator.MoveNext();

                                                                                    if (enumerator.Current == marker[17])
                                                                                    {
                                                                                        enumerator.MoveNext();

                                                                                        if (enumerator.Current == marker[18])
                                                                                        {
                                                                                            enumerator.MoveNext();

                                                                                            if (enumerator.Current == marker[19])
                                                                                            {
                                                                                                enumerator.MoveNext();

                                                                                                if (enumerator.Current == marker[20])
                                                                                                {
                                                                                                    enumerator.MoveNext();

                                                                                                    if (enumerator.Current == marker[21])
                                                                                                    {
                                                                                                        enumerator.MoveNext();

                                                                                                        if (enumerator.Current == marker[22])
                                                                                                        {
                                                                                                            enumerator.MoveNext();

                                                                                                            if (enumerator.Current == marker[23])
                                                                                                            {
                                                                                                                enumerator.MoveNext();

                                                                                                                if (enumerator.Current == marker[24])
                                                                                                                {
                                                                                                                    enumerator.MoveNext();

                                                                                                                    if (enumerator.Current == marker[25])
                                                                                                                    {
                                                                                                                        enumerator.MoveNext();

                                                                                                                        if (enumerator.Current == marker[26])
                                                                                                                        {
                                                                                                                            enumerator.MoveNext();

                                                                                                                            if (enumerator.Current == marker[27])
                                                                                                                            {
                                                                                                                                enumerator.MoveNext();

                                                                                                                                if (enumerator.Current == marker[28])
                                                                                                                                {
                                                                                                                                    enumerator.MoveNext();

                                                                                                                                    if (enumerator.Current == marker[29])
                                                                                                                                    {
                                                                                                                                        enumerator.MoveNext();

                                                                                                                                        if (enumerator.Current == marker[30])
                                                                                                                                        {
                                                                                                                                            enumerator.MoveNext();

                                                                                                                                            if (enumerator.Current == marker[31])
                                                                                                                                            {
                                                                                                                                                enumerator.MoveNext();

                                                                                                                                                if (enumerator.Current == marker[32])
                                                                                                                                                {
                                                                                                                                                    enumerator.MoveNext();

                                                                                                                                                    if (enumerator.Current == marker[33])
                                                                                                                                                    {
                                                                                                                                                        enumerator.MoveNext();

                                                                                                                                                        if (enumerator.Current == marker[34])
                                                                                                                                                        {
                                                                                                                                                            enumerator.MoveNext();

                                                                                                                                                            if (enumerator.Current == marker[35])
                                                                                                                                                            {
                                                                                                                                                                enumerator.MoveNext();

                                                                                                                                                                if (enumerator.Current == marker[36])
                                                                                                                                                                {
                                                                                                                                                                    enumerator.MoveNext();

                                                                                                                                                                    if (enumerator.Current == marker[37])
                                                                                                                                                                    {
                                                                                                                                                                        enumerator.MoveNext();

                                                                                                                                                                        if (enumerator.Current == marker[38])
                                                                                                                                                                        {
                                                                                                                                                                            enumerator.MoveNext();

                                                                                                                                                                            if (enumerator.Current == marker[39])
                                                                                                                                                                            {
                                                                                                                                                                                break;
                                                                                                                                                                            }

                                                                                                                                                                            list.Add(marker[38]);
                                                                                                                                                                        }

                                                                                                                                                                        list.Add(marker[37]);
                                                                                                                                                                    }

                                                                                                                                                                    list.Add(marker[36]);
                                                                                                                                                                }

                                                                                                                                                                list.Add(marker[35]);
                                                                                                                                                            }

                                                                                                                                                            list.Add(marker[34]);
                                                                                                                                                        }

                                                                                                                                                        list.Add(marker[33]);
                                                                                                                                                    }

                                                                                                                                                    list.Add(marker[32]);
                                                                                                                                                }

                                                                                                                                                list.Add(marker[31]);
                                                                                                                                            }

                                                                                                                                            list.Add(marker[30]);
                                                                                                                                        }

                                                                                                                                        list.Add(marker[29]);
                                                                                                                                    }

                                                                                                                                    list.Add(marker[28]);
                                                                                                                                }

                                                                                                                                list.Add(marker[27]);
                                                                                                                            }

                                                                                                                            list.Add(marker[26]);
                                                                                                                        }

                                                                                                                        list.Add(marker[25]);
                                                                                                                    }

                                                                                                                    list.Add(marker[24]);
                                                                                                                }

                                                                                                                list.Add(marker[23]);
                                                                                                            }

                                                                                                            list.Add(marker[22]);
                                                                                                        }

                                                                                                        list.Add(marker[21]);
                                                                                                    }

                                                                                                    list.Add(marker[20]);
                                                                                                }

                                                                                                list.Add(marker[19]);
                                                                                            }

                                                                                            list.Add(marker[18]);
                                                                                        }

                                                                                        list.Add(marker[17]);
                                                                                    }

                                                                                    list.Add(marker[16]);
                                                                                }

                                                                                list.Add(marker[15]);
                                                                            }

                                                                            list.Add(marker[14]);
                                                                        }

                                                                        list.Add(marker[13]);
                                                                    }

                                                                    list.Add(marker[12]);
                                                                }

                                                                list.Add(marker[11]);
                                                            }

                                                            list.Add(marker[10]);
                                                        }

                                                        list.Add(marker[9]);
                                                    }

                                                    list.Add(marker[8]);
                                                }

                                                list.Add(marker[7]);
                                            }

                                            list.Add(marker[6]);
                                        }

                                        list.Add(marker[5]);
                                    }

                                    list.Add(marker[4]);
                                }

                                list.Add(marker[3]);
                            }

                            list.Add(marker[2]);
                        }

                        list.Add(marker[1]);
                    }

                    list.Add(marker[0]);
                }
                #endregion

                list.Add(enumerator.Current);
            }

            enumerator.MoveNext(); // CR
            enumerator.MoveNext(); // LF

            return encoding.GetString(list.ToArray());
        }
    }

    public abstract class RedisObject<T> : RedisObject
    {
        public T Value { get; internal set; }

        public override string ToString()
        {
            return Value?.ToString();
        }
    }

}
