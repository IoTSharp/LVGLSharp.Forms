using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using LVGLSharp.Forms;
using Controls = LVGLSharp.WPF.Controls;

namespace LVGLSharp.WPF;

public static class XamlRuntimeLoader
{
    public static void LoadIntoWindow(Window window, string xamlRelativePath)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentException.ThrowIfNullOrWhiteSpace(xamlRelativePath);

        var doc = LoadXamlDocument(window.GetType().Assembly, EmbeddedResourceFileSystem.NormalizePath(xamlRelativePath));
        var root = doc.Root ?? throw new InvalidOperationException("XAML root element is missing.");

        if (!string.Equals(root.Name.LocalName, "Window", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Root element must be <Window>.");
        }

        ApplyWindowAttributes(window, root);

        var contentElement = root.Elements().FirstOrDefault();
        if (contentElement is null)
        {
            return;
        }

        window.Content = CreateControlTree(contentElement, window.GetType().Assembly);
    }

    public static ApplicationDefinition? TryLoadApplicationDefinition(System.Reflection.Assembly assembly, string appXamlRelativePath)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentException.ThrowIfNullOrWhiteSpace(appXamlRelativePath);

        var stream = EmbeddedResourceFileSystem.TryOpenRead(assembly, EmbeddedResourceFileSystem.NormalizePath(appXamlRelativePath));
        if (stream is null)
        {
            return null;
        }

        using (stream)
        {
            var doc = XDocument.Load(stream, LoadOptions.None);
            var root = doc.Root;
            if (root is null || !string.Equals(root.Name.LocalName, "Application", StringComparison.Ordinal))
            {
                return null;
            }

            return new ApplicationDefinition
            {
                StartupUri = EmbeddedResourceFileSystem.NormalizePath(GetAttribute(root, "StartupUri"))
            };
        }
    }

    private static XDocument LoadXamlDocument(System.Reflection.Assembly assembly, string xamlRelativePath)
    {
        using var stream = EmbeddedResourceFileSystem.OpenRead(assembly, xamlRelativePath);

        return XDocument.Load(stream, LoadOptions.None);
    }

    public static string NormalizeResourcePath(string? path) => EmbeddedResourceFileSystem.NormalizePath(path);

    private static void ApplyWindowAttributes(Window window, XElement element)
    {
        var title = GetAttribute(element, "Title");
        if (!string.IsNullOrWhiteSpace(title))
        {
            window.Title = title;
        }

        if (TryGetDoubleAttribute(element, "Width", out var width))
        {
            window.Width = width;
        }

        if (TryGetDoubleAttribute(element, "Height", out var height))
        {
            window.Height = height;
        }
    }

    private static Control CreateControlTree(XElement element, System.Reflection.Assembly assembly)
    {
        Control control = CreateControl(element.Name.LocalName);
        ApplyCommonProperties(control, element);
        ApplySpecificProperties(control, element, assembly);

        if (control is Controls.Grid grid)
        {
            var gridLayout = CreateGridLayoutContext(grid, element);
            foreach (var childElement in EnumerateContainerChildren(element, isGrid: true))
            {
                AddChildControl(grid, CreateControlTree(childElement, assembly), childElement, gridLayout);
            }
        }
        else if (control is Controls.StackPanel stackPanel)
        {
            foreach (var childElement in EnumerateContainerChildren(element, isGrid: false))
            {
                AddChildControl(stackPanel, CreateControlTree(childElement, assembly), childElement, gridLayout: null);
            }
        }

        return control;
    }

    private static Control CreateControl(string elementName)
    {
        return elementName switch
        {
            "Grid" => new Controls.Grid(),
            "StackPanel" => new Controls.StackPanel(),
            "Button" => new Controls.Button(),
            "CheckBox" => new Controls.CheckBox(),
            "ComboBox" => new Controls.ComboBox(),
            "Label" => new Controls.Label(),
            "RadioButton" => new Controls.RadioButton(),
            "TextBlock" => new Controls.TextBlock(),
            "TextBox" => new Controls.TextBox(),
            "Image" => new Controls.Image(),
            _ => throw new NotSupportedException($"Unsupported XAML element: <{elementName}>.")
        };
    }

    private static void ApplyCommonProperties(Control control, XElement element)
    {
        if (TryGetIntAttribute(element, "Width", out var width))
        {
            control.Width = width;
        }

        if (TryGetIntAttribute(element, "Height", out var height))
        {
            control.Height = height;
        }

        if (TryGetThicknessAttribute(element, "Margin", out var margin))
        {
            ApplyMargin(control, margin);
        }

        if (TryGetEnumAttribute<HorizontalAlignment>(element, "HorizontalAlignment", out var horizontalAlignment))
        {
            ApplyHorizontalAlignment(control, horizontalAlignment);
        }

        if (TryGetEnumAttribute<VerticalAlignment>(element, "VerticalAlignment", out var verticalAlignment))
        {
            ApplyVerticalAlignment(control, verticalAlignment);
        }
    }

    private static void ApplySpecificProperties(Control control, XElement element, System.Reflection.Assembly assembly)
    {
        switch (control)
        {
            case Controls.Button button:
                button.Content = GetAttribute(element, "Content") ?? button.Content;
                break;
            case Controls.CheckBox checkBox:
                checkBox.Content = GetAttribute(element, "Content") ?? checkBox.Content;
                if (TryGetBoolAttribute(element, "IsChecked", out var checkBoxIsChecked))
                {
                    checkBox.IsChecked = checkBoxIsChecked;
                }
                break;
            case Controls.Label label:
                label.Content = GetAttribute(element, "Content") ?? label.Content;
                break;
            case Controls.RadioButton radioButton:
                radioButton.Content = GetAttribute(element, "Content") ?? radioButton.Content;
                if (TryGetBoolAttribute(element, "IsChecked", out var radioButtonIsChecked))
                {
                    radioButton.Checked = radioButtonIsChecked;
                }
                break;
            case Controls.ComboBox comboBox:
                ApplyComboBoxItems(comboBox, element);
                if (TryGetIntAttribute(element, "SelectedIndex", out var selectedIndex))
                {
                    comboBox.SelectedIndex = selectedIndex;
                }
                break;
            case Controls.StackPanel stackPanel:
                if (TryGetEnumAttribute<Orientation>(element, "Orientation", out var orientation))
                {
                    stackPanel.Orientation = orientation;
                }
                break;
            case Controls.TextBlock textBlock:
                textBlock.Text = GetAttribute(element, "Text") ?? textBlock.Text;
                if (TryGetEnumAttribute<TextWrapping>(element, "TextWrapping", out var textWrapping))
                {
                    textBlock.TextWrapping = textWrapping;
                }
                break;
            case Controls.TextBox textBox:
                textBox.Text = GetAttribute(element, "Text") ?? textBox.Text;
                if (TryGetEnumAttribute<TextWrapping>(element, "TextWrapping", out var textBoxWrapping))
                {
                    textBox.TextWrapping = textBoxWrapping;
                }
                break;
            case Controls.Image image:
                var source = GetAttribute(element, "Source");
                if (!string.IsNullOrWhiteSpace(source))
                {
                    image.Source = ResolveImageSourcePath(assembly, source);
                }
                break;
        }
    }

    private static string ResolveImageSourcePath(System.Reflection.Assembly assembly, string source)
    {
        var normalized = NormalizeImageSource(source);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return normalized;
        }

        if (File.Exists(normalized))
        {
            return normalized;
        }

        if (!EmbeddedResourceFileSystem.Exists(assembly, normalized))
        {
            return normalized;
        }

        string cacheDir = Path.Combine(AppContext.BaseDirectory, ".lvglsharp-wpf-assets");
        return EmbeddedResourceFileSystem.MaterializeToCache(assembly, normalized, cacheDir);
    }

    private static string NormalizeImageSource(string source)
    {
        var normalized = source.Trim();
        if (normalized.StartsWith("/", StringComparison.Ordinal))
        {
            normalized = normalized.TrimStart('/');
        }

        return normalized;
    }

    private static void ApplyMargin(Control control, Thickness margin)
    {
        switch (control)
        {
            case Controls.Button button:
                button.Margin = margin;
                break;
            case Controls.CheckBox checkBox:
                checkBox.Margin = margin;
                break;
            case Controls.ComboBox comboBox:
                comboBox.Margin = margin;
                break;
            case Controls.Label label:
                label.Margin = margin;
                break;
            case Controls.RadioButton radioButton:
                radioButton.Margin = margin;
                break;
            case Controls.TextBlock textBlock:
                textBlock.Margin = margin;
                break;
            case Controls.TextBox textBox:
                textBox.Margin = margin;
                break;
            case Controls.Image image:
                image.Margin = margin;
                break;
        }
    }

    private static void ApplyHorizontalAlignment(Control control, HorizontalAlignment value)
    {
        switch (control)
        {
            case Controls.Button button:
                button.HorizontalAlignment = value;
                break;
            case Controls.CheckBox checkBox:
                checkBox.HorizontalAlignment = value;
                break;
            case Controls.ComboBox comboBox:
                comboBox.HorizontalAlignment = value;
                break;
            case Controls.Label label:
                label.HorizontalAlignment = value;
                break;
            case Controls.RadioButton radioButton:
                radioButton.HorizontalAlignment = value;
                break;
            case Controls.TextBlock textBlock:
                textBlock.HorizontalAlignment = value;
                break;
            case Controls.TextBox textBox:
                textBox.HorizontalAlignment = value;
                break;
            case Controls.Image image:
                image.HorizontalAlignment = value;
                break;
        }
    }

    private static void ApplyVerticalAlignment(Control control, VerticalAlignment value)
    {
        switch (control)
        {
            case Controls.Button button:
                button.VerticalAlignment = value;
                break;
            case Controls.CheckBox checkBox:
                checkBox.VerticalAlignment = value;
                break;
            case Controls.ComboBox comboBox:
                comboBox.VerticalAlignment = value;
                break;
            case Controls.Label label:
                label.VerticalAlignment = value;
                break;
            case Controls.RadioButton radioButton:
                radioButton.VerticalAlignment = value;
                break;
            case Controls.TextBlock textBlock:
                textBlock.VerticalAlignment = value;
                break;
            case Controls.TextBox textBox:
                textBox.VerticalAlignment = value;
                break;
            case Controls.Image image:
                image.VerticalAlignment = value;
                break;
        }
    }

    private static bool TryGetIntAttribute(XElement element, string name, out int value)
    {
        value = default;
        var text = GetAttribute(element, name);
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
        {
            return false;
        }

        value = (int)Math.Round(doubleValue);
        return true;
    }

    private static bool TryGetBoolAttribute(XElement element, string name, out bool value)
    {
        value = default;
        var text = GetAttribute(element, name);
        return !string.IsNullOrWhiteSpace(text) && bool.TryParse(text, out value);
    }

    private static bool TryGetDoubleAttribute(XElement element, string name, out double value)
    {
        value = default;
        var text = GetAttribute(element, name);
        return !string.IsNullOrWhiteSpace(text) &&
               double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryGetThicknessAttribute(XElement element, string name, out Thickness thickness)
    {
        thickness = default;
        var text = GetAttribute(element, name);
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var parts = text.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1 && TryParseDouble(parts[0], out var all))
        {
            thickness = new Thickness(all);
            return true;
        }

        if (parts.Length == 2 && TryParseDouble(parts[0], out var horizontal) && TryParseDouble(parts[1], out var vertical))
        {
            thickness = new Thickness(horizontal, vertical, horizontal, vertical);
            return true;
        }

        if (parts.Length == 4 &&
            TryParseDouble(parts[0], out var left) &&
            TryParseDouble(parts[1], out var top) &&
            TryParseDouble(parts[2], out var right) &&
            TryParseDouble(parts[3], out var bottom))
        {
            thickness = new Thickness(left, top, right, bottom);
            return true;
        }

        return false;
    }

    private static bool TryGetEnumAttribute<TEnum>(XElement element, string name, out TEnum value)
        where TEnum : struct, Enum
    {
        value = default;
        var text = GetAttribute(element, name);
        return !string.IsNullOrWhiteSpace(text) && Enum.TryParse(text, ignoreCase: true, out value);
    }

    private static string? GetAttribute(XElement element, string name)
    {
        var direct = element.Attribute(name);
        if (direct is not null)
        {
            return direct.Value;
        }

        var match = element.Attributes().FirstOrDefault(attribute =>
            string.Equals(attribute.Name.LocalName, name, StringComparison.Ordinal) ||
            string.Equals(attribute.Name.ToString(), name, StringComparison.Ordinal));

        return match?.Value;
    }

    private static bool TryParseDouble(string text, out double value)
    {
        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static void ApplyComboBoxItems(Controls.ComboBox comboBox, XElement element)
    {
        foreach (var child in element.Elements())
        {
            if (string.Equals(child.Name.LocalName, "ComboBoxItem", StringComparison.Ordinal))
            {
                var content = GetAttribute(child, "Content");
                if (!string.IsNullOrWhiteSpace(content))
                {
                    comboBox.Items.Add(content);
                    continue;
                }

                var text = child.Value?.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    comboBox.Items.Add(text);
                }
            }
        }
    }

    private static IEnumerable<XElement> EnumerateContainerChildren(XElement containerElement, bool isGrid)
    {
        foreach (var child in containerElement.Elements())
        {
            if (isGrid && IsGridDefinitionCollectionElement(child))
            {
                continue;
            }

            yield return child;
        }
    }

    private static bool IsGridDefinitionCollectionElement(XElement element)
    {
        var name = element.Name.LocalName;
        return string.Equals(name, "Grid.RowDefinitions", StringComparison.Ordinal) ||
               string.Equals(name, "Grid.ColumnDefinitions", StringComparison.Ordinal);
    }

    private static GridLayoutContext CreateGridLayoutContext(Controls.Grid grid, XElement gridElement)
    {
        var rowDefinitions = ReadGridDefinitions(gridElement, "Grid.RowDefinitions", "RowDefinition", "Height");
        var columnDefinitions = ReadGridDefinitions(gridElement, "Grid.ColumnDefinitions", "ColumnDefinition", "Width");

        var maxAttachedRow = 0;
        var maxAttachedColumn = 0;
        foreach (var child in EnumerateContainerChildren(gridElement, isGrid: true))
        {
            if (TryGetAttachedIntAttribute(child, "Grid.Row", out var row) && row > maxAttachedRow)
            {
                maxAttachedRow = row;
            }

            if (TryGetAttachedIntAttribute(child, "Grid.Column", out var column) && column > maxAttachedColumn)
            {
                maxAttachedColumn = column;
            }
        }

        var rowCount = Math.Max(1, Math.Max(rowDefinitions.Count, maxAttachedRow + 1));
        var columnCount = Math.Max(1, Math.Max(columnDefinitions.Count, maxAttachedColumn + 1));

        var totalHeight = grid.Height > 0 ? grid.Height : 1;
        var totalWidth = grid.Width > 0 ? grid.Width : 1;

        var rowHeights = ComputeGridCellLengths(totalHeight, rowCount, rowDefinitions);
        var columnWidths = ComputeGridCellLengths(totalWidth, columnCount, columnDefinitions);

        return new GridLayoutContext(rowHeights, columnWidths);
    }

    private static List<GridDefinitionSpec> ReadGridDefinitions(XElement gridElement, string collectionElementName, string itemElementName, string valueAttributeName)
    {
        var result = new List<GridDefinitionSpec>();
        var collectionElement = gridElement.Elements().FirstOrDefault(e => string.Equals(e.Name.LocalName, collectionElementName, StringComparison.Ordinal));
        if (collectionElement is null)
        {
            return result;
        }

        foreach (var definition in collectionElement.Elements())
        {
            if (!string.Equals(definition.Name.LocalName, itemElementName, StringComparison.Ordinal))
            {
                continue;
            }

            var raw = GetAttribute(definition, valueAttributeName);
            result.Add(ParseGridDefinition(raw));
        }

        return result;
    }

    private static GridDefinitionSpec ParseGridDefinition(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GridDefinitionSpec.Auto();
        }

        var text = value.Trim();
        if (string.Equals(text, "Auto", StringComparison.OrdinalIgnoreCase))
        {
            return GridDefinitionSpec.Auto();
        }

        if (text.EndsWith('*'))
        {
            var weightText = text[..^1].Trim();
            if (string.IsNullOrWhiteSpace(weightText))
            {
                return GridDefinitionSpec.Star(1d);
            }

            return TryParseDouble(weightText, out var starWeight)
                ? GridDefinitionSpec.Star(Math.Max(0.1d, starWeight))
                : GridDefinitionSpec.Star(1d);
        }

        return TryParseDouble(text, out var absolute)
            ? GridDefinitionSpec.Absolute(Math.Max(0d, absolute))
            : GridDefinitionSpec.Auto();
    }

    private static int[] ComputeGridCellLengths(int totalLength, int fallbackCount, List<GridDefinitionSpec> definitions)
    {
        var count = Math.Max(1, Math.Max(definitions.Count, fallbackCount));
        var lengths = new int[count];

        if (definitions.Count == 0)
        {
            var each = count > 0 ? totalLength / count : totalLength;
            for (var i = 0; i < count; i++)
            {
                lengths[i] = each;
            }

            return lengths;
        }

        var remaining = totalLength;
        var flexWeight = 0d;

        for (var i = 0; i < count; i++)
        {
            var spec = i < definitions.Count ? definitions[i] : GridDefinitionSpec.Auto();
            if (spec.Mode == GridDefinitionMode.Absolute)
            {
                lengths[i] = Math.Max(0, (int)Math.Round(spec.Value));
                remaining -= lengths[i];
            }
            else
            {
                flexWeight += spec.Mode == GridDefinitionMode.Star ? spec.Value : 1d;
            }
        }

        remaining = Math.Max(0, remaining);
        if (flexWeight <= 0d)
        {
            return lengths;
        }

        for (var i = 0; i < count; i++)
        {
            var spec = i < definitions.Count ? definitions[i] : GridDefinitionSpec.Auto();
            if (spec.Mode == GridDefinitionMode.Absolute)
            {
                continue;
            }

            var weight = spec.Mode == GridDefinitionMode.Star ? spec.Value : 1d;
            lengths[i] = Math.Max(lengths[i], (int)Math.Round(remaining * (weight / flexWeight)));
        }

        return lengths;
    }

    private static bool TryGetAttachedIntAttribute(XElement element, string fullName, out int value)
    {
        value = default;
        var text = GetAttribute(element, fullName);
        if (string.IsNullOrWhiteSpace(text))
        {
            var dotIndex = fullName.IndexOf('.', StringComparison.Ordinal);
            if (dotIndex >= 0 && dotIndex < fullName.Length - 1)
            {
                text = GetAttribute(element, fullName[(dotIndex + 1)..]);
            }
        }

        return !string.IsNullOrWhiteSpace(text) && int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private static int SumRange(IReadOnlyList<int> values, int start, int length)
    {
        var end = Math.Min(values.Count, start + Math.Max(0, length));
        var total = 0;
        for (var i = start; i < end; i++)
        {
            total += values[i];
        }

        return total;
    }

    private static int OffsetOf(IReadOnlyList<int> values, int index)
    {
        var total = 0;
        var limit = Math.Min(values.Count, Math.Max(0, index));
        for (var i = 0; i < limit; i++)
        {
            total += values[i];
        }

        return total;
    }

    private static void AddChildControl(Control parent, Control child, XElement childElement, GridLayoutContext? gridLayout)
    {
        switch (parent)
        {
            case Controls.Grid grid:
                LayoutGridChild(child, childElement, gridLayout ?? throw new InvalidOperationException("Grid layout context is required."));
                grid.Children.Add(child);
                break;
            case Controls.StackPanel stackPanel:
                LayoutStackPanelChild(stackPanel, child);
                stackPanel.Children.Add(child);
                break;
        }
    }

    private static void LayoutGridChild(Control child, XElement childElement, GridLayoutContext gridLayout)
    {
        var hasRow = TryGetAttachedIntAttribute(childElement, "Grid.Row", out var row);
        var hasColumn = TryGetAttachedIntAttribute(childElement, "Grid.Column", out var column);

        if (!hasRow && !hasColumn)
        {
            row = gridLayout.NextAutoIndex / gridLayout.ColumnCount;
            column = gridLayout.NextAutoIndex % gridLayout.ColumnCount;
            gridLayout.NextAutoIndex++;
        }
        else
        {
            if (!hasRow)
            {
                row = 0;
            }

            if (!hasColumn)
            {
                column = 0;
            }
        }

        row = Math.Clamp(row, 0, gridLayout.RowCount - 1);
        column = Math.Clamp(column, 0, gridLayout.ColumnCount - 1);

        var rowSpan = TryGetAttachedIntAttribute(childElement, "Grid.RowSpan", out var parsedRowSpan)
            ? Math.Max(1, parsedRowSpan)
            : 1;
        var columnSpan = TryGetAttachedIntAttribute(childElement, "Grid.ColumnSpan", out var parsedColumnSpan)
            ? Math.Max(1, parsedColumnSpan)
            : 1;

        var x = OffsetOf(gridLayout.ColumnWidths, column);
        var y = OffsetOf(gridLayout.RowHeights, row);
        var width = SumRange(gridLayout.ColumnWidths, column, columnSpan);
        var height = SumRange(gridLayout.RowHeights, row, rowSpan);

        child.Left += x;
        child.Top += y;

        if (child.Width <= 0 && width > 0)
        {
            child.Width = width;
        }

        if (child.Height <= 0 && height > 0)
        {
            child.Height = height;
        }
    }

    private static void LayoutStackPanelChild(Controls.StackPanel stackPanel, Control child)
    {
        if (stackPanel.Controls.Count == 0)
        {
            return;
        }

        var lastChild = stackPanel.Controls[stackPanel.Controls.Count - 1];
        if (lastChild is null)
        {
            return;
        }

        if (stackPanel.Orientation == Orientation.Horizontal)
        {
            child.Left += lastChild.Right;
        }
        else
        {
            child.Top += lastChild.Bottom;
        }
    }

    private sealed class GridLayoutContext
    {
        public GridLayoutContext(int[] rowHeights, int[] columnWidths)
        {
            RowHeights = rowHeights;
            ColumnWidths = columnWidths;
        }

        public int[] RowHeights { get; }

        public int[] ColumnWidths { get; }

        public int RowCount => RowHeights.Length;

        public int ColumnCount => ColumnWidths.Length;

        public int NextAutoIndex { get; set; }
    }

    private enum GridDefinitionMode
    {
        Absolute,
        Star,
        Auto,
    }

    private readonly struct GridDefinitionSpec
    {
        private GridDefinitionSpec(GridDefinitionMode mode, double value)
        {
            Mode = mode;
            Value = value;
        }

        public GridDefinitionMode Mode { get; }

        public double Value { get; }

        public static GridDefinitionSpec Absolute(double value) => new(GridDefinitionMode.Absolute, value);

        public static GridDefinitionSpec Star(double value) => new(GridDefinitionMode.Star, value);

        public static GridDefinitionSpec Auto() => new(GridDefinitionMode.Auto, 0d);
    }

    public sealed class ApplicationDefinition
    {
        public string? StartupUri { get; init; }
    }
}