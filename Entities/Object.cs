using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StajP.Entities
{
    [Table("objects")]
    public class Object
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        [Column("name")]
        public string Name { get; set; }

        [Column("geometry")]
        public Geometry Geometry { get; set; }
    }
}
