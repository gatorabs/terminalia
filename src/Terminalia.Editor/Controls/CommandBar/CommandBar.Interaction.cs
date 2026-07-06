using Terminalia.Editor.Styling.Colors;

namespace Terminalia.Editor.Controls.CommandBar;

public sealed partial class CommandBar
{
    protected override void OnMouseMove(MouseEventArgs e)
    {
        var hit = HitTest(e.Location);
        if (hit == _hoverItem)
        {
            return;
        }

        _hoverItem = hit;
        Cursor = hit is not null ? Cursors.Hand : Cursors.Default;

        foreach (var item in _items)
        {
            if (item.IsSeparator)
            {
                continue;
            }
            item.TargetColor = item == hit
                ? (item.IsPrimary ? EditorColors.AccentHover : EditorColors.CommandHover)
                : NormalColor(item);
        }

        if (hit is not null)
        {
            _toolTip.Show(hit.Caption, this, new Point(Width - 8, hit.Bounds.Top + 8), 3000);
        }
        else
        {
            _toolTip.Hide(this);
        }

        StartAnim();
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hoverItem = null;
        _pressItem = null;
        Cursor = Cursors.Default;
        _toolTip.Hide(this);

        foreach (var item in _items)
        {
            if (!item.IsSeparator)
            {
                item.TargetColor = NormalColor(item);
            }
        }

        StartAnim();
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }
        var hit = HitTest(e.Location);
        if (hit is null)
        {
            return;
        }

        _pressItem = hit;
        hit.Pulse = 1;
        hit.TargetColor = hit.IsPrimary ? EditorColors.AccentPressed : EditorColors.ButtonPressed;
        StartAnim();
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (_pressItem is null)
        {
            return;
        }
        var pressed = _pressItem;
        _pressItem = null;

        pressed.TargetColor = pressed == _hoverItem
            ? (pressed.IsPrimary ? EditorColors.AccentHover : EditorColors.CommandHover)
            : NormalColor(pressed);

        if (pressed == _hoverItem)
        {
            pressed.Click?.Invoke();
        }

        StartAnim();
        Invalidate();
    }

    private void EntranceTick()
    {
        _entranceOffset = (int)Math.Round(_entranceOffset * 0.66);
        if (Math.Abs(_entranceOffset) <= 1)
        {
            _entranceOffset = 0;
            _entranceTimer.Stop();
        }
        CalculateBounds();
        Invalidate();
    }

    private void AnimTick()
    {
        var allDone = true;
        foreach (var item in _items)
        {
            if (item.IsSeparator)
            {
                continue;
            }
            item.BackColor = EditorColors.Lerp(item.BackColor, item.TargetColor, 0.32);
            item.Pulse *= 0.72;
            if (EditorColors.ColorDistance(item.BackColor, item.TargetColor) < 3 && item.Pulse < 0.01)
            {
                item.BackColor = item.TargetColor;
                item.Pulse = 0;
            }
            else
            {
                allDone = false;
            }
        }
        if (allDone)
        {
            _animTimer.Stop();
        }
        Invalidate();
    }

    private void StartAnim()
    {
        if (!_animTimer.Enabled)
        {
            _animTimer.Start();
        }
    }
}
