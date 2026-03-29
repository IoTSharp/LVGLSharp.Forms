namespace LVGLSharp.WPF;

public class Button : LVGLSharp.Forms.Button
{
}

public class CheckBox : LVGLSharp.Forms.CheckBox
{
}

public class RadioButton : LVGLSharp.Forms.RadioButton
{
}

public class ComboBox : LVGLSharp.Forms.ComboBox
{
}

public class Image : LVGLSharp.Forms.PictureBox
{
}

public class TextBlock : LVGLSharp.Forms.Label
{
}

public class Label : LVGLSharp.Forms.Label
{
}

public class TextBox : LVGLSharp.Forms.TextBox
{
}

public class ProgressBar : LVGLSharp.Forms.ProgressBar
{
}

public class Grid : LVGLSharp.Forms.TableLayoutPanel
{
}

public class StackPanel : LVGLSharp.Forms.FlowLayoutPanel
{
    private Orientation _orientation = Orientation.Vertical;

    public StackPanel()
    {
        PreserveChildLocations = false;
        WrapContents = false;
        AutoMeasureWidth = 96;
        AutoMeasureHeight = 28;
        FlowHorizontally = false;
    }

    public Orientation Orientation
    {
        get => _orientation;
        set
        {
            _orientation = value;
            FlowHorizontally = value == LVGLSharp.WPF.Orientation.Horizontal;
        }
    }
}
