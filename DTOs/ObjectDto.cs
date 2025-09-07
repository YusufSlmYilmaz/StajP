using System.ComponentModel.DataAnnotations;

namespace StajP.DTOs
{
    public class ObjectDto
    {
        [Required(ErrorMessage = "Name zorunludur.")]
        [StringLength(40, ErrorMessage = "Name en fazla 40 karakter olabilir.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "WKT zorunludur.")]
        [RegularExpression(@"^(POINT|LINESTRING|POLYGON)\s?\(.+\)$",
            ErrorMessage = "WKT formatı POINT, LINESTRING veya POLYGON olmalıdır.")]
        public string Wkt { get; set; }
    }
}