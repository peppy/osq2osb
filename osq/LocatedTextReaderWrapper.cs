using System;
using System.IO;
using System.Linq;

namespace osq {
    /// <summary>
    /// Wraps a <see cref="TextReader"/> with <see cref="Location"/> tracking.
    /// </summary>
    public class LocatedTextReaderWrapper : TextReader, IDisposable {
        private TextReader source;
        private Location location;
        private readonly bool mustDisposeSource = false;

        /// <summary>
        /// Gets or sets the current location of the reader.
        /// </summary>
        /// <value>The current location of the reader.</value>
        public Location Location {
            get {
                return this.location;
            }

            set {
                if(value == null) {
                    throw new ArgumentNullException("value");
                }

                this.location = value;
            }
        }

        /// <summary>
        /// Skips white space.
        /// </summary>
        public void SkipWhiteSpace() {
            while(Peek() >= 0 && char.IsWhiteSpace((char)Peek())) {
                Location.AdvanceCharacter((char)Read());
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocatedTextReaderWrapper"/> class.
        /// </summary>
        /// <param name="source">The reader to wrap the <see cref="LocatedTextReaderWrapper"/> around.</param>
        public LocatedTextReaderWrapper(TextReader source) :
            this(source, new Location()) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocatedTextReaderWrapper"/> class.
        /// </summary>
        /// <param name="source">The reader to wrap the <see cref="LocatedTextReaderWrapper"/> around.</param>
        /// <param name="location">The starting location of the reader.</param>
        public LocatedTextReaderWrapper(TextReader source, Location location) {
            if(source == null) {
                throw new ArgumentNullException("source");
            }

            if(location == null) {
                throw new ArgumentNullException("location");
            }

            this.location = location;
            this.source = source;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocatedTextReaderWrapper"/> class.
        /// </summary>
        /// <param name="source">The string to wrap the <see cref="LocatedTextReaderWrapper"/> around.</param>
        public LocatedTextReaderWrapper(string source) :
            this(source, new Location()) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocatedTextReaderWrapper"/> class.
        /// </summary>
        /// <param name="source">The string to wrap the <see cref="LocatedTextReaderWrapper"/> around.</param>
        /// <param name="location">The starting location of the string.</param>
        public LocatedTextReaderWrapper(string source, Location location) :
            this(new StringReader(source), location) {
            this.mustDisposeSource = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocatedTextReaderWrapper"/> class.
        /// </summary>
        /// <param name="source">The stream to wrap the <see cref="LocatedTextReaderWrapper"/> around.</param>
        public LocatedTextReaderWrapper(Stream source) :
            this(source, new Location(), true) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocatedTextReaderWrapper"/> class.
        /// </summary>
        /// <param name="source">The stream to wrap the <see cref="LocatedTextReaderWrapper"/> around.</param>
        /// <param name="location">The starting location of the stream.</param>
        /// <param name="wrapperOwnsStream">If set to <c>true</c>, <paramref name="source"/> will be disposed when the <see cref="LocatedTextReaderWrapper"/> is disposed.</param>
        public LocatedTextReaderWrapper(Stream source, Location location, bool wrapperOwnsStream = true) :
            this(new StreamReader(source), location) {
            this.mustDisposeSource = wrapperOwnsStream;
        }

        /// <summary>
        /// Reads the next character from the input stream and advances the character position by one character.
        /// </summary>
        /// <returns>
        /// The next character from the input stream, or -1 if no more characters are available.
        /// </returns>
        /// <exception cref="T:System.ObjectDisposedException">
        /// The <see cref="T:System.IO.TextReader"/> is closed.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public override int Read() {
            int ret = this.source.Read();

            if(ret >= 0) {
                this.location.AdvanceCharacter((char)ret);
            }

            return ret;
        }

        /// <summary>
        /// Reads a maximum of <paramref name="count"/> characters from the current stream and writes the data to <paramref name="buffer"/>, beginning at <paramref name="index"/>.
        /// </summary>
        /// <param name="buffer">When this method returns, contains the specified character array with the values between <paramref name="index"/> and (<paramref name="index"/> + <paramref name="count"/> - 1) replaced by the characters read from the current source.</param>
        /// <param name="index">The place in <paramref name="buffer"/> at which to begin writing.</param>
        /// <param name="count">The maximum number of characters to read. If the end of the stream is reached before <paramref name="count"/> of characters is read into <paramref name="buffer"/>, the current method returns.</param>
        /// <returns>
        /// The number of characters that have been read. The number will be less than or equal to <paramref name="count"/>, depending on whether the data is available within the stream. This method returns zero if called when no more characters are left to read.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="buffer"/> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The buffer length minus <paramref name="index"/> is less than <paramref name="count"/>.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="index"/> or <paramref name="count"/> is negative.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        /// The <see cref="T:System.IO.TextReader"/> is closed.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public override int Read(char[] buffer, int index, int count) {
            int ret = this.source.Read(buffer, index, count);

            if(ret >= 0) {
                this.location.AdvanceString(string.Concat(buffer.Skip(index).Take(count)));
            }

            return ret;
        }

        /// <summary>
        /// Reads a maximum of <paramref name="count"/> characters from the current stream, and writes the data to <paramref name="buffer"/>, beginning at <paramref name="index"/>.
        /// </summary>
        /// <param name="buffer">When this method returns, this parameter contains the specified character array with the values between <paramref name="index"/> and (<paramref name="index"/> + <paramref name="count"/> -1) replaced by the characters read from the current source.</param>
        /// <param name="index">The place in <paramref name="buffer"/> at which to begin writing.</param>
        /// <param name="count">The maximum number of characters to read.</param>
        /// <returns>
        /// The position of the underlying stream is advanced by the number of characters that were read into <paramref name="buffer"/>.
        /// The number of characters that have been read. The number will be less than or equal to <paramref name="count"/>, depending on whether all input characters have been read.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="buffer"/> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The buffer length minus <paramref name="index"/> is less than <paramref name="count"/>.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="index"/> or <paramref name="count"/> is negative.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        /// The <see cref="T:System.IO.TextReader"/> is closed.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public override int ReadBlock(char[] buffer, int index, int count) {
            int ret = this.source.ReadBlock(buffer, index, count);

            if(ret >= 0) {
                this.location.AdvanceString(string.Concat(buffer.Skip(index).Take(count)));
            }

            return ret;
        }

        /// <summary>
        /// Reads a line of characters from the current stream and returns the data as a string.
        /// </summary>
        /// <returns>
        /// The next line from the input stream, or null if all characters have been read.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="T:System.OutOfMemoryException">
        /// There is insufficient memory to allocate a buffer for the returned string.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        /// The <see cref="T:System.IO.TextReader"/> is closed.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The number of characters in the next line is larger than <see cref="F:System.Int32.MaxValue"/></exception>
        public override string ReadLine() {
            string ret = this.source.ReadLine();

            if(ret != null) {
                this.location.AdvanceString(ret);
                this.location.AdvanceLine();
            }

            return ret;
        }

        /// <summary>
        /// Reads all characters from the current position to the end of the TextReader and returns them as one string.
        /// </summary>
        /// <returns>
        /// A string containing all characters from the current position to the end of the TextReader.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        /// The <see cref="T:System.IO.TextReader"/> is closed.
        /// </exception>
        /// <exception cref="T:System.OutOfMemoryException">
        /// There is insufficient memory to allocate a buffer for the returned string.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The number of characters in the next line is larger than <see cref="F:System.Int32.MaxValue"/></exception>
        public override string ReadToEnd() {
            string ret = this.source.ReadToEnd();

            if(ret != null) {
                this.location.AdvanceString(ret);
            }

            return ret;
        }

        /// <summary>
        /// Closes the <see cref="T:System.IO.TextReader"/> and releases any system resources associated with the TextReader.
        /// </summary>
        public override void Close() {
            // Nothing.
        }

        /// <summary>
        /// Reads the next character without changing the state of the reader or the character source. Returns the next available character without actually reading it from the input stream.
        /// </summary>
        /// <returns>
        /// An integer representing the next character to be read, or -1 if no more characters are available or the stream does not support seeking.
        /// </returns>
        /// <exception cref="T:System.ObjectDisposedException">
        /// The <see cref="T:System.IO.TextReader"/> is closed.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public override int Peek() {
            return this.source.Peek();
        }

        private bool disposed = false;

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.TextReader"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing) {
            if(!this.disposed) {
                if(disposing) {
                    if(this.mustDisposeSource && this.source != null) {
                        this.source.Dispose();
                    }
                }

                this.source = null;
                this.disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}