namespace SoloLeveling.Common
{
    //유닛이 가진 기본 속성 타입
    // 추후 속성 시스템 만들기 위해 선언
    [Flags]
    public enum Attribute
    {
        Shadow = 1, Holy=2, Pierce=4, Slash=8
    }
    // 유닛 스킬 모음 
    public enum SkillList
    {
        // --- 일반 몬스터 (0~2) ---
        Attack,        // 기본 공격
        StrongAttack,  // 강공격 (데미지 높음)
        StunAttack,    // 스턴 공격 (행동 제약)

        // --- 엘리트 몬스터  (3~5) ---
        PoisonAttack,  // 독 공격 (도트 데미지)
        BleedAttack,   // 출혈 공격 (도트 데미지)
        DoubleAttack,  // 2회 연속 공격

        // --- 보스 (6~ ) ---
        WideAttack,    // 광역 공격
        SuperStun,     // 강력한 스턴 공격
        FatalStrike    // 치명타 공격
    }
    // 등급 enum 
    // 난이도와 스킬셋 구성을 위해 사용
    public enum UnitClass
    {
        Normal, Elite, Boss
    }
    // 상태 enum Flags 사용으로 동시에 여러 상태 관리용
    // 현재는 미사용
    [Flags]
    public enum Status
    {

        Normal = 1, Stun = 2, DotDamge = 4, Debuff = 8, Buff = 16
    }

    // 플레이어 랭크 관리 
    public enum Rank
    {
        EX, SSS, SS, S, A, B, C, D, E, F
    }
    // 전투가 현재 어떤 상태인지 구분에서 사용
    public enum BattleResult
    {
        Continue,   // 전투 계속 진행
        PlayerWin,  // 아군 승리
        EnemyWin    // 적군 승리
    }
    //턴 관리에 사용
    public enum TurnOwner
    {
        PlayerSide, // 아군 턴
        EnemySide   // 적군 턴
    }
    // 화면 씬 관리용 
    public enum GameScene
    {
        MainMenu,
        Town,
        GateMenu,
        Battle,
        UnitManagement,
        Shop
    }
    // 게이트 난이도
    public enum GateDifficulty
    {
        Easy,
        Normal,
        Hard
    }
    // 전투 선택지 구분
    public enum BattleCommandType
    {
        Attack,  // 기본 공격
        Skill,   // 스킬 사용
        Item     // 아이템 사용
    }
    // 아이템 목록
    public enum ItemType
    {
        HpPotion,
        MpPotion
    }
    // 아이템 효과 처리
    public enum ItemEffectType
    {
        HealHp,
        HealMp
    }
    // 상태이상 처리
    public enum StatusEffectType
    {
        None,
        Stun,
        Poison,
        Bleed
    }
}
