using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JWT.Models;

[Table("Users")]
public class User
{
    [Key]
    [Column("ID_user")]
    public int UserId { get; set; }

    
    [Column("username")]
    [MaxLength(100)]
    public string UserName { get; set; }

    [Column("password")]
    public string UserPassword { get; set; }
    
}