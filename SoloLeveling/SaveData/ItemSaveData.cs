using SoloLeveling.Common;

namespace SoloLeveling.SaveData
{
    // 아이템 보유 정보를 담는 클래스
    internal class ItemSaveData
    {
        // 저장할 아이템 종류
        public ItemType Type { get; set; }
        // 해당 아이템 갯수 저장
        public int Count { get; set; }
    }
}
