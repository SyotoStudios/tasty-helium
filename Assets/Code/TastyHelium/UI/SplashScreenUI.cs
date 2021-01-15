using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TastyHelium
{
    public class SplashScreenUI : MonoBehaviour
    {
        public TMPro.TMP_Text ProgressText;
        public TMPro.TMP_Text DescriptionText;

        public void UpdateProgress(float progress, string description) {
            ProgressText?.SetText(string.Format("{0:P2}", progress));
            DescriptionText?.SetText(description);
        }
    }
}
