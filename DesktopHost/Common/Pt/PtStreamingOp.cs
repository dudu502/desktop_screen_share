//Creation time:2024/3/26 10:28:33
using System;
using System.Collections;
using System.Collections.Generic;
using Protocol.Net;

namespace Development.Net.Pt
{
public class PtStreamingOp
{
    public byte __tag__ { get;private set;}

	public byte OpType{ get;private set;}
	public PtVec2 Position{ get;private set;}
	   
    public PtStreamingOp SetOpType(byte value){OpType=value; __tag__|=1; return this;}
	public PtStreamingOp SetPosition(PtVec2 value){Position=value; __tag__|=2; return this;}
	
    public bool HasOpType(){return (__tag__&1)==1;}
	public bool HasPosition(){return (__tag__&2)==2;}
	
    public static byte[] Write(PtStreamingOp data)
    {
        using(ByteBuffer buffer = new ByteBuffer())
        {
            buffer.WriteByte(data.__tag__);
			if(data.HasOpType())buffer.WriteByte(data.OpType);
			if(data.HasPosition())buffer.WriteBytes(PtVec2.Write(data.Position));
			
            return buffer.GetRawBytes();
        }
    }

    public static PtStreamingOp Read(byte[] bytes)
    {
        using(ByteBuffer buffer = new ByteBuffer(bytes))
        {
            PtStreamingOp data = new PtStreamingOp();
            data.__tag__ = buffer.ReadByte();
			if(data.HasOpType())data.OpType = buffer.ReadByte();
			if(data.HasPosition())data.Position = PtVec2.Read(buffer.ReadBytes());
			
            return data;
        }       
    }
}
}
