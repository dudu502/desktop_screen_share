//Creation time:2024/3/26 10:28:33
using System;
using System.Collections;
using System.Collections.Generic;
using Protocol.Net;

namespace Development.Net.Pt
{
public class PtVec2
{
    public byte __tag__ { get;private set;}

	public float X{ get;private set;}
	public float Y{ get;private set;}
	   
    public PtVec2 SetX(float value){X=value; __tag__|=1; return this;}
	public PtVec2 SetY(float value){Y=value; __tag__|=2; return this;}
	
    public bool HasX(){return (__tag__&1)==1;}
	public bool HasY(){return (__tag__&2)==2;}
	
    public static byte[] Write(PtVec2 data)
    {
        using(ByteBuffer buffer = new ByteBuffer())
        {
            buffer.WriteByte(data.__tag__);
			if(data.HasX())buffer.WriteFloat(data.X);
			if(data.HasY())buffer.WriteFloat(data.Y);
			
            return buffer.GetRawBytes();
        }
    }

    public static PtVec2 Read(byte[] bytes)
    {
        using(ByteBuffer buffer = new ByteBuffer(bytes))
        {
            PtVec2 data = new PtVec2();
            data.__tag__ = buffer.ReadByte();
			if(data.HasX())data.X = buffer.ReadFloat();
			if(data.HasY())data.Y = buffer.ReadFloat();
			
            return data;
        }       
    }
}
}
