using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Migrations;
using OtelResepsiyon.Models;
using OtelResepsiyon.Utility;
using System.Security.Cryptography;

namespace OtelResepsiyon.Controllers
{
    public class OdaController : Controller
    {
        private readonly IOdaRepository _OdaRepository;

        public OdaController(IOdaRepository context)
        {
            _OdaRepository = context;
        }


        public IActionResult Index(int? minFiyat, int? maxFiyat, int? kapasite)
        {
            ViewBag.MinFiyat = minFiyat;
            ViewBag.MaxFiyat = maxFiyat;
            ViewBag.Kapasite = kapasite;
            List<Oda> objOdaList = _OdaRepository.GetAll().ToList();
            if (minFiyat != null && maxFiyat != null)
            {
                objOdaList = objOdaList.Where(o => minFiyat <= o.Fiyat && o.Fiyat <= maxFiyat).ToList();
            }
            if (kapasite != null)
            {
                objOdaList = objOdaList.Where(o => kapasite <= o.Kapasite).ToList();
            }

            return View(objOdaList);
        }
        public IActionResult OdaEkle()
        {
            return View();
        }
        [HttpPost]
        public IActionResult OdaEkle(Oda oda)
        {
            oda.Durum = 0;
            _OdaRepository.Ekle(oda);
            _OdaRepository.Kaydet();
            TempData["basarili"] = "Yeni Oda Başarıyla Eklendi";
            return RedirectToAction("Index", "Oda");
        }

        public IActionResult OdaBilgi(int odaNo)
        {
            Oda objOda= _OdaRepository.Get(o=>o.OdaNo == odaNo);
            return View(objOda);
        }
        public IActionResult OdaTemizlik(int odaNo)
        {
            Oda temizlenenOda=_OdaRepository.Get(o=>o.OdaNo == odaNo);
            temizlenenOda.Durum = 0;
            _OdaRepository.Kaydet();
            return RedirectToAction("Index", "Oda");
        }
        public IActionResult OdaKullanimaKapat(int odaNo)
        {
            Oda kapatilacakOda = _OdaRepository.Get(o => o.OdaNo == odaNo);
            if (kapatilacakOda == null)
            {
                return NotFound();
            }
            kapatilacakOda.Durum = 3;
            _OdaRepository.Kaydet();
            return RedirectToAction("Index", "Oda");
        }
        public IActionResult OdaKullanimaAc(int odaNo)
        {
            Oda acilacakOda = _OdaRepository.Get(o => o.OdaNo == odaNo);
            if (acilacakOda == null)
            {
                return NotFound();
            }
            acilacakOda.Durum = 0;
            _OdaRepository.Kaydet();
            return RedirectToAction("Index", "Oda");
        }

        public IActionResult OdaGuncelle(int? odaNo)
        {
            Oda guncellenecekOda = _OdaRepository.Get(o => o.OdaNo==odaNo);
            if (guncellenecekOda == null)
            {
                return NotFound();
            }
            return View(guncellenecekOda);
        }
        [HttpPost]
        public IActionResult OdaGuncelle(Oda oda)
        {
            if (ModelState.IsValid)
            {
                _OdaRepository.Guncelle(oda);
                _OdaRepository.Kaydet();
                TempData["basarili"] = "Oda Başarıyla Güncellendi";
                return RedirectToAction("Index","Oda");
            }
            return View();
        }

        public IActionResult OdaSil(int odaNo)
        {
            Oda silinecekOda = _OdaRepository.Get(o => o.OdaNo == odaNo);
            if (silinecekOda == null)
            {
                return NotFound();
            }
            _OdaRepository.Sil(silinecekOda);
            _OdaRepository.Kaydet();
            TempData["basarili"] = "Oda Başarıyla Silindi";
            return RedirectToAction("Index","Oda");
        }
    }
}
