public enum UIWindowType
{
    Normal, //普通的(同类型只有唯一弹窗)
    PopupTip, //弹窗的(同类型可预存多个弹窗，前一个关闭后后续自动弹出)
    Fixed, //常显UI
}

public enum UIWindowLayer
{
    // 必须连续，数值越大，层级越高, todo 精简层级
    None = 0,
    Min,
    Under, //最底层
    Currency, //顶级货币
    Normal, //普通层级
    Tips, //提示层级
    Notice, //公告层级
    Waiting, //菊花层级
    Max //最高层级
}