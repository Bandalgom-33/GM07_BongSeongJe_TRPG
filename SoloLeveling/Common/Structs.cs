namespace SoloLeveling.Common
{
    //유닛, 플레이어 공통정보
    public struct BaseInfo 
    {
        public string Name { get; set; }

        private int level;
        public int Level
        {
            get => level;
            set => level = Math.Max(1, value);
        }
    }
    //유닛 베이스 데이터
    public struct UnitBase
    {
        public UnitBase()
        {
        }

        private int hp;
        public int Hp
        {
            get => hp;
            set => hp = Math.Max(0, value);
        }

        private int mp;
        public int Mp
        {
            get => mp;
            set => mp = Math.Max(0, value);
        }

        public int ATk { get; set; }
        public int DEF { get; set; }
        // 공격 우선 순위를 위해서 만들었으나 미구현
        public int SPD { get; set; }

        private int nowHp;
        public int NowHp
        {
            get => nowHp;
            set => nowHp = Math.Max(0, value);
        }

        private int nowMp;
        public int NowMp
        {
            get => nowMp;
            set => nowMp = Math.Max(0, value);
        }

        public HashSet<Attribute> attributes = new HashSet<Attribute>();
        public HashSet<SkillList> skills = new HashSet<SkillList>();
        public UnitClass unitClass { get; set; }
        public HashSet<Status> status = new HashSet<Status>();
        public bool ATKBonus { get; set; }
        public bool DEFBonus { get; set; }
    }
    //플레이어 데이터
    public struct PlayerBase
    {
        public Rank rank { get; set; }

        private int gold;
        public int Gold
        {
            get => gold;
            set => gold = Math.Max(0, value);
        }
    }
}
