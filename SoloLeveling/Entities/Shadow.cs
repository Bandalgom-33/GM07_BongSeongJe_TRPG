using SoloLeveling.Common;
using SoloLeveling.SaveData;

namespace SoloLeveling.Entities
{
    // 플레이어가 보유하고 전투에 사용하는 그림자 유닛 클래스
    internal class Shadow : Unit
    {
        private int currentExp;
        private int maxExp;

        public int CurrentExp => currentExp;
        public int MaxExp => maxExp;

        // 새 Shadow를 직접 생성할 때 사용하는 생성자
        public Shadow(string name, int level, UnitClass grade)
        : base(name, level, grade)
        {
            isPlayerSide = true;
            currentExp = 0;
            maxExp = CalculateMaxExp();
        }

        // 처치한 Monster 정보를 복사해서 Shadow로 변환하는 생성자
        public Shadow(Monster monster)
            : base(monster)
        {
            isPlayerSide = true;
            currentExp = 0;
            maxExp = CalculateMaxExp();
        }

        // Shadow에게 경험치를 추가하고 조건을 만족하면 레벨업
        public void AddExp(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            currentExp += amount;

            while (currentExp >= maxExp)
            {
                currentExp -= maxExp;
                LevelUp();
            }
        }

        // Shadow의 레벨을 올리고 성장 스탯을 적용
        private void LevelUp()
        {
            baseInfo.Level++;
            IncreaseStatsOnLevelUp();
            maxExp = CalculateMaxExp();
        }

        // Shadow가 레벨업했을 때 HP, MP, 공격력, 방어력을 증가
        private void IncreaseStatsOnLevelUp()
        {
            int hpIncrease = 50 + (Level * 5);
            int mpIncrease = 5 + (Level / 2);
            int attackIncrease = 5 + (Level / 3);
            int defenseIncrease = 2 + (Level / 4);

            unitBase.Hp += hpIncrease;
            unitBase.Mp += mpIncrease;
            unitBase.ATk += attackIncrease;
            unitBase.DEF += defenseIncrease;

            unitBase.NowHp = unitBase.Hp;
            unitBase.NowMp = unitBase.Mp;
        }

        // Shadow의 다음 레벨업에 필요한 경험치를 계산
        private int CalculateMaxExp()
        {
            return 80 + (Level - 1) * 40;
        }

        // Shadow의 현재 상태를 저장용 데이터로 변환
        public ShadowSaveData ToSaveData()
        {
            return new ShadowSaveData
            {
                Name = Name,
                Level = Level,
                Grade = Grade,
                CurrentExp = CurrentExp,
                MaxExp = MaxExp,
                CurrentHp = CurrentHp,
                MaxHp = MaxHp,
                CurrentMp = CurrentMp,
                MaxMp = MaxMp,
                Attack = Attack,
                Defense = Defense,
                Skills = GetSkills()
            };
        }

        // 저장 파일에서 불러온 Shadow 정보를 실제 객체로 복원
        public Shadow(ShadowSaveData saveData)
            : base(saveData.Name, saveData.Level, saveData.Grade)
        {
            isPlayerSide = true;

            currentExp = saveData.CurrentExp;
            maxExp = saveData.MaxExp;

            unitBase.Hp = saveData.MaxHp;
            unitBase.NowHp = saveData.CurrentHp;

            unitBase.Mp = saveData.MaxMp;
            unitBase.NowMp = saveData.CurrentMp;

            unitBase.ATk = saveData.Attack;
            unitBase.DEF = saveData.Defense;

            if (saveData.Skills != null && saveData.Skills.Count > 0)
            {
                unitBase.skills.Clear();

                foreach (SkillList skill in saveData.Skills)
                {
                    unitBase.skills.Add(skill);
                }
            }
        }

    }
}
