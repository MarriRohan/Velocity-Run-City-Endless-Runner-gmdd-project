using UnityEngine;

namespace NeonVelocity
{
    public static class RuntimeInputFixer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void FixInputModule()
        {
#if ENABLE_INPUT_SYSTEM
            var standalone = Object.FindFirstObjectByType<UnityEngine.EventSystems.StandaloneInputModule>();
            if (standalone != null)
            {
                var go = standalone.gameObject;
                Object.DestroyImmediate(standalone);
                if (go.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
                {
                    go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                }
                Debug.Log("[NeonVelocity] Runtime: Replaced StandaloneInputModule with InputSystemUIInputModule.");
            }
#endif
        }
    }
}
