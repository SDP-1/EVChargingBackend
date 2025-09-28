using EVChargingBackend.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EVChargingBackend.Services
{
    public class BookingService : IBookingService
    {
        private readonly IMongoCollection<Booking> _bookings;

        public BookingService(IMongoDatabase database)
        {
            _bookings = database.GetCollection<Booking>("Bookings");
        }

        // Create new reservation
        public async Task<Booking> CreateReservationAsync(Booking booking)
        {
            var now = DateTime.UtcNow;
            if ((booking.ReservationDateTime - now).TotalDays > 7)
                throw new InvalidOperationException("Reservation date must be within 7 days.");

            booking.CreatedAt = now;
            booking.Approved = false;
            booking.Confirmed = false;
            booking.Completed = false;
            booking.Canceled = false;

            await _bookings.InsertOneAsync(booking);
            return booking;
        }


        // Update reservation
        public async Task<Booking> UpdateReservationAsync(string bookingId, Booking updatedBooking)
        {
            var existingBooking = await _bookings.Find(b => b.Id == bookingId).FirstOrDefaultAsync();
            if (existingBooking == null)
                throw new KeyNotFoundException("Booking not found");

            if ((existingBooking.ReservationDateTime - DateTime.UtcNow).TotalHours < 12)
                throw new InvalidOperationException("Cannot update less than 12 hours before reservation.");

            var filter = Builders<Booking>.Filter.Eq(b => b.Id, bookingId);
            var update = Builders<Booking>.Update
                .Set(b => b.ReservationDateTime, updatedBooking.ReservationDateTime)
                .Set(b => b.StationId, updatedBooking.StationId)
                .Set(b => b.UpdatedAt, DateTime.UtcNow);

            await _bookings.UpdateOneAsync(filter, update);
            return await _bookings.Find(filter).FirstOrDefaultAsync();
        }


        // Cancel reservation
        public async Task<bool> CancelReservationAsync(string bookingId)
        {
            var existingBooking = await _bookings.Find(b => b.Id == bookingId).FirstOrDefaultAsync();
            if (existingBooking == null)
                throw new KeyNotFoundException("Booking not found");

            if ((existingBooking.ReservationDateTime - DateTime.UtcNow).TotalHours < 12)
                throw new InvalidOperationException("Cannot cancel less than 12 hours before reservation.");

            var filter = Builders<Booking>.Filter.Eq(b => b.Id, bookingId);
            var update = Builders<Booking>.Update
                .Set(b => b.Canceled, true)
                .Set(b => b.UpdatedAt, DateTime.UtcNow);

            var result = await _bookings.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }


        // Get reservation by Id
        public async Task<Booking> GetReservationByIdAsync(string bookingId)
        {
            return await _bookings.Find(b => b.Id == bookingId).FirstOrDefaultAsync();
        }

        //Confirm Booking by EV/station Operator
        public async Task<Booking> ConfirmBookingAsync(string bookingId, string stationOperatorUsername)
        {
            var booking = await GetReservationByIdAsync(bookingId);
            if (booking == null)
                throw new KeyNotFoundException("Booking not found");

            // Optionally: check if this operator is allowed to confirm this station
            // if (booking.StationId != stationAssignedToOperator) throw new UnauthorizedAccessException();

            var filter = Builders<Booking>.Filter.Eq(b => b.Id, bookingId);
            var update = Builders<Booking>.Update
                .Set(b => b.Confirmed, true)
                .Set(b => b.UpdatedAt, DateTime.UtcNow);

            await _bookings.UpdateOneAsync(filter, update);
            return await GetReservationByIdAsync(bookingId);
        }

        // Complete a reservation (mark Completed = true)
        public async Task<Booking> CompleteBookingAsync(string bookingId, string stationOperatorUsername)
        {
            var booking = await GetReservationByIdAsync(bookingId);
            if (booking == null)
                throw new KeyNotFoundException("Booking not found");

            if (!booking.Confirmed)
                throw new InvalidOperationException("Booking must be confirmed first.");

            var filter = Builders<Booking>.Filter.Eq(b => b.Id,bookingId);
            var update = Builders<Booking>.Update
                .Set(b => b.Completed, true)
                .Set(b => b.UpdatedAt, DateTime.UtcNow);

            await _bookings.UpdateOneAsync(filter, update);
            return await GetReservationByIdAsync(bookingId);
        }

        //get all bookings for a User
        public async Task<List<Booking>> GetBookingsByUserIdAsync(string userId)
        {
            var filter = Builders<Booking>.Filter.Eq(b => b.UserId, userId);
            return await _bookings.Find(filter).ToListAsync();
        }

        //get all bookings for a User by Back Office
        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            return await _bookings.Find(_ => true).ToListAsync();
        }


    }

}
