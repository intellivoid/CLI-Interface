﻿//
// FakeDriver.cs: A fake ConsoleDriver for unit tests. 
//
// Authors:
//   Charlie Kindel (github.com/tig)
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Intellivoid.NStack;

namespace Intellivoid.cli {
	/// <summary>
	/// Implements a mock ConsoleDriver for unit testing
	/// </summary>
	public class FakeDriver : ConsoleDriver {
		int cols, rows;
		/// <summary>
		/// 
		/// </summary>
		public override int Cols => cols;
		/// <summary>
		/// 
		/// </summary>
		public override int Rows => rows;

		// The format is rows, columns and 3 values on the last column: Rune, Attribute and Dirty Flag
		int [,,] contents;
		bool [] dirtyLine;

		void UpdateOffscreen ()
		{
			int cols = Cols;
			int rows = Rows;

			contents = new int [rows, cols, 3];
			for (int r = 0; r < rows; r++) {
				for (int c = 0; c < cols; c++) {
					contents [r, c, 0] = ' ';
					contents [r, c, 1] = MakeColor (ConsoleColor.Gray, ConsoleColor.Black);
					contents [r, c, 2] = 0;
				}
			}
			dirtyLine = new bool [rows];
			for (int row = 0; row < rows; row++)
				dirtyLine [row] = true;
		}

		static bool sync = false;

		/// <summary>
		/// 
		/// </summary>
		public FakeDriver ()
		{
			cols = FakeConsole.WindowWidth;
			rows = FakeConsole.WindowHeight; // - 1;
			UpdateOffscreen ();
		}

		bool needMove;
		// Current row, and current col, tracked by Move/AddCh only
		int ccol, crow;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="col"></param>
		/// <param name="row"></param>
		public override void Move (int col, int row)
		{
			ccol = col;
			crow = row;

			if (Clip.Contains (col, row)) {
				FakeConsole.CursorTop = row;
				FakeConsole.CursorLeft = col;
				needMove = false;
			} else {
				FakeConsole.CursorTop = Clip.Y;
				FakeConsole.CursorLeft = Clip.X;
				needMove = true;
			}

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rune"></param>
		public override void AddRune (Rune rune)
		{
			rune = MakePrintable (rune);
			if (Clip.Contains (ccol, crow)) {
				if (needMove) {
					//MockConsole.CursorLeft = ccol;
					//MockConsole.CursorTop = crow;
					needMove = false;
				}
				contents [crow, ccol, 0] = (int)(uint)rune;
				contents [crow, ccol, 1] = currentAttribute;
				contents [crow, ccol, 2] = 1;
				dirtyLine [crow] = true;
			} else
				needMove = true;
			ccol++;
			//if (ccol == Cols) {
			//	ccol = 0;
			//	if (crow + 1 < Rows)
			//		crow++;
			//}
			if (sync)
				UpdateScreen ();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="str"></param>
		public override void AddStr (ustring str)
		{
			foreach (var rune in str)
				AddRune (rune);
		}

		/// <summary>
		/// 
		/// </summary>
		public override void End ()
		{
			FakeConsole.ResetColor ();
			FakeConsole.Clear ();
		}

		static Attribute MakeColor (ConsoleColor f, ConsoleColor b)
		{
			// Encode the colors into the int value.
			return new Attribute () { value = ((((int)f) & 0xffff) << 16) | (((int)b) & 0xffff) };
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="terminalResized"></param>
		public override void Init (Action terminalResized)
		{
			Colors.TopLevel = new ColorScheme ();
			Colors.Base = new ColorScheme ();
			Colors.Dialog = new ColorScheme ();
			Colors.Menu = new ColorScheme ();
			Colors.Error = new ColorScheme ();
			Clip = new Rect (0, 0, Cols, Rows);

			Colors.TopLevel.Normal = MakeColor (ConsoleColor.Green, ConsoleColor.Black);
			Colors.TopLevel.Focus = MakeColor (ConsoleColor.White, ConsoleColor.DarkCyan);
			Colors.TopLevel.HotNormal = MakeColor (ConsoleColor.DarkYellow, ConsoleColor.Black);
			Colors.TopLevel.HotFocus = MakeColor (ConsoleColor.DarkBlue, ConsoleColor.DarkCyan);

			Colors.Base.Normal = MakeColor (ConsoleColor.White, ConsoleColor.Blue);
			Colors.Base.Focus = MakeColor (ConsoleColor.Black, ConsoleColor.Cyan);
			Colors.Base.HotNormal = MakeColor (ConsoleColor.Yellow, ConsoleColor.Blue);
			Colors.Base.HotFocus = MakeColor (ConsoleColor.Yellow, ConsoleColor.Cyan);

			// Focused,
			//    Selected, Hot: Yellow on Black
			//    Selected, text: white on black
			//    Unselected, hot: yellow on cyan
			//    unselected, text: same as unfocused
			Colors.Menu.HotFocus = MakeColor (ConsoleColor.Yellow, ConsoleColor.Black);
			Colors.Menu.Focus = MakeColor (ConsoleColor.White, ConsoleColor.Black);
			Colors.Menu.HotNormal = MakeColor (ConsoleColor.Yellow, ConsoleColor.Cyan);
			Colors.Menu.Normal = MakeColor (ConsoleColor.White, ConsoleColor.Cyan);
			Colors.Menu.Disabled = MakeColor (ConsoleColor.DarkGray, ConsoleColor.Cyan);

			Colors.Dialog.Normal = MakeColor (ConsoleColor.Black, ConsoleColor.Gray);
			Colors.Dialog.Focus = MakeColor (ConsoleColor.Black, ConsoleColor.Cyan);
			Colors.Dialog.HotNormal = MakeColor (ConsoleColor.Blue, ConsoleColor.Gray);
			Colors.Dialog.HotFocus = MakeColor (ConsoleColor.Blue, ConsoleColor.Cyan);

			Colors.Error.Normal = MakeColor (ConsoleColor.White, ConsoleColor.Red);
			Colors.Error.Focus = MakeColor (ConsoleColor.Black, ConsoleColor.Gray);
			Colors.Error.HotNormal = MakeColor (ConsoleColor.Yellow, ConsoleColor.Red);
			Colors.Error.HotFocus = Colors.Error.HotNormal;

			HLine = '\u2500';
			VLine = '\u2502';
			Stipple = '\u2592';
			Diamond = '\u25c6';
			ULCorner = '\u250C';
			LLCorner = '\u2514';
			URCorner = '\u2510';
			LRCorner = '\u2518';
			LeftTee = '\u251c';
			RightTee = '\u2524';
			TopTee = '\u22a4';
			BottomTee = '\u22a5';
			Checked = '\u221a';
			UnChecked = ' ';
			Selected = '\u25cf';
			UnSelected = '\u25cc';
			RightArrow = '\u25ba';
			LeftArrow = '\u25c4';
			UpArrow = '\u25b2';
			DownArrow = '\u25bc';
			LeftDefaultIndicator = '\u25e6';
			RightDefaultIndicator = '\u25e6';
			LeftBracket = '[';
			RightBracket = ']';
			OnMeterSegment = '\u258c';
			OffMeterSegement = ' ';

			//MockConsole.Clear ();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fore"></param>
		/// <param name="back"></param>
		/// <returns></returns>
		public override Attribute MakeAttribute (Color fore, Color back)
		{
			return MakeColor ((ConsoleColor)fore, (ConsoleColor)back);
		}

		int redrawColor = -1;
		void SetColor (int color)
		{
			redrawColor = color;
			IEnumerable<int> values = Enum.GetValues (typeof (ConsoleColor))
			      .OfType<ConsoleColor> ()
			      .Select (s => (int)s);
			if (values.Contains (color & 0xffff)) {
				FakeConsole.BackgroundColor = (ConsoleColor)(color & 0xffff);
			}
			if (values.Contains ((color >> 16) & 0xffff)) {
				FakeConsole.ForegroundColor = (ConsoleColor)((color >> 16) & 0xffff);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void UpdateScreen ()
		{
			int rows = Rows;
			int cols = Cols;

			FakeConsole.CursorTop = 0;
			FakeConsole.CursorLeft = 0;
			for (int row = 0; row < rows; row++) {
				dirtyLine [row] = false;
				for (int col = 0; col < cols; col++) {
					contents [row, col, 2] = 0;
					var color = contents [row, col, 1];
					if (color != redrawColor)
						SetColor (color);
					FakeConsole.Write ((char)contents [row, col, 0]);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Refresh ()
		{
			int rows = Rows;
			int cols = Cols;

			var savedRow = FakeConsole.CursorTop;
			var savedCol = FakeConsole.CursorLeft;
			for (int row = 0; row < rows; row++) {
				if (!dirtyLine [row])
					continue;
				dirtyLine [row] = false;
				for (int col = 0; col < cols; col++) {
					if (contents [row, col, 2] != 1)
						continue;

					FakeConsole.CursorTop = row;
					FakeConsole.CursorLeft = col;
					for (; col < cols && contents [row, col, 2] == 1; col++) {
						var color = contents [row, col, 1];
						if (color != redrawColor)
							SetColor (color);

						FakeConsole.Write ((char)contents [row, col, 0]);
						contents [row, col, 2] = 0;
					}
				}
			}
			FakeConsole.CursorTop = savedRow;
			FakeConsole.CursorLeft = savedCol;
		}

		/// <summary>
		/// 
		/// </summary>
		public override void UpdateCursor ()
		{
			//
		}

		/// <summary>
		/// 
		/// </summary>
		public override void StartReportingMouseMoves ()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public override void StopReportingMouseMoves ()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Suspend ()
		{
		}

		int currentAttribute;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="c"></param>
		public override void SetAttribute (Attribute c)
		{
			currentAttribute = c.value;
		}

		Key MapKey (ConsoleKeyInfo keyInfo)
		{
			switch (keyInfo.Key) {
			case ConsoleKey.Escape:
				return Key.Esc;
			case ConsoleKey.Tab:
				return keyInfo.Modifiers == ConsoleModifiers.Shift ? Key.BackTab : Key.Tab;
			case ConsoleKey.Home:
				return Key.Home;
			case ConsoleKey.End:
				return Key.End;
			case ConsoleKey.LeftArrow:
				return Key.CursorLeft;
			case ConsoleKey.RightArrow:
				return Key.CursorRight;
			case ConsoleKey.UpArrow:
				return Key.CursorUp;
			case ConsoleKey.DownArrow:
				return Key.CursorDown;
			case ConsoleKey.PageUp:
				return Key.PageUp;
			case ConsoleKey.PageDown:
				return Key.PageDown;
			case ConsoleKey.Enter:
				return Key.Enter;
			case ConsoleKey.Spacebar:
				return Key.Space;
			case ConsoleKey.Backspace:
				return Key.Backspace;
			case ConsoleKey.Delete:
				return Key.Delete;

			case ConsoleKey.Oem1:
			case ConsoleKey.Oem2:
			case ConsoleKey.Oem3:
			case ConsoleKey.Oem4:
			case ConsoleKey.Oem5:
			case ConsoleKey.Oem6:
			case ConsoleKey.Oem7:
			case ConsoleKey.Oem8:
			case ConsoleKey.Oem102:
			case ConsoleKey.OemPeriod:
			case ConsoleKey.OemComma:
			case ConsoleKey.OemPlus:
			case ConsoleKey.OemMinus:
				return (Key)((uint)keyInfo.KeyChar);
			}

			var key = keyInfo.Key;
			if (key >= ConsoleKey.A && key <= ConsoleKey.Z) {
				var delta = key - ConsoleKey.A;
				if (keyInfo.Modifiers == ConsoleModifiers.Control)
					return (Key)((uint)Key.ControlA + delta);
				if (keyInfo.Modifiers == ConsoleModifiers.Alt)
					return (Key)(((uint)Key.AltMask) | ((uint)'A' + delta));
				if (keyInfo.Modifiers == ConsoleModifiers.Shift)
					return (Key)((uint)'A' + delta);
				else
					return (Key)((uint)'a' + delta);
			}
			if (key >= ConsoleKey.D0 && key <= ConsoleKey.D9) {
				var delta = key - ConsoleKey.D0;
				if (keyInfo.Modifiers == ConsoleModifiers.Alt)
					return (Key)(((uint)Key.AltMask) | ((uint)'0' + delta));
				if (keyInfo.Modifiers == ConsoleModifiers.Shift)
					return (Key)((uint)keyInfo.KeyChar);
				return (Key)((uint)'0' + delta);
			}
			if (key >= ConsoleKey.F1 && key <= ConsoleKey.F10) {
				var delta = key - ConsoleKey.F1;

				return (Key)((int)Key.F1 + delta);
			}
			return (Key)(0xffffffff);
		}

		KeyModifiers keyModifiers = new KeyModifiers ();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mainLoop"></param>
		/// <param name="keyHandler"></param>
		/// <param name="keyDownHandler"></param>
		/// <param name="keyUpHandler"></param>
		/// <param name="mouseHandler"></param>
		public override void PrepareToRun (MainLoop mainLoop, Action<KeyEvent> keyHandler, Action<KeyEvent> keyDownHandler, Action<KeyEvent> keyUpHandler, Action<MouseEvent> mouseHandler)
		{
			// Note: Net doesn't support keydown/up events and thus any passed keyDown/UpHandlers will never be called
			(mainLoop.Driver as NetMainLoop).KeyPressed = delegate (ConsoleKeyInfo consoleKey) {
				var map = MapKey (consoleKey);
				if (map == (Key)0xffffffff)
					return;
				keyHandler (new KeyEvent (map, keyModifiers));
				keyUpHandler (new KeyEvent (map, keyModifiers));
			};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="foreground"></param>
		/// <param name="background"></param>
		public override void SetColors (ConsoleColor foreground, ConsoleColor background)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="foregroundColorId"></param>
		/// <param name="backgroundColorId"></param>
		public override void SetColors (short foregroundColorId, short backgroundColorId)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// 
		/// </summary>
		public override void CookMouse ()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public override void UncookMouse ()
		{
		}
	}
}