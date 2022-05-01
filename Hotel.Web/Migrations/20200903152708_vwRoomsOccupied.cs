using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hotel.Web.Migrations
{
    public partial class vwRoomsOccupied : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"
                CREATE OR ALTER VIEW [dbo].[vwRoomsOccupied] AS
                    SELECT r.[From], r.[To], ro.Number As RoomNumber, ro.Level, ro.WithBathroom
                    FROM Reservations r
                    JOIN Rooms ro ON r.RoomId = ro.Id";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW vwRoomsOccupied");
        }
    }
}
