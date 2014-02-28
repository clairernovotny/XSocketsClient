using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace XSocketsClient.Wrapper
{
    sealed class SslStreamWrapper : Stream
    {
        readonly object _locker = new Object();
        readonly Stream _stream;

        AsyncAutoResetEvent _asyncRead;
        AsyncAutoResetEvent _asyncWrite;


        public SslStreamWrapper(Stream stream)
        {
            _stream = stream;
        }

        public override bool CanRead
        {
            get
            {
                return _stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return _stream.CanSeek;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return base.CanTimeout;
            }
        }
        public override bool CanWrite
        {
            get
            {
                return _stream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return _stream.Length;
            }
        }

        public override long Position
        {
            get { return _stream.Position; }
            set { _stream.Position = value; }
        }
        public override void Flush()
        {
            _stream.Flush();
        }

        public override int Read([In, Out] byte[] buffer, int offset, int count)
        {
            try
            {
                lock (_locker)
                    _stream.Read(buffer, offset, count);
            }
            finally
            {
                throw new NotSupportedException("This stream does not support reading");
            }

        }
        public override long Seek(long offset, SeekOrigin origin)
        {

            try
            {
                lock (_locker)
                    _stream.Seek(offset, origin);
            }
            finally
            {
                throw new NotSupportedException("This stream does not support seek");
            }
        }
        public override void SetLength(long value)
        {

            lock (_locker)
                _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (_locker)
                _stream.Write(buffer, offset, count);
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_asyncRead == null)
            {
                lock (this)
                {
                    if (_asyncRead == null)
                        _asyncRead = new AsyncAutoResetEvent(true);
                }
            }

            await _asyncRead.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var result = await _stream.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
                return result;
            }
            finally
            {
                _asyncRead.Set();    
            }
        }


        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_asyncWrite == null)
            {
                lock (this)
                {
                    if (_asyncWrite == null)
                        _asyncWrite = new AsyncAutoResetEvent(true);
                }
            }
            await _asyncWrite.WaitAsync(cancellationToken).ConfigureAwait(false); //Only allow one thread through
            try
            {
                await _stream.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _asyncWrite.Set();    
            }
        }



        
    }
}