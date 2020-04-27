using Cinemachine;
using System.Collections;
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
			SetCurrentCam(mainCam);
		}
	}

	private IEnumerator Refocus() {
		current.enabled = false;
		yield return new WaitForEndOfFrame();
		current.enabled = true;
	}

	/// <summary>
	/// To be used when the player's position is abruptly changed (i.e. teleport).
	/// </summary>
	public void RefocusOnTarget() {
		StartCoroutine(Refocus());
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
