namespace OtelResepsiyon.Models
{
    public interface IMisafirDetayRepository : IRepository<MisafirDetay>
    {
        void Guncelle(MisafirDetay misafirDetay);
        void Kaydet();
    }
}
