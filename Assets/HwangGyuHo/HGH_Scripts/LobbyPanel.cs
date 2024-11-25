using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class LobbyPanel : MonoBehaviour
{
    [SerializeField] RectTransform roomContent;
    [SerializeField] RoomEntry roomEntryPrefab;

    // 방들을 보관할 필요가 있다.
    // 딕셔너리 : 정말 많은 방 중에 이름에 해당하는 방을 찾고 싶으니까
    private Dictionary<string, RoomEntry> roomDictionary = new Dictionary<string, RoomEntry>();

    public void LeaveLobby()
    {
        Debug.Log("로비 퇴장 요청");
        PhotonNetwork.LeaveLobby();
    }

    // 방 상황을 업데이트 해주는 함수
    public void UpdateRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            // 방이 사라진 경우 + 방이 비공개인 경우 + 입장이 불가능한 방인 경우
            if (info.RemovedFromList == true || info.IsVisible == false || info.IsOpen == false)
            {
                // 예외상황 : 로비 들어가자마자 사라지는 방인 경우
                // 로비 들어가자마자 사라지는 방 그럴 경우 예외처리가 필요하다
                // 룸 목록에 추가한 적 없는데, 얘는 이 과정을 안해도 된다
                // 다른방도 처리해야 해서 continue;
                if (roomDictionary.ContainsKey(info.Name) == false)
                    continue;

                // 엔트리에서도 삭제시켜 줄 필요가 있어서 Destroy를 써서 삭제
                Destroy(roomDictionary[info.Name].gameObject);
                // 위 조건에 맞는 것들은 리스트에서 삭제되도록 한다
                roomDictionary.Remove(info.Name);
            }
            // 새로 생긴 방인 경우, 방 목록에 없었던 방일거다
            else if (roomDictionary.ContainsKey(info.Name) == false)
            {
                // 방에 대한 게임오브젝트를 생성해줄 필요가 있다
                // roomContent 의 자식으로 만들어준다
                RoomEntry roomEntry = Instantiate(roomEntryPrefab, roomContent);
                // 위에 새로운 방을 만들어서 아래에  Add 로 추가
                roomDictionary.Add(info.Name, roomEntry);
                // TODO : 방 정보 설정
                roomEntry.SetRoomInfo(info);
            }
            // 방의 정보가 변경된 경우
            else if (roomDictionary.ContainsKey((string)info.Name) == true)
            {
                RoomEntry roomEntry = roomDictionary[info.Name];
                //방 정보 설정
                roomEntry.SetRoomInfo(info);
            }
        }
    }

    public void ClearRoomEntries()
    {
        // 생성했던 방들을 순회하면서 지워주는 작업
        foreach (string name in roomDictionary.Keys)
        {
            Destroy(roomDictionary[name].gameObject);
        }
        // 딕셔너리 상황을 더 이상 필요로 하지 않을 수 있으니까
        // 지우는 함수도 있으면 좋다.
        roomDictionary.Clear();
    }
}
