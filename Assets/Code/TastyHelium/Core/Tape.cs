using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TastyHelium
{
    // Holds everything together like a Band-Aid.
    // Everything depends on it. Literally
    public static class /*Flex*/Tape
    {
        // General Behavior Variables:

        static Tape()
        {
            GameObject g = SafeFind("__game");

            // Locate General Behaviors
        }

        private static GameObject SafeFind(string s)
        {
            GameObject g = GameObject.Find(s);
            if (g == null)
            {
                Error("GameObject " + s + "  not on _preload.");
            }
            return g;
        }

        private static T SafeComponent<T>(GameObject g) where T: Component
        {
            T c = g.GetComponent<T>();
            if (c == null)
            {
                Error("Component " + typeof(T).Name + " not on _preload.");
            }
            return c;
        }

        private static void Error(string error)
        {
            Debug.Log(">>> Cannot proceed... " + error);
            Debug.Log(">>> It is very likely you just forgot to launch");
            Debug.Log(">>> from scene zero, the _preload scene.");
        }
    }
}
