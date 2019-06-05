﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Garage25.Models;

namespace Garage25.Controllers
{
    public class ParkedVehiclesController : Controller
    {
        private readonly Garage25Context _context;

        public ParkedVehiclesController(Garage25Context context)
        {
            _context = context;
        }

        // GET: ParkedVehicles
        public async Task<IActionResult> Index()
        {
            return View(await _context.ParkedVehicle.ToListAsync());
        }

        public async Task<IActionResult> Index2()
        {
            var model = new ParkedVehicleViewModel
            {
                ParkedVehicles = await _context.ParkedVehicle.Select(v => new VehicleIndexViewModel
                {
                    Id = v.Id,
                    RegNo = v.RegNo,
                    ParkingTime = DateTime.Now - v.CheckInTime,
                    Member = v.Member,
                    CheckInTime = v.CheckInTime,
                    VehicleType = v.VehicleType,
                    
                }).ToListAsync(),
            };


            //var Parkedhours = DateTime.Now - model.CheckInTime;
            //model.ParkingTime = Parkedhours;
            //return View("Receipt", model);

            return View(model);
        }


        // GET: ParkedVehicles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkedVehicle = await _context.ParkedVehicle
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parkedVehicle == null)
            {
                return NotFound();
            }

            parkedVehicle.ParkingTime = DateTime.Now - parkedVehicle.CheckInTime;

            var member = await _context.Member
                .FirstOrDefaultAsync(m => m.Id == parkedVehicle.MemberId);
            if (member == null)
            {
                return NotFound();
            }
            ViewData["membername"] = member.UserName;

            var vehicletype = await _context.VehicleType
                .FirstOrDefaultAsync(m => m.Id == parkedVehicle.VehicleTypeId);
            if (vehicletype == null)
            {
                return NotFound();
            }
            ViewData["vehicletypename"] = vehicletype.Type.ToString();

            return View(parkedVehicle);
        }

        // GET: ParkedVehicles/Create
        public IActionResult Create()
        {
            // Add some bogus data
            DateTime now = DateTime.Now;
            var vehicle = new Bogus.DataSets.Vehicle();
            var parkedVehicle = new ParkedVehicle
            {
                RegNo = vehicle.Vin().Substring(0, 6),
                CheckInTime = now,
                ParkingTime = DateTime.Now - now,
            };
            return View(parkedVehicle);
        }

        // POST: ParkedVehicles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RegNo,CheckInTime,ParkingTime")] ParkedVehicle parkedVehicle)
        {
            if (ModelState.IsValid)
            {
                _context.Add(parkedVehicle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(parkedVehicle);
        }

        // GET: ParkedVehicles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkedVehicle = await _context.ParkedVehicle.FindAsync(id);
            if (parkedVehicle == null)
            {
                return NotFound();
            }
            return View(parkedVehicle);
        }

        // POST: ParkedVehicles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RegNo,CheckInTime,ParkingTime")] ParkedVehicle parkedVehicle)
        {
            if (id != parkedVehicle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(parkedVehicle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParkedVehicleExists(parkedVehicle.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(parkedVehicle);
        }

        // GET: ParkedVehicles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var parkedVehicle = await _context.ParkedVehicle
               .FirstOrDefaultAsync(m => m.Id == id);

            var checkouttime = DateTime.Now;
            ViewBag.checkouttime = checkouttime;
            parkedVehicle.ParkingTime = DateTime.Now - parkedVehicle.CheckInTime;

            if (parkedVehicle == null)
            {
                return NotFound();
            }

            return View(parkedVehicle);
        }

        // POST: ParkedVehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOutConfirmed(int id)
        {
            var parkedVehicle = await _context.ParkedVehicle.FindAsync(id);
            var model = new RecieptViewModel
            {
                RegNo = parkedVehicle.RegNo,
                Brand = parkedVehicle.VehicleType.Brand,
                CheckInTime = parkedVehicle.CheckInTime,
                CheckOutTime = DateTime.Now
                //Type = VType.Type,
            };

            var checkouttime = DateTime.Now;
            ViewBag.checkouttime = checkouttime;
            parkedVehicle.ParkingTime = DateTime.Now - parkedVehicle.CheckInTime;

            _context.ParkedVehicle.Remove(parkedVehicle);
            await _context.SaveChangesAsync();
            return View("Receipt", model);
        }


        private bool ParkedVehicleExists(int id)
        {
            return _context.ParkedVehicle.Any(e => e.Id == id);
        }
        private bool ParkedVehicleExists(string regNo)
        {
            return _context.ParkedVehicle.Any(e => e.RegNo == regNo);
        }
        public async Task<IActionResult> Filter(string searchterm)

        {
        if (searchterm == null)
        {
            return Redirect(nameof(Index2));
        }

        searchterm = searchterm.ToLower();
        var model = new ParkedVehicleViewModel
        {

            ParkedVehicles = await _context.ParkedVehicle.Where(r =>
                    r.RegNo.ToLower() == searchterm ||
                    r.Member.UserName.ToLower() == searchterm ||
                    r.VehicleType.Type.ToString().ToLower() == searchterm).Select(v => new VehicleIndexViewModel



                    //Enum.GetName(typeof(VehicleType), r.Type).ToString().ToLower() == searchterm).Select(v => new VehicleIndexViewModel
                    {
                        Id = v.Id,
                        RegNo = v.RegNo,
                        ParkingTime = DateTime.Now - v.CheckInTime,
                        Member = v.Member,
                        CheckInTime = v.CheckInTime,
                        VehicleType = v.VehicleType
                    }).ToListAsync(),
            SearchTerm = searchterm
        };
        return View(nameof(Index2), model);
    }
}
}


