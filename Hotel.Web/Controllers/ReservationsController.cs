using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hotel.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly HotelDbContext hotelDbContext;

        public ReservationsController(HotelDbContext _hotelDbContext)
        {
            hotelDbContext = _hotelDbContext;
        }

        [HttpGet]
        public async Task<IEnumerable<Reservation>> Get()
        {
            return await hotelDbContext.Reservations.Include(r => r.Room).AsNoTracking().ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reservation = await hotelDbContext.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            await hotelDbContext.Entry(reservation).Collection(r => r.Profiles).LoadAsync();
            await hotelDbContext.Entry(reservation).Reference(r => r.Room).LoadAsync();

            return Ok(reservation);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] NewReservation newReservation)
        {
            var room = await hotelDbContext.Rooms.FirstOrDefaultAsync(r => r.Id == newReservation.RoomId);
            var guests = await hotelDbContext.Profiles.Where(p => newReservation.GuestIds.Contains(p.Id)).ToListAsync();

            if (room == null || guests.Count != newReservation.GuestIds.Count)
            {
                return NotFound();
            }

            var reservation = new Reservation
            {
                Created = DateTime.UtcNow,
                From = newReservation.From.Value,
                To = newReservation.To.Value,
                Room = room,
                Profiles = guests
            };

            var createdReservation = await hotelDbContext.Reservations.AddAsync(reservation);
            await hotelDbContext.SaveChangesAsync();

            return Ok(createdReservation.Entity.Id);
        }
    }
}
