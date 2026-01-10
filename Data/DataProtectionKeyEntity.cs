using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace El_Buen_Taco.Data;
[Table("dataprotectionkeys")]
public class DataProtectionKeyEntity
{
    [Column("id")]
    public int Id { get; set; }
    [Column("xml")] 
    public string Xml { get; set; } = null!;
    [Column("friendlyname")]
    public string? FriendlyName { get; set; }
    [Column("createdat")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}