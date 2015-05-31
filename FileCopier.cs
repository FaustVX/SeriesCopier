using System.IO;

namespace SeriesCopier
{
    public class FileCopier
    {
        public delegate void ProgressChangeDelegate(double percentage, long sizeCopied, ref bool cancel);
        public delegate void Completedelegate();

        public FileCopier(string source, string dest)
            : this(new FileInfo(source), new FileInfo(dest))
        { }

        public FileCopier(FileInfo source, FileInfo dest)
        {
            SourceFilePath = source;
            DestFilePath = dest;
        }

        public void Copy(out long fileLength)
        {
            byte[] buffer = new byte[1024 * 10]; // 10KB buffer
            bool cancelFlag = false;

            using (var source = new FileStream(SourceFilePath.FullName, FileMode.Open, FileAccess.Read))
            {
                fileLength = source.Length;
                using (var dest = new FileStream(DestFilePath.FullName, FileMode.CreateNew, FileAccess.Write))
                {
                    long totalBytes = 0;
                    int currentBlockSize = 0;

                    while ((currentBlockSize = source.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        totalBytes += currentBlockSize;
                        double percentage = totalBytes * 100.0 / fileLength;

                        dest.Write(buffer, 0, currentBlockSize);

                        cancelFlag = false;
                        OnProgressChanged(percentage, totalBytes, ref cancelFlag);

                        if (cancelFlag)
                            break;
                    }
                }
            }
            if (cancelFlag)
                DestFilePath.Delete();

            OnComplete();
        }

        public FileInfo SourceFilePath { get; set; }
        public FileInfo DestFilePath { get; set; }

        public event ProgressChangeDelegate OnProgressChanged = delegate { };
        public event Completedelegate OnComplete = delegate { };
    }
}