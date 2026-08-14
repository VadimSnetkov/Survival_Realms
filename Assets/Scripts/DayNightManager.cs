using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

namespace BlockBuildingCraftingSystem
{
    public class DayNightManager : MonoBehaviour
    {
        [HideInInspector]
        public float time;
        public TimeSpan currenttime;
        public Transform SunTransform;
        public Light Sun;
        public Text timetext;
        public Text daytext;

        [HideInInspector]
        public int day;
        private float intensity;
        public Color ambienceColorNight;
        private int speed = 128;
        public float sunRiseHour = 8;
        public float sunSetHour = 20;
        public static DayNightManager Instance;
        public Material skyboxMaterial;

        [Header("Settings")]
        public bool allowTimeChangeInGame = true;
        public float minHour = 0f;
        public float maxHour = 23.99f;

        public int defaultSpeed = 128;

        private const string PREF_TIME = "TimeOfDaySeconds";
        private const string PREF_SPEED = "TimeSpeed";

        [HideInInspector]
        public bool isDark = false;

        public List<Light> lights;

        public UnityEvent EventToInvokeWhenSunRise;
        public UnityEvent EventToInvokeWhenSunSet;


        private void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            ChangeTime();
        }

        private void Start()
        {
            skyboxMaterial.SetFloat("_Exposure", 1);
            RenderSettings.ambientIntensity = 1;

            day = PlayerPrefs.GetInt("Day", 0);
            daytext.text = "DAY " + day.ToString();

            speed = PlayerPrefs.GetInt(PREF_SPEED, defaultSpeed);

            if (PlayerPrefs.HasKey(PREF_TIME))
                time = PlayerPrefs.GetFloat(PREF_TIME);
            else
                time = 46400f * (sunRiseHour / 24f);
        }


        public void Sleep()
        {
            if(time > 0 && time < 3600 * sunRiseHour)
            {
                // day is already counted.
            }
            else
            {
                day += 1;
            }
            PlayerPrefs.SetInt("Day", day);
            PlayerPrefs.Save();
            time = 3600 * sunRiseHour;
            daytext.text = "DAY " + day.ToString();
        }

        private int hour = 0;
        private int minutes = 0;

        public void ChangeTime()
        {
            time += Time.deltaTime * speed;
            if (time > 86400)
            {
                day += 1;
                time = 0;
                daytext.text = "DAY " + day.ToString();
                PlayerPrefs.SetInt("Day", day);
                PlayerPrefs.Save();
                PlayerPrefs.SetFloat(PREF_TIME, time);

            }

            currenttime = TimeSpan.FromSeconds(time);
            string[] temptime = currenttime.ToString().Split(":"[0]);
            minutes = Convert.ToInt32(temptime[1]);
            hour = Convert.ToInt32(temptime[0]);
            timetext.text = temptime[0] + ":" + temptime[1];
            SunTransform.rotation = Quaternion.Euler(new Vector3((time - 21600) / 86400 * 360, 0, 0));

            if (time > 43200)
                intensity = 1 - (43200 - time) / 43200;
            else
                intensity = 1 - ((43200 - time) / 43200 * -1);

            Sun.intensity = 1.8f - intensity;
            float currentRotation = RenderSettings.skybox.GetFloat("_Rotation");
            RenderSettings.skybox.SetFloat("_Rotation", currentRotation + 0.5f * Time.deltaTime);

            if (time < sunRiseHour * 3600 || time > sunSetHour * 3600)
            {
                skyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(skyboxMaterial.GetFloat("_Exposure"), Sun.intensity, 0.02f));
                RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, ambienceColorNight, 0.02f);
                if (!isDark)
                {

                    for (int i = 0; i < lights.Count; i++)
                    {
                        lights[i].enabled = true;
                    }
                    isDark = true;
                    if(EventToInvokeWhenSunSet != null)
                    {
                        EventToInvokeWhenSunSet.Invoke();
                    }
                }
            }
            else
            {
                skyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(skyboxMaterial.GetFloat("_Exposure"), 1, 0.02f));
                RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, Color.white, 0.02f);
                if (isDark)
                {
                    for (int i = 0; i < lights.Count; i++)
                    {
                        lights[i].enabled = false;
                    }
                    isDark = false;
                    if (EventToInvokeWhenSunRise != null)
                    {
                        EventToInvokeWhenSunRise.Invoke();
                    }
                }
            }
        }
        public void SetTimeSpeed(float newSpeed)
        {
            speed = Mathf.RoundToInt(newSpeed);
            PlayerPrefs.SetInt(PREF_SPEED, speed);
            PlayerPrefs.Save();
        }

        public void SetTimeOfDayHours(float hour01to24)
        {
            if (!allowTimeChangeInGame) return;

            float h = Mathf.Clamp(hour01to24, minHour, maxHour);
            time = h * 3600f;

            PlayerPrefs.SetFloat(PREF_TIME, time);
            PlayerPrefs.Save();

            ApplyTimeVisuals();
        }

        public void SetTimeOfDayNormalized(float value01)
        {
            if (!allowTimeChangeInGame) return;

            value01 = Mathf.Clamp01(value01);
            time = value01 * 86400f;

            PlayerPrefs.SetFloat(PREF_TIME, time);
            PlayerPrefs.Save();

            ApplyTimeVisuals();
        }

        public float GetTimeOfDayHours()
        {
            return time / 3600f;
        }

        public float GetTimeOfDayNormalized()
        {
            return time / 86400f;
        }

        private void ApplyTimeVisuals()
        {
            currenttime = TimeSpan.FromSeconds(time);
            string[] temptime = currenttime.ToString().Split(":"[0]);
            minutes = Convert.ToInt32(temptime[1]);
            hour = Convert.ToInt32(temptime[0]);
            timetext.text = temptime[0] + ":" + temptime[1];

            SunTransform.rotation = Quaternion.Euler(new Vector3((time - 21600) / 86400 * 360, 0, 0));

            if (time > 43200)
                intensity = 1 - (43200 - time) / 43200;
            else
                intensity = 1 - ((43200 - time) / 43200 * -1);

            Sun.intensity = 1.8f - intensity;

            if (time < sunRiseHour * 3600 || time > sunSetHour * 3600)
            {
                skyboxMaterial.SetFloat("_Exposure", Sun.intensity);
                RenderSettings.ambientLight = ambienceColorNight;

                if (!isDark)
                {
                    for (int i = 0; i < lights.Count; i++) lights[i].enabled = true;
                    isDark = true;
                    if (EventToInvokeWhenSunSet != null) EventToInvokeWhenSunSet.Invoke();
                }
            }
            else
            {
                skyboxMaterial.SetFloat("_Exposure", 1);
                RenderSettings.ambientLight = Color.white;

                if (isDark)
                {
                    for (int i = 0; i < lights.Count; i++) lights[i].enabled = false;
                    isDark = false;
                    if (EventToInvokeWhenSunRise != null) EventToInvokeWhenSunRise.Invoke();
                }
            }
        }

    }
}