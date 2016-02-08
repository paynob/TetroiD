using UnityEngine;
using System.Collections;

public class CameraSettings : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Camera.main.aspect = 10f / 16f;
	}
}
