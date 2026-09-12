namespace Core
{
    public interface IRunState
    {
        bool TryGetRoomRewardIndex(string roomName, out int index);
        void SaveRoomRewardIndex(string roomName, int index);
    }
}
