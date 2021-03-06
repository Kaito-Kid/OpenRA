﻿#region Copyright & License Information
/*
 * Copyright 2007-2013 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.FileFormats;

namespace OpenRA
{
	public struct Hotkey
	{
		public static Hotkey Invalid = new Hotkey(Keycode.UNKNOWN, Modifiers.None);

		public readonly Keycode Key;
		public readonly Modifiers Modifiers;

		public static bool TryParse(string s, out Hotkey result)
		{
			result = Invalid;

			var parts = s.Split(' ');
			if (parts.Length >= 2)
			{
				if (!Enum.GetNames(typeof(Keycode)).Contains(parts[0]))
					return false;

				var modString = s.Substring(s.IndexOf(' '));
				var modParts = modString.Split(',').Select(x => x.Trim());
				if (modParts.Any(p => !Enum.GetNames(typeof(Modifiers)).Contains(p)))
					return false;

				var key = (Keycode)Enum.Parse(typeof(Keycode), parts[0]);
				var mods = (Modifiers)Enum.Parse(typeof(Modifiers), modString);

				result = new Hotkey(key, mods);
				return true;
			}

			if (parts.Length == 1)
			{
				// HACK: Try parsing as a legacy key name
				// This is a stop-gap solution to keep backwards
				// compatibility while outside code is converted
				var i = 0;
				for (; i < (int)Keycode.LAST; i++)
					if (KeycodeExts.DisplayString((Keycode)i) == parts[0])
						break;

				if (i < (int)Keycode.LAST)
				{
					result = new Hotkey((Keycode)i, Modifiers.None);
					return true;
				}
			}

			return false;
		}

		public static Hotkey FromKeyInput(KeyInput ki)
		{
			return new Hotkey(ki.Key, ki.Modifiers);
		}

		public Hotkey(Keycode virtKey, Modifiers mod)
		{
			Key = virtKey;
			Modifiers = mod;
		}

		public static bool operator !=(Hotkey a, Hotkey b) { return !(a == b); }
		public static bool operator ==(Hotkey a, Hotkey b)
		{
			// Unknown keys are never equal
			if (a.Key == Keycode.UNKNOWN)
				return false;

			return a.Key == b.Key && a.Modifiers == b.Modifiers;
		}

		public override int GetHashCode() { return Key.GetHashCode() ^ Modifiers.GetHashCode(); }

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			return (Hotkey)obj == this;
		}

		public override string ToString() { return "{0} {1}".F(Key, Modifiers.ToString("F")); }

		public string DisplayString()
		{
			var ret = KeycodeExts.DisplayString(Key).ToUpper();

			if (Modifiers.HasModifier(Modifiers.Shift))
				ret = "Shift + " + ret;

			if (Modifiers.HasModifier(Modifiers.Alt))
				ret = "Alt + " + ret;

			if (Modifiers.HasModifier(Modifiers.Ctrl))
				ret = "Ctrl + " + ret;

			if (Modifiers.HasModifier(Modifiers.Meta))
				ret = (Platform.CurrentPlatform == PlatformType.OSX ? "Cmd + " : "Meta + ") + ret;

			return ret;
		}
	}
}
