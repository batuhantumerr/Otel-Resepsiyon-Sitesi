namespace OtelResepsiyon.Models
{
    public interface IOdaRepository : IRepository<Oda>
    {
        void Guncelle(Oda oda);
        void Kaydet();
    }
}
