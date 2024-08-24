namespace ColorMC.Android;

public enum GameRenderBG
{ 
    ColorMC, Pojav
}

public record PhoneConfigObj
{
    public bool LwjglVk { get; set; }
    public GameRenderBG GameRender { get; set; }
}
