namespace WebScraper.Models;

[Flags]
public enum StorageType
{
    None = 0,
    Csv = 1,
    Mongo = 2
}