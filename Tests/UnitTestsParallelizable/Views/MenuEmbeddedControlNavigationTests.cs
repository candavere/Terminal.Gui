using Terminal.Gui.Drawing;
using UnitTests;
using UnitTests.Parallelizable;

namespace ViewsTests;

// OpenCode
public class MenuEmbeddedControlNavigationTests
{
    // OpenCode
    private static (IApplication app, OptionSelector<Schemes> selector, int[] changedCount) CreateOpenedRootWithEmbeddedSelector ()
    {
        VirtualTimeProvider time = new ();
        IApplication app = Application.Create (time);
        app.Init (DriverRegistry.Names.ANSI);
        IRunnable runnable = new Runnable ();

        OptionSelector<Schemes> selector = new () { Title = "Scheme", CanFocus = true };
        int[] changedCount = [0];
        selector.ValueChanged += (_, _) => { changedCount [0]++; };

        MenuItem item = new () { Id = "selectorItem", HelpText = "Pick a scheme", CommandView = selector };

        Menu root = new ([item]) { Id = "root" };
        MenuBarItem bi = new () { Id = "bi", Title = "_Test" };
        PopoverMenu pm = new ();
        bi.PopoverMenu = pm;
        pm.Root = root;
        MenuBar bar = new () { Id = "bar" };
        bar.Add (bi);

        View host = new () { Id = "host", CanFocus = true, Width = Dim.Fill (), Height = Dim.Fill () };
        host.Add (bar);
        ((View)runnable).Add (host);
        app.Begin (runnable);

        app.InjectKey (MenuBar.DefaultKey);

        return (app, selector, changedCount);
    }

    // OpenCode
    [Fact]
    public void Embedded_OptionSelector_Root_Down_Navigates_And_Space_Selects ()
    {
        var (app, selector, changedCount) = CreateOpenedRootWithEmbeddedSelector ();
        using (app)
        {
            // Navigate within the embedded selector: Base -> Menu -> Dialog -> Runnable -> Error
            app.InjectKey (Key.CursorDown);
            app.InjectKey (Key.CursorDown);
            app.InjectKey (Key.CursorDown);
            app.InjectKey (Key.CursorDown);

            app.InjectKey (Key.Space);

            Assert.Equal (Schemes.Error, selector.Value);
            Assert.Equal (1, changedCount [0]);
        }
    }

    // OpenCode
    [Fact]
    public void Embedded_OptionSelector_Root_Up_Wraps_Within_Control ()
    {
        var (app, selector, changedCount) = CreateOpenedRootWithEmbeddedSelector ();
        using (app)
        {
            // Starting on Base (first), Up wraps within the selector to Error (last).
            app.InjectKey (Key.CursorUp);
            app.InjectKey (Key.Space);

            Assert.Equal (Schemes.Error, selector.Value);
            Assert.Equal (1, changedCount [0]);
        }
    }
}
