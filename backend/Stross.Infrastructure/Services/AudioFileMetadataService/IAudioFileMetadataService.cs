namespace Stross.Infrastructure.Services.AudioFileMetadataService;

public interface IAudioFileMetadataService
{
    public int GetDuration(string filePath);
    
    public long GetFileSize(string filePath);
}