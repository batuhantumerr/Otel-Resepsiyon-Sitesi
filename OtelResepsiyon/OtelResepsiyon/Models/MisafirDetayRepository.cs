using OtelResepsiyon.Utility;
using System.Linq.Expressions;

namespace OtelResepsiyon.Models
{
    public class MisafirDetayRepository : Repository<MisafirDetay>, IMisafirDetayRepository
    {
        private OtelDbContext _otelDbContext;
        public MisafirDetayRepository(OtelDbContext otelDbContext) : base(otelDbContext)
        {
            _otelDbContext = otelDbContext;
        }

        public void Guncelle(MisafirDetay misafirDetay)
        {
            _otelDbContext.Update(misafirDetay);
        }

        public void Kaydet()
        {
            _otelDbContext.SaveChanges();
        }
    }
}
