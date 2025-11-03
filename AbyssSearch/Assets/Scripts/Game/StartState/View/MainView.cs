public class MainView : UIWindowController
{
    public static MainView Instance;
    public static MainView Open()
    {
        return UIManager.Instance.OpenUI(UINameConst.Main) as MainView;
    }
    public override void PrivateAwake()
    {
        
    }
}