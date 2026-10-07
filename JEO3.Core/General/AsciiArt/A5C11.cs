using System.Text;

namespace JEO3.Core
{
    public static class A5C11
    {
        #region Styles

        #region Style #1

        // Here I am Hard Coding a specific font... I Suck.
        private static List<KeyValuePair<string, List<string>>> m_Alphabet =
        [
                        new KeyValuePair<string, List<string>>(" ",
                [
                    " ",
                    " ",
                    " ",
                    " ",
                    " ",
                    " "
                ]),
            new KeyValuePair<string, List<string>>("a",
                [
                    "       ",
                    "       ",
                    "  __ _ ",
                    " / _` |",
                    " \\__,_|",
                    "       "
                ]),
                new KeyValuePair<string, List<string>>("b",
                [
                    "       ",
                    "  _    ",
                    " | |__ ",
                    " | '_ \\",
                    " |_.__/",
                    "       "
                ]),
                new KeyValuePair<string, List<string>>("c",
                [
                    "     ",
                    "     ",
                    "  __ ",
                    " / _|",
                    " \\__|",
                    "     "
                ]),
                new KeyValuePair<string, List<string>>("d",
                [
                    "       ",
                    "     _ ",
                    "  __| |",
                    " / _` |",
                    " \\__,_|",
                    "       "
                ]),
                new KeyValuePair<string, List<string>>("e",
                [
                    "      ",
                    "      ",
                    "  ___ ",
                    " / -_)",
                    " \\___|",
                    "      "
                ]),
                new KeyValuePair<string, List<string>>("f",
                [
                    "      ",
                    "   __ ",
                    "  / _|",
                    " |  _|",
                    " |_|  ",
                    "      "

                ]),
                new KeyValuePair<string, List<string>>("g",
                [
                    "       ",
                    "       ",
                    "  __ _ ",
                    " / _` |",
                    " \\__, |",
                    " |___/ "
                ]),
                new KeyValuePair<string, List<string>>("h",
                [
                    "       ",
                    "  _    ",
                    " | |_  ",
                    " | ' \\ ",
                    " |_||_|",
                    "       "
                ]),
                new KeyValuePair<string, List<string>>("i",
                [
                    "    ",
                    "  _ ",
                    " (_)",
                    " | |",
                    " |_|",
                    "    "
                ]),
                new KeyValuePair<string, List<string>>("j",
                [
                    "      ",
                    "    _ ",
                    "   (_)",
                    "   | |",
                    "  _/ |",
                    " |__/ "
                ]),
                new KeyValuePair<string, List<string>>("k",
                [
                    "      ",
                    "  _   ",
                    " | |__",
                    " | / /",
                    " |_\\_\\",
                    "      "
                ]),
                new KeyValuePair<string, List<string>>("l",
                [
                    "    ",
                    "  _ ",
                    " | |",
                    " | |",
                    " |_|",
                    "    "
                ]),
                new KeyValuePair<string, List<string>>("m",
                [
                    "        ",
                    "        ",
                    "  _ __  ",
                    " | '  \\ ",
                    " |_|_|_|",
                    "        "
                ]),
                new KeyValuePair<string, List<string>>("n",
                [
                    "       ",
                    "       ",
                    "  _ _  ",
                    " | ' \\ ",
                    " |_||_|",
                    "       "
                ]),
                new KeyValuePair<string, List<string>>("o",
                [
                    "      ",
                    "      ",
                    "  ___ ",
                    " / _ \\",
                    " \\___/",
                    "      "
                ]),
                new KeyValuePair<string, List<string>>("p",
                [
                    "       ",
                    "       ",
                    "  _ __ ",
                    " | '_ \\",
                    " | .__/",
                    " |_|   "
                ]),
                new KeyValuePair<string, List<string>>("q",
                [
                    "       ",
                    "       ",
                    "  __ _ ",
                    " / _` |",
                    " \\__, |",
                    "    |_|"
                ]),
                new KeyValuePair<string, List<string>>("r",
                [
                    "      ",
                    "      ",
                    "  _ _ ",
                    " | '_|",
                    " |_|  ",
                    "      "
                ]),
                            new KeyValuePair<string, List<string>>("s",
                [
                    "     ",
                    "     ",
                    "  ___",
                    " (_-<",
                    " /__/",
                    "     "
                ]),
                new KeyValuePair<string, List<string>>("t",
                [
                    "      ",
                    "  _   ",
                    " | |_ ",
                    " |  _|",
                    "  \\__|",
                    "      "
                ]),
                new KeyValuePair<string, List<string>>("u",
                [
                    "       ",
                    "       ",
                    "  _  _ ",
                    " | || |",
                    "  \\___/",
                    "       "
                ]),
                new KeyValuePair<string, List<string>>("v",
                [
                    "       ",
                    "       ",
                    " __ _ _",
                    "  \\ V /",
                    "   \\_/ ",
                    "       "
                ]),
                new KeyValuePair<string, List<string>>("w",
                [
                    "         ",
                    "         ",
                    " __ __ __",
                    " \\ V  V /",
                    "  \\_/\\_/ ",
                    "         "
                ]),
                 new KeyValuePair<string, List<string>>("x",
                [
                    "      ",
                    "      ",
                    " __ __",
                    " \\ \\ /",
                    " /_\\_\\",
                    "      "
                ]),
                new KeyValuePair<string, List<string>>("y",
                [
                    "       ",
                    "       ",
                    "  _  _ ",
                    " | || |",
                    "  \\_, |",
                    "  |__/ "
                ]),
                new KeyValuePair<string, List<string>>("z",
                [
                    "     ",
                    "     ",
                    "  ___",
                    " |_ /",
                    " /__|",
                    "     "
                ]),
                new KeyValuePair<string, List<string>>("A",
                [
                    "        ",
                    "    _   ",
                    "   /_\\  ",
                    "  / _ \\ ",
                    " /_/ \\_\\",
                    "        "
                ]),
                new KeyValuePair<string, List<string>>("B",
                [
                    "      ",
                    "  ___ ",
                    " | _ )",
                    " | _ \\",
                    " |___/",
                    "        "
                ]),
                new KeyValuePair<string, List<string>>("C",
                [
                    "       ",
                    "   ___ ",
                    "  / __|",
                    " | (__ ",
                    "  \\___|",
                    "       "
                ]),
                new KeyValuePair<string, List<string>>("D",
                [
                    "       ",
                    "  ___  ",
                    " |   \\ ",
                    " | |) |",
                    " |___/ ",
                    "        "
                ]),
                new KeyValuePair<string, List<string>>("E",
                [
                    "      ",
                    "  ___ ",
                    " | __|",
                    " | _| ",
                    " |___|",
                    "      "
                ]),
                new KeyValuePair<string, List<string>>("F",
                [
                    "      ",
                    "  ___ ",
                    " | __|",
                    " | _| ",
                    " |_|  ",
                    "      "
                ]),
                new KeyValuePair<string, List<string>>("G",
                [
                    "       ",
                    "   ___ ",
                    "  / __|",
                    " | (_ |",
                    "  \\___|",
                    "       "
                ]),
                new KeyValuePair<string, List<string>>("H",
                [
                    "       ",
                    "  _  _ ",
                    " | || |",
                    " | __ |",
                    " |_||_|",
                    "       "
                ]),
                new KeyValuePair<string, List<string>>("I",
                [
                    "      ",
                    "  ___ ",
                    " |_ _|",
                    "  | | ",
                    " |___|",
                    "      "
                ]),
                new KeyValuePair<string, List<string>>("J",
                [
                    "       ",
                    "     _ ",
                    "  _ | |",
                    " | || |",
                    "  \\__/ ",
                    "       "
                ]),
                new KeyValuePair<string, List<string>>("K",
                [
                    "       ",
                    "  _  __",
                    " | |/ /",
                    " | ' < ",
                    " |_|\\_\\",
                    "       "
                ]),
                new KeyValuePair<string, List<string>>("L",
                [
                    "       ",
                    "  _    ",
                    " | |   ",
                    " | |__ ",
                    " |____|",
                    "       "
                ]),
                new KeyValuePair<string, List<string>>("M",
                [
                    "         ",
                    "  __  __ ",
                    " |  \\/  |",
                    " | |\\/| |",
                    " |_|  |_|",
                    "         "
                ]),
                new KeyValuePair<string, List<string>>("N",
                [
                    "       ",
                    "  _  _ ",
                    " | \\| |",
                    " | .` |",
                    " |_|\\_|",
                    "        "
                ]),
                new KeyValuePair<string, List<string>>("O",
                [
                    "        ",
                    "   ___  ",
                    "  / _ \\ ",
                    " | (_) |",
                    "  \\___/ ",
                    "       "
                ]),
                            new KeyValuePair<string, List<string>>("P",
                [
                    "      ",
                    "  ___ ",
                    " | _ \\",
                    " |  _/",
                    " |_|  ",
                    "      "
                ]),
                new KeyValuePair<string, List<string>>("Q",
                [
                    "        ",
                    "   ___  ",
                    "  / _ \\ ",
                    " | (_) |",
                    "  \\__\\_\\",
                    "        "
                ]),
                new KeyValuePair<string, List<string>>("R",
                [
                    "      ",
                    "  ___ ",
                    " | _ \\",
                    " |   /",
                    " |_|_\\",
                    "      "
                ]),
                new KeyValuePair<string, List<string>>("S",
                [
                    "      ",
                    "  ___ ",
                    " / __|",
                    " \\__ \\",
                    " |___/",
                    "      "
                ]),
                new KeyValuePair<string, List<string>>("T",
                [
                    "        ",
                    "  _____ ",
                    " |_   _|",
                    "   | |  ",
                    "   |_|  ",
                    "        "
                ]),
                new KeyValuePair<string, List<string>>("U",
                [
                    "        ",
                    "  _  _  ",
                    " | | | |",
                    " | |_| |",
                    "  \\___/ ",
                    "        "
                ]),
                new KeyValuePair<string, List<string>>("V",
                [
                    "        ",
                    " __   __",
                    " \\ \\ / /",
                    "  \\ V / ",
                    "   \\_/  ",
                    "        "
                ]),
                                    new KeyValuePair<string, List<string>>("W",
                [
                    "           ",
                    " __      __",
                    " \\ \\    / /",
                    "  \\ \\/\\/ / ",
                    "   \\_/\\_/  ",
                    "           "
                ]),
                                    new KeyValuePair<string, List<string>>("X",
                [
                    "       ",
                    " __  __",
                    " \\ \\/ /",
                    "  >  < ",
                    " /_/\\_\\",
                    "       "
                ]),
                new KeyValuePair<string, List<string>>("Y",
                [
                    "        ",
                    " __   __",
                    " \\ \\ / /",
                    "  \\ V / ",
                    "   |_|  ",
                    "        "
                ]),
                new KeyValuePair<string, List<string>>("Z",
                [
                    "      ",
                    "  ____",
                    " |_  /",
                    "  / / ",
                    " /___|",
                    "        "
                ]),
                new KeyValuePair<string, List<string>>("1",
                [
                    " __ ",
                    "/_ |",
                    " | |",
                    " | |",
                    " | |",
                    " |_|"
                ]),
                new KeyValuePair<string, List<string>>("2",
                [
                    " ___  ",
                    "|__ \\ ",
                    "   ) |",
                    "  / / ",
                    " / /_ ",
                    " ____|"
                ]),
                new KeyValuePair<string, List<string>>("3",
                [
                    " ____  ",
                    "|___ \\ ",
                    "  __) |",
                    " |__ <|",
                    " ___) |",
                    "|____/ "
                ]),
                new KeyValuePair<string, List<string>>("4",
                [
                    " _  _   ",
                    "| || |  ",
                    "| || |__",
                    "|__   _|",
                    "   | |  ",
                    "   |_|  "
                ]),
                new KeyValuePair<string, List<string>>("5",
                [
                    " _____ ",
                    "| ____|",
                    "| |__  ",
                    "|___ \\ ",
                    " ___) |",
                    "|____/ "
                ]),
                new KeyValuePair<string, List<string>>("6",
                [
                    "  __ ",
                    " / / ",
                    "/ /_ ",
                    " '_ \\",
                    " (_) ",
                    "\\___/"
                ]),
                new KeyValuePair<string, List<string>>("7",
                [
                    " _______",
                    "|____  /",
                    "    / / ",
                    "   / /  ",
                    "  / /   ",
                    " /_/    "
                ]),
                                new KeyValuePair<string, List<string>>("[",
                [
                    " _____  ",
                    "|   __| ",
                    "|  |    ",
                    "|  |    ",
                    "|  |__  ",
                    "|_____| "
                ]),     new KeyValuePair<string, List<string>>("]",
                [
                    "  _____ ",
                    " |___  |",
                    "    |  |",
                    "    |  |",
                    "  __|  |",
                    " |_____|"
                ]),
                          new KeyValuePair<string, List<string>>("(",
                [
"    ___ ",
"   /  / ",
"  /  /  ",
" (  (   ",
"  \\  \\  ",
"   \\__\\ "
                ]),    new KeyValuePair<string, List<string>>(")",
                [
                    "  __    ",
                    " \\  \\   ",
                    "  \\  \\  ",
                    "   )  ) ",
                    "  /  /  ",
                    " /__/   "
                ]),// The Trajectory Dash / Horizontal Connector
new KeyValuePair<string, List<string>>("-",
[
    "        ",
    "  _____ ",
    " /     \\",
    " \\_____/",
    "        ",
    "        "
]),

// The Arrow Pointer / Traversal Direction
new KeyValuePair<string, List<string>>(">",
[
    "  ___   ",
    "   \\  \\ ",
    "    \\  \\",
    "    /  /",
    "   /__/ ",
    "        "
]),// The Arrow Pointer / Traversal Direction
new KeyValuePair<string, List<string>>("<",
[
    "   __   ",
    "  /  /  ",
    " /  /   ",
    " \\  \\   ",
    "  \\__\\  ",
    "        "
]),

// The Wildcard Select '*'
new KeyValuePair<string, List<string>>("*",
[
    "  _  _  ",
    " | \\/ | ",
    " - ** - ",
    " |_/\\_| ",
    "        ",
    "        "
]),

// The Schema Dot / Separator
new KeyValuePair<string, List<string>>(".",
[
    "        ",
    "        ",
    "        ",
    "   __   ",
    "  |  |  ",
    "  |__|  "
]),

// The Column Comma
new KeyValuePair<string, List<string>>(",",
[
    "        ",
    "        ",
    "        ",
    "   __   ",
    "  |  |  ",
    "  /__/  "
]),new KeyValuePair<string, List<string>>("+",
[
    "   __   ",
    " _|  |_ ",
    "|_    _|",
    "  |__|  ",
    "        ",
    "        "
]),

// The Parameter / Schema Colon ':'
new KeyValuePair<string, List<string>>(":",
[
    "   __   ",
    "  |__|  ",
    "        ",
    "   __   ",
    "  |__|  ",
    "        "
]),

// The Statement Terminator ';'
new KeyValuePair<string, List<string>>(";",
[
    "   __   ",
    "  |__|  ",
    "        ",
    "   __   ",
    "  |  |  ",
    "  /  /  "
]),new KeyValuePair<string, List<string>>("/",
[
    @"     ___",
    @"    /  /",
    @"   /  / ",
    @"  /  /  ",
    @" /  /   ",
    @"/__/    "
]),

// The Backslash '\' (Fully Escaped)
new KeyValuePair<string, List<string>>("\\",
[
    @"___     ",
    @"\  \    ",
    @" \  \   ",
    @"  \  \  ",
    @"   \  \ ",
    @"    \__\" ])
        ];

        //  __ ___  ____  _  _   _____   ________ ___   ___   ___
        // /_ |__ \|___ \| || | | ____| / /____  / _ \ / _ \ / _ \ 
        //  | |  ) | __) | || |_| |__  / /_   / / (_) | (_) | | | |
        //  | | / / |__ <|__   _|___ \| '_ \ / / > _ < \__, | | | |
        //  | |/ /_ ___) |  | |  ___) | (_) / / | (_) |  / /| |_| |
        //  |_|____|____/   |_| |____/ \___/_/   \___/  /_/  \___/


        #endregion

        #region Style #2

        private static List<KeyValuePair<string, List<string>>> m_Alphabet4 =
 [
 new KeyValuePair<string, List<string>>(" ", [
"        ",
"        ",
"        ",
"        ",
"        ",
"        ",
 ]),
 new KeyValuePair<string, List<string>>("A", [
" █████╗ ",
"██╔══██╗",
"███████║",
"██╔══██║",
"██║  ██║",
"╚═╝  ╚═╝",
 ]),
 new KeyValuePair<string, List<string>>("B", [
"██████╗ ",
"██╔══██╗",
"██████╔╝",
"██╔══██╗",
"██████╔╝",
"╚═════╝ ",
 ]),
 new KeyValuePair<string, List<string>>("C", [
" ██████╗",
"██╔════╝",
"██║     ",
"██║     ",
"╚██████╗",
" ╚═════╝",
 ]),
 new KeyValuePair<string, List<string>>("D", [
"██████╗ ",
"██╔══██╗",
"██║  ██║",
"██║  ██║",
"██████╔╝",
"╚═════╝ ",
 ]),
 new KeyValuePair<string, List<string>>("E", [
"███████╗",
"██╔════╝",
"█████╗  ",
"██╔══╝  ",
"███████╗",
"╚══════╝",
 ]),
 new KeyValuePair<string, List<string>>("F", [
"███████╗",
"██╔════╝",
"█████╗  ",
"██╔══╝  ",
"██║     ",
"╚═╝     ",
 ]),
 new KeyValuePair<string, List<string>>("G", [
" ██████╗ ",
"██╔════╝ ",
"██║  ███╗",
"██║   ██║",
"╚██████╔╝",
" ╚═════╝ ",
 ]),
 new KeyValuePair<string, List<string>>("H", [
"██╗  ██╗",
"██║  ██║",
"███████║",
"██╔══██║",
"██║  ██║",
"╚═╝  ╚═╝",
 ]),
 new KeyValuePair<string, List<string>>("I", [
"██╗ ",
"██║ ",
"██║ ",
"██║ ",
"██║ ",
"╚═╝ ",
 ]),
 new KeyValuePair<string, List<string>>("J", [
"     ██╗",
"     ██║",
"     ██║",
"██   ██║",
"╚█████╔╝",
" ╚════╝ ",
 ]),
 new KeyValuePair<string, List<string>>("K", [
"██╗  ██╗",
"██║ ██╔╝",
"█████╔╝ ",
"██╔═██╗ ",
"██║  ██╗",
"╚═╝  ╚═╝",
 ]),
 new KeyValuePair<string, List<string>>("L", [
"██╗     ",
"██║     ",
"██║     ",
"██║     ",
"███████╗",
"╚══════╝",
 ]),
 new KeyValuePair<string, List<string>>("M", [
"███╗   ███╗",
"████╗ ████║",
"██╔████╔██║",
"██║╚██╔╝██║",
"██║ ╚═╝ ██║",
"╚═╝     ╚═╝",
 ]),
 new KeyValuePair<string, List<string>>("N", [
"███╗   ██╗",
"████╗  ██║",
"██╔██╗ ██║",
"██║╚██╗██║",
"██║ ╚████║",
"╚═╝  ╚═══╝",
 ]),
 new KeyValuePair<string, List<string>>("O", [
" ██████╗ ",
"██╔═══██╗",
"██║   ██║",
"██║   ██║",
"╚██████╔╝",
" ╚═════╝ ",
 ]),
 new KeyValuePair<string, List<string>>("P", [
"██████╗ ",
"██╔══██╗",
"██████╔╝",
"██╔═══╝ ",
"██║     ",
"╚═╝     ",
 ]),
 new KeyValuePair<string, List<string>>("Q", [
" ██████╗ ",
"██╔═══██╗",
"██║   ██║",
"██║▄▄ ██║",
"╚██████╔╝",
" ╚══▀▀═╝ ",
 ]),
 new KeyValuePair<string, List<string>>("R", [
"██████╗ ",
"██╔══██╗",
"██████╔╝",
"██╔══██╗",
"██║  ██║",
"╚═╝  ╚═╝",
 ]),
 new KeyValuePair<string, List<string>>("S", [
"███████╗",
"██╔════╝",
"███████╗",
"╚════██║",
"███████║",
"╚══════╝",
 ]),
 new KeyValuePair<string, List<string>>("T", [
"████████╗",
"╚══██╔══╝",
"   ██║   ",
"   ██║   ",
"   ██║   ",
"   ╚═╝   ",
 ]),
 new KeyValuePair<string, List<string>>("U", [
"██╗   ██╗",
"██║   ██║",
"██║   ██║",
"██║   ██║",
"╚██████╔╝",
" ╚═════╝ ",
 ]),
 new KeyValuePair<string, List<string>>("V", [
"██╗   ██╗",
"██║   ██║",
"██║   ██║",
"╚██╗ ██╔╝",
" ╚████╔╝ ",
"  ╚═══╝  ",
 ]),
 new KeyValuePair<string, List<string>>("W", [
"██╗    ██╗",
"██║    ██║",
"██║ █╗ ██║",
"██║███╗██║",
"╚███╔███╔╝",
" ╚══╝╚══╝ ",
 ]),
 new KeyValuePair<string, List<string>>("X", [
"██╗  ██╗",
"╚██╗██╔╝",
" ╚███╔╝ ",
" ██╔██╗ ",
"██╔╝ ██╗",
"╚═╝  ╚═╝",
 ]),
 new KeyValuePair<string, List<string>>("Y", [
"██╗   ██╗",
"╚██╗ ██╔╝",
" ╚████╔╝ ",
"  ╚██╔╝  ",
"   ██║   ",
"   ╚═╝   ",
 ]),
 new KeyValuePair<string, List<string>>("Z", [
"███████╗",
"╚══███╔╝",
"  ███╔╝ ",
" ███╔╝  ",
"███████╗",
"╚══════╝",
 ]),
 new KeyValuePair<string, List<string>>("0", [
" ██████╗ ",
"██╔═████╗",
"██║██╔██║",
"████╔╝██║",
"╚██████╔╝",
" ╚═════╝ ",
 ]),
 new KeyValuePair<string, List<string>>("1", [
" ██╗",
"███║",
"╚██║",
" ██║",
" ██║",
" ╚═╝",
 ]),
 new KeyValuePair<string, List<string>>("2", [
"██████╗ ",
"╚════██╗",
" █████╔╝",
"██╔═══╝ ",
"███████╗",
"╚══════╝",
 ]),
 new KeyValuePair<string, List<string>>("3", [
"██████╗ ",
"╚════██╗",
" █████╔╝",
" ╚═══██╗",
"██████╔╝",
"╚═════╝ ",
 ]),
 new KeyValuePair<string, List<string>>("4", [
"██╗  ██╗",
"██║  ██║",
"███████║",
"╚════██║",
"     ██║",
"     ╚═╝",
 ]),
 new KeyValuePair<string, List<string>>("5", [
"███████╗",
"██╔════╝",
"███████╗",
"╚════██║",
"███████║",
"╚══════╝",
 ]),
 new KeyValuePair<string, List<string>>("6", [
" ██████╗ ",
"██╔════╝ ",
"███████╗ ",
"██╔═══██╗",
"╚██████╔╝",
" ╚═════╝ ",
 ]),
 new KeyValuePair<string, List<string>>("7", [
"███████╗",
"╚════██║",
"    ██╔╝",
"   ██╔╝ ",
"   ██║  ",
"   ╚═╝  ",
 ]),
 new KeyValuePair<string, List<string>>("8", [
" █████╗ ",
"██╔══██╗",
"╚█████╔╝",
"██╔══██╗",
"╚█████╔╝",
" ╚════╝ ",
 ]),
 new KeyValuePair<string, List<string>>("9", [
" █████╗ ",
"██╔══██╗",
"╚██████║",
" ╚═══██║",
" █████╔╝",
" ╚════╝ ",
 ]),

 new KeyValuePair<string, List<string>>(".", [
"   ",
"   ",
"   ",
"   ",
"██╗",
"╚═╝"
 ]),
 new KeyValuePair<string, List<string>>("*", [

"      ",
"      ",
"▄ ██╗▄",
" ████╗",
"▀╚██╔▀",
"  ╚═╝ "
 ]),
 new KeyValuePair<string, List<string>>("%", [
"██╗ ██╗",
"╚═╝██╔╝",
"  ██╔╝ ",
" ██╔╝  ",
"██╔╝██╗",
"╚═╝ ╚═╝"
 ]),

 new KeyValuePair<string, List<string>>("-", [
"      ",
"      ",
"█████╗",
"╚════╝",
"      ",
"      "
 ]),
 new KeyValuePair<string, List<string>>("#", [
" ██╗ ██╗ ",
"████████╗",
"╚██╔═██╔╝",
"████████╗",
"╚██╔═██╔╝",
" ╚═╝ ╚═╝ "
 ])

];

        #endregion

        #endregion

        #region Functions

        public static string GetAsciiArt(string strMessage, bool small = true)
        {
            try
            {
                var message = string.Empty;

                message = small ? GetAsciiArtType1(strMessage) : GetAsciiType2(strMessage);

                return message;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static string GetAsciiArtType1(string strMessage)
        {
            try
            {
                strMessage = "\r\n" + strMessage.Trim();

                string listValidCharacters = " abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567[](),.*<>-+:;?/\\";
                strMessage = String.Join("", strMessage.Where(c => listValidCharacters.Contains(c) == true));

                int intMaxStringLength = m_Alphabet.Where(c => c.Value.Select(line => line.Length > 1).Count() > 0).Select(kvp => kvp.Value).ToList().Select(list => list.Select(val => val.Length).Max()).Max();
                int intMaxRowLength = m_Alphabet.Where(c => c.Value.Select(line => line != "  ").Count() > 0).Select(kvp => String.Join("\n", kvp.Value).Split('\n').Length).Max();

                KeyValuePair<string, List<string>> kvpSpace = m_Alphabet.Where(kvp => kvp.Key == " ").First();
                string strOutput = "";
                for (int intIndex = 0; intIndex < intMaxRowLength; intIndex++)
                {
                    for (int intCharIndex = 0; intCharIndex < strMessage.Length; intCharIndex++)
                    {
                        string strChar = strMessage[intCharIndex].ToString();

                        if (strChar == " ")
                        {
                            strOutput += "   ";

                            continue;
                        }

                        strOutput += m_Alphabet.First(str => str.Key == strMessage[intCharIndex].ToString()).Value[intIndex];
                    }
                    strOutput += "\r\n";
                }

                Console.Write(strOutput);
                return strOutput;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private static string GetAsciiType2(string strMessage)
        {
            try
            {
                strMessage = "\r\n" + strMessage.ToUpper().Trim();

                string listValidCharacters = " abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890.#$%*";
                strMessage = String.Join("", strMessage.Where(c => listValidCharacters.Contains(c) == true));

                int intMaxStringLength = m_Alphabet4.Where(c => c.Value.Select(line => line.Length > 1).Count() > 0).Select(kvp => kvp.Value).ToList().Select(list => list.Select(val => val.Length).Max()).Max();
                int intMaxRowLength = m_Alphabet4.Where(c => c.Value.Select(line => line != "  ").Count() > 0).Select(kvp => String.Join("\n", kvp.Value).Split('\n').Length).Max();

                KeyValuePair<string, List<string>> kvpSpace = m_Alphabet4.Where(kvp => kvp.Key == " ").First();
                string strOutput = "";
                for (int intIndex = 0; intIndex < intMaxRowLength; intIndex++)
                {
                    for (int intCharIndex = 0; intCharIndex < strMessage.Length; intCharIndex++)
                    {
                        string strChar = strMessage[intCharIndex].ToString();

                        if (strChar == " ")
                        {
                            strOutput += "   ";

                            continue;
                        }

                        try
                        {

                            strOutput += m_Alphabet4.First(str => str.Key == strMessage[intCharIndex].ToString()).Value[intIndex];
                        }
                        catch (Exception ex4)
                        {
                            bool stop = true;
                        }
                    }
                    strOutput += "\r\n";
                }

                Console.Write(strOutput);
                return strOutput;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static string GetSqlAsciiArt(string text)
        {
            var sql = new StringBuilder();

            string hr = new string('-', FMT.DividerLength);
            var art = A5C11.GetAsciiArt(text);
            var parts = art.Split("\n").ToList();
            var line = parts.Select(v => v.TrimEnd().Replace("\r", "")).Where(v => v != null && v.Length > 0).Select(v => (new string('-', 2) + "    " + v).PadRight(FMT.DividerLength - 2, ' ').PadRight(FMT.DividerLength - 2, '-') + new string('-', 2)).ToList();

            sql.AppendLine(string.Join("\n", line));
            sql.AppendLine(hr);
            sql.AppendLine(SQLConstants.SelectTop1000.PadRight(FMT.DividerLength - 2, ' ') + "--");
            sql.AppendLine(hr);

            return sql.ToString();
        }

        #endregion
    }
}