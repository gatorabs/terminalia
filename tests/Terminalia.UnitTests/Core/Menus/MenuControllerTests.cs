using Terminalia.Core.Input;
using Terminalia.Core.Menus;

namespace Terminalia.UnitTests.Core.Menus;

public sealed class MenuControllerTests
{
    [Fact]
    public void MenuController_Should_Navigate_Down_With_Wrap()
    {
        var controller = CreateController();

        controller.Handle(InputCommand.MoveDown);
        Assert.Equal(1, controller.SelectedIndex);

        controller.Handle(InputCommand.MoveDown);
        controller.Handle(InputCommand.MoveDown);
        Assert.Equal(0, controller.SelectedIndex);
    }

    [Fact]
    public void MenuController_Should_Navigate_Up_With_Wrap()
    {
        var controller = CreateController();

        controller.Handle(InputCommand.MoveUp);

        Assert.Equal(2, controller.SelectedIndex);
    }

    [Fact]
    public void MenuController_Should_Return_Action_Of_Selected_Item_On_Confirm()
    {
        var controller = CreateController();
        controller.Handle(InputCommand.MoveDown);

        var action = controller.Handle(InputCommand.Confirm);

        Assert.Equal(MenuAction.ReturnToTitle, action);
    }

    [Fact]
    public void MenuController_Should_Ignore_Unrelated_Commands()
    {
        var controller = CreateController();

        Assert.Null(controller.Handle(InputCommand.MoveLeft));
        Assert.Null(controller.Handle(InputCommand.MoveRight));
        Assert.Null(controller.Handle(InputCommand.None));
        Assert.Equal(0, controller.SelectedIndex);
    }

    [Fact]
    public void MenuController_Should_Handle_Single_Item_Menu()
    {
        var menu = new MenuDefinition("solo", [new MenuItemDefinition("quit", MenuAction.QuitGame)]);
        var controller = new MenuController(menu);

        controller.Handle(InputCommand.MoveDown);
        controller.Handle(InputCommand.MoveUp);

        Assert.Equal(0, controller.SelectedIndex);
        Assert.Equal(MenuAction.QuitGame, controller.Handle(InputCommand.Confirm));
    }

    private static MenuController CreateController()
    {
        var menu = new MenuDefinition("paused", [
            new MenuItemDefinition("resume", MenuAction.ResumeGame),
            new MenuItemDefinition("back to title", MenuAction.ReturnToTitle),
            new MenuItemDefinition("quit", MenuAction.QuitGame)
        ]);
        return new MenuController(menu);
    }
}
