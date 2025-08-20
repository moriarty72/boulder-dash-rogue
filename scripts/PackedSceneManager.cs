using Godot;

public static class PackedSceneManager
{
    public static PackedScene RockfordScene = GD.Load<PackedScene>("res://scenes//rockford.tscn");
    public static PackedScene MudScene = GD.Load<PackedScene>("res://scenes//mud-1.tscn");
    public static PackedScene RockScene = GD.Load<PackedScene>("res://scenes//rock.tscn");
    public static PackedScene DiamondScene = GD.Load<PackedScene>("res://scenes//diamond.tscn");
    public static PackedScene MetalWallScene = GD.Load<PackedScene>("res://scenes//metal-wall.tscn");
    public static PackedScene ExplosionScene = GD.Load<PackedScene>("res://scenes//explosion.tscn");
    public static PackedScene EnemySquareScene = GD.Load<PackedScene>("res://scenes//enemy-square.tscn");
    public static PackedScene EnemyButterflyScene = GD.Load<PackedScene>("res://scenes//enemy-butterfly.tscn");
    public static PackedScene AmoebaScene = GD.Load<PackedScene>("res://scenes//amoeba.tscn");
    public static PackedScene DoorScene = GD.Load<PackedScene>("res://scenes//door.tscn");
    public static PackedScene KeyScene = GD.Load<PackedScene>("res://scenes//key.tscn");
}