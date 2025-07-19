// BulletLogicType에 따라 알맞은 IBulletLogic을 반환해주는 팩토리
public static class BulletLogicFactory
{
    // 싱글톤 패턴으로 메모리 절약
    private static readonly NormalBulletLogic Normal = new NormalBulletLogic();

    /// <summary>
    /// 타입에 맞는 총알 로직 반환
    /// </summary>
    /// <param name="type">총알 로직 타입</param>
    /// <returns>해당하는 IBulletLogic 구현체</returns>
    public static IBulletLogic Create(BulletLogicType type)
    {
        return type switch
        {
            BulletLogicType.Normal => Normal,
            _ => Normal,  // 기본값은 일반 총알
        };
    }
}