namespace SportCourtManagerment.Repository.SlotTimes
{
    public interface ISlotTimeService
    {
        Task<IEnumerable<SlotTime>> GetAllSlotTimesAsync();
        Task<SlotTime?> GetSlotTimeByIdAsync(int slotTimeId);
        Task<bool> CreateSlotTimeAsync(SlotTime slotTime);
        Task<bool> UpdateSlotTimeAsync(SlotTime slotTime);
        Task<bool> DeleteSlotTimeAsync(int slotTimeId);
    }
}
