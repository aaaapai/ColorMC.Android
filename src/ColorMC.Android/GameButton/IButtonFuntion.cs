namespace ColorMC.Android.GameButton;

public interface IButtonFuntion
{
    public bool IsEdit { get; }

    public void ShowSetting();
    public void NextGroup();
    public void LastGroup();
    public void GoEdit(ButtonData data);
}
