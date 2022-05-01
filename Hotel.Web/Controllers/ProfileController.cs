using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Hotel.Web.Models;

namespace Hotel.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly HotelDbContext hotelDbContext;
        private readonly string connectionString;

        public ProfileController(HotelDbContext _hotelDbContext, IConfiguration _configuration)
        {
            connectionString = _configuration.GetConnectionString("HotelDB");
            hotelDbContext = _hotelDbContext;
        }

        [HttpGet]
        public async Task<IEnumerable<Profile>> Get()
        {
            return await hotelDbContext.Profiles.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var profile = await hotelDbContext.Profiles.FindAsync(id);
            if (profile == null)
            {
                return NotFound();
            }

            hotelDbContext.Entry(profile).Reference(p => p.Address).Load();

            return Ok(profile);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Profile profile)
        {
            var createdProfile = await hotelDbContext.Profiles.AddAsync(profile);
            await hotelDbContext.SaveChangesAsync();

            return Ok(createdProfile.Entity);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Profile profile)
        {
            var existingProfile = await hotelDbContext.Profiles.FindAsync(profile.Id);
            if (existingProfile == null)
            {
                return NotFound();
            }

            existingProfile.Ref = profile.Ref;
            existingProfile.Forename = profile.Forename;
            existingProfile.Surname = profile.Surname;
            existingProfile.Email = profile.Email;
            existingProfile.DateOfBirth = profile.DateOfBirth;
            existingProfile.TelNo = profile.TelNo;

            var updatedProfile = hotelDbContext.Update(existingProfile);
            await hotelDbContext.SaveChangesAsync();
            return Ok(updatedProfile.Entity);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingProfile = await hotelDbContext.Profiles.FindAsync(id);
            if (existingProfile == null)
            {
                return NotFound();
            }

            var removedProfile = hotelDbContext.Profiles.Remove(existingProfile);
            await hotelDbContext.SaveChangesAsync();

            return Ok(removedProfile.Entity);
        }

        [HttpPost("GenerateAndInsert")]
        public async Task<IActionResult> GenerateAndInsert([FromBody] int count = 1000)
        {
            Stopwatch s = new Stopwatch();
            s.Start();

            var profiles = GenerateProfiles(count);
            var gererationTime = s.Elapsed.ToString();
            s.Restart();

            hotelDbContext.Profiles.AddRange(profiles);
            var insertedCount = await hotelDbContext.SaveChangesAsync();

            return Ok(new {
                    inserted = insertedCount,
                    generationTime = gererationTime,
                    insertTime = s.Elapsed.ToString()
                });
        }

        [HttpPost("GenerateAndInsertWithSqlCopy")]
        public async Task<IActionResult> GenerateAndInsertWithSqlCopy([FromBody] int count = 1000)
        {
            Stopwatch s = new Stopwatch();
            s.Start();

            var profiles = GenerateProfiles(count);
            var gererationTime = s.Elapsed.ToString();
            s.Restart();

            var dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("Ref");
            dt.Columns.Add("Forename");
            dt.Columns.Add("Surname");
            dt.Columns.Add("Email");
            dt.Columns.Add("TelNo");
            dt.Columns.Add("DateOfBirth");

            foreach (var profile in profiles)
            {
                dt.Rows.Add(string.Empty, profile.Ref, profile.Forename, profile.Surname, profile.Email, profile.TelNo, profile.DateOfBirth);
            }

            using var sqlBulk = new SqlBulkCopy(connectionString);
            sqlBulk.DestinationTableName = "Profiles";
            await sqlBulk.WriteToServerAsync(dt);

            return Ok(new
            {
                inserted = dt.Rows.Count,
                generationTime = gererationTime,
                insertTime = s.Elapsed.ToString()
            });
        }

        [HttpPost("GenerateAndInsertWithLinq2db")]
        public async Task<IActionResult> GenerateAndInsertWithLinq2db([FromBody] int count = 1000)
        {
            Stopwatch s = new Stopwatch();
            s.Start();

            var profiles = GenerateProfiles(count);
            var gererationTime = s.Elapsed.ToString();
            s.Restart();

            using (var db = hotelDbContext.CreateLinqToDbConnection())
            {
                await db.BulkCopyAsync(new BulkCopyOptions { TableName = "Profiles" }, profiles);
            }

            return Ok(new
            {
                inserted = profiles.Count(),
                generationTime = gererationTime,
                insertTime = s.Elapsed.ToString()
            });
        }

        [HttpPost("UpdateProfiles")]
        public async Task<IActionResult> UpdateProfiles([FromBody] int minimalProfileId = 0)
        {
            await hotelDbContext.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE Profiles SET Country = 'Brazil' WHERE LEFT(TelNo, 2) = '48' AND Id > {minimalProfileId}");

            return Ok();
        }

        [HttpGet("GetAllGuestsDate")]
        public IActionResult GetAllGuestsDate([FromQuery] string date)
        {
            var guests = hotelDbContext.GuestArrivals.FromSqlInterpolated($"GetAllGuestsDate {date}").ToList();

            return Ok(guests);
        }

        [HttpGet("GetRoomsOccupied")]
        public IActionResult GetGuestArrivalsFromView([FromQuery] string date)
        {
            var parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var rooms = hotelDbContext.RoomsOccupied.Where(r => r.From <= parsedDate && r.To >= parsedDate);

            return Ok(rooms);
        }

        private IEnumerable<Profile> GenerateProfiles(int count)
        {
            var salutations = new string[] {"Mr", "Mrs"};

            var profileGenerator = new Faker<Profile>()
                .RuleFor(p => p.Ref, v => v.Person.UserName)
                .RuleFor(p => p.Forename, v => v.Person.FirstName)
                .RuleFor(p => p.Surname, v => v.Person.LastName)
                .RuleFor(p => p.Email, v => v.Person.Email)
                .RuleFor(p => p.TelNo, v => v.Person.Phone)
                .RuleFor(p => p.DateOfBirth, v => v.Person.DateOfBirth)
                .RuleFor(p => p.Salutation, v => v.PickRandom(salutations))
                .RuleFor(p => p.Country, v => v.Address.Country());

            return profileGenerator.Generate(count);
        }
    }
}
