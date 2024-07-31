using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OtelResepsiyon.Models
{
    public class MisafirDetay
    {
        [Key]
        public int Id {  get; set; }
        [ValidateNever]
        public int RezervasyonId {  get; set; }
        [ForeignKey("RezervasyonId")]
        [ValidateNever]
        public Rezervasyon Rezervasyon { get; set;}
        public required string MusteriId { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string DogumTarihi { get; set; }
        public char Cinsiyet { get; set; }
        public string Uyruk { get; set; }

        //public void OnGet(int rezervasyonId)
        //{
        //    RezervasyonId=rezervasyonId;
        //}
    }
}
