using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hotel.Web.Controllers
{
    public class NewReservation
    {
        [Required]
        public int RoomId { get; set; }

        [Required]
        public List<int> GuestIds { get; set; }

        [Required]
        public DateTime? From { get; set; }

        [Required]
        public DateTime? To { get; set; }
    }
}
