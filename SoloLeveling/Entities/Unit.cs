using System;
using System.Collections.Generic;
using System.Linq;
using SoloLeveling.Battle;
using SoloLeveling.Common;
using SoloLeveling.Skills;

namespace SoloLeveling.Entities
{
    // Monster와 Shadow가 공통으로 사용하는 기본 유닛 클래스입

    abstract class Unit
    {
        protected static Random rand = new Random();


        protected BaseInfo baseInfo = new BaseInfo();
        protected UnitBase unitBase = new UnitBase();
        protected UnitClass unitGrade;
        protected bool isPlayerSide = false;
        private readonly List<StatusEffect> statusEffects = new List<StatusEffect>();
        public string Name => baseInfo.Name;
        public int Level => baseInfo.Level;
        public int MaxHp => unitBase.Hp;
        public int MaxMp => unitBase.Mp;
        public int CurrentHp => unitBase.NowHp;
        public int CurrentMp => unitBase.NowMp;
        public int Attack => unitBase.ATk;
        public int Defense => unitBase.DEF;
        public UnitClass Grade => unitGrade;
        public bool IsAlive => unitBase.NowHp > 0;
        public bool IsPlayerSide => isPlayerSide;
        public int Speed => unitBase.SPD;
        public HashSet<SkillList> skills => unitBase.skills;
        public IReadOnlyList<StatusEffect> StatusEffects => statusEffects;
        

        // 유닛의 이름, 레벨 기준, 등급을 받아 기본 스탯과 스킬을 설정
        public Unit(string Name, int PlayerLevel, UnitClass grade)
        {
            baseInfo.Name = Name;
            unitGrade = grade;

            // 기본 상태 설정 (노말 상태 부여)
            unitBase.status.Add(Status.Normal);
            unitBase.ATKBonus = false;
            unitBase.DEFBonus = false;

            // 스탯 설정
            BaseStatusSet(PlayerLevel);

            // HashSet에 스킬 저장 (전달받은 grade 사용)
            var selectedSkills = SkillSet(grade);
            foreach (SkillList skill in selectedSkills)
            {
                unitBase.skills.Add(skill);
            }
        }

        // 유닛이 보유한 스킬 목록을 외부에서 확인할 수 있도록 복사본을 반환

        public List<SkillList> GetSkills()
        {
            return unitBase.skills.ToList();
        }
        // 플레이어 레벨 기준으로 유닛의 기본 능력치를 설정
        private void BaseStatusSet(int PlayerLevel)
        {
            // Monster는 생성 시 플레이어 레벨 기준 랜덤 레벨
            // Shadow는 저장되는 유닛이므로 고정레벨
            baseInfo.Level = PlayerLevel;
            unitBase.Hp = 500 + (100 * baseInfo.Level);
            unitBase.Mp = 50 + (5 * baseInfo.Level);
            unitBase.ATk = 100 + (10 * baseInfo.Level);
            unitBase.DEF = 10 + (5 * baseInfo.Level);
            unitBase.NowHp = unitBase.Hp;
            unitBase.NowMp = unitBase.Mp;
        }

        // 선택한 스킬을 대상에게 사용하고 실제 피해량을 반환
        public int UseSkill(SkillList skill, Unit target)
        {
            const int basicAttackMpRecovery = 10;
            SkillData skillData = SkillDatabase.GetSkillData(skill);

            if (!CanUseSkill(skillData.MpCost))
            {

                skill = SkillList.Attack;
                skillData = SkillDatabase.GetSkillData(skill);
                
            }

            if (skill != SkillList.Attack)
            {
                UseMp(skillData.MpCost);
            }
            else if (skill == SkillList.Attack)
            {
                RecoverMp(basicAttackMpRecovery);

            }
            int damage = DamageCalculator.CalculateDamage(this, target, skillData);

            target.TakeDamage(damage);

            return damage;
        }

        // 스킬 셋 결정 로직
        public List<SkillList> SkillSet(UnitClass grade)
        {
            List<SkillList> selectedSkills = new List<SkillList>
            {
                SkillList.Attack
            };

            selectedSkills.AddRange(SkillDatabase.GetRandomSkills(grade, 2));

            return selectedSkills;
        }

        // 받은 피해량만큼 현재 HP를 감소
        public void TakeDamage(int damage)
        {
            unitBase.NowHp = Math.Max(0, unitBase.NowHp - damage);
        }
        // HP 회복을 간단히 호출
        public void Heal(int amount)
        {
            HealHp(amount);
        }

        // HP와 MP를 모두 최대치까지 회복
        public void FullRecover()
        {
            unitBase.NowHp = unitBase.Hp;
            unitBase.NowMp = unitBase.Mp;
        }
        // 현재 MP로 스킬을 사용할 수 있는지 확인
        public bool CanUseSkill(int mpCost)
        {
            return unitBase.NowMp >= mpCost;
        }

        // 스킬 사용 시 MP를 소모
        public bool UseMp(int mpCost)
        {
            if (!CanUseSkill(mpCost))
            {
                return false;
            }
            unitBase.NowMp -= mpCost;
            return true;
        }

        // MP를 회복
        public void RecoverMp(int amount)
        {
            HealMp(amount);
        }

        // HP를 회복하고 실제 회복된 양을 반환
        public int HealHp(int amount)
        {
            int beforeHp = CurrentHp;
            unitBase.NowHp = Math.Min(unitBase.Hp, unitBase.NowHp + Math.Max(0, amount));
            return unitBase.NowHp - beforeHp;
        }

        // MP를 회복하고 실제 회복된 양을 반환
        public int HealMp(int amount)
        {
            int beforeMp = CurrentMp;
            unitBase.NowMp = Math.Min(unitBase.Mp, unitBase.NowMp + Math.Max(0, amount));
            return unitBase.NowMp - beforeMp;
        }
        // 유닛에게 상태 이상을 추가하거나 기존 상태 이상을 갱신
        public void AddStatusEffect(StatusEffect effect)
        {
            if (effect == null || effect.Type == StatusEffectType.None || effect.RemainingTurns <= 0)
            {
                return;
            }

            StatusEffect existingEffect = statusEffects.FirstOrDefault(status => status.Type == effect.Type);

            if (existingEffect != null)
            {
                existingEffect.RemainingTurns = Math.Max(existingEffect.RemainingTurns, effect.RemainingTurns);
                existingEffect.DamagePerTurn = Math.Max(existingEffect.DamagePerTurn, effect.DamagePerTurn);
                return;
            }

            statusEffects.Add(new StatusEffect
            {
                Type = effect.Type,
                RemainingTurns = effect.RemainingTurns,
                DamagePerTurn = effect.DamagePerTurn
            });
        }

        // 특정 상태 이상이 현재 유닛에게 남아 있는지 확인
        public bool HasStatusEffect(StatusEffectType type)
        {
            return statusEffects.Any(status => status.Type == type && status.RemainingTurns > 0);
        }

        // 기절 상태가 있으면 제거하고 이번 턴 행동 불가로 처리
        public bool TryConsumeStun()
        {
            StatusEffect stun = statusEffects.FirstOrDefault(status => status.Type == StatusEffectType.Stun && status.RemainingTurns > 0);

            if (stun == null)
            {
                return false;
            }

            statusEffects.Remove(stun);
            return true;
        }

        // 독, 출혈 같은 지속 피해 상태 이상을 처리하고 결과를 반환
        public List<StatusEffectResult> ProcessStatusEffects()
        {
            List<StatusEffectResult> results = new List<StatusEffectResult>();

            foreach (StatusEffect effect in statusEffects.ToList())
            {
                if (effect.Type == StatusEffectType.Poison || effect.Type == StatusEffectType.Bleed)
                {
                    int beforeHp = CurrentHp;
                    TakeDamage(effect.DamagePerTurn);
                    int damage = Math.Max(0, beforeHp - CurrentHp);

                    results.Add(new StatusEffectResult
                    {
                        Type = effect.Type,
                        Damage = damage
                    });
                }
                if (effect.Type == StatusEffectType.Poison || effect.Type == StatusEffectType.Bleed)
                {
                    effect.RemainingTurns--;
                }
            }

            RemoveExpiredStatusEffects();
            return results;
        }

        // 남은 턴이 끝난 상태 이상을 목록에서 제거
        public void RemoveExpiredStatusEffects()
        {
            statusEffects.RemoveAll(status => status.RemainingTurns <= 0);
        }

        // 전투 종료 시 남아 있는 상태 이상을 모두 제거
        public void ClearStatusEffects()
        {
            statusEffects.Clear();
        }

        // 적 유닛을 아군 유닛으로 만들 때 사용
        protected Unit(Unit source)
        {
            baseInfo.Name = source.baseInfo.Name;
            baseInfo.Level = source.baseInfo.Level;

            unitGrade = source.unitGrade;

            unitBase.Hp = source.unitBase.Hp;
            unitBase.NowHp = source.unitBase.NowHp;
            unitBase.Mp = source.unitBase.Mp;
            unitBase.NowMp = source.unitBase.NowMp;
            unitBase.ATk = source.unitBase.ATk;
            unitBase.DEF = source.unitBase.DEF;

            unitBase.ATKBonus = source.unitBase.ATKBonus;
            unitBase.DEFBonus = source.unitBase.DEFBonus;


            foreach (Status status in source.unitBase.status)
            {
                unitBase.status.Add(status);
            }

            foreach (SkillList skill in source.unitBase.skills)
            {
                unitBase.skills.Add(skill);
            }
        }
    }
}
