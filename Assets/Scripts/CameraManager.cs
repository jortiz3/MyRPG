using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
	public static CameraManager instance;

	private CinemachineVirtualCamera current;
	[SerializeField]
	private CinemachineVirtualCamera mainCam;
	[SerializeField]
	private CinemachineVirtualCamera structureEditCam;

	private void Awake() {
		if (instance != null) {
			Destroy(gameObject);
		} else {
			instance = this;
		}
	}

	public void Reset() {
		SetCurrentCam(mainCam);
	}

	private void SetCurrentCam(CinemachineVirtualCamera cam) {
		if (current != null) {
			current.enabled = false;
		}

		cam.enabled = true;
		current = cam;
	}

	public void ShowStructureCam(Transform follow) {
		structureEditCam.m_Follow = follow;
		SetCurrentCam(structureEditCam);
	}
}
