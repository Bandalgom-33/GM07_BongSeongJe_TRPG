using SoloLeveling.Common;
using System.Collections.Generic;

namespace SoloLeveling.Skills
{
    // 스킬 정보를 저장하는 데이터 클래스
    public class SkillData
    {

        public SkillList Skill { get; }
        public string Name { get; }
        public int MpCost { get; }
        public double DamageMultiplier { get; }

        public bool IsBasicAttack { get; }

        public StatusEffectType StatusEffectType { get; }

        public int StatusChance { get; }

        public int StatusDuration { get; }

        public int StatusDamagePerTurn { get; }
        public string Description { get; }
        public SkillData
            (
            SkillList skill,
            string name,
            int mpCost,
            double damageMultiplier,
            bool isBasicAttack,
            StatusEffectType statusEffectType,
            int statusChance,
            int statusDuration,
            int statusDamagePerTurn,
            string description
            )
        {
            Skill = skill;
            Name = name;
            MpCost = mpCost;
            DamageMultiplier = damageMultiplier;
            IsBasicAttack = isBasicAttack;
            StatusEffectType = statusEffectType;
            StatusChance = statusChance;
            StatusDuration = statusDuration;
            StatusDamagePerTurn = statusDamagePerTurn;
            Description = description;
        }
    }
    // 게임에서 사용하는 모든 스킬 정보를 관리하는 데이터베이스 클래스
    public static class SkillDatabase
    {
        //스킬 목록 
        private static readonly Dictionary<SkillList, SkillData> skills =
            new Dictionary<SkillList, SkillData>
            {
                // =========================
                // 일반 몬스터 스킬
                // =========================

                {
                    SkillList.Attack,
                    new SkillData
                    (
                        SkillList.Attack,
                        "기본 공격",
                        0,
                        1.0,
                        true,
                        StatusEffectType.None,
                        0,
                        0,
                        0,
                        "적을 기본 공격합니다."
                    )
                },

                {
                    SkillList.StrongAttack,
                    new SkillData
                    (
                        SkillList.StrongAttack,
                        "강공격",
                        5,
                        1.5,
                        false,
                        StatusEffectType.None,
                        0,
                        0,
                        0,
                        "강한 힘으로 적을 공격합니다."
                    )
                },

                {
                    SkillList.StunAttack,
                    new SkillData
                    (
                        SkillList.StunAttack,
                        "스턴 공격",
                        8,
                        1.2,
                        false,
                        StatusEffectType.Stun,
                        50,
                        1,
                        0,
                        "적을 공격하고 일정 확률로 스턴 상태를 부여합니다."
                    )
                },

                // =========================
                // 엘리트 몬스터 스킬
                // =========================

                {
                    SkillList.PoisonAttack,
                    new SkillData
                    (
                        SkillList.PoisonAttack,
                        "독 공격",
                        10,
                        1.1,
                        false,
                        StatusEffectType.Poison,
                        70,
                        3,
                        50,
                        "적에게 독 상태를 부여합니다."
                    )
                },

                {
                    SkillList.BleedAttack,
                    new SkillData
                    (
                        SkillList.BleedAttack,
                        "출혈 공격",
                        10,
                        1.3,
                        false,
                        StatusEffectType.Bleed,
                        70,
                        3,
                        40,
                        "적에게 출혈 상태를 부여합니다."
                    )
                },

                {
                    SkillList.DoubleAttack,
                    new SkillData
                    (
                        SkillList.DoubleAttack,
                        "연속 공격",
                        12,
                        0.8,
                        false,
                        StatusEffectType.None,
                        0,
                        0,
                        0,
                        "빠르게 2회 공격합니다."
                    )
                },

                // =========================
                // 보스 몬스터 스킬
                // =========================

                {
                    SkillList.WideAttack,
                    new SkillData
                    (
                        SkillList.WideAttack,
                        "광역 공격",
                        15,
                        1.4,
                        false,
                        StatusEffectType.Bleed,
                        30,
                        3,
                        40,
                        "모든 적을 공격합니다."
                    )
                },

                {
                    SkillList.SuperStun,
                    new SkillData
                    (
                        SkillList.SuperStun,
                        "강력한 스턴",
                        18,
                        1.3,
                        false,
                        StatusEffectType.Stun,
                        80,
                        1,
                        0,
                        "강한 충격으로 적을 스턴 상태로 만듭니다."
                    )
                },

                {
                    SkillList.FatalStrike,
                    new SkillData
                    (
                        SkillList.FatalStrike,
                        "치명타 공격",
                        20,
                        2.5,
                        false,
                        StatusEffectType.Bleed,
                        50,
                        3,
                        40,
                        "치명적인 일격을 가합니다."
                    )
                }
            };

        // SkillList를 기반으로 해당 스킬의 상세 데이터를 반환
        public static SkillData GetSkillData(SkillList skill)
        {
            return skills[skill];
        }

        // 유닛 등급에 따라 랜덤으로 뽑을 수 있는 스킬 후보를 반환
        public static List<SkillList> GetRandomSkillCandidates(UnitClass unitClass)
        {
            if (unitClass == UnitClass.Normal)
            {
                return new List<SkillList>
                {
                    SkillList.StrongAttack,
                    SkillList.StunAttack
                };
            }

            if (unitClass == UnitClass.Elite)
            {
                return new List<SkillList>
                {
                    SkillList.StrongAttack,
                    SkillList.StunAttack,
                    SkillList.PoisonAttack,
                    SkillList.BleedAttack,
                    SkillList.DoubleAttack
                };
            }

            return new List<SkillList>
            {
                SkillList.StrongAttack,
                SkillList.StunAttack,
                SkillList.PoisonAttack,
                SkillList.BleedAttack,
                SkillList.DoubleAttack,
                SkillList.WideAttack,
                SkillList.SuperStun,
                SkillList.FatalStrike
            };
        }

        // 유닛 등급에 맞는 스킬 후보 중 지정한 개수만큼 랜덤 선택
        public static List<SkillList> GetRandomSkills(UnitClass unitClass, int count)
        {
            Random random = new Random();
            List<SkillList> candidates = GetRandomSkillCandidates(unitClass)
                .Where(skill => !GetSkillData(skill).IsBasicAttack)
                .Distinct()
                .ToList();
            List<SkillList> selectedSkills = new List<SkillList>();

            while (selectedSkills.Count < count && candidates.Count > 0)
            {
                int index = random.Next(candidates.Count);
                selectedSkills.Add(candidates[index]);
                candidates.RemoveAt(index);
            }

            return selectedSkills;
        }
    }

}
