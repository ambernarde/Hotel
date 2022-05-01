using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Models
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Room> Rooms { get; set; }

        public virtual DbSet<Profile> Profiles { get; set; }

        public virtual DbSet<Reservation> Reservations { get; set; }

        public virtual DbSet<Address> Address { get; set; }

        // from stored procedure
        public virtual DbSet<GuestArrival> GuestArrivals { get; set; }

        // from view
        public virtual DbSet<RoomOccupied> RoomsOccupied { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<RoomOccupied>(eb =>
                {
                    eb.HasNoKey();
                    eb.ToView("vwRoomsOccupied");
                });
        }
    }
}
