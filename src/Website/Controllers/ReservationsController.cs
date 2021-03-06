﻿using Microsoft.AspNetCore.Mvc;

namespace Website.Controllers
{
    public class ReservationsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Reserve(int id)
        {
            return RedirectToAction("Added");
        }

        public IActionResult Added()
        {
            return View();
        }

        public IActionResult Review()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Finalize()
        {
            return RedirectToAction("Review");
        }

        [HttpPost]
        public IActionResult Checkout()
        {
            return RedirectToAction("CheckedOut");
        }

        public IActionResult CheckedOut()
        {
            return View();
        }
    }
}
