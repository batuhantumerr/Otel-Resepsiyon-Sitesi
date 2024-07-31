using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OtelResepsiyon.Models
{
    public class Rezervasyon
    {
        [Key]
        public int RezervasyonId { get; set; }
        public DateTime GirisTarihi { get; set; }
        public DateTime CikisTarihi { get; set; }
        public int MisafirSayisi { get; set; }
        public string? OdaTipi {  get; set; }
        public string Telefon { get; set; }
        public string? Ad_Soyad {get; set;} 
        public string? AracPlaka { get; set; }
        [ValidateNever]
        public int? OdaNo { get; set; }
        [ForeignKey("OdaNo")]
        [ValidateNever]
        public Oda? Oda { get; set; }
        public int Durum {  get; set; }

        //public RezervasyonDurum Durum { get; set; }
        //public enum RezervasyonDurum
        //{
        //    Aktif,       0
        //    Kullanimda,  1
        //    Gecmis,      2
        //    Iptal        3
        //}
        public int? OdenenTutar { get; set; }
        public int ToplamTutar { get; set; }
    }
}
