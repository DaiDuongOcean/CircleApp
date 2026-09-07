using System.Collections.Concurrent;

namespace CircleApp.Services
{
    // Theo dõi user nào đang online, dùng chung cho ChatHub và Controller
    // (1 user có thể mở nhiều tab/thiết bị nên đếm số kết nối, không phải bool đơn giản)
    public static class OnlineUsersTracker
    {
        private static readonly ConcurrentDictionary<int, int> _connectionCounts = new();

        // Gọi khi user kết nối SignalR. Trả về true nếu đây là kết nối ĐẦU TIÊN (vừa chuyển từ offline -> online)
        public static bool AddConnection(int userId)
        {
            var newCount = _connectionCounts.AddOrUpdate(userId, 1, (_, old) => old + 1);
            return newCount == 1;
        }

        // Gọi khi user ngắt kết nối. Trả về true nếu đây là kết nối CUỐI CÙNG (vừa chuyển từ online -> offline)
        public static bool RemoveConnection(int userId)
        {
            if (_connectionCounts.TryGetValue(userId, out int count))
            {
                var newCount = count - 1;
                if (newCount <= 0)
                {
                    _connectionCounts.TryRemove(userId, out _);
                    return true;
                }
                _connectionCounts[userId] = newCount;
            }
            return false;
        }

        public static bool IsOnline(int userId)
        {
            return _connectionCounts.ContainsKey(userId);
        }

        public static List<int> GetOnlineUserIds(IEnumerable<int> userIds)
        {
            return userIds.Where(id => _connectionCounts.ContainsKey(id)).ToList();
        }
    }
}