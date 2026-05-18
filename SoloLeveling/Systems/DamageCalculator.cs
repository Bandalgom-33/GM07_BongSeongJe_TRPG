using SoloLeveling.Common;
using SoloLeveling.Entities;
using SoloLeveling.Skills;

namespace SoloLeveling.Battle
{
    // 공격자와 방어자 정보를 기준으로 최종 피해량을 계산하는 클래스
    static class DamageCalculator
    {

        public static int CalculateDamage(Unit attacker, Unit defender, SkillData skillData)
        {
            int baseDamage = attacker.Attack - defender.Defense;
            baseDamage = Math.Max(1, baseDamage);


            int finalDamage = (int)(baseDamage * skillData.DamageMultiplier);
            return Math.Max(1, finalDamage);
        }
    }
}
