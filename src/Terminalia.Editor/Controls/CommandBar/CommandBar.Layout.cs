using Terminalia.Editor.Controls.Inspector;
using Terminalia.Editor.Styling.Layout;

namespace Terminalia.Editor.Controls.CommandBar;

public sealed partial class CommandBar
{
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        _entranceOffset = -56;
        _entranceTimer.Start();
    }

    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);
        CalculateBounds();
        Invalidate();
    }

    public Rectangle GetPanelItemBounds(InspectorSection section)
    {
        var index = section switch
        {
            InspectorSection.Map => 0,
            InspectorSection.Tool => 1,
            InspectorSection.Palette => 2,
            InspectorSection.Run => 3,
            InspectorSection.Game => 4,
            InspectorSection.Object => 5,
            _ => -1
        };
        if (index < 0)
        {
            return Rectangle.Empty;
        }
        var buttonIndex = 0;
        foreach (var item in _items)
        {
            if (item.IsSeparator)
            {
                continue;
            }
            if (buttonIndex == index)
            {
                return item.Bounds;
            }
            buttonIndex++;
        }
        return Rectangle.Empty;
    }

    private void CalculateBounds()
    {
        var sepCount = _items.Count(c => c.IsSeparator);
        var buttonCount = _items.Count - sepCount;
        var available = Height - EditorLayout.BrandSectionHeight - EditorLayout.CommandPadding;
        var sizeOnlyNeeded = buttonCount * EditorLayout.CommandSize + sepCount * SepAdvance;

        int size, gap;
        if (sizeOnlyNeeded <= available)
        {
            size = EditorLayout.CommandSize;
            gap = buttonCount > 1
                ? Math.Min(EditorLayout.CommandGap, (available - sizeOnlyNeeded) / (buttonCount - 1))
                : EditorLayout.CommandGap;
        }
        else
        {
            size = Math.Max(24, (available - Math.Max(0, buttonCount - 1) * MinGap - sepCount * SepAdvance) / Math.Max(1, buttonCount));
            gap = buttonCount > 1
                ? Math.Max(MinGap, (available - buttonCount * size - sepCount * SepAdvance) / (buttonCount - 1))
                : MinGap;
        }

        var x = (Width - size) / 2 + _entranceOffset;
        var y = EditorLayout.BrandSectionHeight;

        foreach (var item in _items)
        {
            if (item.IsSeparator)
            {
                item.Bounds = new Rectangle(x, y + 3, size, 10);
                y += SepAdvance;
            }
            else
            {
                item.Bounds = new Rectangle(x, y, size, size);
                y += size + gap;
            }
        }
    }

    private CommandBarItem? HitTest(Point p)
    {
        foreach (var item in _items)
        {
            if (!item.IsSeparator && item.Bounds.Contains(p))
            {
                return item;
            }
        }
        return null;
    }
}
