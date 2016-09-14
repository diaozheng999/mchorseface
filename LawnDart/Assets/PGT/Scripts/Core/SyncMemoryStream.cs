using System.IO;
using System.Threading;

namespace PGT.Core
{
    public class SyncMemoryStream : MemoryStream
    {
        public override int Read(byte[] buffer, int offset, int count)
        {
            int ret;
            lock (this)
            {
                ret = base.Read(buffer, offset, count);
            }
            return ret;
        }

        public int ReadUnsync(byte[] buffer, int offset, int count)
        {
            return base.Read(buffer, offset, count);
        }
        

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (this)
            {
                base.Write(buffer, offset, count);
            }
        }

        public void WriteUnsync(byte[] buffer, int offset, int count)
        {
            base.Write(buffer, offset, count);
        }

        public override int ReadByte()
        {
            int b;
            lock (this)
            {
                b = base.ReadByte();
            }
            return b;
        }

        public int ReadByteUnsync()
        {
            return base.ReadByte();
        }

        public override void WriteByte(byte value)
        {
            lock (this)
            {
                base.WriteByte(value);
            }
        }

        public void WriteByteUnsync(byte value)
        {
            base.WriteByte(value);
        }

        public override byte[] ToArray()
        {
            byte[] b;
            lock (this)
            {
                b = base.ToArray();
            }
            return b;
        }

        public byte[] ToArrayUnsync()
        {
            return base.ToArray();
        }

        public override string ToString()
        {
            string str;
            lock (this)
            {
                str = base.ToString();
            }
            return str;
        }

        public string ToStringUnsync()
        {
            return base.ToString();
        }

        public override byte[] GetBuffer()
        {
            byte[] buf;
            lock (this)
            {
                buf = base.GetBuffer();
            }
            return buf;
        }

        public byte[] GetBufferUnsync()
        {
            return base.GetBuffer();
        }

        public override long Seek(long offset, SeekOrigin loc)
        {
            long offs;
            lock (this)
            {
                offs = base.Seek(offset, loc);
            }
            return offs;
        }

        public long SeekUnsync(long offset, SeekOrigin loc)
        {
            return base.Seek(offset, loc);
        }

        public void Empty()
        {
            lock (this)
            {
                base.Seek(0, SeekOrigin.Begin);
                base.SetLength(0);
            }
        }
        
    }
}
