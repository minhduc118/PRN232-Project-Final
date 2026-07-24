using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SportCourtManagent_Server.Utils;

namespace SportCourtManagent_Server.DTOs.Bookings
{
    public class CreateBookingRequestDto
    {

        [Required(ErrorMessage = "Court ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Court ID must be a positive integer.")]
        public int CourtId { get; set; }

        public int SlotId { get; set; }

        public List<int>? SlotIds { get; set; }

        [StringLength(50, ErrorMessage = "Promotion code cannot exceed 50 characters.")]
        public string? PromoCode { get; set; }


        [Required(ErrorMessage = "Booking date is required.")]
        [DataType(DataType.Date, ErrorMessage = "Booking date must be a valid date")]
        [FutureOrPresentDate(ErrorMessage = "Booking date must be greater than or equal to today.")]
        public DateTime BookingDate { get; set; }

        public List<BookingServiceRequestDto> BookingServices { get; set; } = new List<BookingServiceRequestDto>();
    }
}
