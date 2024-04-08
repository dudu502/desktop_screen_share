//Creation time:2024/4/8 16:07:25
using System;
using System.Collections;
using System.Collections.Generic;
using Protocol.Net;

namespace Development.Net.Pt
{
public class PtMessagePackage
{
    public byte __tag__ { get;private set;}

	public ushort MessageId{ get;private set;}
	public byte[] Content { get;private set;}
	public byte[] ToIp{ get;private set;}
	public int ToPort{ get;private set;}
	public byte[] FromIp{ get;private set;}
	public int FromPort{ get;private set;}
	   
    public PtMessagePackage SetMessageId(ushort value){MessageId=value; __tag__|=1; return this;}
	public PtMessagePackage SetContent(byte[] value){Content=value; __tag__|=2; return this;}
	public PtMessagePackage SetToIp(byte[] value){ToIp=value; __tag__|=4; return this;}
	public PtMessagePackage SetToPort(int value){ToPort=value; __tag__|=8; return this;}
	public PtMessagePackage SetFromIp(byte[] value){FromIp=value; __tag__|=16; return this;}
	public PtMessagePackage SetFromPort(int value){FromPort=value; __tag__|=32; return this;}
	
    public bool HasMessageId(){return (__tag__&1)==1;}
	public bool HasContent(){return (__tag__&2)==2;}
	public bool HasToIp(){return (__tag__&4)==4;}
	public bool HasToPort(){return (__tag__&8)==8;}
	public bool HasFromIp(){return (__tag__&16)==16;}
	public bool HasFromPort(){return (__tag__&32)==32;}
	
    public static byte[] Write(PtMessagePackage data)
    {
        using(ByteBuffer buffer = new ByteBuffer())
        {
            buffer.WriteByte(data.__tag__);
			if(data.HasMessageId())buffer.WriteUInt16(data.MessageId);
			if(data.HasContent())buffer.WriteBytes(data.Content);
			if(data.HasToIp())buffer.WriteBytes(data.ToIp);
			if(data.HasToPort())buffer.WriteInt32(data.ToPort);
			if(data.HasFromIp())buffer.WriteBytes(data.FromIp);
			if(data.HasFromPort())buffer.WriteInt32(data.FromPort);
			
            return buffer.GetRawBytes();
        }
    }
        public static PtMessagePackage BuildParams(ushort messageId, params object[] pars)
        {
            using (ByteBuffer buffer = new ByteBuffer())
            {
                foreach (object i in pars)
                {
                    Type iType = i.GetType();
                    if (iType == typeof(int))
                    {
                        buffer.WriteInt32((int)i);
                    }
                    else if (iType == typeof(uint))
                    {
                        buffer.WriteUInt32((uint)i);
                    }
                    else if (iType == typeof(float))
                    {
                        buffer.WriteFloat((float)i);
                    }
                    else if (iType == typeof(bool))
                    {
                        buffer.WriteBool((bool)i);
                    }
                    else if (iType == typeof(long))
                    {
                        buffer.WriteInt64((long)i);
                    }
                    else if (iType == typeof(ulong))
                    {
                        buffer.WriteUInt64((ulong)i);
                    }
                    else if (iType == typeof(short))
                    {
                        buffer.WriteInt16((short)i);
                    }
                    else if (iType == typeof(ushort))
                    {
                        buffer.WriteUInt16((ushort)i);
                    }
                    else if (iType == typeof(byte))
                    {
                        buffer.WriteByte((byte)i);
                    }
                    else if (iType == typeof(string))
                    {
                        buffer.WriteString((string)i);
                    }
                    else if (iType == typeof(byte[]))
                    {
                        buffer.WriteBytes((byte[])i);
                    }
                    else
                    {
                        throw new Exception("BuildParams Type is not supported. " + iType.ToString());
                    }
                }
                return Build(messageId, buffer.GetRawBytes());
            }

        }
        public static PtMessagePackage Build(ushort messageId)
        {
            PtMessagePackage package = new PtMessagePackage();
            package.SetMessageId(messageId);
            return package;
        }
        public static PtMessagePackage Build(ushort messageId, byte[] content)
        {
            PtMessagePackage package = Build(messageId);
            package.SetContent(content);
            return package;
        }
        public static PtMessagePackage Read(byte[] bytes)
    {
        using(ByteBuffer buffer = new ByteBuffer(bytes))
        {
            PtMessagePackage data = new PtMessagePackage();
            data.__tag__ = buffer.ReadByte();
			if(data.HasMessageId())data.MessageId = buffer.ReadUInt16();
			if(data.HasContent())data.Content = buffer.ReadBytes();
			if(data.HasToIp())data.ToIp = buffer.ReadBytes();
			if(data.HasToPort())data.ToPort = buffer.ReadInt32();
			if(data.HasFromIp())data.FromIp = buffer.ReadBytes();
			if(data.HasFromPort())data.FromPort = buffer.ReadInt32();
			
            return data;
        }       
    }
}
}
