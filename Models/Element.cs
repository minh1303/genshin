using System.ComponentModel.DataAnnotations;

namespace genshin.Models;

public class Element
{
    public int Id { get; set; }
    
    public required string Name { get; set; } 
    public List<Character>? Characters { get; set; }
}