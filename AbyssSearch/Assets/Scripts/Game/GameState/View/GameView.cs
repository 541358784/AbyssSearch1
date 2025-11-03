public class GameView : UIWindowController
{
    public static GameView Instance;
    public static GameView Open()
    {
        return UIManager.Instance.OpenUI(UINameConst.Game) as GameView;
    }
    public override void PrivateAwake()
    {
        
    }
}