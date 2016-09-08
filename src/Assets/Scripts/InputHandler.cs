using UnityEngine;
using System.Collections;

public class InputHandler : MonoBehaviour {

	#region PRIVATE_MEMBERS
	private const float DOUBLE_TAP_MAX_DELAY = 0.5f;//seconds
	private int mTapCount = 0;
	private float mTimeSinceLastTap = 0;
	#endregion //PRIVATE_MEMBERS

	public GameObject tux;


	#region MONOBEHAVIOUR_METHODS
	void Start() 
	{
		mTapCount = 0;
		mTimeSinceLastTap = 0;
	}

	void Update() 
	{
		HandleTap2();

		#if UNITY_ANDROID
		// On Android, the Back button is mapped to the Esc key
		if (Input.GetKeyUp(KeyCode.Escape))
		{
		#if (UNITY_5_2 || UNITY_5_1 || UNITY_5_0)
		Application.LoadLevel("Vuforia-1-About");   
		#else // UNITY_5_3 or above
			//UnityEngine.SceneManagement.SceneManager.LoadScene("Vuforia-1-About");
			Debug.Log("test ");
		#endif
		}
		#endif
	}
	#endregion //MONOBEHAVIOUR_METHODS


	#region PRIVATE_METHODS
    private void HandleTap2()
    {
        for (int i= 0; i < Input.touchCount; ++i)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Began) tux.GetComponent<TuxController>().MakeAnimJump();
        }
        if (Input.GetMouseButtonUp(0)) tux.GetComponent<TuxController>().MakeAnimJump();
    }

	private void HandleTap()
	{
		if (mTapCount == 1)
		{
			mTimeSinceLastTap += Time.deltaTime;
			if (mTimeSinceLastTap > DOUBLE_TAP_MAX_DELAY)
			{
				// too late for double tap, we confirm it was a single tap
				OnSingleTapConfirmed();

				// reset touch count and timer
				mTapCount = 0;
				mTimeSinceLastTap = 0;
			}
		}
		else if (mTapCount == 2)
		{
			// we got a double tap
			OnDoubleTap();

			// reset touch count and timer
			mTimeSinceLastTap = 0;
			mTapCount = 0;
		}

		if (Input.GetMouseButtonUp(0))
		{
			mTapCount++;
		}
	}

	protected virtual void OnSingleTapConfirmed()
	{
		Debug.Log("Single tap found ");
		tux.GetComponent<TuxController>().MakeAnimJump();
	}

	protected virtual void OnDoubleTap()
	{
		Debug.Log("Double tap found ");
		CameraSettings camSettings = GetComponentInChildren<CameraSettings>();
		if (camSettings) {
			camSettings.TriggerAutofocusEvent();
		}
	}
	#endregion // PRIVATE_METHODS
}
