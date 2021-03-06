using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hotel.Web.Models;

namespace Hotel.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly HotelDbContext hotelDbContext;

        public RoomController(HotelDbContext _hotelDbContext)
        {
            hotelDbContext = _hotelDbContext;
        }

        [HttpGet]
        public async Task<IEnumerable<Room>> Get()
        {
            return await hotelDbContext.Rooms.AsNoTracking().ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var room = await hotelDbContext.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            return Ok(room);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Room room)
        {
            var createdRoom = await hotelDbContext.Rooms.AddAsync(room);
            await hotelDbContext.SaveChangesAsync();

            return Ok(createdRoom.Entity);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Room room)
        {
            var existingRoom = await hotelDbContext.Rooms.FindAsync(room.Id);
            if (existingRoom == null)
            {
                return NotFound();
            }

            existingRoom.Number = room.Number;
            existingRoom.Description = room.Description;
            existingRoom.LastBooked = room.LastBooked;
            existingRoom.Level = room.Level;
            existingRoom.RoomType = room.RoomType;
            existingRoom.NumberOfPlacesToSleep = room.NumberOfPlacesToSleep;

            var updatedRoom = hotelDbContext.Update(existingRoom);
            await hotelDbContext.SaveChangesAsync();
            return Ok(updatedRoom.Entity);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingRoom = await hotelDbContext.Rooms.FindAsync(id);
            if (existingRoom == null)
            {
                return NotFound();
            }

            var removedRoom = hotelDbContext.Rooms.Remove(existingRoom);
            await hotelDbContext.SaveChangesAsync();

            return Ok(removedRoom.Entity);
        }
    }
}
