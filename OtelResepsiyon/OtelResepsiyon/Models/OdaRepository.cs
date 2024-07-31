using OtelResepsiyon.Utility;
using System.Linq.Expressions;

namespace OtelResepsiyon.Models
{
    public class OdaRepository : Repository<Oda>, IOdaRepository
    {
        private OtelDbContext _otelDbContext;
        public OdaRepository(OtelDbContext otelDbContext) : base(otelDbContext)
        {
            _otelDbContext = otelDbContext;
        }

        public void Guncelle(Oda Oda)
        {
            _otelDbContext.Update(Oda);
        }

        public void Kaydet()
        {
            _otelDbContext.SaveChanges();
        }
    }
}
