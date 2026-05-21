using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MessHallAPI.Managers
{
    /// <summary>
    /// This class contains a List of accepted keys please use the strings when setting the keybind for your button.
    /// </summary>
    public class KeybindManager
    {
        public static string spaceKey = "space";
        public static string enterKey = "enter";
        public static string tabKey = "tab";
        public static string commaKey = "comma";
        public static string quoteKey = "quote";
        public static string semicolonKey = "semicolon";
        public static string periodKey = "period";
        public static string slashKey = "slash";
        public static string backslashKey = "backslash";
        public static string leftbracketKey = "leftbracket";
        public static string rightbracketKey = "rightbracket";
        public static string minusKey = "minus";
        public static string equalsKey = "equals";
        public static string aKey = "a";
        public static string bKey = "b";
        public static string cKey = "c";
        public static string dKey = "d";
        public static string eKey = "e";
        public static string fKey = "f";
        public static string gKey = "g";
        public static string hKey = "h";
        public static string iKey = "i";
        public static string jKey = "j";
        public static string kKey = "k";
        public static string lKey = "l";
        public static string mKey = "m";
        public static string nKey = "n";
        public static string oKey = "o";
        public static string pKey = "p";
        public static string qKey = "q";
        public static string rKey = "r";
        public static string sKey = "s";
        public static string tKey = "t";
        public static string uKey = "u";
        public static string vKey = "v";
        public static string wKey = "w";
        public static string xKey = "x";
        public static string yKey = "y";
        public static string zKey = "z";
        public static string oneKey = "1";
        public static string twoKey = "2";
        public static string threeKey = "3";
        public static string fourKey = "4";
        public static string fiveKey = "5";
        public static string sixKey = "6";
        public static string sevenKey = "7";
        public static string eightKey = "8";
        public static string nineKey = "9";
        public static string zeroKey = "0";
        public static string f1Key = "f1";
        public static string f2Key = "f2";
        public static string f3Key = "f3";
        public static string f4Key = "f4";
        public static string f5Key = "f5";
        public static string f6Key = "f6";
        public static string f7Key = "f7";
        public static string f8Key = "f8";
        public static string f9Key = "f9";
        public static string f10Key = "f10";
        public static string f11Key = "f11";
        public static string f12Key = "f12";

        internal static int StringToKeybind(string Keybind)
        {
            switch (Keybind)
            {
                case "space":
                    return 0;
                case "enter":
                    return 1;
                case "tab":
                    return 3;
                case "backquote":
                    return 4;
                case "quote":
                    return 5;
                case "semicolon":
                    return 6;
                case "comma":
                    return 7;
                case "period":
                    return 8;
                case "slash":
                    return 9;
                case "backslash":
                    return 10;
                case "leftbracket":
                    return 11;
                case "rightbracket":
                    return 12;
                case "minus":
                    return 13;
                case "equals":
                    return 14;
                case "a":
                    return 15;
                case "b":
                    return 16;
                case "c":
                    return 17;
                case "d":
                    return 18;
                case "e":
                    return 19;
                case "f":
                    return 20;
                case "g":
                    return 21;
                case "h":
                    return 22;
                case "i":
                    return 23;
                case "j":
                    return 24;
                case "k":
                    return 25;
                case "l":
                    return 26;
                case "m":
                    return 27;
                case "n":
                    return 28;
                case "o":
                    return 29;
                case "p":
                    return 30;
                case "q":
                    return 31;
                case "r":
                    return 32;
                case "s":
                    return 33;
                case "t":
                    return 34;
                case "u":
                    return 35;
                case "v":
                    return 36;
                case "w":
                    return 37;
                case "x":
                    return 38;
                case "y":
                    return 39;
                case "z":
                    return 40;
                case "1":
                    return 41;
                case "2":
                    return 42;
                case "3":
                    return 43;
                case "4":
                    return 44;
                case "5":
                    return 45;
                case "6":
                    return 46;
                case "7":
                    return 47;
                case "8":
                    return 48;
                case "9":
                    return 49;
                case "0":
                    return 50;
                case "leftshift":
                    return 51;
                case "rightshift":
                    return 52;
                case "leftalt":
                    return 53;
                case "rightalt":
                    return 54;
                case "leftcrlt":
                    return 55;
                case "rightcrlt":
                    return 56;
                case "leftmeta":
                    return 57;
                case "rightmeta":
                    return 58;
                case "contextmenu":
                    return 59;
                case "escape":
                    return 60;
                case "leftarrow":
                    return 61;
                case "downarrow":
                    return 62;
                case "rightarrow":
                    return 63;
                case "uparrow":
                    return 64;
                case "backspace":
                    return 65;
                case "pagedown":
                    return 66;
                case "pageup":
                    return 67;
                case "home":
                    return 68;
                case "end":
                    return 69;
                case "insert":
                    return 70;
                case "delete":
                    return 71;
                case "capslock":
                    return 72;
                case "numlock":
                    return 73;
                case "printscreen":
                    return 74;
                case "scrolllock":
                    return 75;
                case "pause":
                    return 76;
                case "numpadenter":
                    return 77;
                case "numpaddivide":
                    return 78;
                case "numpadmultiply":
                    return 79;
                case "numpadplus":
                    return 80;
                case "numpadminus":
                    return 81;
                case "numpadperiod":
                    return 82;
                case "numpad0":
                    return 83;
                case "numpad1":
                    return 84;
                case "numpad2":
                    return 85;
                case "numpad3":
                    return 86;
                case "numpad4":
                    return 87;
                case "numpad5":
                    return 88;
                case "numpad6":
                    return 89;
                case "numpad7":
                    return 90;
                case "numpad8":
                    return 91;
                case "numpad9":
                    return 92;
                case "f1":
                    return 93;
                case "f2":
                    return 94;
                case "f3":
                    return 95;
                case "f4":
                    return 96;
                case "f5":
                    return 97;
                case "f6":
                    return 98;
                case "f7":
                    return 99;
                case "f8":
                    return 100;
                case "f9":
                    return 101;
                case "f10":
                    return 102;
                case "f11":
                    return 103;
                case "f12":
                    return 104;
                case "OEM1":
                    return 105;
                case "OEM2":
                    return 106;
                case "OEM3":
                    return 107;
                case "OEM4":
                    return 108;
                case "OEM5":
                    return 109;
            }
            return 0;
        }

        internal static Vector2 StringToV2(string Keybind)
        {
            switch (Keybind)
            {
                case "space":
                    return new Vector2(5, 5);
                case "enter":
                    return new Vector2(12, 4);
                case "tab":
                    return new Vector2(13, 4);
                case "quote":
                    return new Vector2(2, 10);
                case "semicolon":
                    return new Vector2(9, 2);
                case "comma":
                    return new Vector2(11, 2);
                case "period":
                    return new Vector2(12, 2);
                case "slash":
                    return new Vector2(8, 2);
                case "backslash":
                    return new Vector2(7, 5);
                case "leftbracket":
                    return new Vector2(6, 2);
                case "rightbracket":
                    return new Vector2(7, 2);
                case "minus":
                    return new Vector2(4, 2);
                case "equals":
                    return new Vector2(5, 2);
                case "a":
                    return new Vector2(0, 0);
                case "b":
                    return new Vector2(1, 0);
                case "c":
                    return new Vector2(2, 0);
                case "d":
                    return new Vector2(3, 0);
                case "e":
                    return new Vector2(4, 0);
                case "f":
                    return new Vector2(5, 0);
                case "g":
                    return new Vector2(6, 0);
                case "h":
                    return new Vector2(7, 0);
                case "i":
                    return new Vector2(8, 0);
                case "j":
                    return new Vector2(9, 0);
                case "k":
                    return new Vector2(10, 0);
                case "l":
                    return new Vector2(11, 0);
                case "m":
                    return new Vector2(12, 0);
                case "n":
                    return new Vector2(13, 0);
                case "o":
                    return new Vector2(14, 0);
                case "p":
                    return new Vector2(15, 0);
                case "q":
                    return new Vector2(0, 1);
                case "r":
                    return new Vector2(1, 1);
                case "s":
                    return new Vector2(2, 1);
                case "t":
                    return new Vector2(3, 1);
                case "u":
                    return new Vector2(4, 1);
                case "v":
                    return new Vector2(5, 1);
                case "w":
                    return new Vector2(6, 1);
                case "x":
                    return new Vector2(7, 1);
                case "y":
                    return new Vector2(8, 1);
                case "z":
                    return new Vector2(9, 1);
                case "1":
                    return new Vector2(10, 1);
                case "2":
                    return new Vector2(11, 1);
                case "3":
                    return new Vector2(12, 1);
                case "4":
                    return new Vector2(13, 1);
                case "5":
                    return new Vector2(14, 1);
                case "6":
                    return new Vector2(15, 1);
                case "7":
                    return new Vector2(0, 2);
                case "8":
                    return new Vector2(1, 2);
                case "9":
                    return new Vector2(2, 2);
                case "0":
                    return new Vector2(3, 2);
                case "leftshift":
                    return new Vector2(0, 6);
                case "leftalt":
                    return new Vector2(7, 4);
                case "leftcrlt":
                    return new Vector2(6, 4);
                case "escape":
                    return new Vector2(11, 3);
                case "leftarrow":
                    return new Vector2(1, 5);
                case "downarrow":
                    return new Vector2(4, 5);
                case "rightarrow":
                    return new Vector2(3, 5);
                case "uparrow":
                    return new Vector2(2, 5);
                case "backspace":
                    return new Vector2(14, 4);
                case "pagedown":
                    return new Vector2(4, 4);
                case "pageup":
                    return new Vector2(3, 4);
                case "home":
                    return new Vector2(0, 4);
                case "end":
                    return new Vector2(2, 4);
                case "insert":
                    return new Vector2(15, 3);
                case "delete":
                    return new Vector2(1, 4);
                case "capslock":
                    return new Vector2(8, 4);
                case "printscreen":
                    return new Vector2(12, 3);
                case "scrolllock":
                    return new Vector2(13, 3);
                case "pause":
                    return new Vector2(14, 3);
                case "f1":
                    return new Vector2(15, 2);
                case "f2":
                    return new Vector2(0, 3);
                case "f3":
                    return new Vector2(1, 3);
                case "f4":
                    return new Vector2(2, 3);
                case "f5":
                    return new Vector2(3, 3);
                case "f6":
                    return new Vector2(4, 3);
                case "f7":
                    return new Vector2(5, 3);
                case "f8":
                    return new Vector2(6, 3);
                case "f9":
                    return new Vector2(7, 3);
                case "f10":
                    return new Vector2(8, 3);
                case "f11":
                    return new Vector2(9, 3);
                case "f12":
                    return new Vector2(10, 3);
            }
            return new Vector2(0, 0);
        }

        internal static bool IsKeyAccepted(string Keybind)
        {
            switch (Keybind)
            {
                case "space":
                    return true;
                case "enter":
                    return true;
                case "tab":
                    return true;
                case "quote":
                    return true;
                case "semicolon":
                    return true;
                case "comma":
                    return true;
                case "period":
                    return true;
                case "slash":
                    return true;
                case "backslash":
                    return true;
                case "leftbracket":
                    return true;
                case "rightbracket":
                    return true;
                case "minus":
                    return true;
                case "equals":
                    return true;
                case "a":
                    return true;
                case "b":
                    return true;
                case "c":
                    return true;
                case "d":
                    return true;
                case "e":
                    return true;
                case "f":
                    return true;
                case "g":
                    return true;
                case "h":
                    return true;
                case "i":
                    return true;
                case "j":
                    return true;
                case "k":
                    return true;
                case "l":
                    return true;
                case "m":
                    return true;
                case "n":
                    return true;
                case "o":
                    return true;
                case "p":
                    return true;
                case "q":
                    return true;
                case "r":
                    return true;
                case "s":
                    return true;
                case "t":
                    return true;
                case "u":
                    return true;
                case "v":
                    return true;
                case "w":
                    return true;
                case "x":
                    return true;
                case "y":
                    return true;
                case "z":
                    return true;
                case "1":
                    return true;
                case "2":
                    return true;
                case "3":
                    return true;
                case "4":
                    return true;
                case "5":
                    return true;
                case "6":
                    return true;
                case "7":
                    return true;
                case "8":
                    return true;
                case "9":
                    return true;
                case "0":
                    return true;
                case "leftshift":
                    return true;
                case "leftalt":
                    return true;
                case "leftcrlt":
                    return true;
                case "escape":
                    return true;
                case "leftarrow":
                    return true;
                case "downarrow":
                    return true;
                case "rightarrow":
                    return true;
                case "uparrow":
                    return true;
                case "backspace":
                    return true;
                case "pagedown":
                    return true;
                case "pageup":
                    return true;
                case "home":
                    return true;
                case "end":
                    return true;
                case "insert":
                    return true;
                case "delete":
                    return true;
                case "capslock":
                    return true;
                case "printscreen":
                    return true;
                case "scrolllock":
                    return true;
                case "pause":
                    return true;
                case "f1":
                    return true;
                case "f2":
                    return true;
                case "f3":
                    return true;
                case "f4":
                    return true;
                case "f5":
                    return true;
                case "f6":
                    return true;
                case "f7":
                    return true;
                case "f8":
                    return true;
                case "f9":
                    return true;
                case "f10":
                    return true;
                case "f11":
                    return true;
                case "f12":
                    return true;
            }
            return false;
        }
    }
}
