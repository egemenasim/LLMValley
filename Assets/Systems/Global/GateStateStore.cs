using UnityEngine;

namespace LLMValley.SaveSystem
{
    public static class GateStateStore
    {
        private const string GateKeyPrefix = "GateOpen_";

        public static bool GetGateOpen(string gateId, bool defaultValue = false)
        {
            if (string.IsNullOrWhiteSpace(gateId))
            {
                return defaultValue;
            }

            return PlayerPrefs.GetInt(GateKeyPrefix + gateId, defaultValue ? 1 : 0) == 1;
        }

        public static void SetGateOpen(string gateId, bool isOpen)
        {
            if (string.IsNullOrWhiteSpace(gateId))
            {
                return;
            }

            PlayerPrefs.SetInt(GateKeyPrefix + gateId, isOpen ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
