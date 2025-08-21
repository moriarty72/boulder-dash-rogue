public partial class KeyController : BaseGridObjectController
{
    public DoorController.Color keyColor;

    public void SetKeyColor(DoorController.Color color)
    {
        keyColor = color;
        (NodeObject as Key).SetVisibleByColor((int)color);
    }

    public override void Respawn()
    {
        base.Respawn();

        if (NodeObject != null)
            (NodeObject as Key).SetVisibleByColor((int)keyColor);
    }
}