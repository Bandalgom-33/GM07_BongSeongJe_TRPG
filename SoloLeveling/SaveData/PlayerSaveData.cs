using SoloLeveling.Common;

namespace SoloLeveling.SaveData
{
    // Player 저장 전용 데이터
    internal class PlayerSaveData
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public int CurrentExp { get; set; }
        public int MaxExp { get; set; }
        public Rank Rank { get; set; }
        public int Gold { get; set; }
        // 보유 유닛 데이터
        public List<ShadowSaveData> OwnedShadows { get; set; } = new List<ShadowSaveData>();
        public List<ItemSaveData> Items { get; set; } = new List<ItemSaveData>();
    }
}
