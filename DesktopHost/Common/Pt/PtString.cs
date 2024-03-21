//Creation time:2024/3/21 14:44:42
using System;
using System.Collections;
using System.Collections.Generic;
using Protocol.Net;

namespace Development.Net.Pt
{
public class PtString
{
    public byte __tag__ { get;private set;}

	public string Value{ get;private set;}
	   
    public PtString SetValue(string value){Value=value; __tag__|=1; return this;}
	
    public bool HasValue(){return (__tag__&1)==1;}
	
    public static byte[] Write(PtString data)
    {
        using(ByteBuffer buffer = new ByteBuffer())
        {
            buffer.WriteByte(data.__tag__);
			if(data.HasValue())buffer.WriteString(data.Value);
			
            return buffer.GetRawBytes();
        }
    }

    public static PtString Read(byte[] bytes)
    {
        using(ByteBuffer buffer = new ByteBuffer(bytes))
        {
            PtString data = new PtString();
            data.__tag__ = buffer.ReadByte();
			if(data.HasValue())data.Value = buffer.ReadString();
			
            return data;
        }       
    }
}
}
