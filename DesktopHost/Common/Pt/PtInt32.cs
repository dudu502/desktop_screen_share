//Creation time:2024/3/19 13:40:40
using System;
using System.Collections;
using System.Collections.Generic;
using Protocol.Net;

namespace Development.Net.Pt
{
public class PtInt32
{
    public byte __tag__ { get;private set;}

	public int Value{ get;private set;}
	   
    public PtInt32 SetValue(int value){Value=value; __tag__|=1; return this;}
	
    public bool HasValue(){return (__tag__&1)==1;}
	
    public static byte[] Write(PtInt32 data)
    {
        using(ByteBuffer buffer = new ByteBuffer())
        {
            buffer.WriteByte(data.__tag__);
			if(data.HasValue())buffer.WriteInt32(data.Value);
			
            return buffer.GetRawBytes();
        }
    }

    public static PtInt32 Read(byte[] bytes)
    {
        using(ByteBuffer buffer = new ByteBuffer(bytes))
        {
            PtInt32 data = new PtInt32();
            data.__tag__ = buffer.ReadByte();
			if(data.HasValue())data.Value = buffer.ReadInt32();
			
            return data;
        }       
    }
}
}
