using LVGLSharp.Forms;
using LVGLSharp.WPF;

namespace LVGLSharp.WPF.Controls;

public class UIElement : Control
{
}

public class UIElementCollection
{
    private readonly ControlCollection _controls;

    internal UIElementCollection(ControlCollection controls)
    {
        _controls = controls;
    }

    public void Add(LVGLSharp.Forms.Control element)
    {
        _controls.Add(element);
    }
}

public abstract class LvglControlAdapter<TControl> : UIElement where TControl : Control
{
    private Thickness _margin;

    public new Thickness Margin
    {
        get => _margin;
        set
        {
            _margin = value;
            Left = (int)Math.Round(value.Left);
            Top = (int)Math.Round(value.Top);
        }
    }

    public HorizontalAlignment HorizontalAlignment { get; set; }

    public VerticalAlignment VerticalAlignment { get; set; }
}

public class Grid : LVGLSharp.WPF.Grid
{
    private readonly UIElementCollection _children;

    public Grid()
    {
        _children = new UIElementCollection(Controls);
    }

    public UIElementCollection Children => _children;
}

public class StackPanel : LVGLSharp.WPF.StackPanel
{
    private readonly UIElementCollection _children;

    public StackPanel()
    {
        _children = new UIElementCollection(Controls);
    }

    public UIElementCollection Children => _children;
}

public class Button : LVGLSharp.WPF.Button
{
    public object? Content
    {
        get => Text;
        set => Text = value?.ToString() ?? string.Empty;
    }

    public new Thickness Margin
    {
        set
        {
            Left = (int)Math.Round(value.Left);
            Top = (int)Math.Round(value.Top);
        }
    }

    public HorizontalAlignment HorizontalAlignment { get; set; }

    public VerticalAlignment VerticalAlignment { get; set; }
}

public class CheckBox : LVGLSharp.WPF.CheckBox
{
    public object? Content
    {
        get => Text;
        set => Text = value?.ToString() ?? string.Empty;
    }

    public bool? IsChecked
    {
        get => Checked;
        set => Checked = value ?? false;
    }

    public new Thickness Margin
    {
        set
        {
            Left = (int)Math.Round(value.Left);
            Top = (int)Math.Round(value.Top);
        }
    }

    public HorizontalAlignment HorizontalAlignment { get; set; }

    public VerticalAlignment VerticalAlignment { get; set; }
}

public class ComboBox : LVGLSharp.WPF.ComboBox
{
    public new Thickness Margin
    {
        set
        {
            Left = (int)Math.Round(value.Left);
            Top = (int)Math.Round(value.Top);
        }
    }

    public HorizontalAlignment HorizontalAlignment { get; set; }

    public VerticalAlignment VerticalAlignment { get; set; }
}

public class Label : LVGLSharp.WPF.TextBlock
{
    public object? Content
    {
        get => Text;
        set => Text = value?.ToString() ?? string.Empty;
    }

    public new Thickness Margin
    {
        set
        {
            Left = (int)Math.Round(value.Left);
            Top = (int)Math.Round(value.Top);
        }
    }

    public HorizontalAlignment HorizontalAlignment { get; set; }

    public VerticalAlignment VerticalAlignment { get; set; }
}

public class RadioButton : LVGLSharp.WPF.RadioButton
{
    public object? Content
    {
        get => Text;
        set => Text = value?.ToString() ?? string.Empty;
    }

    public new Thickness Margin
    {
        set
        {
            Left = (int)Math.Round(value.Left);
            Top = (int)Math.Round(value.Top);
        }
    }

    public HorizontalAlignment HorizontalAlignment { get; set; }

    public VerticalAlignment VerticalAlignment { get; set; }
}

public class TextBlock : LVGLSharp.WPF.TextBlock
{
    public TextWrapping TextWrapping { get; set; }

    public new Thickness Margin
    {
        set
        {
            Left = (int)Math.Round(value.Left);
            Top = (int)Math.Round(value.Top);
        }
    }

    public HorizontalAlignment HorizontalAlignment { get; set; }

    public VerticalAlignment VerticalAlignment { get; set; }
}

public class TextBox : LVGLSharp.WPF.TextBox
{
    public TextWrapping TextWrapping { get; set; }

    public new Thickness Margin
    {
        set
        {
            Left = (int)Math.Round(value.Left);
            Top = (int)Math.Round(value.Top);
        }
    }

    public HorizontalAlignment HorizontalAlignment { get; set; }

    public VerticalAlignment VerticalAlignment { get; set; }
}

public class Image : LVGLSharp.WPF.Image
{
    public string? Source
    {
        get => ImageLocation;
        set => ImageLocation = value;
    }

    public new Thickness Margin
    {
        set
        {
            Left = (int)Math.Round(value.Left);
            Top = (int)Math.Round(value.Top);
        }
    }

    public HorizontalAlignment HorizontalAlignment { get; set; }

    public VerticalAlignment VerticalAlignment { get; set; }
}
