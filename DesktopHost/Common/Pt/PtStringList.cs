//Creation time:2024/3/21 14:44:42
using System;
using System.Collections;
using System.Collections.Generic;
using Protocol.Net;

namespace Development.Net.Pt
{
public class PtStringList
{
    public byte __tag__ { get;private set;}

	public List<PtString> Elements{ get;private set;}
	   
    public PtStringList SetElements(List<PtString> value){Elements=value; __tag__|=1; return this;}
	
    public bool HasElements(){return (__tag__&1)==1;}
	
    public static byte[] Write(PtStringList data)
    {
        using(ByteBuffer buffer = new ByteBuffer())
        {
            buffer.WriteByte(data.__tag__);
			if(data.HasElements())buffer.WriteCollection(data.Elements,element=>PtString.Write(element));
			
            return buffer.GetRawBytes();
        }
    }

    public static PtStringList Read(byte[] bytes)
    {
        using(ByteBuffer buffer = new ByteBuffer(bytes))
        {
            PtStringList data = new PtStringList();
            data.__tag__ = buffer.ReadByte();
			if(data.HasElements())data.Elements = buffer.ReadCollection(retbytes=>PtString.Read(retbytes));
			
            return data;
        }       
    }
}
}
