// BulletLogicType에 따라 알맞은 IBulletLogic을 반환해주는 팩토리
public static class BulletLogicFactory
{
    private static readonly NormalBulletLogic Normal = new NormalBulletLogic();

    public static IBulletLogic Create(BulletLogicType type)
    {
        return type switch
        {
            BulletLogicType.Normal => Normal,
            _ => Normal,
        };
    }
}