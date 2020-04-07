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
		Transform currFollow = current.m_Follow;
		current.m_Follow = null;
		yield return new WaitForEndOfFrame();
		current.m_Follow = currFollow;
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
