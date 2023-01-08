namespace PhotoManager.Domain.Interfaces
{
    public interface IAssetHashCalculatorService
    {
        string CalculateHash(byte[] imageBytes);
        string CalculateVideoHash(string filePath);
    }
}
