using UnityEngine;

/// <summary>
/// Loads sliced visual-pack sprites from Resources/ChickenArt.
/// </summary>
public static class VisualAssets
{
    private static Sprite Load(string path)
    {
        return Resources.Load<Sprite>("ChickenArt/" + path);
    }

    public static Sprite ShipHealthy => Load("Player/ship_healthy");
    public static Sprite ShipDamaged => Load("Player/ship_damaged");
    public static Sprite Laser1 => Load("Player/laser_1");
    public static Sprite Laser2 => Load("Player/laser_2");
    public static Sprite Laser3 => Load("Player/laser_3");
    public static Sprite Missile => Load("Player/missile");

    public static Sprite Egg => Load("Props/egg");
    public static Sprite EggCracked => Load("Props/egg_cracked");
    public static Sprite AsteroidLarge => Load("Props/asteroid_large");
    public static Sprite AsteroidMedium => Load("Props/asteroid_medium");
    public static Sprite WeaponGift => Load("Pickups/weapon_gift");
    public static Sprite FoodDrumstick => Load("Pickups/food_drumstick");
    public static Sprite FoodBurger => Load("Pickups/food_burger");
    public static Sprite BombIcon => Load("Pickups/bomb");
    public static Sprite Heart => Load("Pickups/heart");

    public static Sprite BgLevel1 => Load("Backgrounds/bg_level1");
    public static Sprite BgLevel2 => Load("Backgrounds/bg_level2");
    public static Sprite BgLevel3 => Load("Backgrounds/bg_level3");

    public static Sprite TitleBanner => Load("UI/title_banner");
    public static Sprite BtnNewGame => Load("UI/btn_new_game");
    public static Sprite BtnContinue => Load("UI/btn_continue");
    public static Sprite BtnQuit => Load("UI/btn_quit");
    public static Sprite BtnExit => Load("UI/btn_exit") ?? BtnQuit;
    public static Sprite BtnResume => Load("UI/btn_resume");
    public static Sprite BtnMainMenu => Load("UI/btn_main_menu");
    public static Sprite BtnRetry => Load("UI/btn_retry");
    public static Sprite BtnLevel1 => Load("UI/btn_level_1");
    public static Sprite BtnLevel2 => Load("UI/btn_level_2");
    public static Sprite BtnLevel3 => Load("UI/btn_level_3");
    public static Sprite BannerRoundComplete => Load("UI/banner_round_complete");
    public static Sprite BannerStageComplete => Load("UI/banner_stage_complete");
    public static Sprite BannerGameOver => Load("UI/banner_game_over");
    public static Sprite BannerVictory => Load("UI/banner_victory");
    public static Sprite PanelFrame => Load("UI/panel_frame");

    public static Sprite BossNormal => Load("Enemies/normal/boss");
    public static Sprite BossArmored => Load("Enemies/armored/boss");
    public static Sprite BossAlien => Load("Enemies/alien/boss");

    public static Sprite[] ExplosionFrames => new[]
    {
        Load("VFX/explosion_0"),
        Load("VFX/explosion_1"),
        Load("VFX/explosion_2"),
        Load("VFX/explosion_3")
    };

    public static Sprite[] ExhaustFrames => new[]
    {
        Load("VFX/exhaust_0"),
        Load("VFX/exhaust_1"),
        Load("VFX/exhaust_2"),
        Load("VFX/exhaust_3")
    };

    public static Sprite[] ChickenFrames(string type)
    {
        return new[]
        {
            Load($"Enemies/{type}/chicken_0"),
            Load($"Enemies/{type}/chicken_1"),
            Load($"Enemies/{type}/chicken_2"),
            Load($"Enemies/{type}/chicken_3")
        };
    }
}
