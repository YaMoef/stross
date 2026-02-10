using Stross.Exception.Exceptions;

namespace Stross.Infrastructure.Services.AudioFileMetadataService;

public class AudioFileMetadataService : IAudioFileMetadataService
{
    public int GetDuration(string filePath)
    {
        if (!File.Exists(filePath))
            throw new StrossException($"Audio file not found at path: {filePath}");

        int durationInSeconds;

        using (TagLib.File? file = TagLib.File.Create(filePath))
        {
            durationInSeconds = (int)file.Properties.Duration.TotalSeconds;
        }

        return durationInSeconds;
    }

    public long GetFileSize(string filePath)
    {
        if (!File.Exists(filePath))
            throw new StrossException($"Audio file not found at path: {filePath}");

        FileInfo fileInfo = new FileInfo(filePath);

        return fileInfo.Length;
    }
}
