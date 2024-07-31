using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OtelResepsiyon.Models
{
    public class Oda
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int OdaNo { get; set; }
        public int KatNo { get; set; }
        public string OdaTipi { get; set; }
        //public OdaDurum Durum {  get; set; }
        public int Fiyat { get; set; }
        public int Alan { get; set; }
        //public string Manzara { get; set; }
        public int Kapasite { get; set; }
        public string YatakDuzeni { get; set; }
        public int Durum {  get; set; }
        public int? BosOdaSayisi {  get; set; }

        //public enum OdaDurum
        //{
        //    Kullanima_Hazir,   0
        //    Kullanimda,        1
        //    Temizlenmeli,      2
        //    Kullanima_Kapali   3
        //}

    }

}
