using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QRCODE.Models
{
    [Table("qrstored")]
    public class qrstored
    {
        [Key]
        public int QRID { get; set; }
        public String QRBASE64 { get; set; }
        public String CREATEDDATE { get; set; }
        public String QRSTRING { get; set; }
        public String ISSTORED { get; set; }
        public DateTime LASTUPDATE { get; set; }
        public String STATUS { get; set; }
    }
}
