using Server.Models;
using System.Collections.Concurrent;

namespace Server.Services
{
    /// <summary>
    /// 게임이 진행되는 방을 관리하는 클래스입니다.
    /// </summary>
    public class RoomManager
    {
        private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();
        private readonly ConcurrentDictionary<string, string> _userRooms = new();
        private readonly ConcurrentQueue<string> _waitingRooms = new();

        /// <summary>
        /// 자동으로 방을 매칭합니다.
        /// </summary>
        /// <param name="player">유저 정보</param>
        /// <returns>방 객체</returns>
        public GameRoom QuickMatch(Player player)
        {
            while (_waitingRooms.TryDequeue(out var roomId))
            {
                if (_rooms.TryGetValue(roomId, out var room) && room.Player2 == null)
                {
                    return JoinOrCreateRoom(roomId, player);
                }
            }

            string newRoomId = Guid.NewGuid().ToString("N");
            var newRoom = JoinOrCreateRoom(newRoomId, player);

            _waitingRooms.Enqueue(newRoomId);

            return newRoom;
        }

        /// <summary>
        /// 유저를 방에 입장시킵니다. 방이 없다면 생성하여 입장합니다.
        /// </summary>
        /// <param name="roomId">일장할 방의 고유 식별자</param>
        /// <param name="player">입장하는 유저의 정보</param>
        /// <returns>입장한 방 객체</returns>
        public GameRoom JoinOrCreateRoom(string roomId, Player player)
        {
            var room = _rooms.GetOrAdd(roomId, _ => new GameRoom(roomId, player));

            // 두 번째 유저인 경우
            if (room.Player1 != null && room.Player1.ConnectionId != player.ConnectionId && room.Player2 == null)
            {
                player.Color = StoneColor.White;
                room.Player2 = player;
                room.Status = RoomStatus.Playing;
            }

            _userRooms.TryAdd(player.ConnectionId, roomId);

            return room;
        }

        /// <summary>
        /// 방의 고유 식별자를 통해 방 객체를 반환합니다.
        /// </summary>
        /// <param name="roomId">방의 고유 식별자</param>
        /// <returns>방 객체</returns>
        public GameRoom GetRoom(string roomId)
        { 
            _rooms.TryGetValue(roomId, out var room);
            return room;
        }

        /// <summary>
        /// 방 객체를 삭제합니다.
        /// </summary>
        /// <param name="roomId">방의 고유 식별자</param>
        /// <returns>삭제 성공 여부 (true: 성공, false: 실패)</returns>
        public bool RemoveRoom(string roomId)
        {
            if (_rooms.TryRemove(roomId, out var room))
            {
                if (room.Player1 != null)
                    _userRooms.TryRemove(room.Player1.ConnectionId, out _);
                if (room.Player2 != null)
                    _userRooms.TryRemove(room.Player2.ConnectionId, out _);

                return true;
            }
            return false;
        }

        /// <summary>
        /// 클라이언트 고유 식별자를 통해 해당 클라이언트가 속한 방을 반환합니다.
        /// </summary>
        /// <param name="connectionId">클라이언트 고유 식별자</param>
        /// <returns>클라이언트가 속한 방 객체</returns>
        public GameRoom FindRoomByConnectionId(string connectionId)
        {
            if (_userRooms.TryGetValue(connectionId, out var roomId))
            { 
                _rooms.TryGetValue(roomId, out var room);
                return room;
            }
            return null;
        }
    }
}
