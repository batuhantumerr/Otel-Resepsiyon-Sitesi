namespace OtelResepsiyon.Models
{
    public interface IRezervasyonRepository : IRepository<Rezervasyon>
    {
        void Guncelle(Rezervasyon rezervasyon);
        void Kaydet();
    }
}
