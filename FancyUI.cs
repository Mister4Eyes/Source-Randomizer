using System;

namespace Fancy_Interface
{
    class FancyInterface
    {
        #region FancyItemSelect
        public static int fancyItemSelect(string[] str)
        {
            Console.Clear();
            foreach (string s in str)
            {
                Console.WriteLine(s);
            }
            int cursorPos = 0;
            highlightLine(cursorPos, str, true);
            while (true)
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);
                switch (cki.Key)
                {
                    case ConsoleKey.UpArrow:
                        highlightLine(cursorPos, str, false);
                        cursorPos = mod((cursorPos - 1), str.Length);
                        highlightLine(cursorPos, str, true, true);
                        break;

                    case ConsoleKey.DownArrow:
                        highlightLine(cursorPos, str, false);
                        cursorPos = mod((cursorPos + 1), str.Length);
                        highlightLine(cursorPos, str, true);
                        break;

                    case ConsoleKey.Enter:
                        goto done;

                }
            }
            done:
            Console.Clear();
            return cursorPos;
        }
        public static int fancyItemSelect(string[] str, string message)
        {
            Console.Clear();
            Console.WriteLine(message);
            foreach (string s in str)
            {
                Console.WriteLine(s);
            }
            int cursorPos = 0;
            highlightLine(cursorPos, str, true, true);
            while (true)
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);
                switch (cki.Key)
                {
                    case ConsoleKey.UpArrow:
                        highlightLine(cursorPos, str, false, true);
                        cursorPos = mod((cursorPos - 1), str.Length);
                        highlightLine(cursorPos, str, true, true);
                        break;

                    case ConsoleKey.DownArrow:
                        highlightLine(cursorPos, str, false, true);
                        cursorPos = mod((cursorPos + 1), str.Length);
                        highlightLine(cursorPos, str, true, true);
                        break;

                    case ConsoleKey.Enter:
                        goto done;

                }
            }
            done:
            Console.Clear();
            return cursorPos;
        }
        #endregion
        #region fancySelect
        public static bool[] fancySelect(string[] str, string message)
        {
            Console.Clear();
            Console.WriteLine(message);
            bool[] ret = new bool[str.Length];
            foreach (string s in str)
            {
                Console.WriteLine(" :{0}", s);
            }
            Console.WriteLine("Done");
            int cursorPos = 0;
            highlightLine(cursorPos, str, ret, true, true);
            while (true)
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);
                switch (cki.Key)
                {
                    case ConsoleKey.UpArrow:
                        highlightLine(cursorPos, str, ret, false, true);
                        cursorPos = mod((cursorPos - 1), str.Length + 1);
                        highlightLine(cursorPos, str, ret, true, true);
                        break;

                    case ConsoleKey.DownArrow:
                        highlightLine(cursorPos, str, ret, false, true);
                        cursorPos = mod((cursorPos + 1), str.Length + 1);
                        highlightLine(cursorPos, str, ret, true, true);
                        break;

                    case ConsoleKey.Enter:
                        if (cursorPos >= str.Length)
                        {
                            goto done;
                        }
                        else
                        {
                            ret[cursorPos] = !ret[cursorPos];
                            highlightLine(cursorPos, str, ret, true, true);
                        }
                        break;

                }
            }
            done:
            Console.Clear();
            return ret;
        }
        public static bool[] fancySelect(string[] str)
        {
            Console.Clear();
            bool[] ret = new bool[str.Length];
            foreach (string s in str)
            {
                Console.WriteLine(" :{0}", s);
            }
            Console.WriteLine("Done");
            int cursorPos = 0;
            highlightLine(cursorPos, str, ret, true);
            while (true)
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);
                switch (cki.Key)
                {
                    case ConsoleKey.UpArrow:
                        highlightLine(cursorPos, str, ret, false);
                        cursorPos = mod((cursorPos - 1), str.Length + 1);
                        highlightLine(cursorPos, str, ret, true);
                        break;

                    case ConsoleKey.DownArrow:
                        highlightLine(cursorPos, str, ret, false);
                        cursorPos = mod((cursorPos + 1), str.Length + 1);
                        highlightLine(cursorPos, str, ret, true);
                        break;

                    case ConsoleKey.Enter:
                        if (cursorPos >= str.Length)
                        {
                            goto done;
                        }
                        else
                        {
                            ret[cursorPos] = !ret[cursorPos];
                            highlightLine(cursorPos, str, ret, true);
                        }
                        break;

                }
            }
            done:
            Console.Clear();
            return ret;
        }
        #endregion
        #region highlight Lines
        private static void highlightLine(int i, string[] str, bool[] cheaked, bool highlight)
        {
            ClearConsoleLine(i);

            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, i);
            if (highlight)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            string mess;
            if (i >= str.Length)
            {
                mess = "Done";
            }
            else
            {
                if (cheaked[i])
                {
                    mess = "√:" + str[i];
                }
                else
                {
                    mess = " :" + str[i];
                }
            }
            mess += new string(' ', Console.WindowWidth - mess.Length);
            Console.Write(mess);

            Console.SetCursorPosition(0, currentLineCursor);

            Console.ResetColor();
        }

        private static void highlightLine(int i, string[] str, bool[] cheaked, bool highlight, bool offset)
        {
            ClearConsoleLine(i + 1);

            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, i + 1);
            if (highlight)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            string mess;
            if (i >= str.Length)
            {
                mess = "Done";
            }
            else
            {
                if (cheaked[i])
                {
                    mess = "√:" + str[i];
                }
                else
                {
                    mess = " :" + str[i];
                }
            }
            mess += new string(' ', Console.WindowWidth - mess.Length);
            Console.Write(mess);

            Console.SetCursorPosition(0, currentLineCursor);

            Console.ResetColor();
        }

        private static void highlightLine(int i, string[] str, bool highlight)
        {
            ClearConsoleLine(i);
            
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, i);
            if (highlight)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            string mess = str[i];
            mess += new string(' ', Console.WindowWidth - mess.Length);
            Console.Write(mess);

            Console.SetCursorPosition(0, currentLineCursor);

            Console.ResetColor();
        }

        private static void highlightLine(int i, string[] str, bool highlight, bool offset)
        {
            ClearConsoleLine(i + 1);
           
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, i + 1);
            if (highlight)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            string mess = str[i];
            mess += new string(' ', Console.WindowWidth - mess.Length);
            Console.Write(mess);

            Console.SetCursorPosition(0, currentLineCursor);

            Console.ResetColor();
        }
        #endregion
        #region Console Line clearing
        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
        public static void ClearConsoleLine(int i)
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, i);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
        #endregion

        private static int mod(int a, int n)
        {
            int result = a % n;
            if ((a < 0 && n > 0) || (a > 0 && n < 0))
                result += n;
            return result;
        }

    }
}