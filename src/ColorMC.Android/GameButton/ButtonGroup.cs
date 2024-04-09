using System.Collections.Generic;

namespace ColorMC.Android.GameButton;

public record ButtonGroup
{
    public string Name { get; set; }

    public List<ButtonData> Buttons { get; set; }
}
