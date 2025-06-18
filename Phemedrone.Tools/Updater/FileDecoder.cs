using System;

namespace Phemedrone.Tools.Updater;

public class FileDecoder
{
    public byte[] SourceFile { get; set; }
    
    public FileDecoder(byte[] encodedFileBytes)
    {
        var key = new byte[16];
        var encodedFile = new byte[encodedFileBytes.Length - 16];
        
        Array.Copy(encodedFileBytes, 0, key, 0, key.Length);
        Array.Copy(encodedFileBytes, 16, encodedFile, 0, encodedFile.Length);

        SourceFile = Cryptography.Rc4(encodedFile, key);
    }
}