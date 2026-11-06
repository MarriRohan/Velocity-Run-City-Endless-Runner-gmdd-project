using UnityEngine;

namespace NeonVelocity
{
    [System.Serializable]
    public class DistrictTheme
    {
        public string districtName;
        public float startDistance;
        public Color fogColor;
        public float fogDensity;
        public Color directionalLightColor;
        public float lightIntensity;
        public Color ambientSkyColor;
    }

    public class CityDistrictManager : MonoBehaviour
    {
        public static CityDistrictManager Instance { get; private set; }

        [Header("Districts")]
        [SerializeField] private DistrictTheme[] districts;
        [SerializeField] private Light directionalLight;

        private int currentDistrictIndex = 0;
        private Color targetFogColor;
        private float targetFogDensity;
        private Color targetLightColor;
        private float targetLightIntensity;

        public string CurrentDistrictName => (districts != null && districts.Length > 0) ? districts[currentDistrictIndex].districtName : "METROPOLIS";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeDistricts();
        }

        private void InitializeDistricts()
        {
            if (districts == null || districts.Length == 0)
            {
                districts = new DistrictTheme[]
                {
                    new DistrictTheme
                    {
                        districtName = "DOWNTOWN FINANCIAL DISTRICT",
                        startDistance = 0f,
                        fogColor = new Color(0.72f, 0.80f, 0.88f),
                        fogDensity = 0.0035f,
                        directionalLightColor = new Color(1.0f, 0.96f, 0.88f),
                        lightIntensity = 1.3f,
                        ambientSkyColor = new Color(0.55f, 0.65f, 0.78f)
                    },
                    new DistrictTheme
                    {
                        districtName = "COMMERCIAL AVENUE",
                        startDistance = 600f,
                        fogColor = new Color(0.75f, 0.82f, 0.90f),
                        fogDensity = 0.003f,
                        directionalLightColor = new Color(1.0f, 0.98f, 0.94f),
                        lightIntensity = 1.35f,
                        ambientSkyColor = new Color(0.60f, 0.70f, 0.82f)
                    },
                    new DistrictTheme
                    {
                        districtName = "METROPOLITAN PARKWAY",
                        startDistance = 1400f,
                        fogColor = new Color(0.82f, 0.75f, 0.68f),
                        fogDensity = 0.004f,
                        directionalLightColor = new Color(1.0f, 0.85f, 0.65f),
                        lightIntensity = 1.25f,
                        ambientSkyColor = new Color(0.65f, 0.62f, 0.68f)
                    },
                    new DistrictTheme
                    {
                        districtName = "HARBOR EXPRESSWAY",
                        startDistance = 2400f,
                        fogColor = new Color(0.70f, 0.80f, 0.88f),
                        fogDensity = 0.003f,
                        directionalLightColor = new Color(0.98f, 0.95f, 0.90f),
                        lightIntensity = 1.3f,
                        ambientSkyColor = new Color(0.55f, 0.68f, 0.80f)
                    },
                    new DistrictTheme
                    {
                        districtName = "CENTRAL METROPOLIS",
                        startDistance = 3500f,
                        fogColor = new Color(0.78f, 0.65f, 0.58f),
                        fogDensity = 0.0045f,
                        directionalLightColor = new Color(1.0f, 0.78f, 0.55f),
                        lightIntensity = 1.2f,
                        ambientSkyColor = new Color(0.62f, 0.55f, 0.60f)
                    }
                };
            }

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            ApplyDistrict(0, instant: true);
        }

        public void CheckDistanceProgression(float distance)
        {
            int nextIndex = currentDistrictIndex;
            for (int i = districts.Length - 1; i >= 0; i--)
            {
                if (distance >= districts[i].startDistance)
                {
                    nextIndex = i;
                    break;
                }
            }

            if (nextIndex != currentDistrictIndex)
            {
                currentDistrictIndex = nextIndex;
                ApplyDistrict(currentDistrictIndex, instant: false);

                // Notify UI of new district
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowDistrictNotification(districts[currentDistrictIndex].districtName);
                }
            }
        }

        private void ApplyDistrict(int index, bool instant)
        {
            var d = districts[index];
            targetFogColor = d.fogColor;
            targetFogDensity = d.fogDensity;
            targetLightColor = d.directionalLightColor;
            targetLightIntensity = d.lightIntensity;

            if (instant)
            {
                RenderSettings.fogColor = targetFogColor;
                RenderSettings.fogDensity = targetFogDensity;
                RenderSettings.ambientSkyColor = d.ambientSkyColor;

                if (directionalLight != null)
                {
                    directionalLight.color = targetLightColor;
                    directionalLight.intensity = targetLightIntensity;
                }
            }
        }

        private void Update()
        {
            // Smoothly interpolate environmental lighting & fog
            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, targetFogColor, Time.deltaTime * 1.5f);
            RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, targetFogDensity, Time.deltaTime * 1.5f);

            if (directionalLight != null)
            {
                directionalLight.color = Color.Lerp(directionalLight.color, targetLightColor, Time.deltaTime * 1.5f);
                directionalLight.intensity = Mathf.Lerp(directionalLight.intensity, targetLightIntensity, Time.deltaTime * 1.5f);
            }
        }

        public void ResetDistricts()
        {
            currentDistrictIndex = 0;
            ApplyDistrict(0, instant: true);
        }
    }
}
