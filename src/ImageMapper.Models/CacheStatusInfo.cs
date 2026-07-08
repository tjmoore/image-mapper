namespace ImageMapper.Models;

public record CacheStatusInfo(bool IsCaching, int ProcessedFileCount = 0, int TotalFileCount = 0);
