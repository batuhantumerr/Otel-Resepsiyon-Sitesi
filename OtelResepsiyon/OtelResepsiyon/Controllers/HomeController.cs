
using Microsoft.AspNetCore.Mvc;
using OtelResepsiyon.Models;
using System.Diagnostics;
using System.Timers;
using OtelResepsiyon.Utility;
using System.IO;

namespace OtelResepsiyon.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;
        private readonly IRezervasyonRepository _rezervasyonRepository;
        private readonly IMisafirDetayRepository _misafirDetayRepository;
        private readonly IOdaRepository _odaRepository;

        public HomeController(/*ILogger<HomeController> logger,*/ IRezervasyonRepository rezervasyonRepository, IMisafirDetayRepository misafirDetayRepository,IOdaRepository odaRepository)
        {
            //_logger = logger;
            _rezervasyonRepository = rezervasyonRepository;
            _misafirDetayRepository = misafirDetayRepository;
            _odaRepository = odaRepository;
        }

        public IActionResult Index()
        {
            int toplamKapasite = 0;
            List<Oda> objOdaList = _odaRepository.GetAll().ToList();
            foreach (var oda in objOdaList)
            {
                toplamKapasite += oda.Kapasite;
            }

            List<Rezervasyon> CheckInRezervasyon = _rezervasyonRepository.GetAll().ToList().Where(r => r.GirisTarihi == DateTime.Today && r.Durum == 0).ToList();
            List<Rezervasyon> CheckOutRezervasyon = _rezervasyonRepository.GetAll().ToList().Where(r => r.CikisTarihi == DateTime.Today && r.Durum == 1).ToList();

            ViewBag.BekleyenRezervasyonSayisi = _rezervasyonRepository.GetAll().ToList().Where(r => r.Durum == 0).Count();
            ViewBag.OnaylanmisRezervasyonSayisi = _rezervasyonRepository.GetAll().ToList().Where(r => r.Durum == 1).Count();
            ViewBag.GecmisRezervasyonSayisi = _rezervasyonRepository.GetAll().ToList().Where(r => r.Durum == 2 && (DateTime.Today - r.GirisTarihi).Days < 30).Count();
            ViewBag.IptalRezervasyonSayisi = _rezervasyonRepository.GetAll().ToList().Where(r => r.Durum == 3 && (DateTime.Today - r.GirisTarihi).Days < 30).Count();


            int toplamOdaSayisi = _odaRepository.GetAll().Count();
            int doluOdaSayisi = _odaRepository.GetAll().ToList().Where(o => o.Durum == 1).Count();
            int bosOdaSayisi = toplamOdaSayisi - doluOdaSayisi;
            int dolulukOrani = (int)(((double)doluOdaSayisi / toplamOdaSayisi) * 100);
            ViewBag.DoluOdaSayisi = doluOdaSayisi;
            ViewBag.BosOdaSayisi = bosOdaSayisi;
            ViewBag.DolulukOrani = dolulukOrani;

            ViewBag.AktifKonaklayanSayisi = _misafirDetayRepository.GetAll(includeProps: "Rezervasyon").Where(m => m.Rezervasyon.Durum == 1).Count();
            ViewBag.ToplamKapasite = toplamKapasite;
            ViewBag.ToplamOdaSayisi = toplamOdaSayisi;

            return View(Tuple.Create(CheckInRezervasyon, CheckOutRezervasyon));
        }

        public IActionResult Privacy()
        {
            List<Oda> objOdaList=_odaRepository.GetAll().ToList();
            return View(objOdaList);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}