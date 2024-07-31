using OtelResepsiyon.Utility;
using System.Linq.Expressions;

namespace OtelResepsiyon.Models
{
    public class RezervasyonRepository : Repository<Rezervasyon>, IRezervasyonRepository
    {
        private OtelDbContext _otelDbContext;
        public RezervasyonRepository(OtelDbContext otelDbContext) : base(otelDbContext)
        {
            _otelDbContext = otelDbContext;
        }

        public void Guncelle(Rezervasyon rezervasyon)
        {
            _otelDbContext.Update(rezervasyon);
        }

        public void Kaydet()
        {
            _otelDbContext.SaveChanges();
        }
    }
}
