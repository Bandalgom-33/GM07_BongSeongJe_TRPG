using SoloLeveling.Common;
using SoloLeveling.Entities;
using SoloLeveling.Skills;

namespace SoloLeveling.Systems
{
    // 몬스터 한 종류의 기본 정보를 저장하는 데이터 클래스
    internal class MonsterData
    {
        public string Id { get; }
        public string Name { get; }
        public UnitClass Class { get; }
        public int LevelOffset { get; }
        public GateDifficulty Difficulty { get; }
        public List<SkillList> SkillCandidates { get; }
        public int GoldReward { get; }
        public int ExpReward { get; }

        public MonsterData
            (
            string id,
            string name,
            UnitClass unitClass,
            int levelOffset,
            GateDifficulty difficulty,
            List<SkillList> skillCandidates,
            int goldReward,
            int expReward
            )
        {
            Id = id;
            Name = name;
            Class = unitClass;
            LevelOffset = levelOffset;
            Difficulty = difficulty;
            SkillCandidates = skillCandidates;
            GoldReward = goldReward;
            ExpReward = expReward;
        }
    }

    // 게이트 난이도에 따라 등장할 몬스터 데이터를 관리하는 클래스
    internal static class MonsterDatabase
    {
        private static readonly Random random = new Random();
        // 난이도 별 몬스터 설정
        private static readonly List<MonsterData> monsters = new List<MonsterData>
        {
            new MonsterData(
                "easy_goblin",
                "초급 고블린",
                UnitClass.Normal,
                0,
                GateDifficulty.Easy,
                SkillDatabase.GetRandomSkillCandidates(UnitClass.Normal),
                100,
                30),
            new MonsterData(
                "normal_orc",
                "중급 오크",
                UnitClass.Elite,
                5,
                GateDifficulty.Normal,
                SkillDatabase.GetRandomSkillCandidates(UnitClass.Elite),
                250,
                60),
            new MonsterData(
                "hard_demon",
                "상급 데몬",
                UnitClass.Boss,
                10,
                GateDifficulty.Hard,
                SkillDatabase.GetRandomSkillCandidates(UnitClass.Boss),
                500,
                100)
        };
        // 난이도에 맞는 몬스터 호출
        public static List<MonsterData> GetMonstersByDifficulty(GateDifficulty difficulty)
        {
            return monsters
                .Where(monster => monster.Difficulty == difficulty)
                .ToList();
        }

        // 난이도와 플레이어 레벨을 기준으로 실제 전투용 몬스터 목록을 생성
        public static List<Monster> CreateMonsters(GateDifficulty difficulty, int playerLevel)
        {
            int count = GetMonsterCount(difficulty);
            List<MonsterData> candidates = GetMonstersByDifficulty(difficulty);
            List<Monster> result = new List<Monster>();

            if (candidates.Count == 0)
            {
                candidates = GetMonstersByDifficulty(GateDifficulty.Easy);
            }

            for (int i = 0; i < count; i++)
            {
                MonsterData data = candidates[random.Next(candidates.Count)];
                result.Add(CreateMonster(data, playerLevel));
            }

            return result.Take(3).ToList();
        }

        // MonsterData 하나를 실제 Monster 객체로 변환
        private static Monster CreateMonster(MonsterData data, int playerLevel)
        {
            Monster monster = new Monster(data.Name, playerLevel + data.LevelOffset, data.Class);
            monster.skills.Clear();
            monster.skills.Add(SkillList.Attack);

            List<SkillList> candidates = data.SkillCandidates
                .Where(skill => skill != SkillList.Attack)
                .Distinct()
                .ToList();

            while (monster.skills.Count < 3 && candidates.Count > 0)
            {
                int index = random.Next(candidates.Count);
                monster.skills.Add(candidates[index]);
                candidates.RemoveAt(index);
            }

            return monster;
        }

        // 게이트 난이도에 따라 전투에 등장할 몬스터 수를 조정
        private static int GetMonsterCount(GateDifficulty difficulty)
        {
            if (difficulty == GateDifficulty.Easy)
            {
                return 1;
            }

            if (difficulty == GateDifficulty.Normal)
            {
                return 2;
            }

            return 3;
        }
    }
}
