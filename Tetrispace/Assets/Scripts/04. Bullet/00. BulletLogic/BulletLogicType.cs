// 탄막의 로직 타입에 따라 추가 후, BulletLogicFactory에 IBulletLogic을 구현한 클래스 연결
public enum BulletLogicType
{
    Normal,
    CCW,
    CW,
}