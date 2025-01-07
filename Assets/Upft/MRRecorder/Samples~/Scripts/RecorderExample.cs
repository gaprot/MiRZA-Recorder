using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using Upft.MRRecorder.Runtime;
using Upft.MRRecorder.Runtime.Utils;

namespace Upft.MRRecorder.Sample
{
    public class RecorderExample : MonoBehaviour
    {
        [SerializeField]
        private ARCameraManager _cameraManager;

        [SerializeField]
        private Camera _sceneCamera;

        [Header("UI")]
        [SerializeField]
        private Button _startRecordingButton;

        [SerializeField]
        private TMP_Dropdown _qualityDropdown;

        private VideoRecorder _recorder;

        private void Start()
        {
            _cameraManager ??= FindAnyObjectByType<ARCameraManager>();
            _sceneCamera ??= Camera.main;
            
            CheckPermission();

            _recorder = new VideoRecorder(_cameraManager, _sceneCamera);

            _startRecordingButton.onClick.AddListener(() =>
            {
                if (_recorder.IsRecording)
                {
                    _recorder.StopRecording();
                }
                else
                {
                    var resolution = ((ResolutionQuality)_qualityDropdown.value).ToResolution();
                    var outputPath = PathUtils.GetDefaultOutputPath();
                    var fileName = PathUtils.GetDefaultVideoFileName();
                    _recorder.StartRecording(new(resolution, outputPath, fileName));
                }
            });
        }

        private void CheckPermission()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
            }
        }

        private void Update()
        {
            UpdateView();
        }

        private void UpdateView()
        {
            _startRecordingButton.image.color = _recorder.IsRecording ? Color.green : Color.red;
        }

        private void OnDestroy()
        {
            _startRecordingButton.onClick.RemoveAllListeners();
        }
    }
}
