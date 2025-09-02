using System.ComponentModel.DataAnnotations;

namespace mockAPI.Models;

public class Product
{
    [Key]
    public int Id { get; init; }
    [Required]
    public string Name { get; init; } = string.Empty;
    [DataType("decimal(18,2)")]
    public decimal Price { get; init; }
    public int CategoryId { get; init; }
}
