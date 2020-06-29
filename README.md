# CLI Interface

A simple toolkit for buiding console GUI apps for .NET, .NET Core, and Mono that works on Windows, the Mac, and Linux/Unix.

### Keyboard Input Handling

The input handling of **Intellivoid.cli** is similar in some ways to Emacs and the Midnight Commander, so you can expect some of the special key combinations to be active.

The key `ESC` can act as an Alt modifier (or Meta in Emacs parlance), to allow input on terminals that do not have an alt key.  So to produce the sequence `Alt-F`, you can press either `Alt-F`, or `ESC` followed by the key `F`.

To enter the key `ESC`, you can either press `ESC` and wait 100 milliseconds, or you can press `ESC` twice.

`ESC-0`, and `ESC-1` through `ESC-9` have a special meaning, they map to `F10`, and `F1` to `F9` respectively.

**Intellivoid.cli** respects common Mac and Windows keyboard idoms as well. For example, clipboard operations use the familiar `Control/Command-C, X, V` model.

`CTRL-Q` is used for exiting views (and apps).

### Sample Usage

```csharp
using Intellivoid.cli;

class Demo {
	static void Main ()
	{
		Application.Init ();
		var top = Application.Top;

	// Creates the top-level window to show
		var win = new Window ("MyApp") {
		X = 0,
		Y = 1, // Leave one row for the toplevel menu

		// By using Dim.Fill(), it will automatically resize without manual intervention
		Width = Dim.Fill (),
		Height = Dim.Fill ()
	};
		top.Add (win);

	// Creates a menubar, the item "New" has a help menu.
		var menu = new MenuBar (new MenuBarItem [] {
			new MenuBarItem ("_File", new MenuItem [] {
				new MenuItem ("_New", "Creates new file", NewFile),
				new MenuItem ("_Close", "", () => Close ()),
				new MenuItem ("_Quit", "", () => { if (Quit ()) top.Running = false; })
			}),
			new MenuBarItem ("_Edit", new MenuItem [] {
				new MenuItem ("_Copy", "", null),
				new MenuItem ("C_ut", "", null),
				new MenuItem ("_Paste", "", null)
			})
		});
		top.Add (menu);

	var login = new Label ("Login: ") { X = 3, Y = 2 };
	var password = new Label ("Password: ") {
			X = Pos.Left (login),
		Y = Pos.Top (login) + 1
		};
	var loginText = new TextField ("") {
				X = Pos.Right (password),
				Y = Pos.Top (login),
				Width = 40
		};
		var passText = new TextField ("") {
				Secret = true,
				X = Pos.Left (loginText),
				Y = Pos.Top (password),
				Width = Dim.Width (loginText)
		};
	
	// Add some controls, 
	win.Add (
		// The ones with my favorite layout system
  		login, password, loginText, passText,

		// The ones laid out like an australopithecus, with absolute positions:
			new CheckBox (3, 6, "Remember me"),
			new RadioGroup (3, 8, new [] { "_Personal", "_Company" }),
			new Button (3, 14, "Ok"),
			new Button (10, 14, "Cancel"),
			new Label (3, 18, "Press F9 or ESC plus 9 to activate the menubar"));

		Application.Run ();
	}
}
```

Alternatively, you can encapsulate the app behavior in a new `Window`-derived class, say `App.cs` containing the code above, and simplify your `Main` method to:

```csharp
using Intellivoid.cli;

class Demo {
	static void Main ()
	{
		Application.Run<App> ();
	}
}
```