using System;

namespace global
{
    class Logger
    {
        private Mode debugMode;

        private enum Mode
        {
            NoDebug, Debug, DebugRelou
        }

        public Logger(int mode)
        {
            // Par défault est en mode debug
            if (mode == 0) debugMode = Mode.NoDebug;
            else if (mode == 1) debugMode = Mode.Debug;
            else if (mode == 2) debugMode = Mode.DebugRelou;
            else debugMode = Mode.Debug;
        }

        public void Debug(string msg)
        {
            if (debugMode == Mode.Debug || debugMode == Mode.DebugRelou)
                Console.WriteLine($"%________BEGIN_DEBUG_BEGIN________%\n\n{msg}\n\n%________END_DEBUG_END________%");
        }
        public void DebugRelou(string msg)
        {
            if (debugMode == Mode.DebugRelou)
                Console.WriteLine($"%________BEGIN_DEBUG_BEGIN________%\n\n{msg}\n\n%________END_DEBUG_END________%");
        }

        /// <summary> Change le mode de debuggage en fonction de mode : <br>
        /// mode=0 -> NoDebug <br>
        /// mode=1 -> Debug <br>
        /// mode=2 -> DebugRelou </summary>
        public void ChangeDebugMode(int mode)
        {
            if (mode == 0) debugMode = Mode.NoDebug;
            else if (mode == 1) debugMode = Mode.Debug;
            else if (mode == 2) debugMode = Mode.DebugRelou;
            else
                Console.WriteLine("Logger.ChangeDebugMode(int) : Idiot cette valeur n'est pas prise en compte...");
        }
    }
}