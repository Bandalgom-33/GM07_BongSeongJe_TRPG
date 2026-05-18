using SoloLeveling.Common;

namespace SoloLeveling.Entities
{
    // 몹 종류 생성
    internal class Monster : Unit
    {

        // 적 몬스터를 생성할 때 사용

        public Monster(string name, int playerLevel, UnitClass grade)
            : base(name, playerLevel, grade)
        {
            // Monster는 생성 시 플레이어 레벨 기준 랜덤 레벨을 가진다
            Random rand = new Random();

            baseInfo.Level = rand.Next(playerLevel, playerLevel + 10);
            isPlayerSide = false;
            ApplyRandomBonus();
        }

        // 몬스터가 보유한 스킬 중 하나를 랜덤으로 선택
        public SkillList SelectRandomSkill()
        {
            // 스킬 목록이 비어 있으면 기본 공격을 반환합니다.
            if (unitBase.skills == null || unitBase.skills.Count == 0)
            {
                return SkillList.Attack;
            }

            List<SkillList> skillList = unitBase.skills.ToList();

            int randomIndex = rand.Next(skillList.Count);

            return skillList[randomIndex];
        }
        // Monster 전용 랜덤 능력치 보정입니다.
        private void ApplyRandomBonus()
        {
            unitBase.Hp += rand.Next(1, 100 * baseInfo.Level);

            unitBase.Mp += rand.Next(1, 5 * baseInfo.Level);

            unitBase.ATk += rand.Next(1, 10 * baseInfo.Level);

            unitBase.DEF += rand.Next(1, 5 * baseInfo.Level);

            unitBase.NowHp = unitBase.Hp;
            unitBase.NowMp = unitBase.Mp;
        }
    }
}
