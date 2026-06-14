namespace genshin.Helpers;

public static class ValidationHelper
{
    public static string? ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Name is required";
        
        if (name.Length < 2 || name.Length > 50)
            return "Name must be 2-50 characters";
        
        return null;
    }
    
    public static string? ValidateRarity(int rarity)
    {
        if (rarity < 1 || rarity > 5)
            return "Rarity must be between 1 and 5";
        
        return null;
    }
}