using SoloLeveling.Common;

namespace SoloLeveling.SaveData
{
    // Shadow 저장 전용 데이터

    internal class ShadowSaveData
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public UnitClass Grade { get; set; }
        public int CurrentExp { get; set; }
        public int MaxExp { get; set; }

        public int CurrentHp { get; set; }

        public int MaxHp { get; set; }

        public int CurrentMp { get; set; }

        public int MaxMp { get; set; }

        public int Attack { get; set; }

        public int Defense { get; set; }
        public List<SkillList> Skills { get; set; } = new List<SkillList>();
    }
}
