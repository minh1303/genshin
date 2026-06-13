
namespace genshin.Models;

public class Character
{
    
    public int Id { get; set; }
    public required string Name { get; set; }
    public int Rarity { get; set; }
    public required int ElementId { get; set; }
    public Element? Element { get; set; }
}