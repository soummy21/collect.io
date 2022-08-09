using UnityEngine;

public class Identifiers
{
    public const string PhotonPrefabPath = "PhotonPrefabs";
    public const string PlayerManager = "PlayerManager";
    public const string PlayerController = "PlayerController";
    public const string PlayerCamera = "PlayerCamera";
    public const string MainMenu = "MainMenu";

    public const string Player = "Player";
    public const string Pickup = "Pickup";
    public const string TeamThatScored = "TeamThatScored";
    public const string PickupHit = "FirstPickupHit";

    public static int Move = Animator.StringToHash("isRunning");
}
