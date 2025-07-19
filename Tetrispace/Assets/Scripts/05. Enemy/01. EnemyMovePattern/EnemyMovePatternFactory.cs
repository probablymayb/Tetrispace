public class EnemyMovePatternFactory
{
    public static IEnemyMovePattern Create(EnemyType type)
    {
        return type switch
        {
            EnemyType.I => new IMovePattern(),
            EnemyType.S => new SMovePattern(),
            EnemyType.L => new LMovePattern(),
            EnemyType.Z => new ZMovePattern(),
            EnemyType.O => new OMovePattern(),
            EnemyType.T => new TMovePattern(),
            _ => new IMovePattern(),
        };
    }
}
