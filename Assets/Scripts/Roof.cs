using System.Collections;
using UnityEngine;

/// <summary>
/// Used to animate roofs as the player traverses underneath them. Written by Justin Ortiz
/// </summary>
public class Roof : MonoBehaviour {
	private static float transitionSpeed = 0.95f;

	private bool transitionEnabled;
	private bool transitioning;
	private bool cancelCurrentTransition;
	[SerializeField, Tooltip("The sprite to make transparent upon player trigger.")]
	private SpriteRenderer sprite;

	private void Awake() {
		if (sprite == null) { //if not given value in inspector
			sprite = GetComponent<SpriteRenderer>(); //try to get attached sprite
		}
	}

	public void Disable() {
		transitionEnabled = false;
	}

	public void Enable() {
		transitionEnabled = true;
	}

	private void OnTriggerEnter(Collider other) {
		if (transitionEnabled) { //building open/accessible to player
			if (other.CompareTag("Player")) { //if player entered building
				if (transitioning) { //if currently transitioning
					cancelCurrentTransition = true; //flag for canceling that transition
				}
				StartCoroutine(TransitionTransparency(0f)); //start new transition
			}
		}
	}

	private void OnTriggerExit(Collider other) {
		if (transitionEnabled) { //building open/accessible to player
			if (other.CompareTag("Player")) { //if player exited building
				if (transitioning) { //if currently transitioning
					cancelCurrentTransition = true; //flag for canceling that transition
				}
				StartCoroutine(TransitionTransparency(1f)); //start new transition
			}
		}
	}

	public IEnumerator TransitionTransparency(float desiredAlpha) {
		if (sprite == null) { //if sprite is null
			yield break; //exit async
		}

		while (transitioning) { //wait until previous transition exits
			yield return new WaitForEndOfFrame();
		}
		transitioning = true; //flag this transition as underway

		bool alphaIncrease; //determine whether alpha needs to increase
		if (sprite.color.a < desiredAlpha) { //if curr alpha is less
			alphaIncrease = true; //increase necessary
		} else { //curr alpha is greater
			alphaIncrease = false; //decrease necessary
		}

		Color currColor = sprite.color; //store current color
		while (!cancelCurrentTransition) { //infinitely loop unless a cancel is needed
			if (alphaIncrease) { //if increase needed
				currColor.a += transitionSpeed * Time.deltaTime; //increase the alpha
				sprite.color = currColor; //set the curr color

				if (sprite.color.a >= desiredAlpha) { //if alpha is at desired
					break; //transition complete; leave loop
				}
			} else {
				currColor.a -= transitionSpeed * Time.deltaTime; //decrease the alpha
				sprite.color = currColor; //set the curr color

				if (sprite.color.a <= desiredAlpha) { //if alpha is at desired
					break; //transition complete; leave loop
				}
			}
			yield return new WaitForEndOfFrame();
		}

		if (cancelCurrentTransition) { //if exited loop due to cancel
			cancelCurrentTransition = false; //mark cancel as complete
		}
		transitioning = false; //no longer transitioning
	}
}
