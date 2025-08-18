public partial class KeyController : BaseGridObjectController
{
    public DoorController.Color keyColor;

    public void SetKeyColor(DoorController.Color color)
    {
        keyColor = color;
        (NodeObject as Key).SetVisibleByColor((int)color);
    }
}