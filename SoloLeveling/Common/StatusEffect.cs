namespace SoloLeveling.Common
{
    // 상태이상 정보 저장
    public class StatusEffect
    {
        //상태이상 종류
        public StatusEffectType Type { get; set; }
        // 상태이상 남은 턴
        public int RemainingTurns { get; set; }
        //턴 당 데미지
        public int DamagePerTurn { get; set; }
    }

    //상태이상 처리
    public class StatusEffectResult
    {
        // 원인이 되는 상태이상 종류
        public StatusEffectType Type { get; set; }
        // 실제 피해량
        public int Damage { get; set; }
    }
}
