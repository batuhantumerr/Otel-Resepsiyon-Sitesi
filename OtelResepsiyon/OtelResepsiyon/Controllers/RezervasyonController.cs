using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OtelResepsiyon.Models;
using OtelResepsiyon.Utility;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using System;
using Microsoft.IdentityModel.Tokens;
using Microsoft.CodeAnalysis.Scripting;
using System.Timers;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;


namespace OtelResepsiyon.Controllers
{
    public class RezervasyonController : Controller
    {
        private readonly IRezervasyonRepository _rezervasyonRepository;
        private readonly IOdaRepository _odaRepository;
        private readonly IMisafirDetayRepository _misafirDetayRepository;
        private readonly IMemoryCache _memoryCache;

        public RezervasyonController(IRezervasyonRepository rezervasyonRepository, IOdaRepository odaRepository, IMisafirDetayRepository misafirDetayRepository, IMemoryCache memoryCache)
        {

            _rezervasyonRepository = rezervasyonRepository;
            _odaRepository = odaRepository;
            _misafirDetayRepository = misafirDetayRepository;
            _memoryCache = memoryCache;
        }

        
        public IActionResult Anasayfa()
        {
            //ViewBag.ToplamOdaSayisi=_odaRepository.GetAll().Count();
            //ViewBag.DoluOdaSayisi = _odaRepository.GetAll().ToList().Where(o => o.Durum == 1).Count();
            //ViewBag.BosOdaSayisi=_odaRepository.GetAll().ToList().Where(o=>o.Durum != 1).Count();
            //ViewBag.CikisListe = _rezervasyonRepository.GetAll().ToList().Where(r => r.CikisTarihi == DateTime.Today).ToList();
            return View();
        }
        //public IActionResult Index()
        //{
        //    //Rezervasyon rez = _rezervasyonRepository.Get(r => r.RezervasyonId == (int)_memoryCache.Get("Rezervasyon_id"));
        //    //if (rez.Durum == Rezervasyon.RezervasyonDurum.Gecmis)
        //    //{
        //    //    return RedirectToAction("GecmisRezervasyonlar", "Rezervasyon");
        //    //}
        //if (_memoryCache.Get("GirisTarihi") != null)
        //{
        //    _memoryCache.Remove("GirisTarihi");
        //}
        //if (_memoryCache.Get("CikisTarihi") != null)
        //{
        //    _memoryCache.Remove("CikisTarihi");
        //}
        //if (_memoryCache.Get("Oda_no") != null)
        //{
        //    _memoryCache.Remove("Oda_no");
        //}
        //if (_memoryCache.Get("Rezervasyon_id") != null)
        //{
        //    _memoryCache.Remove("Rezervasyon_id");
        //}
        //if (_memoryCache.Get("RezTarihGuncelleId") != null)
        //{
        //    _memoryCache.Remove("RezTarihGuncelleId");
        //}
        //if (_memoryCache.Get("RezOdaGuncelleId") != null)
        //{
        //    _memoryCache.Remove("RezOdaGuncelleId");
        //}
        //if (_memoryCache.Get("RezBilgiGuncelleId") != null)
        //{
        //    _memoryCache.Remove("RezBilgiGuncelleId");
        //}
        //if (_memoryCache.Get("Misafir_sayisi") != null)
        //{
        //    _memoryCache.Remove("Misafir_sayisi");
        //}
        //    List<Rezervasyon> objRezervasyonList = _rezervasyonRepository.GetAll(includeProps: "Oda").ToList().Where(r=>r.Durum==Rezervasyon.RezervasyonDurum.Aktif|| r.Durum==Rezervasyon.RezervasyonDurum.Kullanimda).ToList();
        //    return View(objRezervasyonList);
        //}

        public IActionResult TarihGir()
        {
            return View();
        }

        [HttpPost]
        public IActionResult BosOdaTipleriGoster(DateTime girisTarihi, DateTime cikisTarihi, int misafirSayisi)
        {
            _memoryCache.Set("GirisTarihi", girisTarihi.ToString());
            _memoryCache.Set("CikisTarihi", cikisTarihi.ToString());
            _memoryCache.Set("MisafirSayisi", misafirSayisi);
            

            // Belirtilen tarih aralığındaki aktif rezervasyonları al
            var aktifRezervasyonlar = _rezervasyonRepository

                .GetAll()
                .Where(r => r.Durum == 0 && r.GirisTarihi < cikisTarihi && r.CikisTarihi > girisTarihi)
                .ToList();

            // Belirtilen tarih aralığındaki kullanımda olan rezervasyonları al
            var kullanilanRezervasyonlar = _rezervasyonRepository
                .GetAll()
                .Where(r => r.Durum == 1 && r.GirisTarihi < cikisTarihi && r.CikisTarihi > girisTarihi)
                .ToList();

            // Tüm odaları al
            var tumOdalar = _odaRepository.GetAll().ToList().Where(o=>o.Kapasite>=misafirSayisi && o.Durum!=3);

            // Kullanılan oda tiplerini ve sayılarını bul
            var kullanilanOdaTipleri = kullanilanRezervasyonlar.Select(r => r.OdaTipi).ToList();

            // Aktif rezervasyonlarda kullanılan oda tiplerini de bul
            var aktifRezervasyonOdaTipleri = aktifRezervasyonlar.Select(r => r.OdaTipi).ToList();

            var odaSayilari = tumOdalar
                    .GroupBy(o => o.OdaTipi)
                    .ToDictionary(grp => grp.Key, grp => new
                    {
                        ToplamOdaSayisi = grp.Count(),
                        AktifRezervasyonSayisi = aktifRezervasyonlar.Count(r => r.OdaTipi == grp.Key),
                        KullanilanRezervasyonSayisi = kullanilanRezervasyonlar.Count(r => r.OdaTipi == grp.Key)
                    });

            // Boş olan oda tiplerini, fiyatlarını ve sayılarını hesapla
            var bosOdaBilgileri = tumOdalar
                .GroupBy(o => new { o.OdaTipi, o.Fiyat, o.Alan})
                .Select(grp => new Oda
                {
                    OdaTipi = grp.Key.OdaTipi,
                    Fiyat = grp.Key.Fiyat,
                    Alan = grp.Key.Alan,
                    BosOdaSayisi = Math.Max(0, odaSayilari[grp.Key.OdaTipi].ToplamOdaSayisi -
                                                  odaSayilari[grp.Key.OdaTipi].AktifRezervasyonSayisi -
                                                  odaSayilari[grp.Key.OdaTipi].KullanilanRezervasyonSayisi)
                })
                .Where(bosOda => bosOda.BosOdaSayisi > 0) // BosOdaSayisi sıfırdan büyükse ekle
                .ToList();

            // BosOdaBilgileriViewModel listesini ekrana gönder
            return View(bosOdaBilgileri);
        }

        //var bosOdalar = new List<Oda>();
        //List<Oda> objOdaList = _odaRepository.GetAll().ToList().Where(o => o.Kapasite >= misafirSayisi).ToList();
        //List<Rezervasyon> tarihlerAraligindaRezervasyonlar = _rezervasyonRepository.GetAll(includeProps: "Oda").Where(o => o.GirisTarihi < cikisTarihi && o.CikisTarihi > girisTarihi && (o.Durum == 0 || o.Durum==1))
        //.ToList();

        //// Belirtilen tarih aralığındaki aktif ve kullanımda olan rezervasyonlara ait oda tiplerini ve fiyatlarını al
        //var rezervasyonaAitOdaTipleri = tarihlerAraligindaRezervasyonlar
        //    .Select(r => new { OdaTipi = r.OdaTipi})
        //    .Distinct()
        //    .ToList();

        //// Kullanılmayan oda tiplerini, fiyatlarını ve sayılarını hesapla
        //var bosOdaTipleri = objOdaList
        //    .Where(o => !rezervasyonaAitOdaTipleri.Any(r => r.OdaTipi == o.OdaTipi ))
        //    .GroupBy(o => new { o.OdaTipi })
        //    .Select(grp => new Oda
        //    {
        //        OdaTipi = grp.Key.OdaTipi,
        //        BosOdaSayisi = grp.Count()
        //    })
        //    .ToList();


        //foreach (var oda in objOdaList)
        //{
        //    bool odaMusaitlik = tarihlerAraligindaRezervasyonlar.All(r => r.OdaNo != oda.OdaNo);

        //    if (odaMusaitlik)
        //    {
        //        bosOdalar.Add(oda);
        //    }
        //}

        // Belirtilen tarih aralığındaki aktif rezervasyonlara ait oda tiplerini al
        //var rezervasyonaAitOdaTipleri = tarihlerAraligindaRezervasyonlar
        //    .Select(r => new { OdaTipi = r.OdaTipi, Fiyat = r.Oda.Fiyat, Alan=r.Oda.Alan })
        //    .Distinct()
        //    .ToList();

        //// Kullanılmayan oda tiplerini ve sayılarını hesapla
        //var bosOdaTipleri = objOdaList
        //    .Where(o => !rezervasyonaAitOdaTipleri.Any(r => r.OdaTipi == o.OdaTipi && r.Fiyat == o.Fiyat && r.Alan==o.Alan))
        //    .GroupBy(o => new { o.OdaTipi, o.Fiyat,o.Alan })
        //    .Select(grp => new Oda
        //    {
        //        OdaTipi = grp.Key.OdaTipi,
        //        Fiyat=grp.Key.Fiyat,
        //        Alan=grp.Key.Alan,
        //        BosOdaSayisi = grp.Count()
        //    })
        //    .ToList();


        // List<Oda> KullanilmayanOdaTipleri = bosOdalar
        //.GroupBy(o => o.OdaTipi) // Oda tiplerine göre grupla
        //.Select(grp => new Oda
        //{
        //    OdaTipi = grp.Key, // Oda tipini ata
        //    Fiyat = grp.FirstOrDefault()?.Fiyat ?? 0,// İlk odanın fiyatını al (Varsayılan değer olarak 0) 
        //    BosOdaSayisi = grp.Count()
        //})
        //.ToList();

        // List<Rezervasyon> aktifRezervasyonlar=_rezervasyonRepository.GetAll().ToList().Where(r => r.Durum == 0).ToList();

        // var odaTipineGoreRezervasyonSayisi = aktifRezervasyonlar
        // .GroupBy(r => r.OdaTipi) // Oda tiplerine göre grupla
        // .Select(grp => new{
        // OdaTipi = grp.Key, // Oda tipi
        // RezervasyonSayisi = grp.Count() // Grup içindeki rezervasyon sayısı
        // })
        // .ToList();

        // List<Oda> bosOdaTipleri = KullanilmayanOdaTipleri
        // .Select(oda => new Oda{
        // OdaTipi=oda.OdaTipi,
        // BosOdaSayisi = oda.BosOdaSayisi - odaTipineGoreRezervasyonSayisi.FirstOrDefault(r => r.OdaTipi == oda.OdaTipi)?.RezervasyonSayisi ?? 0
        // })
        // .Where(bosOda => bosOda.BosOdaSayisi > 0)
        // .ToList();

        //return View(bosOdaTipleri);
    //}


    //[HttpPost]
    //public IActionResult BosOdaTipleriGoster(DateTime girisTarihi, DateTime cikisTarihi, int misafirSayisi)
    //{
    //    _memoryCache.Set("GirisTarihi", girisTarihi.ToString());
    //    _memoryCache.Set("CikisTarihi", cikisTarihi.ToString());
    //    _memoryCache.Set("MisafirSayisi", misafirSayisi);
    //    var bosOdalar = new List<Oda>();
    //    List<Oda> objOdaList = _odaRepository.GetAll().ToList().Where(o => o.Kapasite >= misafirSayisi).ToList();
    //    List<Rezervasyon> tarihlerAraligindaRezervasyonlar = _rezervasyonRepository.GetAll(includeProps: "Oda").Where(o => o.GirisTarihi < cikisTarihi && o.CikisTarihi > girisTarihi && (o.Durum == 0 || o.Durum == 1)).ToList();

    //    foreach (var oda in objOdaList)
    //    {
    //        bool odaMusaitlik = tarihlerAraligindaRezervasyonlar.All(r => r.OdaNo != oda.OdaNo);

    //        if (odaMusaitlik)
    //        {
    //            bosOdalar.Add(oda);
    //        }
    //    }
    //    List<Oda> bosOdaTipleri = bosOdalar
    //   .GroupBy(o => o.OdaTipi) // Oda tiplerine göre grupla
    //   .Select(grp => new Oda
    //   {
    //       OdaTipi = grp.Key, // Oda tipini ata
    //       Fiyat = grp.FirstOrDefault()?.Fiyat ?? 0 ,// İlk odanın fiyatını al (Varsayılan değer olarak 0)
    //       BosOdaSayisi=grp.Count()
    //   })
    //   .ToList();

    //    return View(bosOdaTipleri);
    //}
        public IActionResult BosOdalariGoster(int rezervasyonId)
        {
            _memoryCache.Set("Oda_sec_rez_id", rezervasyonId);
            Rezervasyon ilgiliRezervasyon = _rezervasyonRepository.Get(r => r.RezervasyonId==rezervasyonId);                      
            var bosOdalar = new List<Oda>();
            List<Oda> objOdaList = _odaRepository.GetAll().ToList().Where(o => o.OdaTipi==ilgiliRezervasyon.OdaTipi.Trim() && (o.Durum==0 || o.Durum==2)).ToList();
            List<Rezervasyon> tarihlerAraligindaRezervasyonlar = _rezervasyonRepository.GetAll(includeProps: "Oda").Where(r => r.GirisTarihi < ilgiliRezervasyon.CikisTarihi && r.CikisTarihi > ilgiliRezervasyon.GirisTarihi && r.Durum == 1).ToList();

            foreach (var oda in objOdaList)
            {
                bool odaMusaitlik = tarihlerAraligindaRezervasyonlar.All(r => r.OdaNo != oda.OdaNo);

                if (odaMusaitlik)
                {
                    bosOdalar.Add(oda);
                }
            }
            return View(bosOdalar);
        }

        //public IActionResult BosOdalariGoster(DateTime girisTarihi, DateTime cikisTarihi, int misafirSayisi)
        //{
        //    _memoryCache.Set("GirisTarihi", girisTarihi.ToString());
        //    _memoryCache.Set("CikisTarihi", cikisTarihi.ToString());
        //    _memoryCache.Set("MisafirSayisi", misafirSayisi);
        //    var bosOdalar = new List<Oda>();
        //    List<Oda> objOdaList = _odaRepository.GetAll().ToList().Where(o => o.Kapasite >= misafirSayisi).ToList();
        //    List<Rezervasyon> tarihlerAraligindaRezervasyonlar = _rezervasyonRepository.GetAll(includeProps: "Oda").Where(o => o.GirisTarihi < cikisTarihi && o.CikisTarihi > girisTarihi && (o.Durum == 0 || o.Durum == 1)).ToList();

        //    foreach (var oda in objOdaList)
        //    {
        //        bool odaMusaitlik = tarihlerAraligindaRezervasyonlar.All(r => r.OdaNo != oda.OdaNo);

        //        if (odaMusaitlik)
        //        {
        //            bosOdalar.Add(oda);
        //        }
        //    }
        //    return View(bosOdalar);
        //}

        public IActionResult RezBosOdalariGoster(int rezervasyonId)
        {
            Rezervasyon ilgiliRezervasyon = _rezervasyonRepository.Get(r => r.RezervasyonId==rezervasyonId);
            _memoryCache.Set("GirisTarihi", ilgiliRezervasyon.GirisTarihi.ToString());
            _memoryCache.Set("CikisTarihi", ilgiliRezervasyon.CikisTarihi.ToString());
            _memoryCache.Set("Misafir_sayisi", ilgiliRezervasyon.MisafirSayisi);
            var rezbosOdalar = new List<Oda>();
            List<Oda> objOdaList = _odaRepository.GetAll().ToList().Where(o => o.Kapasite >= ilgiliRezervasyon.MisafirSayisi && o.OdaTipi==ilgiliRezervasyon.OdaTipi).ToList();
            List<Rezervasyon> reztarihlerAraligindaRezervasyonlar = _rezervasyonRepository.GetAll(includeProps: "Oda").Where(o => o.GirisTarihi < ilgiliRezervasyon.CikisTarihi && o.CikisTarihi > ilgiliRezervasyon.GirisTarihi).ToList();

            foreach (var oda in objOdaList)
            {
                bool odaMusaitlik = reztarihlerAraligindaRezervasyonlar.All(r => r.OdaNo != oda.OdaNo);

                if (odaMusaitlik)
                {
                    rezbosOdalar.Add(oda);
                }
            }
            return View(rezbosOdalar);
        }

        public IActionResult OdaSec(int odaNo)
        {
            _memoryCache.Set("Oda_no", odaNo);
            return RedirectToAction("RezervasyonEkle","Rezervasyon");
        }

        public IActionResult OdaTipiSec(string odaTipi)
        {
            _memoryCache.Set("Oda_tipi",odaTipi);
            return RedirectToAction("RezervasyonEkle", "Rezervasyon");
        }


        public IActionResult RezOdaTipiSec(string odaTipi)
        {
            _memoryCache.Set("Oda_tipi", odaTipi);
             return RezervasyonOdaTipiGuncelle(odaTipi);
        }
        public IActionResult GuncelMisafirler()
        {
            List<MisafirDetay> objGuncelMisafirList=_misafirDetayRepository.GetAll(includeProps:"Rezervasyon").Where(m=>m.Rezervasyon.Durum==1).ToList();
            return View(objGuncelMisafirList);
                
        }

        public IActionResult GecmisMisafirler()
        {
            List<MisafirDetay> objGecmisMisafirList=_misafirDetayRepository.GetAll(includeProps:"Rezervasyon").Where(m=>m.Rezervasyon.Durum==2).ToList();
            return View(objGecmisMisafirList);
            
        }

        public IActionResult MisafirEkle()
        {
            return View();
        }

        [HttpPost]
        public IActionResult MisafirEkle(MisafirDetay misafirDetay)
        {
            if (ModelState.IsValid)
            {
                misafirDetay.RezervasyonId = (int)_memoryCache.Get("Rezervasyon_id");
                _misafirDetayRepository.Ekle(misafirDetay);
                _misafirDetayRepository.Kaydet();
                return Redirect("/Rezervasyon/MisafirleriGoster?RezervasyonId=" + misafirDetay.RezervasyonId);
            }

            return View();
        }

        public IActionResult MisafirGuncelle(int id)
        {  
            MisafirDetay guncellenecekMisafir=_misafirDetayRepository.Get(m=>m.Id==id);
            return guncellenecekMisafir == null ? NotFound() : View(guncellenecekMisafir);
        }

        [HttpPost]
        public IActionResult MisafirGuncelle(MisafirDetay misafir)
        {
            if (ModelState.IsValid)
            {
                _misafirDetayRepository.Guncelle(misafir);
                _misafirDetayRepository.Kaydet();
                return RedirectToAction("MisafirleriGoster", "Rezervasyon", new { RezervasyonId = misafir.RezervasyonId });
            }
            return View();
        }

        public IActionResult MisafirSil(int id)
        {
            MisafirDetay silinecekMisafir=_misafirDetayRepository.Get(m=>m.Id == id);
            _misafirDetayRepository.Sil(silinecekMisafir);
            _misafirDetayRepository.Kaydet();
            return RedirectToAction("KullanimdaRezervasyonlar", "Rezervasyon");
        }

        public IActionResult MisafirleriGoster(int rezervasyonId)
        {
           _memoryCache.Set("Rezervasyon_id", rezervasyonId);
            ViewBag.RezervasyonId=rezervasyonId;
           var odadakiMisafirler = new List<MisafirDetay>();
           List<MisafirDetay> odadakiMisafirlerId=_misafirDetayRepository.GetAll().ToList().Where(r=>r.RezervasyonId == rezervasyonId).ToList();
           foreach(var misafir in odadakiMisafirlerId)
           { 
                odadakiMisafirler.Add(misafir); 
           }    
            return View(odadakiMisafirler);
        }
        public IActionResult GecmisMisafirleriGoster(int rezervasyonId)
        {
            _memoryCache.Set("Rezervasyon_id", rezervasyonId);
            var odadakiMisafirler = new List<MisafirDetay>();
            List<MisafirDetay> odadakiMisafirlerId = _misafirDetayRepository.GetAll().ToList().Where(r => r.RezervasyonId == rezervasyonId).ToList();
            foreach (var misafir in odadakiMisafirlerId)
            {
                odadakiMisafirler.Add(misafir);
            }

            return View(odadakiMisafirler);
        }


        public IActionResult AktifRezervasyonlar()
        {
            if (_memoryCache.Get("GirisTarihi") != null)
            {
                _memoryCache.Remove("GirisTarihi");
            }
            if (_memoryCache.Get("CikisTarihi") != null)
            {
                _memoryCache.Remove("CikisTarihi");
            }
            if (_memoryCache.Get("Misafir_sayisi") != null)
            {
                _memoryCache.Remove("Misafir_sayisi");
            }
            if (_memoryCache.Get("Oda_no") != null)
            {
                _memoryCache.Remove("Oda_no");
            }
            if (_memoryCache.Get("Oda_sec_rez_id") != null)
            {
                _memoryCache.Remove("Oda_sec_rez_id");
            }
            List<Rezervasyon> objAktifRezervasyonList = _rezervasyonRepository.GetAll(includeProps: "Oda").ToList().Where(r => r.Durum == 0).ToList();
            return View(objAktifRezervasyonList);
        }

        public IActionResult KullanimdaRezervasyonlar()
        {
            if (_memoryCache.Get("Rezervasyon_id") != null)
            {
                _memoryCache.Remove("Rezervasyon_id");
            }
            List<Rezervasyon> objKullanimdaRezervasyonList = _rezervasyonRepository.GetAll(includeProps: "Oda").ToList().Where(r => r.Durum == 1).ToList();
            return View(objKullanimdaRezervasyonList);
        }

        public IActionResult GecmisRezervasyonlar()
        {
            if (_memoryCache.Get("Rezervasyon_id") != null)
            {
                _memoryCache.Remove("Rezervasyon_id");
            }
            List<Rezervasyon> gecmisRezervasyonlar = _rezervasyonRepository.GetAll().ToList().Where(r => r.Durum == 2).ToList();
            return View(gecmisRezervasyonlar);
        }

        public IActionResult IptalRezervasyonlar()
        {
            List<Rezervasyon> iptalRezervasyonlar = _rezervasyonRepository.GetAll().ToList().Where(r => r.Durum ==3).ToList();
            return View(iptalRezervasyonlar);
        }

        public IActionResult RezervasyonEkle()
        {
            ViewBag.GirisTarihi = _memoryCache.Get("GirisTarihi");
            ViewBag.CikisTarihi = _memoryCache.Get("CikisTarihi");
            ViewBag.MisafirSayisi = _memoryCache.Get("MisafirSayisi");

            return View();
        }

        [HttpPost]
        public JsonResult RezervasyonEkle(string telefon,string ad_soyad,int odenenTutar)
        {
            
            if (ModelState.IsValid)
            {
                Rezervasyon rezervasyon = new Rezervasyon();
                rezervasyon.GirisTarihi = DateTime.Parse((string)_memoryCache.Get("GirisTarihi"));
                rezervasyon.CikisTarihi = DateTime.Parse((string)_memoryCache.Get("CikisTarihi"));
                rezervasyon.MisafirSayisi = (int)_memoryCache.Get("MisafirSayisi");
                rezervasyon.OdaTipi = (string)_memoryCache.Get("Oda_tipi");
                rezervasyon.Telefon=telefon;
                rezervasyon.Ad_Soyad = ad_soyad;
                rezervasyon.Durum = 0;
                rezervasyon.OdenenTutar = odenenTutar;
                TimeSpan fark = rezervasyon.CikisTarihi - rezervasyon.GirisTarihi;
                int gunSayisi = fark.Days;
                int odaFiyat = _odaRepository.Get(o => o.OdaTipi == rezervasyon.OdaTipi).Fiyat;
                rezervasyon.ToplamTutar = odaFiyat * gunSayisi;
                _rezervasyonRepository.Ekle(rezervasyon);
                _rezervasyonRepository.Kaydet();


                return Json(new { success = true });
            }
            return Json(new { success = false });


        }

        public IActionResult RezervasyonCheckIn(int odaNo)
        {
            Rezervasyon ilgiliRezervasyon = _rezervasyonRepository.Get(r => r.RezervasyonId == (int)_memoryCache.Get("Oda_sec_rez_id") && r.Durum == 0);
            ilgiliRezervasyon.Durum = 1;
            ilgiliRezervasyon.OdaNo = odaNo;
            Oda ilgiliOda = _odaRepository.Get(o => o.OdaNo == odaNo);
            ilgiliOda.Durum = 1;
            _odaRepository.Kaydet();
            _rezervasyonRepository.Kaydet();
            return RedirectToAction("KullanimdaRezervasyonlar", "Rezervasyon");
        }

        public IActionResult RezervasyonCheckOut(int rezervasyonId)
        {
            Rezervasyon ilgiliRezervasyon = _rezervasyonRepository.Get(r => r.RezervasyonId == rezervasyonId); 
            ilgiliRezervasyon.Durum = 2;
            Oda ilgiliOda = _odaRepository.Get(o => o.OdaNo == ilgiliRezervasyon.OdaNo);
            ilgiliOda.Durum = 2;
            ilgiliRezervasyon.CikisTarihi = DateTime.Today;
            _odaRepository.Kaydet();
            _rezervasyonRepository.Kaydet();
            return RedirectToAction("KullanimdaRezervasyonlar", "Rezervasyon");
        }

        

        public bool OdaBosMu(int odaNo,DateTime girisTarihi,DateTime cikisTarihi)
        {
            Oda ilgiliOda=_odaRepository.Get(o => o.OdaNo == odaNo);
            int ilgiliOdaTipiSayisi=_odaRepository.GetAll().ToList().Where(o => o.OdaTipi == ilgiliOda.OdaTipi && o.Durum != 3).Count();
            int ilgiliOdaTipiAktifRezervasyonSayisi = _rezervasyonRepository.GetAll().ToList().Where(r => r.OdaTipi == ilgiliOda.OdaTipi && r.GirisTarihi < cikisTarihi && r.CikisTarihi > girisTarihi && r.Durum == 0).Count();
            return ilgiliOdaTipiSayisi - ilgiliOdaTipiAktifRezervasyonSayisi > 0 ? true : false;
        }

        public bool OdaTipiBosMu(string odaTipi,DateTime girisTarihi,DateTime cikisTarihi)
        {
            List<Rezervasyon> ilgiliOdaTipiRezervasyonlar=_rezervasyonRepository.GetAll().ToList().Where(r=>r.OdaTipi==odaTipi && r.GirisTarihi < cikisTarihi && r.CikisTarihi > girisTarihi && r.RezervasyonId != (int)_memoryCache.Get("RezTarihGuncelleId") && (r.Durum == 0 || r.Durum == 1)).ToList();
            int odaTipiSayisi= _odaRepository.GetAll().ToList().Where(o => o.OdaTipi == odaTipi && o.Durum!=3).Count();
            return ilgiliOdaTipiRezervasyonlar.Count()<odaTipiSayisi ? true : false;
        }
        public IActionResult GeriDon()
        {
            if ((int)_memoryCache.Get("RezDurum") == 0)
            {
                return RedirectToAction("AktifRezervasyonlar", "Rezervasyon");
            }
            else if ((int)_memoryCache.Get("RezDurum") == 1)
            {
                return RedirectToAction("KullanimdaRezervasyonlar");
            }
            return RedirectToAction("Anasayfa", "Rezervasyon");
        }

        public IActionResult RezervasyonTarihGuncelle(int rezervasyonId)
        {
            if (_memoryCache.Get("RezTarihGuncelleId") != null)
            {
                _memoryCache.Remove("RezTarihGuncelleId");
            }
            if (_memoryCache.Get("RezDurum") != null)
            {
                _memoryCache.Remove("RezDurum");
            }
            _memoryCache.Set("RezDurum", _rezervasyonRepository.Get(r => r.RezervasyonId == rezervasyonId).Durum);
            _memoryCache.Set("RezTarihGuncelleId", rezervasyonId);
            Rezervasyon guncellenecekRezervasyon=_rezervasyonRepository.Get(r=>r.RezervasyonId==rezervasyonId);
            return View(guncellenecekRezervasyon);
        }  
        
        [HttpPost]
        public  IActionResult RezervasyonTarihGuncelle(DateTime GirisTarihi,DateTime CikisTarihi)
        {
            Rezervasyon ilgiliRezervasyon = _rezervasyonRepository.Get(r => r.RezervasyonId == (int)_memoryCache.Get("RezTarihGuncelleId"));
            if (ilgiliRezervasyon.Durum == 0)
            {
                if (OdaTipiBosMu(ilgiliRezervasyon.OdaTipi, GirisTarihi, CikisTarihi))
                {
                    ilgiliRezervasyon.GirisTarihi = GirisTarihi;
                    ilgiliRezervasyon.CikisTarihi = CikisTarihi;
                    TimeSpan fark = ilgiliRezervasyon.CikisTarihi - ilgiliRezervasyon.GirisTarihi;
                    int gunSayisi = fark.Days;
                    int odaFiyat = _odaRepository.Get(o => o.OdaTipi == ilgiliRezervasyon.OdaTipi).Fiyat;
                    ilgiliRezervasyon.ToplamTutar = odaFiyat * gunSayisi;
                    _rezervasyonRepository.Kaydet();
                    return RedirectToAction("AktifRezervasyonlar", "Rezervasyon");
                }
            }
            else if (ilgiliRezervasyon.Durum == 1)
            {
                if (OdaBosMu(ilgiliRezervasyon.OdaNo.Value, GirisTarihi, CikisTarihi))
                {
                    ilgiliRezervasyon.GirisTarihi = GirisTarihi;
                    ilgiliRezervasyon.CikisTarihi = CikisTarihi;
                    TimeSpan fark = ilgiliRezervasyon.CikisTarihi - ilgiliRezervasyon.GirisTarihi;
                    int gunSayisi = fark.Days;
                    ilgiliRezervasyon.ToplamTutar = ilgiliRezervasyon.Oda.Fiyat * gunSayisi;
                    _rezervasyonRepository.Kaydet();
                    return RedirectToAction("KullanimdaRezervasyonlar", "Rezervasyon");
                }
            }
            else
            {
                //Hata mesajı

            }
            return RedirectToAction("GeriDon", "Rezervasyon");
    }

        //public IActionResult RezBosOdalar(int rezervasyonId)
        //{
        //    if (_memoryCache.Get("RezOdaGuncelleId") != null)
        //    {
        //        _memoryCache.Remove("RezOdaGuncelleId");
        //    }
        //    _memoryCache.Set("RezOdaGuncelleId", rezervasyonId);
        //    Rezervasyon ilgiliRezervasyon = _rezervasyonRepository.Get(r => r.RezervasyonId == rezervasyonId);
        //    //RezBosOdalariGoster(ilgiliRezervasyon.GirisTarihi, ilgiliRezervasyon.CikisTarihi, ilgiliRezervasyon.MisafirSayisi);
        //    var rezbosOdalar = new List<Oda>();
        //    List<Oda> objOdaList = _odaRepository.GetAll().ToList().Where(o => o.Kapasite >= ilgiliRezervasyon.MisafirSayisi).ToList();
        //    List<Rezervasyon> reztarihlerAraligindaRezervasyonlar = _rezervasyonRepository.GetAll(includeProps: "Oda").Where(o => o.GirisTarihi < ilgiliRezervasyon.CikisTarihi && o.CikisTarihi > ilgiliRezervasyon.GirisTarihi && (o.Durum == 0 || o.Durum == 1)).ToList();

        //    foreach (var oda in objOdaList)
        //    {
        //        bool odaMusaitlik = reztarihlerAraligindaRezervasyonlar.All(r => r.OdaNo != oda.OdaNo);

        //        if (odaMusaitlik)
        //        {
        //            rezbosOdalar.Add(oda);
        //        }
        //    }
        //    return View(rezbosOdalar);
        //}

        public IActionResult RezBosOdaTipleri(int rezervasyonId)
        {
            if (_memoryCache.Get("RezOdaTipiGuncelleId") != null)
            {
                _memoryCache.Remove("RezOdaTipiGuncelleId");
            }
            _memoryCache.Set("RezOdaTipiGuncelleId", rezervasyonId);
            Rezervasyon ilgiliRezervasyon = _rezervasyonRepository.Get(r => r.RezervasyonId == rezervasyonId);
            //RezBosOdalariGoster(ilgiliRezervasyon.GirisTarihi, ilgiliRezervasyon.CikisTarihi, ilgiliRezervasyon.MisafirSayisi);
            // Belirtilen tarih aralığındaki aktif rezervasyonları al
            var aktifRezervasyonlar = _rezervasyonRepository
                .GetAll()
                .Where(r => r.Durum == 0 && r.GirisTarihi < ilgiliRezervasyon.CikisTarihi && r.CikisTarihi > ilgiliRezervasyon.GirisTarihi)
                .ToList();

            // Belirtilen tarih aralığındaki kullanımda olan rezervasyonları al
            var kullanilanRezervasyonlar = _rezervasyonRepository
                .GetAll()
                .Where(r => r.Durum == 1 && r.GirisTarihi < ilgiliRezervasyon.CikisTarihi && r.CikisTarihi > ilgiliRezervasyon.GirisTarihi)
                .ToList();

            // Tüm odaları al
            var tumOdalar = _odaRepository.GetAll().ToList().Where(o => o.Kapasite >= ilgiliRezervasyon.MisafirSayisi && o.Durum!=3);

            // Kullanılan oda tiplerini ve sayılarını bul
            var kullanilanOdaTipleri = kullanilanRezervasyonlar.Select(r => r.OdaTipi).ToList();

            // Aktif rezervasyonlarda kullanılan oda tiplerini de bul
            var aktifRezervasyonOdaTipleri = aktifRezervasyonlar.Select(r => r.OdaTipi).ToList();

            var odaSayilari = tumOdalar
                    .GroupBy(o => o.OdaTipi)
                    .ToDictionary(grp => grp.Key, grp => new
                    {
                        ToplamOdaSayisi = grp.Count(),
                        AktifRezervasyonSayisi = aktifRezervasyonlar.Count(r => r.OdaTipi == grp.Key),
                        KullanilanRezervasyonSayisi = kullanilanRezervasyonlar.Count(r => r.OdaTipi == grp.Key)
                    });

            // Boş olan oda tiplerini, fiyatlarını ve sayılarını hesapla
            var bosOdaBilgileri = tumOdalar
                .GroupBy(o => new { o.OdaTipi, o.Fiyat ,o.Alan})
                .Select(grp => new Oda
                {
                    OdaTipi = grp.Key.OdaTipi,
                    Fiyat = grp.Key.Fiyat,
                    Alan= grp.Key.Alan,
                    BosOdaSayisi = Math.Max(0, odaSayilari[grp.Key.OdaTipi].ToplamOdaSayisi -
                                                  odaSayilari[grp.Key.OdaTipi].AktifRezervasyonSayisi -
                                                  odaSayilari[grp.Key.OdaTipi].KullanilanRezervasyonSayisi)
                })
                .Where(bosOda => bosOda.BosOdaSayisi > 0) // BosOdaSayisi sıfırdan büyükse ekle
                .ToList();
            // BosOdaBilgileriViewModel listesini ekrana gönder
            return View(bosOdaBilgileri);
        }

        public IActionResult RezervasyonOdaTipiGuncelle(string odaTipi)
        {
            Rezervasyon ilgiliRezervasyon = _rezervasyonRepository.Get(r => r.RezervasyonId == (int)_memoryCache.Get("RezOdaTipiGuncelleId"));
            ilgiliRezervasyon.OdaTipi= odaTipi;
            TimeSpan fark = ilgiliRezervasyon.CikisTarihi - ilgiliRezervasyon.GirisTarihi;
            int gunSayisi = fark.Days;
            ilgiliRezervasyon.ToplamTutar = _odaRepository.Get(r=>r.OdaTipi==odaTipi).Fiyat * gunSayisi;
            _rezervasyonRepository.Kaydet();
            return RedirectToAction("AktifRezervasyonlar", "Rezervasyon");
        }

        public IActionResult RezervasyonOdaGuncelle(int odaNo)
        {
            Rezervasyon ilgiliRezervasyon = _rezervasyonRepository.Get(r => r.RezervasyonId == (int)_memoryCache.Get("RezOdaGuncelleId"));
            ilgiliRezervasyon.OdaNo = odaNo;
            TimeSpan fark = ilgiliRezervasyon.CikisTarihi - ilgiliRezervasyon.GirisTarihi;
            int gunSayisi = fark.Days;
            ilgiliRezervasyon.ToplamTutar = _odaRepository.Get(r => r.OdaNo == odaNo).Fiyat * gunSayisi;
            _rezervasyonRepository.Kaydet();
            return RedirectToAction("KullanimdaRezervasyonlar", "Rezervasyon");
        }



        public IActionResult RezervasyonBilgiGuncelle(int rezervasyonId)
        {
            if (_memoryCache.Get("RezBilgiGuncelleId") != null)
            {
                _memoryCache.Remove("RezBilgiGuncelleId");
            }
            if (_memoryCache.Get("RezDurum") != null)
            {
                _memoryCache.Remove("RezDurum");
            }
            _memoryCache.Set("RezDurum", _rezervasyonRepository.Get(r => r.RezervasyonId == rezervasyonId).Durum);
            _memoryCache.Set("RezBilgiGuncelleId", rezervasyonId);
            Rezervasyon guncellenecekRezervasyon = _rezervasyonRepository.Get(r => r.RezervasyonId == rezervasyonId);
            return View(guncellenecekRezervasyon);
        }
        [HttpPost]
        public IActionResult RezervasyonBilgiGuncelle(int misafirSayisi,string telefon,string ad_soyad,string aracPlaka,int odenenTutar)
        {
            Rezervasyon ilgiliRezervasyon = _rezervasyonRepository.Get(r => r.RezervasyonId == (int)_memoryCache.Get("RezBilgiGuncelleId"));
            ilgiliRezervasyon.MisafirSayisi = misafirSayisi;
            ilgiliRezervasyon.Telefon = telefon;
            ilgiliRezervasyon.Ad_Soyad=ad_soyad;
            ilgiliRezervasyon.AracPlaka = aracPlaka;
            ilgiliRezervasyon.OdenenTutar= odenenTutar;
            _rezervasyonRepository.Kaydet();
            return RedirectToAction("GeriDon", "Rezervasyon");

        }  
        
        //public IActionResult RezervasyonGuncelle(int rezervasyonId)
        //{
            
        //    Rezervasyon guncellenecekRezervasyon = _rezervasyonRepository.Get(r => r.RezervasyonId==rezervasyonId);
        //    return guncellenecekRezervasyon == null ? NotFound() : View(guncellenecekRezervasyon);
        //}

        //[HttpPost]
        //public IActionResult RezervasyonGuncelle(Rezervasyon rezervasyon)
        //{
        //    _rezervasyonRepository.Guncelle(rezervasyon);
        //    _rezervasyonRepository.Kaydet();
        //    return RedirectToAction("Index", "Rezervasyon");
        //}
        //public IActionResult RezervasyonIptal(int rezervasyonId)
        //{
        //    Rezervasyon iptalRezervasyon = _rezervasyonRepository.Get(m => m.RezervasyonId == rezervasyonId);
        //    return View(iptalRezervasyon);
        //}

        //[HttpPost, ActionName("RezervasyonIptal")]
        public IActionResult RezervasyonIptal(int rezervasyonId)
        {
            Rezervasyon iptalRezervasyon = _rezervasyonRepository.Get(m => m.RezervasyonId == rezervasyonId);
            if (iptalRezervasyon == null)
            {
                return NotFound();
            }
            iptalRezervasyon.Durum = 3;
            _rezervasyonRepository.Kaydet();
            return RedirectToAction("IptalRezervasyonlar","Rezervasyon");
        }
    }
}
