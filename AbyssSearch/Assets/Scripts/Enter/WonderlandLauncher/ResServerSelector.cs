//  **********************************************
//  Copyright(c) 2021 by com.ustar
//  All right reserved
// 
//  Author : Besure.Chen
//  Date :2023-11-02 12:18
//  Ver : 1.0.0
//  Description : ResServerSelector.cs
//  ChangeLog :
//  **********************************************
#if !PRODUCTION_PACKAGE || UNITY_EDITOR
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Wonderland
{
    public class ResServerSelector:MonoBehaviour
    {
        private int index = 0;
        private ToggleGroup _resourceDropdown;
        private Button _button;

        private TaskCompletionSource<bool> waitSelectTask;

        private string[] availableResUrls;
        private void Awake()
        {
            // ConfigurationController.Instance.version = VersionStatus.DEBUG;
            //
            // availableResUrls = new[]
            // {
            //     ConfigurationController.Instance.ResServerURL,
            //     ConfigurationController.Instance.ResServerURL + "dev/"
            // };
            
            _resourceDropdown = transform.Find("Content/ToggleGroup").GetComponent<ToggleGroup>();
            
            if (PlayerPrefs.HasKey("ResUrlDropdownValue"))
            {
                index = PlayerPrefs.GetInt("ResUrlDropdownValue");
            }

            var lastToggleOn = index;
            for (int i = 0; i < _resourceDropdown.transform.childCount; i++)
            {
                var tmpIndex = i;
                var child = _resourceDropdown.transform.GetChild(i).GetComponent<Toggle>();
                child.onValueChanged.AddListener(arg0 =>
                {
                    if (arg0)
                        index = tmpIndex;
                });
                if (tmpIndex == lastToggleOn)
                    child.isOn = true;
            }
            
            _button = transform.Find("Content/ConfirmButton").GetComponent<Button>();
            _button.onClick.AddListener(OnConfirmClicked);
        }

        public async Task WaitServerSelection()
        {
            waitSelectTask = new TaskCompletionSource<bool>();
            await waitSelectTask.Task;
        }

        private void OnConfirmClicked()
        {
            var resUrl = availableResUrls[index];
            // ConfigurationController.Instance.Res_Server_URL_Beta = resUrl;
            PlayerPrefs.SetInt("ResUrlDropdownValue", index);

            if (waitSelectTask != null)
                waitSelectTask.SetResult(true);
            GameObject.Destroy(gameObject);
        }
    }
}

#endif