namespace AintBnB.BusinessLogic.Interfaces
{
    public interface IDeletionService
    {
        void DeleteAccommodation(int id);
        void DeleteBooking(int id);
        void DeleteUser(int id);
    }
}
